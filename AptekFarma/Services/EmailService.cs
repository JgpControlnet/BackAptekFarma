namespace AptekFarma.Services
{
    using System.Diagnostics;
    using System.Net;
    using System.Net.Mail;
    using System.Text;

    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;

        public EmailService(IConfiguration configuration)
        {
            var emailConfig = configuration.GetSection("Email_Config");
            _smtpHost = emailConfig["SMTP_HOST"];
            _smtpPort = int.Parse(emailConfig["SMTP_PORT"]);
            _smtpUser = emailConfig["SMTP_USERNAME"];
            _smtpPass = emailConfig["SMTP_PASSWORD"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try {
                using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            } catch (Exception ex) {
                Debug.WriteLine("No ha sido posible enviar el correo:", ex.Message);
            }
            
        }
    }

}
