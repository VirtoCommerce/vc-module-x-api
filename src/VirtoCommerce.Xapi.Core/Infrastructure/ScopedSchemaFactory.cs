using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Introspection;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public class ScopedSchemaFactory<TMarker> : SchemaFactory
    {
        public ScopedSchemaFactory(
            IEnumerable<ISchemaBuilder> schemaBuilders,
            IServiceProvider services,
            ISchemaFilter schemaFilter)
            : base(schemaBuilders, services, schemaFilter)
        {
        }

        protected override List<ISchemaBuilder> GetSchemaBuilders()
        {
            var schemaBuilders = base.GetSchemaBuilders();

            // find all builders inside this assembly
            var currentAssembly = typeof(TMarker).Assembly;

            var subSchemaBuilders = schemaBuilders
                .Where(p => IsDeclaredInAssembly(p.GetType(), currentAssembly))
                .ToList();

            return subSchemaBuilders;
        }

        /// <summary>
        /// A builder belongs to the assembly if the builder type or any of its non-abstract base types is declared there.
        /// This keeps a client subclass (registered via OverrideSchemaBuilder) of a builder from the scoped assembly in the scoped schema.
        /// Abstract bases are skipped because they are shared by builders of unrelated assemblies.
        /// </summary>
        private static bool IsDeclaredInAssembly(Type builderType, Assembly assembly)
        {
            for (var type = builderType; type != null; type = type.BaseType)
            {
                if (!type.IsAbstract && type.Assembly == assembly)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
