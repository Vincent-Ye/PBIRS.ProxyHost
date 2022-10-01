using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SoftMatrix.PBIRS.ProxyHost
{
    public  class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            //所有的请求均路由到ProxyController
            config.Routes.MapHttpRoute(
                name: "PowerBI",
                routeTemplate: "{*subPath}",
                defaults: new { controller = "Proxy" }
            );
            appBuilder.UseWebApi(config);
        }
    }
}
