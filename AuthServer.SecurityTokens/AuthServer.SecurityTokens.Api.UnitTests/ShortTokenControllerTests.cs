using AuthServer.Common.Messages;
using AuthServer.SecurityTokens.Api.Controllers;
using AuthServer.SecurityTokens.Models;
using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.SecurityTokens.Api.UnitTests
{
    [TestFixture]
    public class ShortTokenControllerTests
    {
        [Test]
        public void Controller_Class_Has_Correct_Route_Attribute()
        {
            Assert.IsTrue(AttrHelper.ClassHasAttr<ShortTokenController, RouteAttribute>());
            var routeStr = AttrHelper.GetClassAttrValue<ShortTokenController, RouteAttribute, string>(x => x.Template);
            Assert.AreEqual("api/v1/token", routeStr);
        }

        [Test]
        public void Ctor_When_Called_Sets_Logger()
        {
            // Arrange
            ILogger logger = new Mock<ILogger>().Object;

            // Act
            ShortTokenController shortTokenController = new ShortTokenController(logger);

            // Assert
            Assert.AreEqual(logger, shortTokenController.Logger);
        }

        [Test]
        public async Task Create_When_Called_Returns_Short_Token()
        {
            // Arrange
            ILogger logger = new Mock<ILogger>().Object;
            ShortTokenController shortTokenController = new ShortTokenController(logger);
            CreateSTokenModel model = new CreateSTokenModel() { RToken = "some refresh token" };
            var generateShortTokenCommandMock = new Mock<IGenerateShortTokenCommand>();
            TokenResult tokenResult = new TokenResult("", "", DateTime.Now);
            generateShortTokenCommandMock.Setup(x => x.Execute(It.IsAny<string>())).ReturnsAsync(tokenResult);

            // Act
            var result = await shortTokenController.Create(model, generateShortTokenCommandMock.Object);

            // Assert 
            Assert.IsInstanceOf<CreatedResult>(result);
            var createdResult = (CreatedResult)result;
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(tokenResult, createdResult.Value);
        }
        [Test]
        public async Task Create_When_Called_With_Null_Model_Returns_BadRequestObjectResult()
        {
            // Arrange
            ILogger logger = new Mock<ILogger>().Object;
            ShortTokenController shortTokenController = new ShortTokenController(logger);
            var generateShortTokenCommandMock = new Mock<IGenerateShortTokenCommand>();
            TokenResult tokenResult = new TokenResult("", "", DateTime.Now);
            generateShortTokenCommandMock.Setup(x => x.Execute(It.IsAny<string>())).ReturnsAsync(tokenResult);

            // Act
            var result = await shortTokenController.Create(null as CreateSTokenModel, generateShortTokenCommandMock.Object);

            // Assert 
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOf<Message>(badRequestResult.Value);
        }
        [Test]
        public async Task Create_Unhandled_Exception_From_Command_Is_Catched_And_Logged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            ShortTokenController shortTokenController = new ShortTokenController(loggerMock.Object);
            var generateShortTokenCommandMock = new Mock<IGenerateShortTokenCommand>();
            var exFromCommand = new Exception();
            generateShortTokenCommandMock.Setup(x => x.Execute(It.IsAny<string>())).ThrowsAsync(exFromCommand);
            CreateSTokenModel model = new CreateSTokenModel() { RToken = "some refresh token" };

            // Act
            var result = await shortTokenController.Create(model, generateShortTokenCommandMock.Object);

            // Assert 
            loggerMock.Verify(x => x.LogAsync(It.Is<Log>(l => l.Type == LogType.Error && l.Context == "ShortTokenController.Create"), default), Times.Once);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOf<Message>(badRequestResult.Value);
        }

        [Test]
        public void Create_Method_Is_Http_Post()
        {
            Assert.IsTrue(AttrHelper.MethodHasAttr<ShortTokenController, HttpPostAttribute>("Create"));
            var routeStr = AttrHelper.GetMethodAttrValue<ShortTokenController, HttpPostAttribute, string>("Create", x => x.Template);
            Assert.AreEqual("", routeStr);
        }
    }
}
