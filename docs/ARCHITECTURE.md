# ACT - Asset Compliance Tracker Architecture

## System Overview

ACT (Asset Compliance Tracker) is a hybrid on-premises/cloud PCI compliance monitoring solution designed to track and validate the compliance status of virtual machines and physical servers across multiple environments.

## Architecture Components

### Core Components

1. **ACT Web Application** (.NET 8.0 MVC)
   - Hosted on AWS Elastic Beanstalk
   - Provides web interface for compliance management
   - Handles user authentication via ADFS
   - Manages build specifications and compliance reports

2. **ACT Lambda Functions** (.NET 8.0)
   - Serverless data processing functions
   - Handles scheduled tasks via CloudWatch Events
   - Performs database operations and data gathering

3. **MySQL Database** (AWS Aurora Serverless)
   - Stores compliance specifications and node data
   - Entity Framework Code-First with migrations
   - Supports both QA and Production environments

4. **Chef Cookbook & InSpec Profiles**
   - Deploys compliance tests to target servers
   - Retrieves specifications from ACT web service
   - Executes compliance validation on Linux/Windows servers

## AWS Deployment Architecture

```mermaid
graph TB
    subgraph "AWS Cloud"
        subgraph "Compute Services"
            EB[AWS Elastic Beanstalk<br/>ACT Web Application<br/>.NET 8.0 MVC]
            Lambda[AWS Lambda<br/>ACT ETL Functions<br/>.NET 8.0]
        end
        
        subgraph "Storage & Database"
            Aurora[AWS Aurora Serverless<br/>MySQL Database]
            S3[AWS S3<br/>Artifacts & Logs]
        end
        
        subgraph "DevOps & Monitoring"
            CB[AWS CodeBuild<br/>CI/CD Pipeline]
            CW[AWS CloudWatch<br/>Logs & Events]
            SM[AWS Systems Manager<br/>Parameter Store]
        end
        
        subgraph "Networking & Security"
            ALB[Application Load Balancer]
            ADFS[ADFS Server<br/>Authentication]
        end
    end
    
    subgraph "On-Premises Infrastructure"
        subgraph "Chef Infrastructure"
            CS[Chef Server/Automate]
            CN[Chef Nodes<br/>Linux/Windows Servers]
        end
        
        subgraph "Compliance Profiles"
            Linux[InSpec Linux Profile<br/>csg_linux_compliant_server]
            Windows[InSpec Windows Profile<br/>csg_windows_compliant_server]
        end
    end
    
    subgraph "External Systems"
        Mail[SMTP Server<br/>Email Notifications]
        Artifacts[Artifact Repository<br/>InSpec Profiles]
    end
    
    %% Connections
    ALB --> EB
    EB --> Aurora
    EB --> S3
    EB --> ADFS
    Lambda --> Aurora
    Lambda --> S3
    Lambda --> CS
    CW --> Lambda
    CB --> EB
    CB --> Lambda
    CB --> S3
    EB --> SM
    Lambda --> SM
    CS --> CN
    CN --> Linux
    CN --> Windows
    CN --> EB
    Lambda --> Mail
    CN --> Artifacts
    
    %% Styling
    classDef aws fill:#FF9900,stroke:#232F3E,stroke-width:2px,color:#FFFFFF
    classDef onprem fill:#4CAF50,stroke:#2E7D32,stroke-width:2px,color:#FFFFFF
    classDef external fill:#9C27B0,stroke:#4A148C,stroke-width:2px,color:#FFFFFF
    
    class EB,Lambda,Aurora,S3,CB,CW,SM,ALB aws
    class CS,CN,Linux,Windows onprem
    class Mail,Artifacts,ADFS external
```

## Code Structure Architecture

The ACT solution follows a layered architecture pattern with clear separation of concerns across multiple .NET 8.0 projects:

```mermaid
graph TB
    subgraph "Repository Structure"
        subgraph ".NET Solution (ACT.sln)"
            subgraph "Presentation Layer"
                Web[act.core.web<br/>ASP.NET Core MVC<br/>• Controllers<br/>• Views<br/>• Models<br/>• Services<br/>• Framework]
            end
            
            subgraph "Application Layer"
                Lambda[act.core.etl.lambda<br/>AWS Lambda Functions<br/>• Bootstrap<br/>• Migrator<br/>• Function Handlers]
                
                ETL[act.core.etl<br/>Business Logic<br/>• Gatherer<br/>• ComplianceModel<br/>• IGatherer Interface<br/>• MailSettings]
            end
            
            subgraph "Data Access Layer"
                Data[act.core.data<br/>Entity Framework<br/>• ActDbContext<br/>• Entity Models<br/>• Migrations<br/>• Extensions<br/>• Constants]
            end
        end
        
        subgraph "Infrastructure as Code"
            Chef[Chef/cookbooks/act<br/>Configuration Management<br/>• Recipes<br/>• Resources<br/>• Libraries<br/>• Attributes]
            
            Compliance[Compliance/<br/>InSpec Profiles<br/>• Linux Profile<br/>• Windows Profile<br/>• Controls<br/>• Libraries]
        end
        
        subgraph "DevOps & Deployment"
            Docker[Docker/<br/>• Dockerfile<br/>• Container Config]
            
            Build[BuildSpec.yml<br/>AWS CodeBuild<br/>• Build Pipeline<br/>• Artifact Generation]
        end
    end
    
    %% Dependencies
    Web --> ETL
    Web --> Data
    Lambda --> ETL
    Lambda --> Data
    ETL --> Data
    Chef --> Web
    Compliance --> Chef
    
    %% Styling
    classDef presentation fill:#E3F2FD,stroke:#1976D2,stroke-width:2px
    classDef application fill:#F3E5F5,stroke:#7B1FA2,stroke-width:2px
    classDef data fill:#E8F5E8,stroke:#388E3C,stroke-width:2px
    classDef infrastructure fill:#FFF3E0,stroke:#F57C00,stroke-width:2px
    classDef devops fill:#FAFAFA,stroke:#616161,stroke-width:2px
    
    class Web presentation
    class Lambda,ETL application
    class Data data
    class Chef,Compliance infrastructure
    class Docker,Build devops
```

