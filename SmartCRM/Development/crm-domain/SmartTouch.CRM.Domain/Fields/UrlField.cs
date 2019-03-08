using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Fields
{
    public class UrlField : Field
    {
        public UrlField()
        {
            this.Title = "Url";
            this.FieldCode = "UrlField";
        }
        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class FacebookUrlField : UrlField
    {

        public FacebookUrlField()
        {
            this.Id = (int)ContactFields.FacebookUrl;
            this.Title = "Facebook Url";
            this.FieldCode = "FacebookUrlField";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class TwitterUrlField : UrlField
    {

        public TwitterUrlField()
        {
            this.Id = (int)ContactFields.TwitterUrl;
            this.Title = "Twitter Url";
            this.FieldCode = "TwitterUrlField";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class LinkedInUrlField : UrlField
    {
        public LinkedInUrlField()
        {
            this.Id = (int)ContactFields.LinkedInUrl;
            this.Title = "LinkedIn Url";
            this.FieldCode = "LinkedInUrlField";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class GooglePlusUrlField : UrlField
    {
        public GooglePlusUrlField()
        {
            this.Id = (int)ContactFields.GooglePlusUrl;
            this.Title = "GooglePlus Url";
            this.FieldCode = "GooglePlusUrlField";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class WebsiteUrlField : UrlField
    {

        public WebsiteUrlField()
        {
            this.Id = (int)ContactFields.WebsiteUrl;
            this.Title = "Website Url";
            this.FieldCode = "WbesiteUrlField";
        }


        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class BlogUrlField : UrlField
    {
        public BlogUrlField()
        {
            this.Id = (int)ContactFields.BlogUrl;
            this.Title = "Blog Url";
            this.FieldCode = "BlogUrlField";
        }
        protected override void Validate()
        {
            base.Validate();
        }

    }
}
