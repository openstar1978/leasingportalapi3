using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Hangfire;
using System.Configuration;
using Hangfire.Dashboard;
using System.Web.Services.Description;

[assembly: OwinStartup(typeof(LeasingPortalApi.Startup))]

namespace LeasingPortalApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
               .UseSqlServerStorage(ConfigurationManager.ConnectionStrings["BackgroundConString"].ConnectionString);

            app.UseHangfireDashboard("/backgroundjobs");
            app.UseHangfireServer();

            //signalr
            //app.MapSignalR();
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }
     }
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // In case you need an OWIN context, use the next line, `OwinContext` class
            // is the part of the `Microsoft.Owin` package.
            var owinContext = new OwinContext(context.GetOwinEnvironment());

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return owinContext.Authentication.User.Identity.IsAuthenticated;
        }
    }

}
