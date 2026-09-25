# ACT - Asset Compliance Tracker

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![AWS](https://img.shields.io/badge/AWS-Cloud-orange.svg)](https://aws.amazon.com/)
[![Chef](https://img.shields.io/badge/Chef-Automation-green.svg)](https://www.chef.io/)
[![MySQL](https://img.shields.io/badge/MySQL-8.0-blue.svg)](https://www.mysql.com/)

ACT is a comprehensive hybrid on-premises/cloud PCI compliance monitoring solution designed to track and validate the compliance status of virtual machines and physical servers across multiple environments. The system provides automated compliance testing, centralized specification management, and detailed reporting capabilities.

## 🏗️ Architecture Overview

ACT consists of four main components working together to provide end-to-end compliance monitoring:

### Core Components

1. **ACT Web Application** (.NET 8.0 MVC)
   - Centralized web interface for compliance management
   - Build specification creation and management
   - Node assignment and compliance reporting
   - ADFS-based authentication and authorization

2. **ACT Lambda Functions** (.NET 8.0 Serverless)
   - Automated data processing and maintenance tasks
   - Scheduled execution via AWS CloudWatch Events
   - Integration with Chef Automate servers
   - Email notifications and data cleanup

3. **MySQL Database** (AWS Aurora Serverless)
   - Stores compliance specifications and node data
   - Entity Framework Code-First with automated migrations
   - Supports multiple environments (QA, Production)

4. **Chef Integration** (Cookbook + InSpec Profiles)
   - Deploys compliance tests to target servers
   - Platform-specific compliance validation (Linux/Windows)
   - Automated reporting back to Chef Automate

### AWS Services Used

- **AWS Elastic Beanstalk**: Web application hosting
- **AWS Lambda**: Serverless function execution
- **AWS Aurora Serverless**: Managed MySQL database
- **AWS CodeBuild**: CI/CD pipeline automation
- **AWS CloudWatch**: Logging, monitoring, and scheduled events
- **AWS S3**: Artifact storage and data persistence
- **AWS Systems Manager**: Secure configuration management

## 📋 Key Features

### Compliance Management
- **Multi-Platform Support**: Linux, Windows, Unix, and appliance compliance
- **Hierarchical Specifications**: OS specs with application-specific extensions
- **PCI Classification**: A/B/C class assignment for risk-based compliance
- **Automated Testing**: InSpec-based compliance validation
- **Real-time Reporting**: Live compliance status and detailed reports

### Automation & Scheduling
- **Scheduled Data Gathering**: Hourly collection from Chef Automate
- **Automated Notifications**: Daily email alerts for compliance issues
- **Data Maintenance**: Automated cleanup of old compliance data
- **Node Management**: Automatic handling of non-reporting nodes

### Integration Capabilities
- **Chef Automate Integration**: Seamless data collection and reporting
- **ADFS Authentication**: Enterprise single sign-on support
- **Email Notifications**: SMTP-based alerting system
- **Extensible Architecture**: Plugin system for custom functionality

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- MySQL 8.0+
- Visual Studio 2022 or VS Code
- AWS CLI (for cloud deployment)
- Chef Development Kit (for cookbook development)

### Local Development Setup
```bash
# Clone repository
git clone https://github.com/your-org/assetcompliancetracker.git
cd assetcompliancetracker

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Setup database
docker run --name act-mysql -e MYSQL_ROOT_PASSWORD=1234 -p 3306:3306 -d mysql:8.0

# Run web application
cd src/act.core.web
dotnet run
```

**📖 For detailed setup instructions, see [Local Development Guide](docs/LOCAL_DEVELOPMENT.md)**

## 🏗️ Architecture Documentation

For comprehensive architecture information, deployment diagrams, and system design details:

**📖 See [Architecture Documentation](docs/ARCHITECTURE.md)**

## 📦 Repository Structure

```
├── src/                          # Source code
│   ├── act.core.web/            # MVC Web Application
│   ├── act.core.etl.lambda/     # Lambda Functions
│   ├── act.core.etl/            # ETL Business Logic
│   └── act.core.data/           # Entity Framework Data Layer
├── Chef/                        # Chef Cookbook
│   └── cookbooks/act/           # ACT Chef Cookbook
├── Compliance/                  # InSpec Compliance Profiles
│   ├── csg_linux_compliant_server/    # Linux compliance tests
│   └── csg_windows_compliant_server/  # Windows compliance tests
├── Docker/                      # Docker configuration
├── docs/                        # Documentation
│   ├── ARCHITECTURE.md          # System architecture
│   └── LOCAL_DEVELOPMENT.md     # Development setup
├── BuildSpec.yml               # AWS CodeBuild configuration
└── ACT.sln                     # Visual Studio solution
```

## 🔧 Development Workflow

### Web Application Development
```bash
cd src/act.core.web
dotnet run
# Access at https://localhost:44363
```

### Lambda Function Testing
```bash
cd src/act.core.etl.lambda
# Test database migration
dotnet run -- '{"name":"databaseupdate","index":0}'
# Test data gathering
dotnet run -- '{"name":"gather","index":1}'
```

### Chef Cookbook Development
```bash
cd Chef/cookbooks/act
cookstyle .           # Syntax checking
chef exec rspec       # Unit tests
kitchen test          # Integration tests
```

### InSpec Profile Development
```bash
cd Compliance/csg_linux_compliant_server
inspec check .        # Validate profile
inspec exec .         # Run compliance tests
```

## 🚀 Deployment

### AWS Deployment Pipeline
The system uses AWS CodeBuild for automated deployment:

1. **Source**: Code changes trigger the build pipeline
2. **Build**: Compiles .NET applications and packages artifacts
3. **Deploy**: 
   - Web app to Elastic Beanstalk (QA and Production)
   - Lambda functions to AWS Lambda
   - Database migrations via Lambda

### Manual Deployment
```bash
# Build and package
dotnet publish src/act.core.web -c Release
dotnet publish src/act.core.etl.lambda -c Release

# Deploy to AWS (requires AWS CLI configuration)
aws elasticbeanstalk create-application-version --application-name ACT
aws lambda update-function-code --function-name ACT-ETL
```

## 🔍 Lambda Functions

The system includes several Lambda functions for automated operations:

| Function | Purpose | Schedule |
|----------|---------|----------|
| `databaseupdate` | Apply Entity Framework migrations | On-demand |
| `gather` | Collect data from Chef Automate servers | Hourly |
| `email` | Send compliance notifications | Daily |
| `reset` | Reset non-reporting node status | Hourly |
| `purgedetails` | Clean up compliance details | Daily |
| `purgeruns` | Remove old compliance runs (28+ days) | Daily |
| `purgeinactive` | Remove deactivated nodes (7+ days) | Daily |

## 🎯 Compliance Testing

### Supported Platforms
- **Linux**: CentOS, RHEL, RedHat distributions
- **Windows**: Server and Client versions
- **Other**: Unix, Appliances, Mainframes (port-only testing)

### PCI Classification Levels
- **Class A (1)**: Most secure - directly handles PCI data
- **Class B (2)**: More secure - communicates with Class A nodes
- **Class C (4)**: Standard - no PCI data interaction

### Testing Process
1. **Specification Retrieval**: Chef cookbook calls ACT web service
2. **Attribute Setting**: Node attributes configured from specifications
3. **Compliance Execution**: InSpec profiles run platform-specific tests
4. **Result Reporting**: Compliance status reported to Chef Automate
5. **Data Collection**: Lambda functions gather results for centralized reporting

## 🔌 Extensibility

### Lambda Function Extensions
Create custom ETL functions by implementing `LambdaAddinBase`:

```csharp
public class MyLambdaAddin: LambdaAddinBase
{
    public override IDictionary<string, Func<IServiceScope, Argument, Task<int>>> ProcessFunctions { get; } =
        new Dictionary<string, Func<IServiceScope, Argument, Task<int>>>
        {
            { "myfunction", async (scope, args) => await MyCustomFunction() }
        };
}
```

### Configuration
Add to `appsettings.json`:
```json
{
  "AddIns": ["org.mycompany.MyEtlFunctions"]
}
```

## 📊 Monitoring & Alerting

### CloudWatch Integration
- Application and Lambda function logs
- Custom metrics for compliance tracking
- Automated alerting for system issues

### Email Notifications
- Unassigned nodes alerts
- Non-reporting nodes notifications
- Compliance status summaries

## 🔒 Security

### Authentication
- ADFS Federation for web application
- AWS IAM roles for service communication
- Parameter Store for secure configuration

### Data Protection
- SSL/TLS encryption in transit
- Database encryption at rest
- Secure credential management

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow .NET coding standards
- Include unit tests for new functionality
- Update documentation for architectural changes
- Test Chef cookbook changes with Test Kitchen
- Validate InSpec profiles before submission

## 📄 License

This project is proprietary software. Copyright (c) 2017 CSG Systems International, Inc. and/or its affiliates ("CSG"). All Rights Reserved.

## 📞 Support

For technical support and questions:
- **Issues**: Use GitHub Issues for bug reports and feature requests
- **Documentation**: See [docs/](docs/) directory for detailed guides
- **Architecture**: Review [ARCHITECTURE.md](docs/ARCHITECTURE.md) for system design
- **Development**: Follow [LOCAL_DEVELOPMENT.md](docs/LOCAL_DEVELOPMENT.md) for setup

---

**🏆 ACT provides enterprise-grade PCI compliance monitoring with the flexibility of cloud deployment and the reliability of automated testing.**
