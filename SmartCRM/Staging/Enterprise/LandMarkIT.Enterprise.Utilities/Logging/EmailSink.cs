using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Formatters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.Utilities.Logging
{
    public static class EmailSinkExtensions
    {
        public static SinkSubscription<EmailSink> LogToEmail(this IObservable<EventEntry> eventStream, string subject = "Error Observed", IEventTextFormatter formatter = null)
        {
            var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            string username = smtpSection.Network.UserName;
            string host = smtpSection.Network.Host;
            int port = smtpSection.Network.Port;
            string password = smtpSection.Network.Password;
            bool ssl = smtpSection.Network.EnableSsl;
            string from = smtpSection.From.Split(';')[0];
            string recipients = smtpSection.From;
            var sink = new EmailSink(host, port, recipients, subject, username, password, ssl, from, formatter);
            var subscription = eventStream.Subscribe(sink);
            return new SinkSubscription<EmailSink>(subscription, sink);
        }
    }
    public sealed class EmailSink : IObserver<EventEntry>
    {
        private IEventTextFormatter formatter;

        private MailAddress sender;

        private MailAddressCollection recipients = new MailAddressCollection();

        private string subject;

        private string host;

        private int port;

        private bool ssl;

        private NetworkCredential credentials;

        public EmailSink(string host, int port, string recipients, string subject, string username, string password, bool ssl, string from, IEventTextFormatter formatter)
        {
            this.formatter = formatter ?? new EventTextFormatter();
            this.host = host;
            this.port = GuardPort(port);
            this.credentials = new NetworkCredential(username, password);
            this.sender = new MailAddress(from);
            this.recipients = GuardRecipients(recipients);
            this.subject = subject;
            this.ssl = ssl;
        }

        public void OnNext(EventEntry entry)
        {
            if (entry != null && entry.Schema.Level < EventLevel.Warning)
            {
                using (var writer = new StringWriter())
                {
                    this.formatter.WriteEvent(entry, writer);
                    var log = writer.ToString();
                    var name = typeof(UnsupportedOperationException).Name;
                    string enviornment = (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["Environment"])) ? ConfigurationManager.AppSettings["Environment"].ToString() : "";
                    if (!log.Contains(name))
                    {
                        if( !(enviornment.ToUpper() == "QA" || enviornment.ToUpper() == "LOCALHOST" ))
                            Post(log, entry.Schema.Level);
                    }
                    
                }
            }
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        private void Post(string body, EventLevel level)
        {
            string enviornment = (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["Environment"])) ? ConfigurationManager.AppSettings["Environment"] + " : " : "";
            using (var client = new SmtpClient(this.host, this.port)
            {
                Credentials = this.credentials,
                EnableSsl = ssl
            })
            using (var message = new MailMessage(this.sender, this.recipients[0])
            {
                Body = body,
                Subject = enviornment + level.ToString() + " : " + subject
            })
            {
                for (int i = 1; i < this.recipients.Count; i++)
                    message.CC.Add(this.recipients[i]);
                try
                {
                    Logger.Current.Informational("Sending mail...");
                    client.Send(message);
                }
                catch (SmtpException e)
                {
                    Logger.Current.Informational("SMTP error sending email: " + e.StackTrace.ToString());
                    SemanticLoggingEventSource.Log.CustomSinkUnhandledFault("SMTP error sending email: " + e.Message);
                }
                catch (InvalidOperationException e)
                {
                    SemanticLoggingEventSource.Log.CustomSinkUnhandledFault("Configuration error sending email: " + e.Message);
                }
                catch (Exception e)
                {
                    Logger.Current.Informational("Error while sending email " + e.Message);
                }
            }
        }

        private static int GuardPort(int port)
        {
            if (port < 0)
                throw new ArgumentOutOfRangeException("port");
            return port;
        }

        private static MailAddressCollection GuardRecipients(string recipients)
        {
            var collection = new MailAddressCollection();
            var rcpts = recipients.Split(';');
            rcpts = (rcpts.Count() > 1) ? rcpts.Skip(1).ToArray() : rcpts.ToArray();
            rcpts.ToList().ForEach(r =>
            {
                collection.Add(r);
            });
            return collection;
        }
    }
}
