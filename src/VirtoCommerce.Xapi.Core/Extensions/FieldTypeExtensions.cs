using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Resolvers;
using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Infrastructure;
using IDistributedLock = VirtoCommerce.Platform.Core.DistributedLock.IDistributedLock;

namespace VirtoCommerce.Xapi.Core.Extensions
{
    public static class FieldTypeExtensions
    {
        public static FieldBuilder<TSourceType, TReturnType> Argument<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> fieldBuilder, Type type, string name)
        {
            var arg = new QueryArgument(type)
            {
                Name = name,
            };

            fieldBuilder.FieldType.Arguments.Add(arg);

            return fieldBuilder;
        }

        [Obsolete("Use the overload that takes IDistributedLock from VirtoCommerce.Platform.Core.DistributedLock.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public static FieldBuilder<TSourceType, TReturnType> ResolveSynchronized<TSourceType, TReturnType>(
            this FieldBuilder<TSourceType, TReturnType> fieldBuilder,
            string resourceKeyPrefix,
            string resourceKeyProperty,
            IDistributedLockService distributedLockService,
            Func<IResolveFieldContext<TSourceType>, TReturnType> resolve)
        {
            fieldBuilder.FieldType.Resolver = new FuncFieldResolver<TSourceType, TReturnType>(ResolveWrapper);

            return fieldBuilder;

            TReturnType ResolveWrapper(IResolveFieldContext<TSourceType> context)
            {
                // Find resource key in context
                var resourceKey = GetResourceKey(context, resourceKeyPrefix, resourceKeyProperty);

                return string.IsNullOrEmpty(resourceKey)
                    ? resolve(context)
                    : distributedLockService.Execute(resourceKey, () => resolve(context));
            }
        }

        /// <summary>
        /// Resolves the field synchronously under the Platform distributed lock <c>{resourceKeyPrefix}:{command[resourceKeyProperty]}</c>,
        /// waiting <c>VirtoCommerce:GraphQLDistributedLock:Timeout</c> (10 seconds by default).
        /// Resolves without a lock when the command has no such property. A busy resource becomes <see cref="LockError"/>.
        /// Prefer <see cref="ResolveSynchronizedAsync{TSourceType, TReturnType}(FieldBuilder{TSourceType, TReturnType}, string, string, IDistributedLock, Func{IResolveFieldContext{TSourceType}, Task{TReturnType}})"/>:
        /// this overload blocks the resolver thread while it waits.
        /// </summary>
        public static FieldBuilder<TSourceType, TReturnType> ResolveSynchronized<TSourceType, TReturnType>(
            this FieldBuilder<TSourceType, TReturnType> fieldBuilder,
            string resourceKeyPrefix,
            string resourceKeyProperty,
            IDistributedLock distributedLock,
            Func<IResolveFieldContext<TSourceType>, TReturnType> resolve)
        {
            ArgumentNullException.ThrowIfNull(distributedLock);
            ArgumentNullException.ThrowIfNull(resolve);

            fieldBuilder.FieldType.Resolver = new FuncFieldResolver<TSourceType, TReturnType>(ResolveWrapper);

            return fieldBuilder;

            TReturnType ResolveWrapper(IResolveFieldContext<TSourceType> context)
            {
                var resourceKey = GetResourceKey(context, resourceKeyPrefix, resourceKeyProperty);
                if (string.IsNullOrEmpty(resourceKey))
                {
                    return resolve(context);
                }

                // The lock code awaits with ConfigureAwait(false), so blocking here cannot deadlock.
                using var handle = distributedLock.AcquireForGraphQLAsync(resourceKey, context).GetAwaiter().GetResult();
                return resolve(context);
            }
        }

        [Obsolete("Use the overload that takes IDistributedLock from VirtoCommerce.Platform.Core.DistributedLock.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public static FieldBuilder<TSourceType, TReturnType> ResolveSynchronizedAsync<TSourceType, TReturnType>(
            this FieldBuilder<TSourceType, TReturnType> fieldBuilder,
            string resourceKeyPrefix,
            string resourceKeyProperty,
            IDistributedLockService distributedLockService,
            Func<IResolveFieldContext<TSourceType>, Task<TReturnType>> resolve)
        {
            fieldBuilder.FieldType.Resolver = new FuncFieldResolver<TSourceType, TReturnType>(ctx => ResolveWrapperAsync(ctx));

            return fieldBuilder;

            async ValueTask<TReturnType> ResolveWrapperAsync(IResolveFieldContext<TSourceType> context)
            {
                // Find resource key in context
                var resourceKey = GetResourceKey(context, resourceKeyPrefix, resourceKeyProperty);

                return string.IsNullOrEmpty(resourceKey)
                    ? await resolve(context)
                    : await distributedLockService.ExecuteAsync(resourceKey, async () => await resolve(context));
            }
        }

        /// <summary>
        /// Resolves the field under the Platform distributed lock <c>{resourceKeyPrefix}:{command[resourceKeyProperty]}</c>,
        /// waiting <c>VirtoCommerce:GraphQLDistributedLock:Timeout</c> (10 seconds by default).
        /// Resolves without a lock when the command has no such property. A busy resource becomes <see cref="LockError"/>.
        /// </summary>
        public static FieldBuilder<TSourceType, TReturnType> ResolveSynchronizedAsync<TSourceType, TReturnType>(
            this FieldBuilder<TSourceType, TReturnType> fieldBuilder,
            string resourceKeyPrefix,
            string resourceKeyProperty,
            IDistributedLock distributedLock,
            Func<IResolveFieldContext<TSourceType>, Task<TReturnType>> resolve)
        {
            ArgumentNullException.ThrowIfNull(distributedLock);
            ArgumentNullException.ThrowIfNull(resolve);

            fieldBuilder.FieldType.Resolver = new FuncFieldResolver<TSourceType, TReturnType>(ctx => ResolveWrapperAsync(ctx));

            return fieldBuilder;

            async ValueTask<TReturnType> ResolveWrapperAsync(IResolveFieldContext<TSourceType> context)
            {
                var resourceKey = GetResourceKey(context, resourceKeyPrefix, resourceKeyProperty);
                if (string.IsNullOrEmpty(resourceKey))
                {
                    return await resolve(context);
                }

                await using var handle = await distributedLock.AcquireForGraphQLAsync(resourceKey, context);
                return await resolve(context);
            }
        }

        private static string GetResourceKey<TSourceType>(IResolveFieldContext<TSourceType> context, string resourceKeyPrefix, string resourceKeyProperty)
        {
            var command = context.GetArgument<IDictionary<string, object>>("command");

            if (command == null ||
                !command.TryGetValue(resourceKeyProperty, out var value) ||
                value is not string resourceKey ||
                string.IsNullOrEmpty(resourceKey))
            {
                return null;
            }

            return $"{resourceKeyPrefix}:{resourceKey}";
        }
    }
}