### Project Dependencies

```mermaid
graph LR
    subgraph "Dependency Flow"
        Web[act.core.web] --> ETL[act.core.etl]
        Web --> Data[act.core.data]
        Lambda[act.core.etl.lambda] --> ETL
        Lambda --> Data
        ETL --> Data
    end
    
    subgraph "External Dependencies"
        Web --> AWS1[AWS SDK<br/>S3, Systems Manager]
        Web --> EF1[Entity Framework<br/>MySQL Provider]
        Web --> ADFS[ADFS Authentication]
        
        Lambda --> AWS2[AWS SDK<br/>Lambda, S3, Core]
        Lambda --> EF2[Entity Framework<br/>MySQL Provider]
        
        ETL --> MySQL[MySQL Connector]
        Data --> EF3[Entity Framework<br/>Core & Tools]
    end
    
    classDef project fill:#E1F5FE,stroke:#0277BD,stroke-width:2px
    classDef external fill:#FFF8E1,stroke:#F57F17,stroke-width:2px
    
    class Web,Lambda,ETL,Data project
    class AWS1,AWS2,EF1,EF2,EF3,MySQL,ADFS external
```

### Key Architectural Patterns

1. **Layered Architecture**
   - **Presentation**: MVC web application with controllers, views, and models
   - **Application**: Business logic and Lambda function handlers
   - **Data Access**: Entity Framework with database context and models

2. **Dependency Injection**
   - All projects use Microsoft.Extensions.DependencyInjection
   - Services registered in Bootstrap classes
   - Scoped lifetime for database contexts

3. **Configuration Management**
   - appsettings.json for application configuration
   - AWS Systems Manager for secure parameters
   - Environment-specific configuration files

4. **Entity Framework Code-First**
   - Database schema defined in C# entity classes
   - Migrations for database versioning
   - Repository pattern through DbContext

## Data Flow Architecture

```mermaid
sequenceDiagram
    participant CN as Chef Node
    participant CB as Chef Cookbook
    participant WA as ACT Web App
    participant DB as Aurora Database
    participant LF as Lambda Functions
    participant CS as Chef Server
    participant CW as CloudWatch Events
    
    Note over CW,LF: Scheduled Lambda Execution
    CW->>LF: Trigger gather function (hourly)
    LF->>CS: Retrieve compliance data
    LF->>DB: Update node compliance status
    
    Note over CN,WA: Chef Node Registration
    CN->>CB: Execute ACT cookbook
    CB->>WA: GET /BuildSpec/RetrieveFor?fqdn={node}
    WA->>DB: Query build specifications
    DB-->>WA: Return node specifications
    WA-->>CB: Return JSON specifications
    CB->>CN: Set node attributes
    CN->>CN: Run InSpec compliance tests
    CN->>CS: Report compliance results
    
    Note over CW,LF: Scheduled Maintenance
    CW->>LF: Trigger reset function (hourly)
    LF->>DB: Reset non-reporting nodes
    CW->>LF: Trigger email function (daily)
    LF->>DB: Query compliance status
    LF-->>LF: Send email notifications
    CW->>LF: Trigger purge functions (daily)
    LF->>DB: Clean up old data
```

## Component Interactions

### 1. Web Application Flow
- Users authenticate via ADFS Federation
- Create and manage build specifications
- Assign specifications to nodes
- View compliance dashboards and reports

### 2. Lambda Functions Flow
- **gather**: Retrieves data from Chef Automate servers
- **email**: Sends compliance notifications
- **reset**: Resets status of non-reporting nodes
- **purgedetails**: Cleans up compliance details
- **purgeruns**: Removes old compliance runs
- **purgeinactive**: Removes deactivated nodes

### 3. Chef Integration Flow
- Cookbook retrieves node specifications from ACT web service
- Sets attributes for InSpec compliance tests
- Executes platform-specific compliance validation
- Reports results back to Chef Automate

## Deployment Environments

### Production Environment
- **Web App**: Elastic Beanstalk Production environment
- **Database**: Aurora Serverless Production cluster
- **Lambda**: Production Lambda functions with CloudWatch scheduling

### QA Environment  
- **Web App**: Elastic Beanstalk QA environment
- **Database**: Aurora Serverless QA cluster
- **Lambda**: QA Lambda functions for testing

## Security Architecture

### Authentication & Authorization
- ADFS Federation for web application authentication
- AWS IAM roles for service-to-service communication
- Parameter Store for secure configuration management

### Network Security
- Application Load Balancer with SSL termination
- VPC with private subnets for database
- Security groups restricting access to necessary ports

### Data Security
- SSL/TLS encryption in transit
- Database encryption at rest
- Secure parameter storage for connection strings

## Scalability & High Availability

### Auto Scaling
- Elastic Beanstalk auto-scaling for web application
- Aurora Serverless automatic scaling for database
- Lambda automatic scaling based on demand

### High Availability
- Multi-AZ deployment for Elastic Beanstalk
- Aurora Serverless built-in high availability
- Lambda inherent fault tolerance

## Monitoring & Logging

### CloudWatch Integration
- Application logs from Elastic Beanstalk
- Lambda function execution logs
- Custom metrics for compliance tracking

### Alerting
- Email notifications for compliance issues
- CloudWatch alarms for system health
- Chef Automate reporting for node compliance