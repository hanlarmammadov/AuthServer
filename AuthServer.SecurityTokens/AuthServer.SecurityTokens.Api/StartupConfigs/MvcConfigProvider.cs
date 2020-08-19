﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AuthServer.SecurityTokens.Api.StartupConfigs
{
    public class MvcConfigProvider
    {
        public Action<MvcOptions> GetMvcOptionsConfigurer()
        {
            Action<MvcOptions> configurer = (MvcOptions opt) =>
            {
                opt.Filters.Add(new RequireHttpsAttribute());
                var cont = new MediaTypeHeaderValue("application/xml");
                opt.FormatterMappings.SetMediaTypeMappingForFormat("xml", "application/xml");
                opt.RespectBrowserAcceptHeader = true;
                opt.ReturnHttpNotAcceptable = true;
            };

            return configurer;
        }

        public Action<MvcJsonOptions> GetJsonOptionsConfigurer()
        {
            Action<MvcJsonOptions> configurer = (MvcJsonOptions opt) =>
            {
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; 
            };

            return configurer;
        }
    }
}
