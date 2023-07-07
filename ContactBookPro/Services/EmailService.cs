using ContactBookPro.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;



namespace ContactBookPro.Services
{
    public class EmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        //go to the config file and get its settings, put the values into the mailSettings and pass them in here
        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // look in the local, if it's null look in the environment
            var emailSender = _mailSettings.Email ?? Environment.GetEnvironmentVariable("Email");

            if (string.IsNullOrEmpty(emailSender))
            {
                throw new ArgumentNullException(nameof(emailSender), "Email sender address is not configured.");
            }

            MimeMessage newEmail = new MimeMessage();
            //MimeMessage newEmail = new();

            // we have a new email object. We set the sender
            newEmail.Sender = MailboxAddress.Parse(emailSender);

            //set where it sends to
            foreach(var emailAddress in email.Split(";"))
            {
                newEmail.To.Add(MailboxAddress.Parse(emailAddress));
            }
            //set the subject
            newEmail.Subject = subject;
            // format the message. We are passing in the html that was sent
            //make an email body from the mimekit
            BodyBuilder emailBody = new();
            emailBody.HtmlBody = htmlMessage;
            // add it to email and set it corretly
            newEmail.Body = emailBody.ToMessageBody();

            //log in
            using SmtpClient smtpClient = new();
            try
            {
                //try and login to gmail, get the host, password and the port
                var host = _mailSettings.Host ?? Environment.GetEnvironmentVariable("Host");
                // If mailSettings port is not equla to 0, look locally, else look in the environment. port comes in as int.
                var port = _mailSettings.Port !=0 ? _mailSettings.Port : int.Parse(Environment.GetEnvironmentVariable("Port")!);
                var password = _mailSettings.Password ?? Environment.GetEnvironmentVariable("Password");
                // try to connect, include security/encryption
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                //authenticate with our password
                await smtpClient.AuthenticateAsync(emailSender, password);
                //send email
                await smtpClient.SendAsync(newEmail);
                //disconnect
                await smtpClient.DisconnectAsync(true);

            }
            catch(Exception ex)
            {
                var error = ex.Message;
                throw;
            }

        }
    }
}
