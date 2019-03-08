using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ObjectMappers
{
    public static class MapperConfigurationProvider
    {
        private static IConfigurationProvider _instance;
        public static IConfigurationProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    var config = new MapperConfiguration(cfg =>
                    {
                        cfg.AddProfile<SmartTouch.CRM.ApplicationServices.ObjectMappers.EntityToDbProfile>();
                        cfg.AddProfile<SmartTouch.CRM.ApplicationServices.ObjectMappers.ViewModelToEntityProfile>();
                        cfg.AddProfile<SmartTouch.CRM.ApplicationServices.ObjectMappers.DbToEntityProfile>();
                        cfg.AddProfile<SmartTouch.CRM.ApplicationServices.ObjectMappers.EntityToViewModelProfile>();
                    });
                    _instance = config.CreateMapper().ConfigurationProvider;
                }
                return _instance;
            }
        }
    }
}
