using System;
using act.core.data.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Extensions;
using Pomelo.EntityFrameworkCore.MySql.Storage;

namespace act.core.data
{
    public class ActDbContextDesignTimeFactory: IDesignTimeDbContextFactory<ActDbContext>
    {
        public ActDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ActDbContext>();
            var connectionString = "Server=localhost;Database=act;User=root;Password=12345;";
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            builder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 32)), options => options
            .EnableRetryOnFailure());
            return new ActDbContext(builder.Options);
        }
    }
}