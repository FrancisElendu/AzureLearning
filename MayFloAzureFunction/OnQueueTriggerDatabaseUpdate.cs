using System;
using Azure.Storage.Queues.Models;
using MayFloAzureFunction.Data;
using MayFloAzureFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MayFloAzureFunction;

public class OnQueueTriggerDatabaseUpdate
{
    private readonly ILogger<OnQueueTriggerDatabaseUpdate> _logger;
    private readonly ApplicationDbContext _dbContext;

    public OnQueueTriggerDatabaseUpdate(ILogger<OnQueueTriggerDatabaseUpdate> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [Function(nameof(OnQueueTriggerDatabaseUpdate))]
    public void Run([QueueTrigger("SalesRequestInBound")] QueueMessage message)  //note , Connection = "" is removed from the parameter list because we have a connection to the storage account in localsetting
    {
        var messageBody = message.Body.ToString();
        var salesRequest = JsonConvert.DeserializeObject<SalesRequest>(messageBody);
        if(salesRequest != null)
        {
            salesRequest.Status = "";  //will remove this once the fix for status not being null is put in place
            _dbContext.Add(salesRequest);
            _dbContext.SaveChanges();
        }
        else
        {
            _logger.LogWarning("Message body could not be deserialized");
        }
        _logger.LogInformation("C# Queue trigger function processed: {messageText}", message.MessageText);
    }
}