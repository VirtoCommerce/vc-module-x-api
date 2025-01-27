using GraphQL.Types;
using MediatR;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Queries;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class CountryType : ExtendableGraphType<Country>
    {
        public CountryType(IMediator mediator)
        {
            Field(x => x.Id, nullable: false).Description("Code of country. For example 'USA'.");
            Field(x => x.Name, nullable: false).Description("Name of country. For example 'United States of America'.");
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<CountryRegionType>>>>("regions")
                .Description("Country regions.")
                .ResolveAsync(async context =>
                {
                    var response = await mediator.Send(new GetRegionsQuery() { CountryId = context.Source.Id });
                    return response.Regions;
                });
        }
    }
}
