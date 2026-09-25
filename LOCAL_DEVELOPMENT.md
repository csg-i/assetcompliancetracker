# Local Development Setup Guide

This guide provides comprehensive instructions for setting up the Asset Compliance Tracker (ACT) development environment on your local machine.

## Prerequisites

### Required Software

1. **.NET Core 8.0 SDK**
   ```bash
   # Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   # Verify installation:
   dotnet --version
   ```

2. **MySQL Server 8.0+**
   ```bash
   # Ubuntu/Debian:
   sudo apt update
   sudo apt install mysql-server
   
   # macOS (using Homebrew):
   brew install mysql
   
   # Windows: Download from https://dev.mysql.com/downloads/mysql/
   ```

3. **Chef Workstation**
   ```bash
   # Download from: https://downloads.chef.io/tools/workstation
   # Verify installation:
   chef --version
   inspec --version
   ```

4. **Git**
   ```bash
   # Most systems have git pre-installed
   git --version
   ```

5. **IDE/Editor** (recommended)
   - Visual Studio 2022 (Windows/Mac)
   - Visual Studio Code (cross-platform)
   - JetBrains Rider (cross-platform)

### Optional Tools

- **Docker Desktop** (for containerized development)
- **MySQL Workbench** (database management GUI)
- **Postman** (API testing)

## Environment Setup

### 1. Clone the Repository

```bash
git clone https://github.com/csg-i/assetcompliancetracker.git
cd assetcompliancetracker
```

### 2. Database Setup

#### Create Local MySQL Database

```sql
-- Connect to MySQL as root
mysql -u root -p

-- Create database and user
CREATE DATABASE ACT;
CREATE USER 'act_user'@'localhost' IDENTIFIED BY 'your_secure_password';
GRANT ALL PRIVILEGES ON ACT.* TO 'act_user'@'localhost';
FLUSH PRIVILEGES;
EXIT;
```

#### Apply Entity Framework Migrations

```bash
# Navigate to the data project
cd src/act.core.data

# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Apply migrations to create database schema
dotnet ef database update --connection "Server=localhost;Database=ACT;User=act_user;Password=your_secure_password"
```

### 3. SSL Certificate Setup

The application requires HTTPS for local development. A self-signed certificate is included in the repository.

#### Using the Included Certificate

The repository includes `localhost.pfx` with password `abcact`. This certificate is already configured in `appsettings.json`.

#### Creating Your Own Certificate (Optional)

```bash
# Generate a new self-signed certificate
dotnet dev-certs https -ep localhost.pfx -p your_password --trust

# Update appsettings.json with your password
```

### 4. Configuration Files

#### Web Application Configuration

Create `src/act.core.web/appsettings.Development.json`:

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
      "AWS": "Information",
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
    "MetadataAddress": "https://your-adfs-server.com/FederationMetadata/2007-06/FederationMetadata.xml",
    "Wtrealm": "https://localhost:44363"
  },
  "Mail": {
    "Host": "your-smtp-server.com",
    "Port": 25,
    "From": "act-dev@yourcompany.com"
  },
  "ConnectionStrings": {
    "ActDB": "Server=localhost;Database=ACT;User=act_user;Password=your_secure_password"
  },
  "HelpLinks": [],
  "InventorySystemLinkFormat": "https://your-inventory-system.com/{0}"
}
```

#### Lambda Configuration

Create `src/act.core.etl.lambda/appsettings.Development.json`:

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
    "Host": "your-smtp-server.com",
    "Port": 25,
    "From": "act-dev@yourcompany.com"
  },
  "ConnectionStrings": {
    "ActDB": "Server=localhost;Database=ACT;User=act_user;Password=your_secure_password"
  },
  "AddIns": []
}
```

## Required Environment Variables

### Database Configuration

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__ActDB` | MySQL connection string | `Server=localhost;Database=ACT;User=act_user;Password=your_password` |

### ADFS Authentication

| Variable | Description | Example |
|----------|-------------|---------|
| `ADFS__MetadataAddress` | ADFS metadata URL | `https://adfs.company.com/FederationMetadata/2007-06/FederationMetadata.xml` |
| `ADFS__Wtrealm` | Application realm identifier | `https://localhost:44363` |

### Email Configuration

| Variable | Description | Example |
|----------|-------------|---------|
| `Mail__Host` | SMTP server hostname | `smtp.company.com` |
| `Mail__Port` | SMTP server port | `25` or `587` |
| `Mail__From` | Sender email address | `act@company.com` |

### Chef Automate Integration

| Variable | Description | Example |
|----------|-------------|---------|
| `ChefAutomateUrl` | Chef Automate server URL | `https://automate.company.com` |
| `ChefAutomateOrg` | Organization name | `your-org` |
| `ChefAutomateToken` | API access token | `your-api-token` |

### AWS Configuration (for cloud features)

| Variable | Description | Example |
|----------|-------------|---------|
| `AWS_REGION` | AWS region | `us-east-2` |
| `AWS_ACCESS_KEY_ID` | AWS access key | `AKIA...` |
| `AWS_SECRET_ACCESS_KEY` | AWS secret key | `your-secret-key` |

## Required Credentials

### Database Credentials

- **MySQL Username**: `act_user` (or your preferred username)
- **MySQL Password**: Secure password for the database user
- **Database Name**: `ACT`

### ADFS Integration

