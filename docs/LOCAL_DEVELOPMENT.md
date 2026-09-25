# ACT - Local Development Setup

## Overview

This guide provides comprehensive instructions for setting up the ACT (Asset Compliance Tracker) development environment locally. The system consists of multiple .NET 8.0 applications, a MySQL database, and Chef/InSpec components.

## Prerequisites

### Required Software

1. **.NET 8.0 SDK**
   ```bash
   # Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   # Verify installation:
   dotnet --version  # Should show 8.0.x
   ```

2. **MySQL Server 8.0+**
   ```bash
   # Option 1: MySQL Community Server
   # Download from: https://dev.mysql.com/downloads/mysql/
   
   # Option 2: Docker (Recommended for development)
   docker run --name act-mysql -e MYSQL_ROOT_PASSWORD=1234 -p 3306:3306 -d mysql:8.0
   ```

3. **Visual Studio 2022** or **Visual Studio Code**
   - Visual Studio 2022 Community/Professional/Enterprise
   - VS Code with C# extension pack

4. **Git**
   ```bash
   git --version  # Verify installation
   ```

5. **Docker** (Optional, for containerized development)
   ```bash
   docker --version
   docker-compose --version
   ```

### Optional Tools

1. **AWS CLI** (for AWS integration testing)
   ```bash
   # Install AWS CLI v2
   # Configure with development credentials
   aws configure
   ```

2. **Chef Development Kit** (for Chef cookbook development)
   ```bash
   # Download from: https://downloads.chef.io/tools/chefdk
   chef --version
   ```

3. **InSpec** (for compliance testing)
   ```bash
   # Included with Chef DK or install separately
   inspec version
   ```

## Database Setup

### Local MySQL Setup

1. **Create Database**
   ```sql
   CREATE DATABASE ACT;
   CREATE USER 'actuser'@'localhost' IDENTIFIED BY 'actpassword';
   GRANT ALL PRIVILEGES ON ACT.* TO 'actuser'@'localhost';
   FLUSH PRIVILEGES;
   ```

2. **Connection String Configuration**
   - Web App: `Server=localhost;Database=ACT;User=actuser;Password=actpassword`
   - Lambda: `Server=localhost;Database=act;User=actuser;Password=actpassword`

### Docker MySQL Setup (Alternative)

1. **Create docker-compose.yml**
   ```yaml
   version: '3.8'
   services:
     mysql:
       image: mysql:8.0
       container_name: act-mysql
       environment:
         MYSQL_ROOT_PASSWORD: 1234
         MYSQL_DATABASE: ACT
         MYSQL_USER: actuser
         MYSQL_PASSWORD: actpassword
       ports:
         - "3306:3306"
       volumes:
         - mysql_data:/var/lib/mysql
   
   volumes:
     mysql_data:
   ```

2. **Start Database**
   ```bash
   docker-compose up -d mysql
   ```

## Environment Configuration

### Web Application (act.core.web)

1. **Create appsettings.Development.json**
   ```json
   {
     "Logging": {
       "Region": "us-east-2",
       "LogGroup": "/act/Dev-Log",
       "IncludeLogLevel": true,
       "IncludeCategory": true,
       "IncludeNewline": true,
       "IncludeException": true,
       "IncludeEventId": false,
       "IncludeScopes": true,
       "LogLevel": {
         "Default": "Debug",
         "act.core.web": "Debug",
         "act.core.data": "Debug",
         "AWS": "Debug",
         "System": "Information",
         "Microsoft": "Information"
       }
     },
     "Kestrel": {
       "Certificates": {
         "Default": {
           "Path": "localhost.pfx",
           "Password": "abcact"
         }
       }
     },
     "ADFS": {
       "MetadataAddress": "https://dev-sso.mycompany.com/FederationMetadata/2007-06/FederationMetadata.xml",
       "Wtrealm": "https://localhost:44363"
     },
     "Mail": {
       "Host": "localhost",
       "Port": 25,
       "From": "act-dev@localhost"
     },
     "ConnectionStrings": {
       "ActDB": "Server=localhost;Database=ACT;User=actuser;Password=actpassword"
     },
     "HelpLinks": [],
     "InventorySystemLinkFormat": "https://dev-inventory.myorg.org/{0}",
     "AWS": {
       "Region": "us-east-2",
       "Profile": "development"
     }
   }
   ```

