using System;
using GraphQL.Types;
using MediatR;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Queries;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class CountryType : ExtendableGraphType<Country>
    {
        public CountryType()
        {
            Field(x => x.Id, nullable: false).Description("Code of country. For example 'USA'.");
            Field(x => x.Name, nullable: false).Description("Name of country. For example 'United States of America'.");
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<CountryRegionType>>>>("regions")
                .Description("Country regions.")
                .ResolveAsync(async context =>
                {
                    var response = await context.GetMediator().Send(new GetRegionsQuery { CountryId = context.Source.Id });

                    return response.Regions;
                });
        }

        [Obsolete("Use the constructor without IMediator. The mediator is resolved from context.RequestServices per request.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public CountryType(IMediator mediator)
            : this()
        {
        }
    }
}
