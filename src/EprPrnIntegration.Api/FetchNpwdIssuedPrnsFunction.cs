using EprPrnIntegration.Api.Models;
using EprPrnIntegration.Common.Client;
using EprPrnIntegration.Common.Mappers;
using EprPrnIntegration.Common.Configuration;
using EprPrnIntegration.Common.Models;
using EprPrnIntegration.Common.RESTServices.BackendAccountService.Interfaces;
using EprPrnIntegration.Common.Service;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace EprPrnIntegration.Api
{
    public class FetchNpwdIssuedPrnsFunction
    {
        private readonly ILogger<FetchNpwdIssuedPrnsFunction> _logger;
        private readonly INpwdClient _npwdClient;
        private readonly IServiceBusProvider _serviceBusProvider;
        private readonly IEmailService _emailService;
        private readonly IOptions<FeatureManagementConfiguration> _featureConfig;
        private readonly IOrganisationService _organisationService;
        private readonly IPrnService _prnService;
        private readonly IValidator<NpwdPrn> _validator;
        public FetchNpwdIssuedPrnsFunction(ILogger<FetchNpwdIssuedPrnsFunction> logger, INpwdClient npwdClient, IServiceBusProvider serviceBusProvider, IEmailService emailService, IOrganisationService organisationService, IPrnService prnService, IValidator<NpwdPrn> validator)
        {
            _logger = logger;
            _npwdClient = npwdClient;
            _serviceBusProvider = serviceBusProvider;
            _emailService = emailService;
            _featureConfig = featureConfig;
            _organisationService = organisationService;
            _prnService = prnService;
            _validator = validator;
        }

        [Function("FetchNpwdIssuedPrnsFunction")]
        //public async Task Run([TimerTrigger("%FetchNpwdIssuedPrns:Schedule%")] TimerInfo timerInfo)
        public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            bool isOn = _featureConfig.Value.RunIntegration ?? false;
            if (!isOn)
            {
                _logger.LogInformation("FetchNpwdIssuedPrnsFunction function is disabled by feature flag");
                return;
            }

            _logger.LogInformation($"FetchNpwdIssuedPrnsFunction function started at: {DateTime.UtcNow}");

            var lastFetched = await GetLastFetchedTimeAsync();
            var currentRunDateTime = DateTime.UtcNow;

            var filter = "(EvidenceStatusCode eq 'EV-CANCEL' or EvidenceStatusCode eq 'EV-AWACCEP' or EvidenceStatusCode eq 'EV-AWACCEP-EPR')";
            if (lastFetched != null)
            {
                filter = $@"""{filter} and ((StatusDate ge {lastFetched.Value.ToUniversalTime():O} and StatusDate lt {currentRunDateTime.ToUniversalTime():O}) 
                            or (ModifiedOn ge {lastFetched.Value.ToUniversalTime():O} and ModifiedOn lt {currentRunDateTime.ToUniversalTime():O}))""";
            }

            var npwdIssuedPrns = new List<NpwdPrn>();
            try
            {
                npwdIssuedPrns = await _npwdClient.GetIssuedPrns(filter);
                if (npwdIssuedPrns == null || npwdIssuedPrns.Count == 0)
                {
                    _logger.LogWarning($"No Prns Exists in npwd for filter {filter}");
                    return;
                }
                _logger.LogInformation("Total: {Count} fetched from Npwd with filter {filter}", npwdIssuedPrns.Count, filter);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Failed Get Prns from npwd for filter {filter} with exception {ex}", filter, ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed Get Prns method for filter {filter} with exception {ex}", filter, ex.Message);
                throw;
            }

            try
            {
                await _serviceBusProvider.SendFetchedNpwdPrnsToQueue(npwdIssuedPrns);
                _logger.LogInformation("Issued Prns Pushed into Message Queue");
                SetLastFetchTime(currentRunDateTime);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed pushing issued prns in message queue with exception: {ex}", ex);
                throw;
            }

            try
            {
                await ProcessIssuedPrnsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed fetching prns from the queue with exception: {ex}", ex);
                throw;
            }
            
            _logger.LogInformation($"FetchNpwdIssuedPrnsFunction function Completed at: {DateTime.UtcNow}");
        }

        private async Task ProcessIssuedPrnsAsync()
        {
            while (true)
            {
                var messages = await _serviceBusProvider.ReceiveFetchedNpwdPrnsFromQueue();

                if (!messages.Any())
                {
                    _logger.LogInformation("No messages found in the queue. Exiting the processing loop.");
                    break;
                }

                foreach (var message in messages)
                {
                    var messageContent = JsonSerializer.Deserialize<NpwdPrn>(message.Body.ToString());

                    try
                    {
                        _logger.LogInformation("Validating message with Id: {MessageId}", message.MessageId);

                        var validationResult = _validator.Validate(messageContent!);

                        if (validationResult.IsValid)
                        {
                            try
                            {
                                _logger.LogInformation("Validation passed for message Id: {MessageId}. Processing the PRN.", message.MessageId);

                                // Save to PRN DB
                                var request = NpwdPrnToSavePrnDetailsRequestMapper.Map(messageContent!);
                                await _prnService.SavePrn(request);
                                _logger.LogInformation("Successfully saved PRN details for EvidenceNo: {EvidenceNo}", request.EvidenceNo);

                                // Get list of producers
                                var producerEmails = await _organisationService.GetPersonEmailsAsync(messageContent!.IssuedToEPRId!, CancellationToken.None);
                                _logger.LogInformation("Fetched {ProducerCount} producers for OrganisationId: {EPRId}", producerEmails.Count, messageContent.IssuedToEPRId);

                                var producers = new List<ProducerEmail>();
                                foreach (var producer in producerEmails)
                                {
                                    var producerEmail = new ProducerEmail
                                    { //TODO: Confirm if the fields mapped are ok
                                        EmailAddress = producer.Email,// "venkata.rangala.external@eviden.com",
                                        FirstName = producer.FirstName, // "Venkat",
                                        LastName = producer.LastName, // "Rangala",
                                        NameOfExporterReprocessor = request.ReprocessorAgency!,
                                        NameOfProducerComplianceScheme = request.RecoveryProcessCode!,
                                        PrnNumber = request.EvidenceNo!,
                                        Material = request.EvidenceMaterial!,
                                        Tonnage = Convert.ToDecimal(request.EvidenceTonnes),
                                        IsPrn = true //How about PERN Check?
                                    };
                                    producers.Add(producerEmail);
                                }

                                _logger.LogInformation("Sending email notifications to {ProducerCount} producers.", producers.Count);
                                _emailService.SendEmailsToProducers(producers, messageContent!.IssuedToEPRId!);

                                _logger.LogInformation("Successfully processed and sent emails for message Id: {MessageId}", message.MessageId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing message Id: {MessageId}. Adding it back to the queue.", message.MessageId);
                                await _serviceBusProvider.SendMessageBackToFetchPrnQueue(message);
                                continue;
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Validation failed for message Id: {MessageId}. Sending to error queue.", message.MessageId);
                            await _serviceBusProvider.SendMessageToErrorQueue(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error while processing message Id: {MessageId}.", message.MessageId);
                        throw;
                    }
                }
            }
        }

        private void SetLastFetchTime(DateTime currentRunDateTime)
        {
            _logger.LogInformation($"Setting CurrentRunDateTime For Future lastRun {currentRunDateTime:O}");
            //Pending pushing last run logic is done by Ehsan 
        }

        private async Task<DateTime?> GetLastFetchedTimeAsync()
        {
            _logger.LogInformation("Getting Future lastRun DateTime to fetch data");
            //pending setting null as the logic to pull lastrun is going to be done by Ehsan
            //Make sure you set in utc format and parse in utc format
            return null;
        }
    }
}
