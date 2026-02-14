using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MayFloAzureFunction.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendBirthdayMessage(string name, string email)
        {
            var subject = "Happy Birthday 🎉";

            var body = $"""
            Greetings, {name},

            We would like to wish you a very happy birthday. 🎂

            Sincerely,
            Francis
            """;

            await SendMessage(email, subject, body);
        }


        private async Task SendMessage(string recipientEmail, string subject, string body)
        {
            var ourEmail = _configuration.GetValue<string>("EMAIL_SETTINGS:EMAIL");
            var password = _configuration.GetValue<string>("EMAIL_SETTINGS:PASSWORD");
            var host = _configuration.GetValue<string>("EMAIL_SETTINGS:HOST");
            var port = _configuration.GetValue<int>("EMAIL_SETTINGS:PORT");

            var smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(ourEmail, password);

            var message = new MailMessage(ourEmail!, recipientEmail, subject, body);
            await smtpClient.SendMailAsync(message);
        }
    }
}
