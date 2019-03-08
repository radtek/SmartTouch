using SmartTouch.CRM.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;


namespace SmartTouch.CRM.Web.Utilities
{
    public class SmarttouchControllerFactory : DefaultControllerFactory
    {
        protected override SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, Type controllerType)
        {
            var defaultSessionStateBehaviour = SessionStateBehavior.ReadOnly;
            string action = string.Empty;
            if (controllerType == null)
            {
                return defaultSessionStateBehaviour;
            }
            if (requestContext.RouteData.Values["ms_directroutematches"] == null)
            {
                action = requestContext.RouteData.Values["action"].ToString();
            }
            else
            {
                action = ((IEnumerable<RouteData>)requestContext.RouteData.Values["ms_directroutematches"]).FirstOrDefault().Values["action"].ToString();
            }
            var methods = controllerType.GetMethods();
            MethodInfo mi = methods.Where(m => m.Name == action).FirstOrDefault();

            if(mi != null)
            {
                var actionSessionState = mi.GetCustomAttributes(typeof(SmarttouchSessionStateBehaviourAttribute), false).OfType<SmarttouchSessionStateBehaviourAttribute>().FirstOrDefault();
                if (actionSessionState != null)
                {
                    return actionSessionState.Behavior;
                }
                else
                {
                    return defaultSessionStateBehaviour;
                }
            }
            else
            {
                return defaultSessionStateBehaviour;
            }
        }
        public override IController CreateController(System.Web.Routing.RequestContext requestContext, string controllerName)         
        {
            string controllername = requestContext.RouteData.Values["controller"].ToString();
            IController controller = null;
            if (controllername.Replace(" ","").ToLower()== "campaignimage")
            {
                controller = GetController(requestContext, "Campaign", "CampaignTrackerImage");
            }
            else
            {
                try
                {
                    controller = base.CreateController(requestContext, controllerName);
                }
                catch
                {
                    controller = GetController(requestContext, "Error", "AnonymousError");
                }
                
            }
                
            return controller;
        }
        public override void ReleaseController(IController controller)
        {
            IDisposable dispose = controller as IDisposable; if (dispose != null)
            {
                dispose.Dispose();
            }
        }
        private IController GetController(RequestContext requestContext, string controllerName, string actionName)
        {
            requestContext.RouteData.Values["controller"] = controllerName;
            requestContext.RouteData.Values["action"] = actionName;
            Assembly asm = Assembly.GetExecutingAssembly();

            var controllerType = asm.GetTypes().Where(type => typeof(Controller).IsAssignableFrom(type)).Where(t=>t.Name == controllerName +"Controller").FirstOrDefault();

            IController controller = IoC.Container.GetInstance(controllerType) as IController;
            (controller as Controller).ActionInvoker = new CustomDefaultActionInvoker();
            return controller;
        }
    }
    public class CustomDefaultActionInvoker : ControllerActionInvoker
    {
        protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
        {
            var reflectedControllerDescriptor = new ReflectedControllerDescriptor(controllerDescriptor.ControllerType);
            var actions = reflectedControllerDescriptor.GetCanonicalActions();
            foreach(var action in actions)
            {
                if (action.ActionName == actionName)
                    return action;
            }
            return controllerDescriptor.FindAction(controllerContext, actionName);
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple =false, Inherited=true)]
    public sealed class SmarttouchSessionStateBehaviourAttribute : Attribute
    {
         public SessionStateBehavior Behavior { get; private set; }
         public SmarttouchSessionStateBehaviourAttribute(SessionStateBehavior behavior = SessionStateBehavior.ReadOnly)
        {
            this.Behavior = behavior;
        }
    }
    
}