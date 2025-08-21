# ACT (Asset Compliance Tracker) - Repository Summary

## Project Overview

**ACT (Asset Compliance Tracker)** is a hybrid on-premise/cloud PCI compliance monitoring solution designed to track and manage compliance status of virtual machines and physical servers across enterprise environments. The system provides centralized compliance tracking, automated reporting, and audit-ready documentation for PCI DSS compliance requirements.

## Technology Stack

### Backend Technologies
- **.NET 8** - All components (Lambda functions, MVC web application, and Entity Framework)
- **Entity Framework Core** - Code-first database with migrations
- **MySQL** - Database engine (Aurora MySQL)

### AWS Services
- **AWS Lambda** - Serverless ETL functions
- **Elastic Beanstalk** - Web application hosting
- **Aurora MySQL** - Managed database
- **S3** - Data protection key storage
- **CloudWatch** - Logging and scheduled triggers
- **CodeBuild** - CI/CD pipeline

### Infrastructure & DevOps
- **Chef Automate** - Compliance data collection
- **Chef InSpec** - Compliance testing framework
- **Docker** - Containerization
- **ADFS** - Authentication and authorization

## System Architecture

### Core Components

#### 1. Web Application (`act.core.web`)
- **Framework**: .NET 8 MVC
- **Purpose**: Central dashboard and management interface
- **Key Features**:
  - PCI compliance dashboard with visual reporting
  - Build specification wizard (OS Specs and App Specs)
  - Node management and assignment
  - Score cards for different organizational levels
  - ADFS integration for single sign-on
  - Responsive UI based on jayMVC framework

#### 2. Lambda ETL Functions (`act.core.etl.lambda`)
- **Framework**: .NET 8
- **Purpose**: Automated data processing and maintenance
- **Functions**:
  - `gather` - Collects compliance data from Chef Automate servers
  - `databaseupdate` - Applies Entity Framework migrations
  - `email` - Sends notifications for unassigned/non-reporting nodes
  - `reset` - Resets compliance status for inactive nodes
  - `purgedetails` - Cleans up compliance details
  - `purgeruns` - Removes old compliance runs (28+ days)
  - `purgeinactive` - Removes deactivated nodes (7+ days)

#### 3. Data Layer (`act.core.data`)
- **Framework**: Entity Framework Core
- **Database**: Aurora MySQL
- **Key Entities**:
  - `Node` - Servers and systems being monitored
  - `BuildSpecification` - Compliance specifications (OS and App specs)
  - `ComplianceResult` - Compliance test results and status
  - `Environment` - Logical groupings of systems
  - `Employee` - User management and ownership

#### 4. Chef Integration
- **Chef Cookbook**: Wrapper around audit cookbook
- **Chef InSpec**: Compliance tests for Linux and Windows
- **Integration**: REST API calls between Lambda and Chef Automate

## Key Features

### Compliance Management
- **Build Specifications**: Template-based compliance definitions
  - **OS Specs**: Platform-specific base configurations
  - **App Specs**: Application-specific extensions
- **Node Classification**: PCI scope classification (A, B, C levels)
- **Platform Support**: Linux, Windows Server, Unix, Appliances
- **Automated Testing**: Chef InSpec compliance validation

### Reporting & Dashboards
- **Executive Dashboard**: High-level compliance overview
- **Score Cards**: Multi-level reporting (Executive, Director, Owner, Product, Platform)
- **Audit Reports**: PCI assessor-ready documentation
- **Real-time Status**: Current compliance state tracking

### Automation & Scheduling
- **Hourly Tasks**: Data gathering and compliance reset
- **Daily Tasks**: Email notifications and data purging
- **On-Demand**: Database migrations and manual operations

## Deployment Architecture

### Development Workflow
1. **Source Control**: Git-based version control
2. **Build Pipeline**: AWS CodeBuild with `BuildSpec.yml`
3. **Artifacts**: Separate packages for Lambda, QA, and Production
4. **Deployment**: Automated deployment to Elastic Beanstalk and Lambda

