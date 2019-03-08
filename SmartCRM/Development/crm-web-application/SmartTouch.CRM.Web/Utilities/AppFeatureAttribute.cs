using SmartTouch.CRM.Entities;
using System;

namespace SmartTouch.CRM.Web.Utilities
{
    public sealed class AppFeatureAttribute : Attribute
    {
        public AppFeatureAttribute(AppFeatures featureName = AppFeatures.NOT_APPLICABLE)
        {
            this.Feature = featureName;
        }
        public AppFeatures Feature { get; set; }
    }
}