using System;
using FluentAssertions;
using GraphQL;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.Xapi.Core.Extensions;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Extensions
{
    public class ResolveFieldContextExtensionsTests
    {
        [Fact]
        public void GetMediator_RequestServicesPopulated_ReturnsMediatorFromRequestScope()
        {
            var mediatorMock = new Mock<IMediator>();
            var services = new ServiceCollection();
            services.AddSingleton(mediatorMock.Object);
            var provider = services.BuildServiceProvider();

            var context = new ResolveFieldContext
            {
                RequestServices = provider,
            };

            var result = context.GetMediator();

            result.Should().BeSameAs(mediatorMock.Object);
        }

        [Fact]
        public void GetMediator_RequestServicesNull_Throws()
        {
            var context = new ResolveFieldContext
            {
                RequestServices = null,
            };

            Action act = () => context.GetMediator();

            act.Should().Throw<InvalidOperationException>().WithMessage("*RequestServices*");
        }
    }
}
