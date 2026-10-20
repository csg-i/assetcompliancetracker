using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using act.core.data;
using act.core.etl;
using Environment = act.core.data.Environment;

namespace act.core.etl.tests
{
    public class GathererTests : IDisposable
    {
        private readonly ActDbContext _dbContext;
        private readonly Gatherer _gatherer;
        private readonly ILoggerFactory _loggerFactory;

        public GathererTests()
        {
            // Setup SQLite in-memory database
            var options = new DbContextOptionsBuilder<ActDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _dbContext = new ActDbContext(options);
            _dbContext.Database.EnsureCreated();
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
            // For SQLite testing, we'll create the minimal required tables using raw SQL
            // This avoids the complexity of full entity relationships for testing the DELETE logic
            
            var connection = _dbContext.Database.GetDbConnection();
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS BuildSpecification (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL
                );
                
                CREATE TABLE IF NOT EXISTS Node (
                    InventoryItemId INTEGER PRIMARY KEY,
                    Fqdn TEXT NOT NULL,
                    IsActive INTEGER NOT NULL,
                    DeactivatedDate TEXT,
                    LastComplianceResultDate TEXT,
                    OwnerEmployeeId INTEGER,
                    ProductCode TEXT,
                    FunctionId INTEGER,
                    EnvironmentId INTEGER,
                    BuildSpecificationId INTEGER,
                    PciScope INTEGER,
                    Platform INTEGER,
                    ComplianceStatus INTEGER
                );
                
                INSERT OR IGNORE INTO BuildSpecification (Id, Name) VALUES (1, 'Test Build Spec');
            ";
            command.ExecuteNonQuery();
            
            connection.Close();
        }

        [Fact]
        public async Task PurgeInactiveNodes_ActiveNodeWithLastComplianceResultDateOlderThan15Days_ShouldDelete()
        {
            // Arrange
            await InsertNodeAsync(1, "test1.example.com", isActive: true, 
                deactivatedDate: null, 
                lastComplianceResultDate: DateTime.Today.AddDays(-16));

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.True(result >= 1, "Should return count >= 1 indicating nodes were deleted");
            var remainingNode = await GetNodeAsync(1);
            Assert.Null(remainingNode); // Node should be deleted
        }

        [Fact]
        public async Task PurgeInactiveNodes_ActiveNodeWithLastComplianceResultDateExactly15DaysOld_ShouldNotDelete()
        {
            // Arrange
            await InsertNodeAsync(2, "test2.example.com", isActive: true, 
                deactivatedDate: null, 
                lastComplianceResultDate: DateTime.Today.AddDays(-15));

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.Equal(0, result); // Should return 0 as no nodes qualify for deletion
            var remainingNode = await GetNodeAsync(2);
            Assert.NotNull(remainingNode); // Node should still exist
        }

        [Fact]
        public async Task PurgeInactiveNodes_ActiveNodeWithLastComplianceResultDateWithin15Days_ShouldNotDelete()
        {
            // Arrange
            await InsertNodeAsync(3, "test3.example.com", isActive: true, 
                deactivatedDate: null, 
                lastComplianceResultDate: DateTime.Today.AddDays(-5));

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.Equal(0, result);
            var remainingNode = await GetNodeAsync(3);
            Assert.NotNull(remainingNode); // Node should still exist
        }

        [Fact]
        public async Task PurgeInactiveNodes_InactiveNodeDeactivatedMoreThan7DaysAgo_ShouldDelete()
        {
            // Arrange
            await InsertNodeAsync(4, "test4.example.com", isActive: false, 
                deactivatedDate: DateTime.Today.AddDays(-8), 
                lastComplianceResultDate: DateTime.Today.AddDays(-1));

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.True(result >= 1, "Should return count >= 1 indicating nodes were deleted");
            var remainingNode = await GetNodeAsync(4);
            Assert.Null(remainingNode); // Node should be deleted
        }

        [Fact]
        public async Task PurgeInactiveNodes_InactiveNodeDeactivatedLessThan7DaysAgoWithRecentCompliance_ShouldNotDelete()
        {
            // Arrange
            await InsertNodeAsync(5, "test5.example.com", isActive: false, 
                deactivatedDate: DateTime.Today.AddDays(-3), 
                lastComplianceResultDate: DateTime.Today.AddDays(-3));

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.Equal(0, result);
            var remainingNode = await GetNodeAsync(5);
            Assert.NotNull(remainingNode); // Node should still exist
        }

