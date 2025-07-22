using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Helpers;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class ExtendableInputObjectGraphType : ExtendableInputObjectGraph<object>
    {
    }

    public class ExtendableInputObjectGraph<TSourceType> : InputObjectGraphType<TSourceType>
    {
        public override FieldType AddField(FieldType fieldType)
        {
            fieldType.Type = GraphTypeExtensionHelper.GetActualComplexType(fieldType.Type);

            return base.AddField(fieldType);
        }
    }

    public class ExtendableGraphType<TSourceType> : ObjectGraphType<TSourceType>
    {
        public FieldType ExtendableField<TGraphType>(
            string name,
            string description = null,
            QueryArguments arguments = null,
            Func<IResolveFieldContext<TSourceType>, object> resolve = null,
            string deprecationReason = null)
            where TGraphType : IGraphType
        {
            return AddField(new FieldType
            {
                Name = name,
                Description = description,
                DeprecationReason = deprecationReason,
                Type = GraphTypeExtensionHelper.GetActualComplexType<TGraphType>(),
                Arguments = arguments,
                Resolver = resolve != null
                    ? new FuncFieldResolver<TSourceType, object>(context =>
                        {
                            context.CopyArgumentsToUserContext();
                            return resolve(context);
                        })
                    : null
            });
        }

        public FieldType ExtendableFieldAsync<TGraphType>(
          string name,
          string description = null,
          QueryArguments arguments = null,
          Func<IResolveFieldContext<TSourceType>, Task<object>> resolve = null,
          string deprecationReason = null)
          where TGraphType : IGraphType
        {
            return AddField(new FieldType
            {
                Name = name,
                Description = description,
                DeprecationReason = deprecationReason,
                Type = GraphTypeExtensionHelper.GetActualComplexType<TGraphType>(),
                Arguments = arguments,
                Resolver = resolve != null
                    ? new FuncFieldResolver<TSourceType, object>(context =>
                    {
                        context.CopyArgumentsToUserContext();
                        return new ValueTask<object>(resolve(context));
                    })
                    : null
            });
        }

        public void LocalizedField(Expression<Func<TSourceType, string>> expression, SettingDescriptor descriptor, ILocalizableSettingService localizableSettingService, bool nullable)
        {
            // Add original field
            Field(expression, nullable);

            // Add localized field
            var getValue = expression.Compile();
            var localizedFieldName = expression.NameOf().ToCamelCase() + "DisplayValue";

            if (nullable)
            {
                Field<StringGraphType>(localizedFieldName).ResolveAsync(async context =>
                {
                    return await localizableSettingService.TranslateAsync(getValue(context.Source), descriptor.Name, context.GetCultureName());
                });
            }
            else
            {
                Field<NonNullGraphType<StringGraphType>>(localizedFieldName).ResolveAsync(async context =>
                {
                    return await localizableSettingService.TranslateAsync(getValue(context.Source), descriptor.Name, context.GetCultureName());
                });
            }
        }
    }
}