2. **SSL Certificate Setup**
   ```bash
   # Generate development certificate
   dotnet dev-certs https --trust
   
   # Or use the provided localhost.pfx with password "abcact"
   ```

### Lambda Application (act.core.etl.lambda)

1. **Create appsettings.Development.json**
   ```json
   {
     "Logging": {
       "IncludeScopes": false,
       "LogLevel": {
         "Default": "Debug",
         "Microsoft": "Information",
         "System": "Information"
       }
     },
     "Mail": {
       "Host": "localhost",
       "Port": 25,
       "From": "act-lambda-dev@localhost"
     },
     "ConnectionStrings": {
       "ActDB": "Server=localhost;Database=act;User=actuser;Password=actpassword"
     },
     "AddIns": [],
     "AWS": {
       "Region": "us-east-2",
       "Profile": "development"
     }
   }
   ```

## Environment Variables

### Required Environment Variables

```bash
# Database Configuration
export ACT_DB_CONNECTION="Server=localhost;Database=ACT;User=actuser;Password=actpassword"

# AWS Configuration (if using AWS services)
export AWS_REGION="us-east-2"
export AWS_PROFILE="development"

# ADFS Configuration
export ACT_ADFS_METADATA_URL="https://dev-sso.mycompany.com/FederationMetadata/2007-06/FederationMetadata.xml"
export ACT_ADFS_REALM="https://localhost:44363"

# Mail Configuration
export ACT_MAIL_HOST="localhost"
export ACT_MAIL_PORT="25"
export ACT_MAIL_FROM="act-dev@localhost"
```

### PowerShell (Windows)
```powershell
$env:ACT_DB_CONNECTION="Server=localhost;Database=ACT;User=actuser;Password=actpassword"
$env:AWS_REGION="us-east-2"
$env:AWS_PROFILE="development"
```

### Environment File (.env)
Create a `.env` file in the project root:
```env
ACT_DB_CONNECTION=Server=localhost;Database=ACT;User=actuser;Password=actpassword
AWS_REGION=us-east-2
AWS_PROFILE=development
ACT_ADFS_METADATA_URL=https://dev-sso.mycompany.com/FederationMetadata/2007-06/FederationMetadata.xml
ACT_ADFS_REALM=https://localhost:44363
ACT_MAIL_HOST=localhost
ACT_MAIL_PORT=25
ACT_MAIL_FROM=act-dev@localhost
```

## AWS Credentials Setup

### Local AWS Configuration

1. **Install AWS CLI**
   ```bash
   pip install awscli
   # or download from https://aws.amazon.com/cli/
   ```

2. **Configure AWS Credentials**
   ```bash
   aws configure --profile development
   # Enter your development AWS credentials:
   # AWS Access Key ID: YOUR_ACCESS_KEY
   # AWS Secret Access Key: YOUR_SECRET_KEY
   # Default region name: us-east-2
   # Default output format: json
   ```

3. **Alternative: Environment Variables**
   ```bash
   export AWS_ACCESS_KEY_ID="your-access-key"
   export AWS_SECRET_ACCESS_KEY="your-secret-key"
   export AWS_DEFAULT_REGION="us-east-2"
   ```

### AWS Services for Development

1. **S3 Bucket** (for file storage)
   ```bash
   aws s3 mb s3://act-dev-artifacts --region us-east-2
   ```

2. **Systems Manager Parameters** (for configuration)
   ```bash
   aws ssm put-parameter --name "/act/dev/database/connection" \
     --value "Server=localhost;Database=ACT;User=actuser;Password=actpassword" \
     --type "SecureString" --region us-east-2
   ```

## Building and Running

### 1. Clone Repository
```bash
git clone https://github.com/your-org/assetcompliancetracker.git
cd assetcompliancetracker
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build Solution
```bash
dotnet build -c Debug
```

### 4. Database Migration
```bash
# Run from the web project directory
cd src/act.core.web
dotnet ef database update

# Or run the lambda migration function
cd ../act.core.etl.lambda
dotnet run -- '{"name":"databaseupdate","index":0}'
```

### 5. Run Web Application
```bash
cd src/act.core.web
dotnet run

# Application will be available at:
# https://localhost:44363 (HTTPS)
# http://localhost:5000 (HTTP)
```

### 6. Run Lambda Functions Locally
```bash
cd src/act.core.etl.lambda