        [Fact]
        public async Task PurgeInactiveNodes_MixedSet_ShouldDeleteOnlyQualifyingNodes()
        {
            // Arrange
            // Node 1: Active, LastComplianceResultDate = 20 days ago → should be deleted
            await InsertNodeAsync(6, "test6.example.com", isActive: true, 
                deactivatedDate: null, 
                lastComplianceResultDate: DateTime.Today.AddDays(-20));

            // Node 2: Inactive, DeactivatedDate = 10 days ago → should be deleted
            await InsertNodeAsync(7, "test7.example.com", isActive: false, 
                deactivatedDate: DateTime.Today.AddDays(-10), 
                lastComplianceResultDate: DateTime.Today.AddDays(-1));

            // Node 3: Active, LastComplianceResultDate = 10 days ago → should be retained
            await InsertNodeAsync(8, "test8.example.com", isActive: true, 
                deactivatedDate: null, 
                lastComplianceResultDate: DateTime.Today.AddDays(-10));

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.True(result >= 2, "Should return count >= 2 indicating two nodes were deleted");
            
            var remainingNode1 = await GetNodeAsync(6);
            var remainingNode2 = await GetNodeAsync(7);
            var remainingNode3 = await GetNodeAsync(8);

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

            await InsertNodeAsync(9, "test9.example.com", isActive: true, 
                deactivatedDate: null, 
                lastComplianceResultDate: DateTime.Today.AddDays(-20));

            // Act
            await testGatherer.PurgeInactiveNodes();

            // Assert
            Assert.True(testLogger.LogEntries.Exists(entry => 
                entry.Contains("Stale Compliance Node") && 
                entry.Contains("test9.example.com")), 
                "Should log stale compliance node information");
        }

        [Fact]
        public async Task PurgeInactiveNodes_NodeWithNullLastComplianceResultDate_ShouldNotDelete()
        {
            // Arrange
            await InsertNodeAsync(10, "test10.example.com", isActive: true, 
                deactivatedDate: null, 
                lastComplianceResultDate: null);

            // Act
            var result = await _gatherer.PurgeInactiveNodes();

            // Assert
            Assert.Equal(0, result); // Should return 0 as no nodes qualify for deletion
            var remainingNode = await GetNodeAsync(10);
            Assert.NotNull(remainingNode); // Node should still exist (NULL is not less than a date)
        }

        private async Task InsertNodeAsync(long inventoryItemId, string fqdn, bool isActive, 
            DateTime? deactivatedDate, DateTime? lastComplianceResultDate)
        {
            var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Node (InventoryItemId, Fqdn, IsActive, DeactivatedDate, LastComplianceResultDate, 
                                  OwnerEmployeeId, ProductCode, FunctionId, EnvironmentId, BuildSpecificationId, 
                                  PciScope, Platform, ComplianceStatus)
                VALUES (@id, @fqdn, @active, @deactivated, @compliance, 1, 'TEST', 1, 1, 1, 0, 0, 0);
            ";
            
            var parameters = new[]
            {
                CreateParameter(command, "@id", inventoryItemId),
                CreateParameter(command, "@fqdn", fqdn),
                CreateParameter(command, "@active", isActive ? 1 : 0),
                CreateParameter(command, "@deactivated", deactivatedDate?.ToString("yyyy-MM-dd HH:mm:ss")),
                CreateParameter(command, "@compliance", lastComplianceResultDate?.ToString("yyyy-MM-dd HH:mm:ss"))
            };
            
            command.Parameters.AddRange(parameters);
            await command.ExecuteNonQueryAsync();
        }
        
        private async Task<Node> GetNodeAsync(long inventoryItemId)
        {
            var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT InventoryItemId FROM Node WHERE InventoryItemId = @id";
            command.Parameters.Add(CreateParameter(command, "@id", inventoryItemId));
            
            var result = await command.ExecuteScalarAsync();
            return result != null ? new Node { InventoryItemId = (long)result } : null;
        }
        
        private static System.Data.Common.DbParameter CreateParameter(System.Data.Common.DbCommand command, 
            string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            return parameter;
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