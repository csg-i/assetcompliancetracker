# ACT (Asset Compliance Tracker) - Local Development Setup

This guide provides comprehensive instructions for setting up a local development environment for the ACT (Asset Compliance Tracker) application.

## Prerequisites

### Required Tools

1. **.NET 8 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version` (should show 8.x.x)

2. **MySQL Server** (Local Database)
   - Download from: https://dev.mysql.com/downloads/mysql/
   - Alternative: Use Docker MySQL container (see Docker Setup section)

3. **Git**
   - Download from: https://git-scm.com/downloads

4. **IDE/Editor** (Choose one)
   - **Visual Studio 2022** (recommended for Windows)
   - **Visual Studio Code** with C# extension
   - **JetBrains Rider**

### Optional Tools

1. **Docker Desktop** (for containerized MySQL)
   - Download from: https://www.docker.com/products/docker-desktop

2. **MySQL Workbench** (Database management)
   - Download from: https://dev.mysql.com/downloads/workbench/

## Clone and Setup

### 1. Clone Repository

```bash
git clone <repository-url>
cd ACT
```

### 2. Restore Dependencies

```bash
# Restore NuGet packages for all projects
dotnet restore
```

## Database Setup

### Option A: Local MySQL Installation

1. **Install MySQL Server** with the following configuration:
   - Port: 3306 (default)
   - Root password: Set to your preference (you'll need to update config)

2. **Create Database:**
   ```sql
   CREATE DATABASE ACT;
   ```

3. **Update Connection String:**
   - Edit `src/act.core.web/appsettings.json`
   - Update the `ActDB` connection string:
   ```json
   {
     "ConnectionStrings": {
       "ActDB": "Server=localhost;Database=ACT;User=root;Password=YOUR_PASSWORD"
     }
   }
   ```

### Option B: Docker MySQL (Recommended)

1. **Start MySQL Container:**
   ```bash
   docker run --name act-mysql \
     -e MYSQL_ROOT_PASSWORD=act123 \
     -e MYSQL_DATABASE=ACT \
     -p 3306:3306 \
     -d mysql:8.0
   ```

2. **Update Connection String:**
   ```json
   {
     "ConnectionStrings": {
       "ActDB": "Server=localhost;Database=ACT;User=root;Password=act123"
     }
   }
   ```

### 3. Apply Database Migrations

```bash
# Navigate to the web project directory
cd src/act.core.web

# Apply Entity Framework migrations
dotnet ef database update --project ../act.core.data
```

## SSL Certificate Setup

The application requires HTTPS and includes a development certificate (`localhost.pfx`).

### Certificate Configuration

The certificate is already included in the project with these settings:
- **Path:** `localhost.pfx`
- **Password:** `abcact`
- **HTTPS Port:** 44363

### Trust Development Certificate (if needed)

If you encounter SSL issues, you may need to trust the .NET development certificate:

```bash
dotnet dev-certs https --trust
```

## Configuration

### Required Configuration Files

1. **Web Application:** `src/act.core.web/appsettings.json`
2. **Development Override:** `src/act.core.web/appsettings.Development.json`

### Key Configuration Sections

#### 1. Database Connection
```json
{
  "ConnectionStrings": {
    "ActDB": "Server=localhost;Database=ACT;User=root;Password=YOUR_PASSWORD"
  }
}
```

#### 2. ADFS Authentication (Development)
```json
{
  "ADFS": {
    "MetadataAddress": "https://adfs.myorg.org/FederationMetadata/2007-06/FederationMetadata.xml",
    "Wtrealm": "https://localhost:44363"
  }
}
```

**Note:** For local development, you may need to:
- Update the `MetadataAddress` to point to your organization's ADFS server
- Or disable ADFS authentication temporarily (see Authentication Setup section)

#### 3. AWS Configuration (Optional for local development)
```json
{
  "AWS": {
    "Profile": "default",
    "Region": "us-west-2"
  },
  "bucket": "your-s3-bucket-name",
  "keyPrefix": "act-local"
}
```

#### 4. Email Configuration
```json
{
  "Mail": {
    "Host": "smtp.yourorg.com",
    "Port": 25,
    "From": "act@yourorg.com"
  }
}
```

## Authentication Setup

### Option A: Use ADFS (Production-like)

1. Ensure your organization's ADFS server is accessible
2. Update the `ADFS` configuration section with correct URLs
3. Ensure your local development URL (`https://localhost:44363`) is registered with ADFS

### Option B: Disable Authentication (Development Only)

