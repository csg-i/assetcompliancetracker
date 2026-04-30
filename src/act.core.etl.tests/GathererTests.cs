using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using act.core.data;
using act.core.etl;

namespace act.core.etl.tests
{
    public class GathererTests : IDisposable
    {
        private readonly ActDbContext _dbContext;
        private readonly Gatherer _gatherer;
        private readonly ILoggerFactory _loggerFactory;

        public GathererTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ActDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ActDbContext(options);
            _loggerFactory = new NullLoggerFactory();

            // Setup configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();

            _gatherer = new Gatherer(_dbContext, _loggerFactory, configuration);

            // Seed required data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Create a basic BuildSpecification for testing
            var buildSpec = new BuildSpecification
            {
                Id = 1,
                Name = "Test Build Spec",
                OwnerEmployeeId = 1,
                BuildSpecificationType = BuildSpecificationTypeConstant.OperatingSystem
            };
            _dbContext.BuildSpecifications.Add(buildSpec);

            // Create an Employee for testing
            var employee = new Employee
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                SamAccountName = "testuser",
                IsActive = true
            };
            _dbContext.Employees.Add(employee);

            // Create a Product for testing
            var product = new Product
            {
                Code = "TEST",
                Name = "Test Product",
                ExludeFromReports = false
            };
            _dbContext.Products.Add(product);

            // Create a Function for testing
            var function = new Function
            {
                Id = 1,
                Name = "Test Function"
            };
            _dbContext.Functions.Add(function);

            // Create an Environment for testing
            var environment = new Environment
            {
                Id = 1,
                Name = "Test",
                Description = "Test Environment",
                ChefAutomateUrl = "https://test.example.com",
                ChefAutomateOrg = "test",
                ChefAutomateToken = "test-token",
                Color = "#000000"
            };
            _dbContext.Environments.Add(environment);

            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task PurgeInactiveNodes_ActiveNodeWithLastComplianceResultDateOlderThan15Days_ShouldDelete()
        {
            // Arrange
            var node = new Node
            {
                InventoryItemId = 1,
                Fqdn = "test1.example.com",
                IsActive = true,
                DeactivatedDate = null,
                LastComplianceResultDate = DateTime.Today.AddDays(-16),
                OwnerEmployeeId = 1,
                ProductCode = "TEST",
                FunctionId = 1,
                EnvironmentId = 1,
                BuildSpecificationId = 1,
                PciScope = PciScopeConstant.A,
                Platform = PlatformConstant.WindowsServer,
                ComplianceStatus = ComplianceStatusConstant.Failed
            };
            _dbContext.Nodes.Add(node);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.True(result >= 1000, "Should return count >= 1000 indicating nodes were processed");
            var remainingNode = await _dbContext.Nodes.FindAsync(1L);
            Assert.Null(remainingNode); // Node should be deleted
        }

        [Fact]
        public async Task PurgeInactiveNodes_ActiveNodeWithLastComplianceResultDateExactly15DaysOld_ShouldNotDelete()
        {
            // Arrange
            var node = new Node
            {
                InventoryItemId = 2,
                Fqdn = "test2.example.com",
                IsActive = true,
                DeactivatedDate = null,
                LastComplianceResultDate = DateTime.Today.AddDays(-15),
                OwnerEmployeeId = 1,
                ProductCode = "TEST",
                FunctionId = 1,
                EnvironmentId = 1,
                BuildSpecificationId = 1,
                PciScope = PciScopeConstant.A,
                Platform = PlatformConstant.WindowsServer,
                ComplianceStatus = ComplianceStatusConstant.Failed
            };
            _dbContext.Nodes.Add(node);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.Equal(0, result); // Should return 0 as no nodes qualify for deletion
            var remainingNode = await _dbContext.Nodes.FindAsync(2L);
            Assert.NotNull(remainingNode); // Node should still exist
        }

