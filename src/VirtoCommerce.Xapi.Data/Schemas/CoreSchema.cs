using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using MediatR;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Queries;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Xapi.Data.Schemas
{
    public class CoreSchema : ISchemaBuilder
    {
        private readonly IMediator _mediator;

        public CoreSchema(IMediator mediator)
        {
            _mediator = mediator;
        }

        public void Build(ISchema schema)
        {
            #region countries query

#pragma warning disable S125 // Sections of code should not be commented out
            /*                         
               query {
                     countries
               }                         
            */
#pragma warning restore S125 // Sections of code should not be commented out

            #endregion

            _ = schema.Query.AddField(new FieldType
            {
                Name = "countries",
                Type = GraphTypeExtensionHelper.GetActualComplexType<NonNullGraphType<ListGraphType<NonNullGraphType<CountryType>>>>(),
                Resolver = new FuncFieldResolver<object>(async context =>
                {
                    //var fields = context.SubFields.Values.GetAllNodesPaths(context).ToArray();

                    var result = await _mediator.Send(new GetCountriesQuery());

                    return result.Countries;
                })
            });

            #region regions query

#pragma warning disable S125 // Sections of code should not be commented out
            /*                         
               query {
                     regions(countryId: "country code")
               }                         
            */
#pragma warning restore S125 // Sections of code should not be commented out

            #endregion

            _ = schema.Query.AddField(new FieldType
            {
                Name = "regions",
                Arguments = new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "countryId" }),
                Type = GraphTypeExtensionHelper.GetActualComplexType<NonNullGraphType<ListGraphType<NonNullGraphType<CountryRegionType>>>>(),
                Resolver = new FuncFieldResolver<object>(async context =>
                {
                    var result = await _mediator.Send(new GetRegionsQuery
                    {
                        CountryId = context.GetArgument<string>("countryId"),
                    });

                    return result.Regions;
                })
            });

#pragma warning disable S125 // Sections of code should not be commented out

        }
    }
}
