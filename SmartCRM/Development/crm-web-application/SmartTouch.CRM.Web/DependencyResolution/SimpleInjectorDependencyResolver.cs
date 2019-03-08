using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Diagnostics;
using System.Web.Http.Dependencies;

using SimpleInjector;
using SimpleInjector.Extensions;

namespace SmartTouch.CRM.Web.DependencyResolution
{
    public class SimpleInjectorDependencyResolver : System.Web.Http.Dependencies.IDependencyResolver
    {
        private readonly Container container;

        public SimpleInjectorDependencyResolver(
            Container container)
        {
            this.container = container;
        }

        [DebuggerStepThrough]
        public IDependencyScope BeginScope()
        {
            return this;
        }

        [DebuggerStepThrough]
        public object GetService(Type serviceType)
        {
            return ((IServiceProvider)this.container)
                .GetService(serviceType);
        }

        [DebuggerStepThrough]
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return this.container.GetAllInstances(serviceType);
        }

        [DebuggerStepThrough]
        public void Dispose()
        {
        }
    }
}