        [Fact]
        public async Task PurgeInactiveNodes_ActiveNodeWithLastComplianceResultDateWithin15Days_ShouldNotDelete()
        {
            // Arrange
            var node = new Node
            {
                InventoryItemId = 3,
                Fqdn = "test3.example.com",
                IsActive = true,
                DeactivatedDate = null,
                LastComplianceResultDate = DateTime.Today.AddDays(-5),
                OwnerEmployeeId = 1,
                ProductCode = "TEST",
                FunctionId = 1,
                EnvironmentId = 1,
                BuildSpecificationId = 1,
                PciScope = PciScopeConstant.A,
                Platform = PlatformConstant.WindowsServer,
                ComplianceStatus = ComplianceStatusConstant.Failed
            };
            _dbContext.Nodes.Add(node);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.Equal(0, result);
            var remainingNode = await _dbContext.Nodes.FindAsync(3L);
            Assert.NotNull(remainingNode); // Node should still exist
        }

        [Fact]
        public async Task PurgeInactiveNodes_InactiveNodeDeactivatedMoreThan7DaysAgo_ShouldDelete()
        {
            // Arrange
            var node = new Node
            {
                InventoryItemId = 4,
                Fqdn = "test4.example.com",
                IsActive = false,
                DeactivatedDate = DateTime.Today.AddDays(-8),
                LastComplianceResultDate = DateTime.Today.AddDays(-1), // Recent compliance date
                OwnerEmployeeId = 1,
                ProductCode = "TEST",
                FunctionId = 1,
                EnvironmentId = 1,
                BuildSpecificationId = 1,
                PciScope = PciScopeConstant.A,
                Platform = PlatformConstant.WindowsServer,
                ComplianceStatus = ComplianceStatusConstant.Failed
            };
            _dbContext.Nodes.Add(node);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.True(result >= 1000, "Should return count >= 1000 indicating nodes were processed");
            var remainingNode = await _dbContext.Nodes.FindAsync(4L);
            Assert.Null(remainingNode); // Node should be deleted
        }

        [Fact]
        public async Task PurgeInactiveNodes_InactiveNodeDeactivatedLessThan7DaysAgoWithRecentCompliance_ShouldNotDelete()
        {
            // Arrange
            var node = new Node
            {
                InventoryItemId = 5,
                Fqdn = "test5.example.com",
                IsActive = false,
                DeactivatedDate = DateTime.Today.AddDays(-3),
                LastComplianceResultDate = DateTime.Today.AddDays(-3),
                OwnerEmployeeId = 1,
                ProductCode = "TEST",
                FunctionId = 1,
                EnvironmentId = 1,
                BuildSpecificationId = 1,
                PciScope = PciScopeConstant.A,
                Platform = PlatformConstant.WindowsServer,
                ComplianceStatus = ComplianceStatusConstant.Failed
            };
            _dbContext.Nodes.Add(node);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.Equal(0, result);
            var remainingNode = await _dbContext.Nodes.FindAsync(5L);
            Assert.NotNull(remainingNode); // Node should still exist
        }