### Environment Structure
- **QA Environment**: Testing and validation
- **Production Environment**: Live system
- **Shared Services**: Aurora MySQL database and S3 storage

### Security Implementation
- **Authentication**: ADFS/WS-Federation integration
- **Data Protection**: ASP.NET Core data protection with S3 key storage
- **Network Security**: VPC isolation and HTTPS enforcement
- **Database Security**: Aurora encryption at rest and in transit

## Data Flow

### Compliance Data Collection
1. **On-Premise Servers** run Chef InSpec compliance tests
2. **Chef Automate** servers collect and store results
3. **Lambda functions** periodically gather data via REST APIs
4. **Aurora MySQL Database** stores processed compliance information
5. **Web Application** presents dashboards and reports

### User Interactions
1. **Users access** web application via ADFS authentication
2. **Dashboard displays** real-time compliance status
3. **Build specifications** are created and managed
4. **Nodes are assigned** to appropriate specifications
5. **Reports are generated** for audit purposes

## Extensibility

### Lambda Add-ins
- **Plugin Architecture**: Support for custom ETL functions
- **Service Registration**: Dependency injection integration
- **Configuration-Based**: JSON configuration for new functions

### Build Specification Types
- **Inheritance Model**: App Specs inherit from OS Specs
- **Platform-Specific**: Different workflows for different platforms
- **Component-Based**: Modular specification building

## Compliance & Audit Features

### PCI DSS Support
- **Scope Classification**: Automated PCI scope determination
- **Evidence Collection**: Comprehensive audit trail
- **Report Generation**: Assessor-ready documentation
- **Exception Management**: Justification and approval workflow

### Monitoring & Alerting
- **Automated Notifications**: Email alerts for compliance issues
- **Status Tracking**: Real-time compliance state monitoring
- **Trend Analysis**: Historical compliance data analysis

## Technical Highlights

### Performance Optimizations
- **Aurora MySQL**: Managed database with high availability and performance
- **Connection Pooling**: Efficient database connection management
- **Caching**: Memory caching for frequently accessed data
- **Optimized Queries**: Entity Framework optimization

### Scalability Features
- **Horizontal Scaling**: Elastic Beanstalk auto-scaling
- **Serverless Architecture**: Lambda functions scale automatically
- **Database Scaling**: Aurora MySQL can scale with read replicas and instance sizing
- **Multi-Environment**: Support for multiple Chef Automate servers

### Maintainability
- **Clean Architecture**: Separation of concerns across layers
- **Dependency Injection**: Loose coupling and testability
- **Entity Framework**: Code-first database migrations
- **Configuration Management**: Environment-specific settings

## Repository Structure

```
ACT/
├── src/                          # Source code
│   ├── act.core.web/            # MVC web application
│   ├── act.core.data/           # Entity Framework data layer
│   ├── act.core.etl/            # ETL business logic
│   └── act.core.etl.lambda/     # Lambda function implementation
├── Chef/                        # Chef cookbook and configuration
├── Compliance/                  # InSpec compliance tests
├── Docker/                      # Docker configuration
├── Doc/                         # Documentation and screenshots
├── BuildSpec.yml               # AWS CodeBuild configuration
└── ACT.sln                     # Visual Studio solution file
```

## Business Value

### Compliance Efficiency
- **Automated Monitoring**: Reduces manual compliance checking
- **Centralized Management**: Single source of truth for compliance
- **Audit Readiness**: Always-current documentation for assessors
- **Risk Reduction**: Proactive identification of compliance gaps

### Operational Benefits
- **Time Savings**: Automated data collection and reporting
- **Visibility**: Real-time compliance status across organization
- **Standardization**: Consistent compliance specifications
- **Scalability**: Supports growth in server infrastructure

This repository represents a mature, production-ready compliance monitoring system that successfully bridges on-premise infrastructure with cloud-based analytics and reporting capabilities.