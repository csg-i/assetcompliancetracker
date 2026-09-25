# Asset Compliance Tracker (ACT) - Architecture Documentation

## Overview

Asset Compliance Tracker (ACT) is a hybrid on-premises/cloud PCI compliance monitoring solution designed to monitor virtual machines and physical servers. The system provides automated compliance checking, reporting, and management capabilities for organizations maintaining PCI DSS compliance.

## System Architecture

ACT consists of four main .NET Core 8.0 components working together to provide comprehensive compliance monitoring:

### Core Components

1. **act.core.data** - Entity Framework data layer with MySQL database support
2. **act.core.etl** - Extract, Transform, Load operations for compliance data
3. **act.core.web** - ASP.NET Core MVC web application for user interface
4. **act.core.etl.lambda** - AWS Lambda functions for automated data processing

## Code Structure

```mermaid
graph TB
    subgraph "Repository Structure"
        subgraph "Source Code (src/)"
            DATA[act.core.data<br/>📊 Entity Framework<br/>Data Layer]
            ETL[act.core.etl<br/>🔄 Extract Transform Load<br/>Business Logic]
            WEB[act.core.web<br/>🌐 ASP.NET Core MVC<br/>Web Application]
            LAMBDA[act.core.etl.lambda<br/>⚡ AWS Lambda<br/>Functions]
        end
        
        subgraph "Chef Components"
            COOKBOOK[Chef/cookbooks/act/<br/>📋 Chef Cookbook<br/>Automation]
            LINUX_SPEC[Compliance/<br/>csg_linux_compliant_server/<br/>🐧 Linux InSpec Profile]
            WIN_SPEC[Compliance/<br/>csg_windows_compliant_server/<br/>🪟 Windows InSpec Profile]
        end
        
        subgraph "Deployment"
            DOCKER[Docker/<br/>🐳 Dockerfile<br/>Container Config]
            BUILD[BuildSpec.yml<br/>🏗️ CodeBuild<br/>CI/CD Pipeline]
        end
        
        subgraph "Configuration"
            SOLUTION[ACT.sln<br/>📁 Visual Studio<br/>Solution File]
            DOCS[Documentation/<br/>📚 Architecture &<br/>Setup Guides]
        end
    end
    
    subgraph "Dependencies & Relationships"
        WEB --> DATA
        WEB --> ETL
        LAMBDA --> DATA
        LAMBDA --> ETL
        ETL --> DATA
        
        COOKBOOK --> LINUX_SPEC
        COOKBOOK --> WIN_SPEC
        
        BUILD --> WEB
        BUILD --> LAMBDA
        BUILD --> DOCKER
        
        SOLUTION --> DATA
        SOLUTION --> ETL
        SOLUTION --> WEB
        SOLUTION --> LAMBDA
    end
    
    subgraph "External Integrations"
        CHEF_AUTO[Chef Automate<br/>Servers]
        MYSQL_DB[(MySQL Database<br/>Aurora Serverless)]
        AWS_SERVICES[AWS Services<br/>Lambda, Beanstalk, CloudWatch]
    end
    
    ETL --> CHEF_AUTO
    DATA --> MYSQL_DB
    WEB --> MYSQL_DB
    LAMBDA --> MYSQL_DB
    LAMBDA --> AWS_SERVICES
    WEB --> AWS_SERVICES
    
    classDef coreProject fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef chefComponent fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef deployment fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    classDef config fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef external fill:#fce4ec,stroke:#880e4f,stroke-width:2px
    
    class DATA,ETL,WEB,LAMBDA coreProject
    class COOKBOOK,LINUX_SPEC,WIN_SPEC chefComponent
    class DOCKER,BUILD deployment
    class SOLUTION,DOCS config
    class CHEF_AUTO,MYSQL_DB,AWS_SERVICES external
```

### Project Dependencies