For local development, you can temporarily disable authentication:

1. **Edit `src/act.core.web/Startup.cs`:**
   - Comment out or modify the authentication configuration
   - Remove the `[Authorize]` filter requirement

**⚠️ Warning:** Only disable authentication for local development. Never deploy without proper authentication.

## Build and Run

### 1. Build Solution

```bash
# Build entire solution
dotnet build

# Or build in Release mode
dotnet build -c Release
```

### 2. Run Web Application

```bash
# Navigate to web project
cd src/act.core.web

# Run the application
dotnet run
```

The application will be available at:
- **HTTPS:** https://localhost:44363
- **HTTP:** http://localhost:8080

### 3. Run with Hot Reload (Development)

```bash
# Run with file watching and hot reload
dotnet watch run --project src/act.core.web
```

## Development Workflow

### Database Migrations

When making database changes:

```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/act.core.data --startup-project src/act.core.web

# Apply migrations
dotnet ef database update --project src/act.core.data --startup-project src/act.core.web
```

### Testing

```bash
# Run all tests (if test projects exist)
dotnet test

# Run specific project tests
dotnet test src/ProjectName.Tests
```

### Code Formatting

The project is configured with `TreatWarningsAsErrors` set to `true`, so ensure code quality:

```bash
# Check for build warnings/errors
dotnet build --verbosity normal
```

## Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Verify MySQL is running: `mysql -u root -p`
   - Check connection string credentials
   - Ensure database `ACT` exists

2. **SSL Certificate Issues**
   - Trust development certificates: `dotnet dev-certs https --trust`
   - Check if `localhost.pfx` exists in the web project

3. **Authentication Errors**
   - Verify ADFS configuration
   - Consider temporarily disabling authentication for development

4. **Port Already in Use**
   - Change ports in `appsettings.Development.json`
   - Kill processes using ports 44363 or 8080

5. **Missing Dependencies**
   - Run `dotnet restore` again
   - Clear NuGet cache: `dotnet nuget locals all --clear`

### Logs and Debugging

- **Application logs:** Check console output when running `dotnet run`
- **Database logs:** Check MySQL error logs
- **AWS logs:** CloudWatch logs (if AWS integration is configured)

### Health Check Endpoint

The application includes health check endpoints:
- **Simple:** `https://localhost:44363/Health`
- **Detailed:** `https://localhost:44363/Health/Detailed`

## Environment Variables

For sensitive configuration, you can use environment variables:

```bash
# Set connection string via environment variable
export ConnectionStrings__ActDB="Server=localhost;Database=ACT;User=root;Password=your_password"

# Set AWS configuration
export AWS__Region="us-west-2"
export AWS__Profile="default"
```

## Docker Development (Alternative)

### Run Entire Stack with Docker

```bash
# Build and run with Docker Compose (if docker-compose.yml exists)
docker-compose up --build

# Or run just the web application
docker build -f Docker/Dockerfile -t act-web .
docker run -p 443:443 act-web
```

## IDE-Specific Setup

### Visual Studio 2022

1. Open `ACT.sln`
2. Set `act.core.web` as startup project
3. Configure launch settings for HTTPS
4. Use built-in NuGet package manager for dependencies

### Visual Studio Code

1. Install C# extension
2. Open workspace folder
3. Configure launch.json and tasks.json for debugging
4. Use integrated terminal for `dotnet` commands

### JetBrains Rider

1. Open `ACT.sln`
2. Configure run configuration for web project
3. Set up database connection in Database tool window

## Production Considerations

When preparing for production deployment:

1. **Update Configuration:**
   - Use production connection strings
   - Configure proper ADFS endpoints
   - Set up AWS credentials and S3 buckets

2. **Security:**
   - Use proper SSL certificates
   - Enable authentication
   - Secure connection strings (use Azure Key Vault, AWS Secrets Manager, etc.)

3. **Performance:**
   - Build in Release mode
   - Configure connection pooling
   - Set up proper logging levels

## Additional Resources

- **.NET 8 Documentation:** https://docs.microsoft.com/en-us/dotnet/
- **Entity Framework Core:** https://docs.microsoft.com/en-us/ef/core/
- **ASP.NET Core:** https://docs.microsoft.com/en-us/aspnet/core/
- **MySQL Documentation:** https://dev.mysql.com/doc/

## Support

For development issues:

1. Check this documentation
2. Review application logs
3. Check database connectivity
4. Verify configuration settings
5. Consult team documentation or reach out to the development team

---

**Happy Coding! 🚀**