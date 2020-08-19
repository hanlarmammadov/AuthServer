using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;

namespace AuthServer.SecurityTokens.Api.UnitTests
{
    [TestFixture]
    public class StartupTests
    { 
        [Test]
        [Ignore("Startup class should be refactored first.")]
        public void ConfigureServices_IGenerateRefreshTokenCommand_Registered_With_DI_Container()
        {
            // Arrange
            var servicesCollectionMock = new Mock<IServiceCollection>();
            Startup startup = new Startup(null);

            // Act
            startup.ConfigureServices(servicesCollectionMock.Object);

            // Assert
            servicesCollectionMock.Verify(x => x.AddTransient(typeof(IGenerateRefreshTokenCommand), It.IsAny<Func<IServiceProvider, object>>()), Times.Once);
        }
    }
}