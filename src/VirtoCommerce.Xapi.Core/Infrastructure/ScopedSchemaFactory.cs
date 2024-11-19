using System;
using System.Collections.Generic;
using System.Linq;
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

            // find all builders with inside this project
            var currentAssembly = typeof(TMarker).Assembly;

            var subSchemaBuilders = schemaBuilders
                .Where(p => p.GetType().Assembly == currentAssembly)
                .ToList();

            return subSchemaBuilders;
        }
    }
}
