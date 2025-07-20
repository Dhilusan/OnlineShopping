using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShopping.Utility
{
    public class EmailSender : IEmailSender {
        public Task SendEmailAsync(string email, string subject, string htmlMessage) {

            //MailMessage message = new MailMessage();
            //message.From = new MailAddress("dhilusan99@gmail.com", "Online Shopping");
            //message.To.Add(email);
            //message.Subject = subject;
            //message.IsBodyHtml = true;
            //message.Body = "Online Shopping";
            //SmtpClient client = new SmtpClient("smtp.gmail.com");
            //client.EnableSsl = true;
            //client.Port = 587;
            //client.UseDefaultCredentials = true;
            //client.Credentials = new System.Net.NetworkCredential("dhilusan99@gmail.com", "lkyfnzgieflvnlsc");
            //client.Send(message);

            return Task.CompletedTask;
        }

        public void SendEmail(string email, string subject, string htmlMessage)
        {

            MailMessage message = new MailMessage();
            message.From = new MailAddress("dhilusan99@gmail.com", "Online Shopping");
            message.To.Add(email);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = "Online Shopping";
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.EnableSsl = true;
            client.Port = 587;
            client.UseDefaultCredentials = true;
            client.Credentials = new System.Net.NetworkCredential("dhilusan99@gmail.com", "lkyfnzgieflvnlsc");
            client.Send(message);
        }
    }
}