```mermaid
graph LR
    subgraph ".NET Core Projects"
        DATA[act.core.data<br/>Entity Framework<br/>MySQL Provider]
        ETL[act.core.etl<br/>Business Logic<br/>Chef Integration]
        WEB[act.core.web<br/>MVC Web App<br/>ADFS Auth]
        LAMBDA[act.core.etl.lambda<br/>AWS Lambda<br/>Scheduled Tasks]
    end
    
    subgraph "Key Dependencies"
        EF[Entity Framework Core 8.0]
        MYSQL[Pomelo MySQL Provider]
        AWS_SDK[AWS SDK .NET]
        ASPNET[ASP.NET Core 8.0]
        ADFS_AUTH[WS-Federation Auth]
        NEWTONSOFT[Newtonsoft.Json]
    end
    
    DATA --> EF
    DATA --> MYSQL
    
    ETL --> DATA
    
    WEB --> DATA
    WEB --> ETL
    WEB --> ASPNET
    WEB --> ADFS_AUTH
    WEB --> AWS_SDK
    
    LAMBDA --> DATA
    LAMBDA --> ETL
    LAMBDA --> AWS_SDK
    LAMBDA --> NEWTONSOFT
    
    classDef project fill:#bbdefb,stroke:#1976d2,stroke-width:2px
    classDef dependency fill:#c8e6c9,stroke:#388e3c,stroke-width:2px
    
    class DATA,ETL,WEB,LAMBDA project
    class EF,MYSQL,AWS_SDK,ASPNET,ADFS_AUTH,NEWTONSOFT dependency
```

### Application Layer Architecture

```mermaid
graph TB
    subgraph "Presentation Layer"
        UI[Web UI Controllers<br/>Views & Models]
        API[REST API Endpoints<br/>JSON Responses]
        AUTH[ADFS Authentication<br/>Claims & Authorization]
    end
    
    subgraph "Business Logic Layer"
        BL[Business Logic<br/>act.core.etl]
        CHEF_INT[Chef Integration<br/>API Clients]
        EMAIL[Email Services<br/>Notification Logic]
        SCHED[Scheduled Tasks<br/>Lambda Functions]
    end
    
    subgraph "Data Access Layer"
        EF_CTX[Entity Framework<br/>DbContext]
        REPOS[Repository Pattern<br/>Data Models]
        MIGRATIONS[Database Migrations<br/>Schema Management]
    end
    
    subgraph "Infrastructure Layer"
        DB[(MySQL Database<br/>Aurora Serverless)]
        CHEF_SRV[Chef Automate<br/>External APIs]
        AWS_SVC[AWS Services<br/>CloudWatch, S3]
        MAIL_SRV[Mail Server<br/>SMTP]
    end
    
    subgraph "Cross-Cutting Concerns"
        LOG[Logging<br/>CloudWatch Integration]
        CONFIG[Configuration<br/>appsettings.json]
        CACHE[Caching<br/>Memory & Distributed]
        SECURITY[Security<br/>SSL, Encryption]
    end
    
    UI --> BL
    API --> BL
    AUTH --> UI
    AUTH --> API
    
    BL --> EF_CTX
    CHEF_INT --> BL
    EMAIL --> BL
    SCHED --> BL
    
    EF_CTX --> REPOS
    REPOS --> MIGRATIONS
    
    REPOS --> DB
    CHEF_INT --> CHEF_SRV
    EMAIL --> MAIL_SRV
    SCHED --> AWS_SVC
    
    LOG -.-> UI
    LOG -.-> BL
    LOG -.-> EF_CTX
    CONFIG -.-> BL
    CONFIG -.-> EF_CTX
    CACHE -.-> BL
    SECURITY -.-> UI
    SECURITY -.-> API
    
    classDef presentation fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    classDef business fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    classDef data fill:#e8f5e8,stroke:#388e3c,stroke-width:2px
    classDef infrastructure fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    classDef crosscutting fill:#fce4ec,stroke:#c2185b,stroke-width:2px
    
    class UI,API,AUTH presentation
    class BL,CHEF_INT,EMAIL,SCHED business
    class EF_CTX,REPOS,MIGRATIONS data
    class DB,CHEF_SRV,AWS_SVC,MAIL_SRV infrastructure
    class LOG,CONFIG,CACHE,SECURITY crosscutting
```

### Technology Stack

- **.NET Core 8.0** - Primary application framework
- **Entity Framework Core** - ORM with MySQL support via Pomelo provider
- **MySQL** - Primary database (AWS Aurora Serverless in production)
- **Chef InSpec** - Compliance testing framework
- **Docker** - Containerization for deployment
- **AWS Services** - Lambda, Elastic Beanstalk, Aurora Serverless, CloudWatch

## AWS Deployment Architecture

