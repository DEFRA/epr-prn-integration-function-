﻿using EprPrnIntegration.Common.Models;
using EprPrnIntegration.Common.Service;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using EprPrnIntegration.Common.Client;
using Microsoft.Azure.Functions.Worker;
using AutoFixture;

namespace EprPrnIntegration.Api.UnitTests
{
    public class FetchNpwdIssuedPrnsFunctionTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<ILogger<FetchNpwdIssuedPrnsFunction>> _mockLogger;
        private readonly Mock<INpwdClient> _mockNpwdClient;
        private readonly Mock<IServiceBusProvider> _mockServiceBusProvider;
        private readonly FetchNpwdIssuedPrnsFunction _function;

        public FetchNpwdIssuedPrnsFunctionTests()
        {
               _fixture = new Fixture();
            // Mock dependencies
            _mockLogger = new Mock<ILogger<FetchNpwdIssuedPrnsFunction>>();
            _mockNpwdClient = new Mock<INpwdClient>();
            _mockServiceBusProvider = new Mock<IServiceBusProvider>();

            // Initialize the function with mocked dependencies
            _function = new FetchNpwdIssuedPrnsFunction(
                _mockLogger.Object,
                _mockNpwdClient.Object,
                _mockServiceBusProvider.Object);
        }

        [Fact]
        public async Task Run_FetchesPrnsAndPushesToQueue_Successfully()
        {
            // Arrange
            var npwdIssuedPrns = _fixture.CreateMany<NpwdPrn>().ToList();

            _mockNpwdClient.Setup(client => client.GetIssuedPrns(It.IsAny<string>()))
                           .ReturnsAsync(npwdIssuedPrns); 

            _mockServiceBusProvider.Setup(provider => provider.SendFetchedNpwdPrnsToQueue(It.IsAny<List<NpwdPrn>>()))
                                   .Returns(Task.CompletedTask);

            // Act
            await _function.Run(new TimerInfo());

            // Assert
            _mockNpwdClient.Verify(client => client.GetIssuedPrns(It.IsAny<string>()), Times.Once);
            _mockServiceBusProvider.Verify(provider => provider.SendFetchedNpwdPrnsToQueue(It.IsAny<List<NpwdPrn>>()), Times.Once);
            _mockLogger.VerifyLog(x => x.LogInformation(It.Is<string>(s => s.Contains("function started"))));
            _mockLogger.VerifyLog(x => x.LogInformation(It.Is<string>(s => s.Contains("Prns Pushed into Message"))));
            _mockLogger.VerifyLog(x => x.LogInformation(It.Is<string>(s => s.Contains("function Completed"))));
        }

        [Fact]
        public async Task Run_NoPrnsFetched_LogsWarningAndReturns()
        {
            _mockNpwdClient.Setup(client => client.GetIssuedPrns(It.IsAny<string>()))
                           .ReturnsAsync([]);

            _mockServiceBusProvider.Setup(provider => provider.SendFetchedNpwdPrnsToQueue(It.IsAny<List<NpwdPrn>>()))
                                   .Returns(Task.CompletedTask);

            // Act
            await _function.Run(new TimerInfo());

            // Assert
            _mockNpwdClient.Verify(client => client.GetIssuedPrns(It.IsAny<string>()), Times.Once);
            _mockServiceBusProvider.Verify(provider => provider.SendFetchedNpwdPrnsToQueue(It.IsAny<List<NpwdPrn>>()), Times.Never);
            _mockLogger.VerifyLog(x => x.LogWarning(It.Is<string>(s => s.Contains("No Prns Exists"))));
        }

        [Fact]
        public async Task Run_FetchPrnsThrowsException_LogsErrorAndThrows()
        {
            var exception = new HttpRequestException("Error fetching PRNs");
            _mockNpwdClient.Setup(client => client.GetIssuedPrns(It.IsAny<string>()))
                           .ThrowsAsync(exception); 

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _function.Run(new TimerInfo())); 
            _mockLogger.VerifyLog(logger => logger.LogError(It.Is<string>(s => s.Contains("Failed Get Prns from npwd"))), Times.Once);
            Assert.Equal("Error fetching PRNs", ex.Message); 
        }

        [Fact]
        public async Task Run_PushPrnsToQueueThrowsException_LogsErrorAndThrows()
        {
            // Arrange
            var npwdIssuedPrns = _fixture.CreateMany<NpwdPrn>().ToList();

            _mockNpwdClient.Setup(client => client.GetIssuedPrns(It.IsAny<string>()))
                           .ReturnsAsync(npwdIssuedPrns); 

            var exception = new Exception("Error pushing to queue");
            _mockServiceBusProvider.Setup(provider => provider.SendFetchedNpwdPrnsToQueue(It.IsAny<List<NpwdPrn>>()))
                                   .ThrowsAsync(exception); 

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _function.Run(new TimerInfo()));
            _mockLogger.VerifyLog(logger => logger.LogError(It.Is<string>(s => s.Contains("Failed pushing issued prns in message queue"))), Times.Once); 
            Assert.Equal("Error pushing to queue", ex.Message); 
        }
    }
}