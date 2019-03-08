using log4net.Config;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Logging;

namespace SmartTouchAdhocServices
{
    public class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            HostFactory.Run(x =>
                {
                    x.Service<JobScheduler>();
                    x.SetServiceName("SmartTouchAdhocServices");
                    x.SetDisplayName("SmartTouch Adhoc Services");
                    x.StartAutomatically();
                    x.UseLog4Net();
                });
            HostLogger.Get<Program>().Info("Service Created");
        }
    }

    public class JobScheduler : ServiceControl
    {
        public bool Start(HostControl hostControl)
        {
            //Quartz.Net Job Scheduler
            //use http://www.cronmaker.com/ to build cron expression
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            
            scheduler.ScheduleJob(CreateJob<ServiceStatusChecker>(), CreateTrigger(ConfigurationManager.AppSettings["CRON_EXPRESSION"].ToString(), "Service Alert"));
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            return true;
        }
        private IJobDetail CreateJob<T>() where T : IJob
        {
            return JobBuilder.Create<T>().Build();
        }

        private ITrigger CreateTrigger(string cronExpression, string jobName)
        {
            return TriggerBuilder.Create()
                            .WithIdentity(jobName)
                            .StartNow()
                            .WithCronSchedule(cronExpression)
                            .Build();
        }
    }

    public class ServiceStatusChecker : IJob
    {
        public enum SimpleServiceCommands
        { StopWorker = 128, RestartWorker, CheckWorker };

        public void Check()
        {
            var serviceNames = ConfigurationManager.AppSettings["services"].ToString().Split(';');
            foreach (var serviceName in serviceNames)
            {
                var service = new ServiceController(serviceName);
                Alert(service);
            }
        }
        void Alert(ServiceController service)
        {
            
            try
            {
                HostLogger.Get<ServiceStatusChecker>().Info(service.DisplayName + " is : "+ service.Status.ToString());
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    SendMail(service);
                    Thread.Sleep(1000);
                    service.Refresh();
                    try
                    {
                        HostLogger.Get<ServiceStatusChecker>().Info("Restarting Service : " + service.DisplayName);
                        service.ExecuteCommand((int)SimpleServiceCommands.RestartWorker);
                    }
                    catch (Exception e)
                    {
                        HostLogger.Get<ServiceStatusChecker>().Error("Failed to restart service : ", e);
                    }
                }
            }
            catch(Exception e)
            {
                HostLogger.Get<ServiceStatusChecker>().Error("Service doesn't exist ", e);
            }
            
        }

        void SendMail(ServiceController service)
        {
            try
            {
                var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                string username = smtpSection.Network.UserName;
                string host = smtpSection.Network.Host;
                int port = smtpSection.Network.Port;
                string password = smtpSection.Network.Password;
                var credentials = new NetworkCredential(username, password);
                bool ssl = smtpSection.Network.EnableSsl;
                string sender = smtpSection.From;
                var environment = ConfigurationManager.AppSettings["environment"].ToString();
                string subject = environment + " : " + service.DisplayName + " Service " + service.Status;
                using (var client = new SmtpClient(host, port) { Credentials = credentials, EnableSsl = ssl })
                using (var message = new MailMessage(sender, sender) { Body = subject, Subject = subject })
                {
                    try
                    {
                        HostLogger.Get<ServiceStatusChecker>().Info("Sending mail..");
                        client.Send(message);
                    }
                    catch (Exception e)
                    {
                        HostLogger.Get<ServiceStatusChecker>().Error("Failed to send mail 1", e);
                    }
                }
            }
            catch(Exception e)
            {
                HostLogger.Get<ServiceStatusChecker>().Error("Failed to send mail 2", e);
            }
            
        }

        public void Execute(IJobExecutionContext context)
        {
            Check();
        }
    }
}
