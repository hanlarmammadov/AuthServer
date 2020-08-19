using AuthServer.Common.Messages;
using AuthServer.SecurityTokens.Api.Controllers;
using AuthServer.SecurityTokens.Models;
using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using AuthServer.SecurityTokens.Services.Queries.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.SecurityTokens.Api.UnitTests
{
    [TestFixture]
    public class RefreshTokenControllerTests
    {
        [Test]
        public void Controller_Class_Has_Correct_Route_Attribute()
        {
            Assert.IsTrue(AttrHelper.ClassHasAttr<ShortTokenController, RouteAttribute>());
            var routeStr = AttrHelper.GetClassAttrValue<ShortTokenController, RouteAttribute, string>(x => x.Template);
            Assert.AreEqual("api/v1/rtoken", routeStr);
        }

        [Test]
        public void Ctor_When_Called_Sets_Logger()
        {
            // Arrange
            ILogger logger = new Mock<ILogger>().Object;

            // Act
            RefreshTokenController controller = new RefreshTokenController(logger);

            // Assert
            Assert.AreEqual(logger, controller.Logger);
        }

        [Test]
        public async Task GetTokensInfoForAccount_When_Called_Returns_Ok_With_List_Of_All_Tokens_For_Accound()
        {
            // Arrange
            ILogger logger = new Mock<ILogger>().Object;
            RefreshTokenController controller = new RefreshTokenController(logger);
            string accountId = "some id";
            // Query mock
            var queryMock = new Mock<IGetAllTokensForAccountQuery>();
            var accountTokens = new List<AccountTokenModel>() { new AccountTokenModel() { }, new AccountTokenModel() { } };
            queryMock.Setup(x => x.Execute(accountId)).ReturnsAsync(accountTokens);

            // Act
            var actionResult = await controller.GetTokensInfoForAccount(accountId, queryMock.Object);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            Assert.IsInstanceOf<Page<AccountTokenModel>>(okResult.Value);
            var resultPage = (Page<AccountTokenModel>)okResult.Value;
            Assert.AreEqual(accountTokens, resultPage.List);
            Assert.AreEqual(resultPage.List.Count(), resultPage.TotalItems);
        }
        [Test]
        public async Task GetTokensInfoForAccount_When_Called_With_Null_AccountId_Returns_BadRequest()
        {
            // Arrange
            ILogger logger = new Mock<ILogger>().Object;
            RefreshTokenController controller = new RefreshTokenController(logger);
            var query = new Mock<IGetAllTokensForAccountQuery>().Object;

            // Act
            var actionResult = await controller.GetTokensInfoForAccount(null as string, query);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actionResult);
            var badRequestResult = (BadRequestObjectResult)actionResult;
            Assert.IsInstanceOf<Message>(badRequestResult.Value);
            var errorMessage = (Message)badRequestResult.Value;
            Assert.AreEqual("Something went wrong.", errorMessage.Text);
        }
        [Test]
        public async Task GetTokensInfoForAccount_When_Exception_Is_Thrown_Logs_Exception_And_Returns_BadRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            RefreshTokenController controller = new RefreshTokenController(loggerMock.Object);
            // Query mock
            var queryMock = new Mock<IGetAllTokensForAccountQuery>();
            var exception = new Exception();
            queryMock.Setup(x => x.Execute(It.IsAny<string>())).ThrowsAsync(exception);

            // Act
            var actionResult = await controller.GetTokensInfoForAccount("some id", queryMock.Object);

            // Assert 
            loggerMock.Verify(x => x.LogAsync(It.Is<Log>(l => l.Type == LogType.Error && l.Context == "RefreshTokenController.GetTokensInfoForAccount"), default), Times.Once);
            Assert.IsInstanceOf<BadRequestObjectResult>(actionResult);
            var badRequestResult = (BadRequestObjectResult)actionResult;
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOf<Message>(badRequestResult.Value);
            var errorMessage = (Message)badRequestResult.Value;
            Assert.AreEqual("Something went wrong.", errorMessage.Text);
        } 
        [Test]
        public void GetTokensInfoForAccount_Method_Is_Http_Get()
        {
            Assert.IsTrue(AttrHelper.MethodHasAttr<RefreshTokenController, HttpGetAttribute>("GetTokensInfoForAccount"));
            var routeStr = AttrHelper.GetMethodAttrValue<RefreshTokenController, HttpGetAttribute, string>("GetTokensInfoForAccount", x => x.Template);
            Assert.AreEqual("account/{accountId}", routeStr);
        }

    }
}
