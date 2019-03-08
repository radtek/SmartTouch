﻿using System;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace SmartTouch.CRM.Web.App_Start
{
    public class Fingerprint
    {
        public static string Tag(string rootRelativePath)
        {
            if (HttpRuntime.Cache[rootRelativePath] == null)
            {
                string absolute = HostingEnvironment.MapPath("~" + rootRelativePath);

                DateTime date = File.GetLastWriteTime(absolute);
                int index = rootRelativePath.LastIndexOf('/');

                string result = rootRelativePath.Insert(index, "/v-" + date.Ticks);
                HttpRuntime.Cache.Insert(rootRelativePath, result, new CacheDependency(absolute));
            }

            return HttpRuntime.Cache[rootRelativePath] as string;
        }
    }
}