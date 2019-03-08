using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository;
//using LandmarkIT.Enterprise.CommunicationManager.Contracts;
//using LandmarkIT.Enterprise.CommunicationManager.Operations;

namespace LandmarkIT.Enterprise.CommunicationManager
{
    public static class IoC
    {
        public static Container  Container{ get; set; }

        public static void Configure(Container container)
        {
            container.Register<IUnitOfWork, DatabaseUnitOfWork>(Lifestyle.Singleton);

           // container.Register<ISmsIdeaTextService, SmsIdeaService>();

            IoC.Container = container;
        }
    }
}
