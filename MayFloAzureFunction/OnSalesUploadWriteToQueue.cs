using MayFloAzureFunction.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MayFloAzureFunction;

public class OnSalesUploadWriteToQueue
{
    private readonly ILogger<OnSalesUploadWriteToQueue> _logger;

    public OnSalesUploadWriteToQueue(ILogger<OnSalesUploadWriteToQueue> logger)
    {
        _logger = logger;
    }

    [Function("OnSalesUploadWriteToQueue")]
    [QueueOutput("SalesRequestInBound")] //note , Connection = "AzureWebJobsStorage" is removed from the parameter list because we have a connection to the storage account in localsetting
    public async Task<SalesRequest> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        var requestBody = await  new StreamReader(req.Body).ReadToEndAsync();
        var salesRequestData = JsonConvert.DeserializeObject<SalesRequest>(requestBody);
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        //return salesRequestData != null
        //    ? new OkObjectResult(salesRequestData)
        //    : new BadRequestObjectResult("Please pass a valid sales request in the request body");
        return salesRequestData ?? new SalesRequest();
    }
}