# Test database migration
dotnet run -- '{"name":"databaseupdate","index":0}'

# Test gather function
dotnet run -- '{"name":"gather","index":1}'

# Test email function
dotnet run -- '{"name":"email","index":0}'
```

## Development Workflow

### 1. Database Changes
```bash
# Add new migration
cd src/act.core.data
dotnet ef migrations add YourMigrationName

# Update database
cd ../act.core.web
dotnet ef database update
```

### 2. Testing Lambda Functions
```bash
cd src/act.core.etl.lambda

# Test all functions
dotnet test

# Run specific function
dotnet run -- '{"name":"functionname","index":0}'
```

### 3. Chef Cookbook Development
```bash
cd Chef/cookbooks/act

# Test cookbook syntax
cookstyle .

# Run unit tests
chef exec rspec

# Test with Test Kitchen (requires Vagrant/Docker)
kitchen test
```

### 4. InSpec Profile Development
```bash
cd Compliance/csg_linux_compliant_server

# Validate profile
inspec check .

# Run against local system
inspec exec . --reporter cli json:results.json

# Run with attributes
inspec exec . --input-file attributes.yml
```

## Debugging

### Visual Studio 2022
1. Open `ACT.sln`
2. Set `act.core.web` as startup project
3. Press F5 to start debugging
4. Set breakpoints as needed

### Visual Studio Code
1. Open project folder
2. Install C# extension
3. Use `.vscode/launch.json` configuration:
   ```json
   {
     "version": "0.2.0",
     "configurations": [
       {
         "name": "Launch Web App",
         "type": "coreclr",
         "request": "launch",
         "preLaunchTask": "build",
         "program": "${workspaceFolder}/src/act.core.web/bin/Debug/net8.0/act.core.web.dll",
         "args": [],
         "cwd": "${workspaceFolder}/src/act.core.web",
         "stopAtEntry": false,
         "serverReadyAction": {
           "action": "openExternally",
           "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
         },
         "env": {
           "ASPNETCORE_ENVIRONMENT": "Development"
         }
       }
     ]
   }
   ```

## Common Issues and Solutions

### 1. Database Connection Issues
```bash
# Check MySQL is running
sudo systemctl status mysql  # Linux
net start mysql               # Windows

# Test connection
mysql -u actuser -p -h localhost ACT
```

### 2. SSL Certificate Issues
```bash
# Trust development certificates
dotnet dev-certs https --trust

# Clear and regenerate
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### 3. AWS Credentials Issues
```bash
# Verify AWS configuration
aws sts get-caller-identity --profile development

# Check credentials file
cat ~/.aws/credentials
```

### 4. Port Conflicts
```bash
# Check what's using port 44363
netstat -tulpn | grep 44363  # Linux
netstat -ano | findstr 44363 # Windows

# Change port in launchSettings.json if needed
```

## Testing

### Unit Tests
```bash
# Run all tests
dotnet test

# Run specific project tests
dotnet test src/act.core.data.tests/
```

### Integration Tests
```bash
# Requires test database
export ConnectionStrings__ActDB="Server=localhost;Database=ACT_Test;User=actuser;Password=actpassword"
dotnet test --filter Category=Integration
```

### Chef Cookbook Tests
```bash
cd Chef/cookbooks/act
chef exec rspec spec/
```

### InSpec Profile Tests
```bash
cd Compliance/csg_linux_compliant_server
inspec exec . --reporter cli
```

## Performance Optimization

### Development Database Optimization
```sql
-- Enable query logging for debugging
SET GLOBAL general_log = 'ON';
SET GLOBAL general_log_file = '/var/log/mysql/general.log';

-- Optimize for development
SET GLOBAL innodb_flush_log_at_trx_commit = 2;
SET GLOBAL sync_binlog = 0;
```

### Application Performance
```bash
# Enable detailed logging
export ASPNETCORE_ENVIRONMENT=Development
export Logging__LogLevel__Default=Debug

# Profile with dotnet-trace
dotnet tool install --global dotnet-trace
dotnet-trace collect --process-id <pid>
```

## Security Considerations

### Development Security
- Never commit real credentials to version control
- Use development/test ADFS endpoints
- Use local or development AWS accounts
- Rotate development credentials regularly

### Local Network Security
- Use HTTPS even in development
- Configure firewall rules appropriately
- Use strong passwords for local databases
- Keep development tools updated