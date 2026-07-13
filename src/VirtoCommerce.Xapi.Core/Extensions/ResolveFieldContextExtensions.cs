using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GraphQL;
using GraphQL.Execution;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.CustomerModule.Core.Extensions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Queries;

namespace VirtoCommerce.Xapi.Core.Extensions
{
    public static class ResolveFieldContextExtensions
    {
        extension(IResolveFieldContext context)
        {
            /// <summary>
            /// Resolves <see cref="IMediator"/> from the per-request DI scope (<see cref="IResolveFieldContext.RequestServices"/>).
            /// GraphQL types and schema builders are built once and act as singletons, so <see cref="IMediator"/> must never be
            /// constructor-injected there: a mediator captured at construction time is bound to the root service provider, and any
            /// Scoped dependency of a handler it dispatches to would fail to resolve (or be silently promoted to a singleton).
            /// Call this from inside a resolver/DataLoader closure, which runs per request, never at construction time.
            /// </summary>
            public IMediator GetMediator()
            {
                return context.RequestServices?.GetRequiredService<IMediator>()
                       ?? throw new InvalidOperationException(
                           "Cannot resolve IMediator: IResolveFieldContext.RequestServices is null. " +
                           "Resolvers that dispatch requests require a request-scoped service provider (ExecutionOptions.RequestServices); " +
                           "the GraphQL HTTP middleware populates it - in tests, set RequestServices on the ResolveFieldContext explicitly.");
            }

            /// <summary>
            /// Get value from user context
            /// </summary>
            /// <typeparam name="T">Type of T</typeparam>
            /// <param name="key">Search key</param>
            /// <param name="defaultValue">Default return if value not founded in UserContext</param>
            /// <returns>Return value of type <typeparamref name="T"/> from UserContext or <paramref name="defaultValue"/></returns>
            /// <exception cref="ArgumentNullException"></exception>
            public T GetValue<T>(string key, T defaultValue)
            {
                ArgumentNullException.ThrowIfNull(context);

                return context.UserContext.TryGetValue(key, out var value)
                    ? CastValue(value, defaultValue)
                    : defaultValue;

                static T CastValue(object value, T defaultValue)
                {
                    return value is ArgumentValue argumentValue ? (T)argumentValue.Value : CastValueAsTyped(value, defaultValue);

                    static T CastValueAsTyped(object value, T defaultValue)
                    {
                        return value is T typedObject ? typedObject : defaultValue;
                    }
                }
            }

            public T GetValue<T>(string key)
            {
                return context.GetValue(key, default(T));
            }

            public T GetArgument<T>(string name) where T : class
            {
                var type = GenericTypeHelper.GetActualType<T>();
                var command = context.GetArgument(type, name) as T;

                return command;
            }

            public bool IsAuthenticated()
            {
                return context.GetCurrentPrincipal()?.Identity?.IsAuthenticated == true;
            }

            public string GetCurrentUserId()
            {
                return context.GetCurrentPrincipal()?.GetCurrentUserId();
            }

            public string GetCurrentOrganizationId()
            {
                return context.GetCurrentPrincipal()?.GetCurrentOrganizationId();
            }

            public ClaimsPrincipal GetCurrentPrincipal()
            {
                return ((GraphQLUserContext)context.UserContext).User;
            }

            public T GetArgumentOrValue<T>(string key)
            {
                return context.GetArgument<T>(key) ?? context.GetValue<T>(key);
            }

            //PT-1606:  Need to check what there is no any alternative way to access to the original request arguments in sub selection
            public void CopyArgumentsToUserContext()
            {
                if (!context.Arguments.IsNullOrEmpty())
                {
                    foreach (var pair in context.Arguments)
                    {
                        context.UserContext.TryAdd(pair.Key, pair.Value);
                    }
                }

                // try to copy "command" variables from parent context
                var commandVariables = context.Variables?.FirstOrDefault(x => x.Name == "command");
                if (commandVariables is { Value: Dictionary<string, object> variables })
                {
                    foreach (var pair in variables)
                    {
                        context.UserContext.TryAdd(pair.Key, pair.Value);
                    }
                }
            }

            public void SetExpandedObjectGraph<T>(T value)
            {
                var entities = value.GetFlatObjectsListWithInterface<IEntity>();

                foreach (var entity in entities)
                {
                    if (!string.IsNullOrEmpty(entity.Id))
                    {
                        context.UserContext.TryAdd(entity.Id, value);
                    }
                }

                var valueObjects = value.GetFlatObjectsListWithInterface<IValueObject>();
                foreach (var @object in valueObjects)
                {
                    if (@object is ValueObject valueObject)
                    {
                        context.UserContext.TryAdd(valueObject.GetCacheKey(), value);
                    }
                }
            }

            public TResult GetValueForSource<TResult>()
            {
                ArgumentNullException.ThrowIfNull(context);

                var result = context.Source switch
                {
                    IEntity entity => context.GetValue<TResult>(entity.Id),
                    ValueObject valueObject => context.GetValue<TResult>(valueObject.GetCacheKey()),
                    _ => default
                };

                return result;
            }

            public void SetCurrencies(IEnumerable<Currency> currencies, string cultureName)
            {
                ArgumentNullException.ThrowIfNull(currencies);

                var currencyList = currencies as ICollection<Currency> ?? currencies.ToList();
                var currenciesWithCulture = currencyList.Select(x => currencyList.GetCurrencyForLanguage(x.Code, cultureName)).ToArray();

                context.UserContext["allCurrencies"] = currenciesWithCulture;
            }

            public void SetCurrency(Currency currency)
            {
                ArgumentNullException.ThrowIfNull(currency);

                context.UserContext["currencyCode"] = currency.Code;
            }

            public T GetDynamicPropertiesQuery<T>() where T : IDynamicPropertiesQuery
            {
                var result = AbstractTypeFactory<T>.TryCreateInstance();
                result.CultureName = context.GetCultureName();

                return result;
            }

            public string GetCultureName()
            {
                return context.GetArgumentOrValue<string>(Constants.CultureName);
            }
        }

        public static Currency GetCurrencyByCode<T>(this IResolveFieldContext<T> userContext, string currencyCode)
        {
            var allCurrencies = userContext.GetValue<IEnumerable<Currency>>("allCurrencies");
            var result = allCurrencies?.FirstOrDefault(x => x.Code.EqualsIgnoreCase(currencyCode))
                         ?? throw new OperationCanceledException($"The currency with code '{currencyCode}' is not registered");

            return result;
        }
    }
}