        [Fact]
        public async Task PurgeInactiveNodes_MixedSet_ShouldDeleteOnlyQualifyingNodes()
        {
            // Arrange
            // Node 1: Active, LastComplianceResultDate = 20 days ago → should be deleted
            var node1 = new Node
            {
                InventoryItemId = 6,
                Fqdn = "test6.example.com",
                IsActive = true,
                DeactivatedDate = null,
                LastComplianceResultDate = DateTime.Today.AddDays(-20),
                OwnerEmployeeId = 1,
                ProductCode = "TEST",
                FunctionId = 1,
                EnvironmentId = 1,
                BuildSpecificationId = 1,
                PciScope = PciScopeConstant.A,
                Platform = PlatformConstant.WindowsServer,
                ComplianceStatus = ComplianceStatusConstant.Failed
            };

            // Node 2: Inactive, DeactivatedDate = 10 days ago → should be deleted
            var node2 = new Node
            {
                InventoryItemId = 7,
                Fqdn = "test7.example.com",
                IsActive = false,
                DeactivatedDate = DateTime.Today.AddDays(-10),
                LastComplianceResultDate = DateTime.Today.AddDays(-1), // Recent compliance
                OwnerEmployeeId = 1,
                ProductCode = "TEST",
                FunctionId = 1,
                EnvironmentId = 1,
                BuildSpecificationId = 1,
                PciScope = PciScopeConstant.A,
                Platform = PlatformConstant.WindowsServer,
                ComplianceStatus = ComplianceStatusConstant.Failed
            };

            // Node 3: Active, LastComplianceResultDate = 10 days ago → should be retained
            var node3 = new Node
            {
                InventoryItemId = 8,
                Fqdn = "test8.example.com",
                IsActive = true,
                DeactivatedDate = null,
                LastComplianceResultDate = DateTime.Today.AddDays(-10),
                OwnerEmployeeId = 1,
                ProductCode = "TEST",
                FunctionId = 1,
                EnvironmentId = 1,
                BuildSpecificationId = 1,
                PciScope = PciScopeConstant.A,
                Platform = PlatformConstant.WindowsServer,
                ComplianceStatus = ComplianceStatusConstant.Failed
            };

            _dbContext.Nodes.AddRange(node1, node2, node3);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.True(result >= 1000, "Should return count >= 1000 indicating nodes were processed");
            
            var remainingNode1 = await _dbContext.Nodes.FindAsync(6L);
            var remainingNode2 = await _dbContext.Nodes.FindAsync(7L);
            var remainingNode3 = await _dbContext.Nodes.FindAsync(8L);

            Assert.Null(remainingNode1); // Node 1 should be deleted
            Assert.Null(remainingNode2); // Node 2 should be deleted
            Assert.NotNull(remainingNode3); // Node 3 should be retained
        }

        [Fact]
        public async Task PurgeInactiveNodes_NodeWithStaleComplianceDate_ShouldLogCorrectMessage()
        {
            // Arrange
            var testLogger = new TestLogger<Gatherer>();
            var testLoggerFactory = new TestLoggerFactory(testLogger);
            var testGatherer = new Gatherer(_dbContext, testLoggerFactory, new ConfigurationBuilder().Build());

            var node = new Node
            {
                InventoryItemId = 9,
                Fqdn = "test9.example.com",
                IsActive = true,
                DeactivatedDate = null,
                LastComplianceResultDate = DateTime.Today.AddDays(-20),
                OwnerEmployeeId = 1,
                ProductCode = "TEST",
                FunctionId = 1,
                EnvironmentId = 1,
                BuildSpecificationId = 1,
                PciScope = PciScopeConstant.A,
                Platform = PlatformConstant.WindowsServer,
                ComplianceStatus = ComplianceStatusConstant.Failed
            };
            _dbContext.Nodes.Add(node);
            await _dbContext.SaveChangesAsync();

            // Act
            await testGatherer.PurgeInactiveNodes();

            // Assert
            Assert.True(testLogger.LogEntries.Exists(entry => 
                entry.Contains("Stale Compliance Node") && 
                entry.Contains("test9.example.com")), 
                "Should log stale compliance node information");
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }

    // Test logger implementation to capture log messages
    public class TestLogger<T> : ILogger<T>
    {
        public List<string> LogEntries { get; } = new List<string>();

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LogEntries.Add(formatter(state, exception));
        }
    }

    public class TestLoggerFactory : ILoggerFactory
    {
        private readonly TestLogger<Gatherer> _testLogger;

        public TestLoggerFactory(TestLogger<Gatherer> testLogger)
        {
            _testLogger = testLogger;
        }

        public ILogger CreateLogger(string categoryName) => _testLogger;

        public ILogger<T> CreateLogger<T>() => (ILogger<T>)_testLogger;

        public void AddProvider(ILoggerProvider provider) { }

        public void Dispose() { }
    }
}