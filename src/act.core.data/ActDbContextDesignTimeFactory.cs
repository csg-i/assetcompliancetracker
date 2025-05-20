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
            builder.UseMySql("Server=localhost;Database=act;User=root;Password=12345;",
            options => options.ServerVersion(new Version(5, 6, 10), ServerType.MySql));
            return new ActDbContext(builder.Options);
        }
    }
}