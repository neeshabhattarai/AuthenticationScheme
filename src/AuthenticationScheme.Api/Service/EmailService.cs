using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace AuthenticationScheme.Service;
public interface IEmailService
{
    Task SendEmail(string to, string subject, string body);
}

public class EmailService:IEmailService
{
 private readonly SmtpSetting _smtpSetting;

 public EmailService(IOptions<SmtpSetting> smtpSetting)
 {
  _smtpSetting = smtpSetting.Value;
 }
 
 public async Task SendEmail(string to, string subject, string body)
 {
  var message = new MailMessage(_smtpSetting.UserName, to, subject, body);
  using (var client = new SmtpClient())
  {
   client.Port=_smtpSetting.Port;
   client.Host = _smtpSetting.Host;
   client.EnableSsl = true;
   client.Credentials=new  NetworkCredential(_smtpSetting.UserName,_smtpSetting.Password);
   await client.SendMailAsync(message);
               
  }

 }
}