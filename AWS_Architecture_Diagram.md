# ACT (Asset Compliance Tracker) - AWS Architecture Diagrams

## High-Level Architecture Overview

```mermaid
graph TB
    subgraph "On-Premise Infrastructure"
        Chef[Chef Automate Servers]
        Servers[Linux/Windows Servers]
        ADFS[ADFS Identity Provider]
    end

    subgraph "AWS Cloud Infrastructure"
        subgraph "Compute Services"
            EB[Elastic Beanstalk<br/>Web Application]
            Lambda[AWS Lambda<br/>ETL Functions]
        end

        subgraph "Data Services"
            Aurora[Aurora Serverless<br/>MySQL Database]
            S3[S3 Bucket<br/>Data Protection Keys]
        end

        subgraph "Monitoring & Scheduling"
            CW[CloudWatch<br/>Logs & Rules]
            EB_Logs[Elastic Beanstalk<br/>Application Logs]
        end

        subgraph "CI/CD"
            CB[CodeBuild<br/>Build Pipeline]
            Artifacts[Build Artifacts<br/>Lambda.zip, BeanstalkProd.zip, BeanstalkQA.zip]
        end
    end

    subgraph "Users"
        Auditors[PCI Auditors]
        Admins[System Administrators]
    end

    %% Data Flow
    Servers -->|Chef Inspec Reports| Chef
    Chef -->|REST API Calls| Lambda
    Lambda -->|Query/Update| Aurora
    Lambda -->|Scheduled Functions| CW
    EB -->|Read/Write| Aurora
    EB -->|Store Keys| S3
    EB -->|Authentication| ADFS
    
    Users -->|HTTPS| EB
    CB -->|Deploy| EB
    CB -->|Deploy| Lambda
    
    %% Styling
    classDef aws fill:#ff9900,stroke:#232f3e,stroke-width:2px,color:#fff
    classDef onprem fill:#4CAF50,stroke:#2E7D32,stroke-width:2px,color:#fff
    classDef users fill:#2196F3,stroke:#1565C0,stroke-width:2px,color:#fff
    
    class EB,Lambda,Aurora,S3,CW,CB,Artifacts,EB_Logs aws
    class Chef,Servers,ADFS onprem
    class Auditors,Admins users
```

## Detailed Component Architecture

```mermaid
graph TB
    subgraph "Web Tier - Elastic Beanstalk"
        subgraph "ACT Web Application"
            MVC[.NET 8 MVC<br/>act.core.web]
            Auth[WS-Federation Auth<br/>ADFS Integration]
            DataProt[Data Protection<br/>Keys in S3]
        end
    end

    subgraph "Application Tier - AWS Lambda"
        subgraph "ETL Lambda Functions"
            LambdaCore[.NET 8<br/>act.core.etl.lambda]
            Functions[
                • databaseupdate<br/>
                • gather (by environment)<br/>
                • email notifications<br/>
                • reset compliance<br/>
                • purge operations
            ]
        end
    end

    subgraph "Data Tier"
        subgraph "Aurora Serverless MySQL"
            DB[(ACT Database)]
            Tables[
                • BuildSpecifications<br/>
                • Nodes<br/>
                • ComplianceResults<br/>
                • Environments<br/>
                • Employees
            ]
        end
        
        subgraph "S3 Storage"
            Keys[ASP.NET Data Protection Keys]
        end
    end

    subgraph "Monitoring & Scheduling"
        subgraph "CloudWatch"
            Rules[CloudWatch Rules<br/>Scheduled Triggers]
            Logs[Application Logs<br/>/act/Dev-Log]
        end
    end

    subgraph "External Systems"
        ChefAuto[Chef Automate Servers<br/>Multiple Environments]
        ADFS_Ext[ADFS Server<br/>sso.mycompany.com]
        SMTP[SMTP Server<br/>Email Notifications]
    end

    %% Connections
    MVC --> DB
    MVC --> Keys
    MVC --> ADFS_Ext
    
    LambdaCore --> DB
    LambdaCore --> ChefAuto
    LambdaCore --> SMTP
    
    Rules -->|Triggers| LambdaCore
    MVC --> Logs
    LambdaCore --> Logs

    %% Styling
    classDef web fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    classDef lambda fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    classDef data fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    classDef monitoring fill:#e8f5e8,stroke:#388e3c,stroke-width:2px
    classDef external fill:#fce4ec,stroke:#c2185b,stroke-width:2px
    
    class MVC,Auth,DataProt web
    class LambdaCore,Functions lambda
    class DB,Tables,Keys data
    class Rules,Logs monitoring
    class ChefAuto,ADFS_Ext,SMTP external
```

## Deployment Pipeline Architecture