```mermaid
graph TB
    subgraph "On-Premises Infrastructure"
        CS[Chef Automate Servers]
        WS[Windows Servers]
        LS[Linux Servers]
    end
    
    subgraph "AWS Cloud Infrastructure"
        subgraph "Compute Services"
            EB[Elastic Beanstalk<br/>Web Application]
            LF[Lambda Functions<br/>Data Processing]
        end
        
        subgraph "Data Services"
            AS[Aurora Serverless<br/>MySQL Database]
            CW[CloudWatch<br/>Logging & Monitoring]
        end
        
        subgraph "CI/CD Pipeline"
            CB[CodeBuild<br/>Build & Deploy]
            S3[S3 Buckets<br/>Artifacts]
        end
        
        subgraph "Networking & Security"
            ALB[Application Load Balancer]
            ADFS[ADFS Integration<br/>Authentication]
        end
    end
    
    CS --> LF
    WS --> CS
    LS --> CS
    
    LF --> AS
    EB --> AS
    EB --> CW
    LF --> CW
    
    CB --> EB
    CB --> LF
    CB --> S3
    
    ALB --> EB
    ADFS --> EB
    
    CW --> LF
```

## Data Flow Architecture

```mermaid
sequenceDiagram
    participant Servers as On-Prem Servers
    participant Chef as Chef Automate
    participant Lambda as AWS Lambda
    participant DB as Aurora Database
    participant Web as Web Application
    participant Users as End Users
    
    Servers->>Chef: InSpec Compliance Reports
    Chef->>Chef: Store Compliance Data
    
    Note over Lambda: Scheduled CloudWatch Events
    Lambda->>Chef: Gather Compliance Data (Hourly)
    Chef->>Lambda: Return Compliance Results
    Lambda->>DB: Store/Update Compliance Data
    
    Lambda->>Lambda: Process Email Notifications (Daily)
    Lambda->>Users: Send Compliance Alerts
    
    Lambda->>DB: Cleanup Old Data (Daily)
    
    Users->>Web: Access Dashboard
    Web->>DB: Query Compliance Data
    DB->>Web: Return Results
    Web->>Users: Display Compliance Status
    
    Users->>Web: Manage Build Specifications
    Web->>DB: Update Configurations
```

## AWS Services Configuration

### Lambda Functions

The system uses a single Lambda function with multiple execution modes triggered by CloudWatch Events:

| Function | Schedule | Purpose |
|----------|----------|---------|
| `databaseupdate` | On-demand | Apply Entity Framework migrations |
| `gather` | Hourly | Collect data from Chef Automate servers |
| `email` | Daily | Send compliance notification emails |
| `reset` | Hourly | Reset status for non-reporting nodes (48+ hours) |
| `purgedetails` | Daily | Clean up old compliance details |
| `purgeruns` | Daily | Remove compliance runs older than 28 days |
| `purgeinactive` | Configurable | Remove deactivated nodes (7+ days old) |

### Elastic Beanstalk Configuration

- **Platform**: Docker running .NET Core 8.0
- **Load Balancer**: Application Load Balancer with HTTPS
- **Auto Scaling**: Configurable based on demand
- **Health Monitoring**: Integrated with CloudWatch
- **Deployment**: Blue/green deployments via CodeBuild

### Aurora Serverless Database

- **Engine**: MySQL 8.0 compatible
- **Scaling**: Automatic based on demand
- **Backup**: Automated daily backups
- **Security**: VPC isolation, encryption at rest
- **Monitoring**: CloudWatch metrics and logs

### CloudWatch Integration

- **Application Logs**: Structured logging from web application and Lambda
- **Metrics**: Custom metrics for compliance status and system health
- **Alarms**: Automated alerting for system issues
- **Events**: Scheduled triggers for Lambda functions

## CI/CD Pipeline

```mermaid
graph LR
    subgraph "Source Control"
        GH[GitHub Repository]
    end
    
    subgraph "Build Process"
        CB[CodeBuild]
        BS[BuildSpec.yml]
    end
    
    subgraph "Artifacts"
        S3A[S3 Artifacts]
        LZ[Lambda.zip]
        PZ[BeanstalkProd.zip]
        QZ[BeanstalkQA.zip]
    end
    
    subgraph "Deployment Targets"
        LP[Lambda Production]
        EBP[Elastic Beanstalk Prod]
        EBQ[Elastic Beanstalk QA]
    end
    
    GH --> CB
    BS --> CB
    CB --> S3A
    
    S3A --> LZ
    S3A --> PZ
    S3A --> QZ
    
    LZ --> LP
    PZ --> EBP
    QZ --> EBQ
```

