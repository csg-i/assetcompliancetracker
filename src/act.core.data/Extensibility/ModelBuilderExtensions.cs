using System;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace act.core.data.Extensibility
{
    internal static class ModelBuilderExtensions
    {
        public static ModelBuilder ModelPluralizer<T>(this ModelBuilder builder, Action<EntityTypeBuilder<T>> action) where T: class
        {
            builder.Entity<T>().ToTable(typeof(T).Name);
            if (action != null)
            {
                return builder.Entity(action);
            }
            return builder;
        }
    }
}