```mermaid
graph LR
    subgraph "Source Control"
        Repo[Git Repository<br/>ACT Source Code]
    end

    subgraph "AWS CodeBuild"
        Build[Build Process<br/>BuildSpec.yml]
        Artifacts[Build Artifacts<br/>• Lambda.zip<br/>• BeanstalkProd.zip<br/>• BeanstalkQA.zip]
    end

    subgraph "Deployment Targets"
        subgraph "QA Environment"
            EB_QA[Elastic Beanstalk QA<br/>Web Application]
            Lambda_QA[Lambda QA<br/>ETL Functions]
        end
        
        subgraph "Production Environment"
            EB_Prod[Elastic Beanstalk Prod<br/>Web Application]
            Lambda_Prod[Lambda Prod<br/>ETL Functions]
        end
    end

    subgraph "Shared Services"
        Aurora_Shared[Aurora Serverless<br/>Shared Database]
        S3_Shared[S3 Bucket<br/>Shared Storage]
    end

    %% Flow
    Repo -->|Trigger| Build
    Build -->|Generate| Artifacts
    Artifacts -->|Deploy| EB_QA
    Artifacts -->|Deploy| Lambda_QA
    Artifacts -->|Deploy| EB_Prod
    Artifacts -->|Deploy| Lambda_Prod
    
    EB_QA --> Aurora_Shared
    EB_Prod --> Aurora_Shared
    Lambda_QA --> Aurora_Shared
    Lambda_Prod --> Aurora_Shared
    
    EB_QA --> S3_Shared
    EB_Prod --> S3_Shared

    %% Styling
    classDef source fill:#e8f5e8,stroke:#388e3c,stroke-width:2px
    classDef build fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    classDef qa fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    classDef prod fill:#ffebee,stroke:#d32f2f,stroke-width:2px
    classDef shared fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    
    class Repo source
    class Build,Artifacts build
    class EB_QA,Lambda_QA qa
    class EB_Prod,Lambda_Prod prod
    class Aurora_Shared,S3_Shared shared
```

## Data Flow Architecture

```mermaid
sequenceDiagram
    participant Servers as On-Prem Servers
    participant Chef as Chef Automate
    participant Lambda as AWS Lambda
    participant Aurora as Aurora DB
    participant Web as Web App
    participant Users as End Users
    participant CW as CloudWatch

    Note over Servers,Chef: Compliance Data Collection
    Servers->>Chef: Chef Inspec compliance reports
    Chef->>Chef: Store compliance results
    
    Note over CW,Lambda: Scheduled ETL Process
    CW->>Lambda: Trigger gather function (hourly)
    Lambda->>Chef: REST API call for compliance data
    Chef-->>Lambda: Return compliance results
    Lambda->>Aurora: Store/update compliance data
    
    Note over CW,Lambda: Scheduled Maintenance
    CW->>Lambda: Trigger reset function (hourly)
    Lambda->>Aurora: Reset non-reporting nodes
    
    CW->>Lambda: Trigger email function (daily)
    Lambda->>Aurora: Query unassigned/non-reporting nodes
    Lambda->>Lambda: Send notification emails
    
    CW->>Lambda: Trigger purge functions (daily)
    Lambda->>Aurora: Purge old data
    
    Note over Users,Web: User Interactions
    Users->>Web: Access ACT dashboard
    Web->>Aurora: Query compliance status
    Aurora-->>Web: Return dashboard data
    Web-->>Users: Display compliance dashboard
    
    Users->>Web: Create/edit build specifications
    Web->>Aurora: Store specification data
    
    Users->>Web: Assign nodes to specifications
    Web->>Aurora: Update node assignments
```

## Security Architecture

```mermaid
graph TB
    subgraph "Authentication & Authorization"
        ADFS[ADFS Server<br/>sso.mycompany.com]
        WSFed[WS-Federation<br/>Authentication]
        Cookies[Encrypted Cookies<br/>45min expiry]
    end

    subgraph "Data Protection"
        S3Keys[S3 Stored Keys<br/>ASP.NET Data Protection]
        HTTPS[HTTPS/TLS<br/>All Communications]
        VPC[VPC Network<br/>Isolation]
    end

    subgraph "Application Security"
        AuthFilter[Authorization Filter<br/>Require Auth by Default]
        CSRF[CSRF Protection<br/>Built-in MVC]
        InputVal[Input Validation<br/>Model Binding]
    end

    subgraph "Database Security"
        AuroraEnc[Aurora Encryption<br/>At Rest & In Transit]
        ConnStr[Encrypted Connection<br/>Strings]
        EF[Entity Framework<br/>Parameterized Queries]
    end

    Users[End Users] --> HTTPS
    HTTPS --> WSFed
    WSFed --> ADFS
    ADFS --> Cookies
    Cookies --> AuthFilter
    AuthFilter --> InputVal
    InputVal --> EF
    EF --> AuroraEnc
    
    S3Keys --> AuthFilter
    VPC --> AuroraEnc

    %% Styling
    classDef auth fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    classDef protection fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    classDef app fill:#e8f5e8,stroke:#388e3c,stroke-width:2px
    classDef db fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    
    class ADFS,WSFed,Cookies auth
    class S3Keys,HTTPS,VPC protection
    class AuthFilter,CSRF,InputVal app
    class AuroraEnc,ConnStr,EF db
```

## Cost Optimization & Scaling

### Current Architecture Benefits:
- **Aurora Serverless**: Automatically scales based on demand, pay only for usage
- **Lambda**: Serverless execution, pay per invocation
- **Elastic Beanstalk**: Auto-scaling web tier based on load
- **S3**: Cost-effective storage for data protection keys

### Monitoring Points:
- CloudWatch logs for all components
- Application performance monitoring
- Database connection pooling
- Lambda execution metrics

### Scaling Considerations:
- Lambda functions can handle multiple Chef Automate servers
- Web application can scale horizontally via Elastic Beanstalk
- Aurora Serverless automatically handles database scaling
- S3 provides unlimited storage capacity