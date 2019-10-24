# ACT - Asset Compliance Tracker
ACT is an hybrid on-prem/cloud PCI monitoring solution for VM's or physical servers.

## Components
- CHEF Inspec Compliance Tests
- CHEF Cookbook
- .NET Core 2.1 AWS Lambda
- .NET Core 2.1 MVC Website for AWS Elastic Beanstalk
- .NET Core 2.1 Entity Framework code-first database for AWS Aurora Serverless with migrations

## CHEF Inspec Compliance Tests
There are two compliance specs, one for linux and one for windows servers or clients.  Basically the Specs take attributes passed in from the node that include:
- OS Name
- OS Version
- TCP Ports
- UDP Ports
- Installed Software/Packages/Features

The Inspec tests then do an RPM query for Linux or some Powershell commands for Windows and then *netstat* for both to get a list of installed components and open ports and compare it to the list passed in.  They are highly optimized to only run the commands 1 time per run and typically are sub second.

## CHEF Cookbook
This cookbook is a simple wrapper around CHEF's audit cookbook.  its sole recipe is used to make a REST call to the ACT Website which returns the following information from ACTs database.
- OS Name
- OS Version
- TCP Ports
- UDP Ports
- Installed Software/Packages/Features

It then sets the list returned into the nodes attributes where they can be retrieved by the CHEF Inspec compliance tests.

## .NET Core 3.0 AWS Lambda
Lambda is an AWS serverless offering. The Lambda component is used to Gather information from the various CHEF Automate servers configured in the database.  It is also an extensible framework allowing for configuration based lambda functions to be added.  There is only one Lambda that need be deployed, but it takes as single JSON argument in the form of

    {"name":"function to run", "index":0}

It supports the following function names out of the box:
- databaseupdate - apply the Entity Framework migrations.  Should be run ON DEMAND.
- gather - (requires index) Gathers the information from the CHEF Automate server for the environment ID passed into the "index" field of the JSON argument. Configure an AWS CloudWatch Rule to run this on an INTERVAL (hourly).
- email - sends emails out for  - Configure an AWS CloudWatch Rule to run this on an INTERVAL (daily).
  - index 0 - Unassigned nodes
  - index 1 - Not reporting nodes
- reset - Resets the compliance status of nodes that have not called in for more than 48 hours to "Not Reporting".  Configure an AWS CloudWatch Rule to run this on an INTERVAL (hourly).
- purgedetails - purges the compliance details. .  Configure an AWS CloudWatch Rule to run this on an INTERVAL (daily).
- purgeruns - purges the compliance runs older than 28 days.  Configure an AWS CloudWatch Rule to run this on an INTERVAL (daily).
- purgeinactive - purges the nodes that are have a deactivated date more than 7 days old.  Configure an AWS CloudWatch Rule to run this on an INTERVAL.

AWS Cloudwatch can hold logs for this Lambda function and Rules can be configured for each of the calls.  The suggested interval is above in (parenthesis).

### Extending the Lambda
You can extend the Lambda by building a new .NET Core DLL that has at least one class that inherits from "act.core.etl.lambda.LambdaAddInBase".  

    public class MyLambdaAddin: LambdaAddinBase
    {
        public override IDictionary<string, Func<IServiceScope, Argument, Task<int>>> ProcessFunctions { get; } =

            new Dictionary<string, Func<IServiceScope, Argument, Task<int>>>
            {
                {
                    "products",
                    async (scope, args) => await scope.ServiceProvider.GetService<IMyETLFunc>().AddOrUpdateProducts()
                },
                {
                    "functions",
                    async (scope, args) => await scope.ServiceProvider.GetService<IMyETLFunc>().AddOrUpdateFunctions()
                }
            };

        public override void AddServices(IServiceCollection services){
            services.AddSingleton<IMyETLFunc,MyETLFunc>();
        }
    }

You can then edit the appsettings.json file you add your component:

    {
      ...,
      "AddIns":[
        "org.mycompany.MyEtlFunctions"
      ]
    }

**Important**: You must also include the compiled DLL and all of its **dependencies** in the Lambda binaries folder as as sibling to *act.core.etl.lambda.dll*

## .NET Core 3.0 MVC Website for Docker on AWS Fargate
The website is a response UI based on the [jayMVC](https://github.com/unscrum/jaymvc) framework.  It interfaces with an ADFS Server via FederationMetaData for logins and allows users to be able to add *Build Specs* for servers. The website is built to be a central repository for housing specs for all nodes across all environments, including Windows/Linux Servers as well as Appliances, UNIX, Mainframes and any other types.  Although we only have Compliance Specs created for Linux and Windows the website can be a one stop shop for every type of server during a PCI Audit.

### Concepts
 Build Specs is that there are some things that are platform/OS specific and some that are application specific.  The Website allows **OS Specs** to be created to cover the basic OS install for your company, and the **App  Specs** to inherit from an **OS Spec** and extend it by adding in more installed components or open ports.

 Nodes can be assigned to an **App Spec** at the end of the wizard, via the **App Spec** Search page, or via the node search page.  

 Nodes when imported should be assigned an PCI Class:
  - A = 1 - Most Secure - touches PCI data
  - B = 2 - More Secure - doesn't touch PCI data but has contact with **A** nodes.
  - C = 4 - Doesn't touch PCI data or have any communication with nodes that are **A**.

  Nodes can can also be assigned to platform type when imported:
  - Linux = 0
  - Windows Server = 1
  - Other = 2
  - Unix = 3
  - Windows Client = 4
  - Appliance = 5

### Screens
The dashboard gives graphs that produce an overview of your organizations PCI Health.

There are screens for searching and adding/editing **Build Specification** as well as Report screens for an **App Spec** that can be shown to PCI Assessors.

The **Spec wizard** will change based on the kind of server. Windows Servers will have 4 steps.
- Windows OS Features
- Windows 3rd Party Apps
- Ports

While Linux Servers will only have 3
- Packages
- Ports

and Other will only have **Ports**

There is also a Node search feature that allows for quickly finding Nodes and assigning them for specs.