- **ADFS Server**: Your organization's ADFS server URL
- **Application Registration**: Register the application in ADFS with appropriate claims
- **Certificate**: SSL certificate for HTTPS communication

### Chef Automate

- **API Token**: Generate an API token in Chef Automate with read permissions
- **Organization Access**: Ensure the token has access to required organizations
- **Network Access**: Ensure the development machine can reach Chef Automate servers

### Email Server

- **SMTP Credentials**: Username/password if authentication is required
- **Network Access**: Ensure the development machine can reach the SMTP server
- **Sender Permissions**: Verify the sender email address is authorized

## Building and Running

### 1. Restore Dependencies

```bash
# From the repository root
dotnet restore
```

### 2. Build the Solution

```bash
# Build all projects
dotnet build

# Or build specific projects
dotnet build src/act.core.web/act.core.web.csproj
dotnet build src/act.core.etl.lambda/act.core.etl.lambda.csproj
```

### 3. Run the Web Application

```bash
# Navigate to web project
cd src/act.core.web

# Run the application
dotnet run

# Or run with specific profile
dotnet run --launch-profile "Development"
```

The application will be available at:
- HTTPS: `https://localhost:44363`
- HTTP: `http://localhost:5000` (redirects to HTTPS)

### 4. Test Lambda Functions Locally

```bash
# Navigate to lambda project
cd src/act.core.etl.lambda

# Run specific lambda function
dotnet run -- '{"name":"databaseupdate","index":0}'
dotnet run -- '{"name":"gather","index":1}'
```

## Chef InSpec Development

### 1. InSpec Profile Development

```bash
# Navigate to compliance profiles
cd Compliance

# Test Linux profile
inspec exec csg_linux_compliant_server --target ssh://user@linux-server

# Test Windows profile
inspec exec csg_windows_compliant_server --target winrm://user@windows-server
```

### 2. Chef Cookbook Development

```bash
# Navigate to cookbook
cd Chef/cookbooks/act

# Install dependencies
chef install

# Test cookbook
kitchen test

# Upload to Chef Server (if configured)
knife cookbook upload act
```

## Database Development

### Entity Framework Commands

```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/act.core.data

# Update database
dotnet ef database update --project src/act.core.data

# Generate SQL script
dotnet ef migrations script --project src/act.core.data

# Drop database (careful!)
dotnet ef database drop --project src/act.core.data
```

### Seeding Test Data

```bash
# Run the database seeding (if implemented)
dotnet run --project src/act.core.etl.lambda -- '{"name":"seed","index":0}'
```

## Testing

### Unit Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Integration Tests

```bash
# Ensure test database is available
# Run integration tests
dotnet test --filter Category=Integration
```

### Manual Testing

1. **Web Application**: Navigate to `https://localhost:44363`
2. **API Endpoints**: Use Postman or curl to test API endpoints
3. **Database**: Use MySQL Workbench to verify data operations
4. **Chef Integration**: Test with actual Chef Automate servers (if available)

## Troubleshooting

### Common Issues

#### SSL Certificate Issues

```bash
# Trust the development certificate
dotnet dev-certs https --trust

# Clear certificate cache
dotnet dev-certs https --clean
```

#### Database Connection Issues

```bash
# Test MySQL connection
mysql -u act_user -p -h localhost ACT

# Check MySQL service status
sudo systemctl status mysql  # Linux
brew services list | grep mysql  # macOS
```

#### Port Conflicts

```bash
# Check what's using port 44363
netstat -tulpn | grep 44363  # Linux
lsof -i :44363  # macOS
netstat -ano | findstr 44363  # Windows
```

#### Chef/InSpec Issues

```bash
# Verify Chef Workstation installation
chef --version
inspec --version

# Check InSpec profile syntax
inspec check Compliance/csg_linux_compliant_server
```

### Debugging

#### Enable Detailed Logging

Add to `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft": "Debug",
      "System": "Debug"
    }
  }
}
```

#### Database Query Logging

Enable EF Core query logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

## Development Workflow

### 1. Feature Development

```bash
# Create feature branch
git checkout -b feature/your-feature-name

# Make changes
# Test locally
# Commit changes
git add .
git commit -m "Add your feature"

# Push and create PR
git push origin feature/your-feature-name
```

### 2. Database Changes

```bash
# Create migration
dotnet ef migrations add YourMigrationName --project src/act.core.data

# Test migration
dotnet ef database update --project src/act.core.data

# Commit migration files
git add src/act.core.data/Migrations/
git commit -m "Add database migration for your feature"
```

### 3. Testing Changes

```bash
# Run unit tests
dotnet test

# Test web application
dotnet run --project src/act.core.web

# Test lambda functions
dotnet run --project src/act.core.etl.lambda -- '{"name":"test","index":0}'
```

## Additional Resources

- [.NET Core Documentation](https://docs.microsoft.com/en-us/dotnet/core/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Chef InSpec Documentation](https://docs.chef.io/inspec/)
- [MySQL Documentation](https://dev.mysql.com/doc/)
- [AWS Lambda .NET Documentation](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)

## Getting Help

- **Internal Documentation**: Check the `Doc/` directory for additional documentation
- **Code Comments**: Review inline documentation in the source code
- **Issue Tracking**: Use GitHub issues for bug reports and feature requests
- **Team Communication**: Contact the development team for specific questions
