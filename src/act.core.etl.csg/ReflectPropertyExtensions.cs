using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace act.core.etl.csg
{
    public static class ReflectPropertyExtensions
    {
        public static TEntity ReflectType<TEntity>(this IDataRecord dr, IDictionary<string,int> fields) where TEntity : class, new()
        {

            var instanceToPopulate = new TEntity();

            var propertyInfos = typeof(TEntity).GetProperties
                (BindingFlags.Public | BindingFlags.Instance);

            //for each public property on the original
            foreach (var pi in propertyInfos)
            {
                if (fields.ContainsKey(pi.Name))
                {
                    var dbValue = dr.GetValue(fields[pi.Name]);
                    if (dbValue != null && dbValue != DBNull.Value)
                    {
                        object value;
                        if (pi.PropertyType.IsEnum)
                            value = Enum.Parse(pi.PropertyType, dbValue.ToString());
                        else if (pi.PropertyType.IsGenericType &&
                                 pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var type = Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType;
                            value = Convert.ChangeType(dbValue, type,
                                CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            value = Convert.ChangeType(dbValue, pi.PropertyType, CultureInfo.InvariantCulture);
                        }

                        pi.SetValue(instanceToPopulate, value);
                    }
                }
            }

            return instanceToPopulate;
        }
    }
}