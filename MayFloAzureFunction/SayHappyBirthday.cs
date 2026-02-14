using System;
using System.Threading.Tasks;
using MayFloAzureFunction.Data;
using MayFloAzureFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MayFloAzureFunction;

public class SayHappyBirthday
{
    private readonly ILogger _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailService _emailService;

    public SayHappyBirthday(ILoggerFactory loggerFactory, ApplicationDbContext dbContext, IEmailService emailService)
    {
        _logger = loggerFactory.CreateLogger<SayHappyBirthday>();
        _dbContext = dbContext;
        _emailService = emailService;
    }

    [Function("SayHappyBirthday")]
    public async Task Run([TimerTrigger("0 0 8 * * *"
#if DEBUG
        , RunOnStartup = true
#endif
        )] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        
        var currentDate = DateTime.Today;
        //var salesRepBirthdayToday = await _dbContext.SalesRequests.Where(x => x.CreatedDate.Day == currentDate.Day && x.CreatedDate.Month == currentDate.Month).ToAsync();
        var salesRepBirthdayToday = await _dbContext.SalesRequests
            .Where(x => x.CreatedDate.HasValue &&
                x.CreatedDate.Value.Day == currentDate.Day &&
                x.CreatedDate.Value.Month == currentDate.Month)
                    .ToListAsync();

        foreach (var salesRep in salesRepBirthdayToday)
        {
            await _emailService.SendBirthdayMessage(salesRep.Name, salesRep.Email);
        }
    }
}