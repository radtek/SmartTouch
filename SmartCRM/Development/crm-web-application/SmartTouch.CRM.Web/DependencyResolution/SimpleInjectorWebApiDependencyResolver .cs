using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http.Dependencies;

namespace SmartTouch.CRM.WebService.DependencyResolution 
{
    public sealed class SimpleInjectorWebApiDependencyResolver : IDependencyResolver
    {
        private readonly Container container;

        public SimpleInjectorWebApiDependencyResolver(
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

        //TODO: fix this
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "This neds to be fixed.")]
        [DebuggerStepThrough]
        public void Dispose()
        {
        }
    }
}