### Build Process

The CodeBuild process follows these steps:

1. **Pre-build**: Create staging directories and copy configuration files
2. **Build**: Restore dependencies, compile, and publish .NET applications
3. **Post-build**: Package applications into deployment-ready ZIP files
4. **Artifacts**: Upload Lambda and Elastic Beanstalk packages to S3

## Database Schema

The system uses Entity Framework Core with a MySQL database containing the following main entities:

### Core Entities

- **Nodes**: Represents servers/VMs being monitored
- **BuildSpecifications**: Defines expected configurations for different server types
- **ComplianceResults**: Stores compliance test results and status
- **Environments**: Chef Automate server configurations
- **Employees**: User management and ownership tracking

### Compliance Entities

- **ComplianceResultTests**: Individual test results
- **ComplianceResultErrors**: Error details for failed tests
- **SoftwareComponents**: Expected software installations
- **Ports**: Expected open network ports
- **Justifications**: Explanations for compliance exceptions

### Reference Data

- **Products**: Product codes for server categorization
- **Functions**: Server functional roles
- **PCI Scope Classifications**: A (most secure), B (more secure), C (least secure)
- **Platform Types**: Linux, Windows Server, Unix, Windows Client, Appliance, Other

## Chef InSpec Integration

### Compliance Profiles

The system includes two main InSpec compliance profiles:

1. **csg_linux_compliant_server** (v2.2.0)
   - Supports: CentOS, RHEL, RedHat
   - Tests: Package installations, open ports, system configurations

2. **csg_windows_compliant_server** (v2.2.0)
   - Supports: Windows Server and Client systems
   - Tests: Windows features, installed software, network ports

### Chef Cookbook

The ACT cookbook (`Chef/cookbooks/act/`) provides:

- REST API integration with ACT web application
- Attribute management for InSpec tests
- Automated compliance test execution
- Result reporting back to Chef Automate

## Security Architecture

### Authentication & Authorization

- **ADFS Integration**: Federated authentication via WS-Federation
- **Claims-based Security**: User identity and role management
- **SSL/TLS**: HTTPS enforcement for all web traffic

### Data Protection

- **Database Encryption**: Aurora Serverless encryption at rest
- **Transit Encryption**: TLS for all API communications
- **VPC Isolation**: Network-level security in AWS
- **IAM Roles**: Least-privilege access for AWS services

### Compliance Features

- **PCI Scope Management**: Automatic classification of servers
- **Audit Trails**: Complete history of compliance status changes
- **Exception Management**: Justification tracking for non-compliant items
- **Automated Reporting**: Regular compliance status notifications

## Monitoring & Observability

### Application Monitoring

- **CloudWatch Logs**: Centralized logging for all components
- **Custom Metrics**: Compliance status, node counts, test results
- **Health Checks**: Automated system health monitoring
- **Performance Metrics**: Response times, error rates, throughput

### Alerting

- **Compliance Alerts**: Notifications for failing nodes
- **System Alerts**: Infrastructure and application issues
- **Email Notifications**: Automated reports to stakeholders
- **Dashboard Views**: Real-time compliance status visualization

## Scalability & Performance

### Horizontal Scaling

- **Elastic Beanstalk**: Auto-scaling web application instances
- **Aurora Serverless**: Automatic database scaling
- **Lambda**: Serverless scaling for background processing

### Performance Optimization

- **Database Indexing**: Optimized queries for large datasets
- **Caching**: Application-level caching for frequently accessed data
- **Asynchronous Processing**: Background jobs for data gathering and cleanup

## Disaster Recovery

### Backup Strategy

- **Database Backups**: Automated Aurora Serverless backups
- **Code Repository**: Git-based source control with GitHub
- **Configuration Management**: Infrastructure as Code practices

### High Availability

- **Multi-AZ Deployment**: Database and application redundancy
- **Load Balancing**: Traffic distribution across multiple instances
- **Health Monitoring**: Automatic failover for unhealthy instances
