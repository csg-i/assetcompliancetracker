﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Extensions;
using Pomelo.EntityFrameworkCore.MySql.Storage;

namespace act.core.data
{
    public static class Bootstrap
    {
        public static IServiceCollection AddActDbContext(this IServiceCollection services, IConfiguration configuration,  int commandTimeout=30, string connectionStringName="ActDb")
        {
            return services.AddDbContext<ActDbContext>(options =>

                options.UseMySql(configuration.GetConnectionString(connectionStringName), new MySqlServerVersion(new Version(8, 0, 32)), o =>
                               {
                                   o.CommandTimeout(commandTimeout);
                               }));


        }
        public static IServiceCollection AddActDbContextPool(this IServiceCollection services, IConfiguration configuration, int commandTimeout=30, string connectionStringName = "ActDb")
        {
            return services.AddDbContextPool<ActDbContext>(options =>
                options.UseMySql(configuration.GetConnectionString(connectionStringName), new MySqlServerVersion(new Version(8, 0, 32)), o =>
                {
                        o.CommandTimeout(commandTimeout);
                    }));
        }
    }
}