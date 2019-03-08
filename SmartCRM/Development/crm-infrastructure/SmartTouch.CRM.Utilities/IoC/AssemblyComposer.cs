using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.StorageProvider.IoC
{
    public sealed class AssemblyComposer
    {
        private static object syncObject = new object();

        public static CompositionContainer Container
        {
            get
            {
                var container = default(CompositionContainer);
                //lock (syncObject)
                //{
                //    if (!CacheHelper.IsExists(CacheKeys.IOC_CONTAINER))
                //    {
                //        var aggregateCatalog = new AggregateCatalog();
                //        var directoryPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                //        aggregateCatalog.Catalogs.Add(new DirectoryCatalog(directoryPath, "SmartTouch.CRM.*"));
                //        container = new CompositionContainer(aggregateCatalog);
                //        CacheHelper.Add<CompositionContainer>(CacheKeys.IOC_CONTAINER, container);
                //    }
                //    container = CacheHelper.Get<CompositionContainer>(CacheKeys.IOC_CONTAINER);
                //}
                return container;
            }
        }
    }
}
