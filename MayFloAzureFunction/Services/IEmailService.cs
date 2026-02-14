
namespace MayFloAzureFunction.Services
{
    public interface IEmailService
    {
        Task SendBirthdayMessage(string name, string email);
    }
}