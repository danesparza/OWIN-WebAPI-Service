OWIN WebAPI Service example
===========================

Sometimes, you just need a good example to get started.  The [OWIN-WebAPI-Service project](https://github.com/danesparza/OWIN-WebAPI-Service) came out of a need to create a self-hosted WebAPI 2 service in a Windows service.  Microsoft says that going forward, [OWIN is the way to go](http://www.asp.net/web-api/overview/hosting-aspnet-web-api/self-host-a-web-api).  I wanted to use [attribute routing](http://www.asp.net/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2) in WebAPI 2.  I couldn't find a decent example anywhere, so I created my own. 

*Please be aware that OWIN (and this project template) are not compatible with .NET 4.0 and older projects.* 

If you're starting from scratch, here are the steps I followed:

### Create the service project ###
If you're starting from scratch, add a new service project to your solution by selecting **'Windows Service'** in the new project template.

### Add the OWIN Nuget packages ###
Add the appropriate OWIN packages to your project by doing a search in the NuGet package manager for `OwinSelfHost`.  This will install the following dependent packages automatically:
* Microsoft.AspNet.WebApi.Client
* Microsoft.AspNet.WebApi.Core
* Microsoft.AspNet.WebApi.Owin
* Microsoft.AspNet.WebApi.OwinSelfHost
* Microsoft.Owin
* Microsoft.Owin.Host.HttpListener
* Microsoft.Owin.Hosting
* Newtonsoft.Json
* Owin

### Create an OWIN configuration handler
Create the file `Startup.cs` and put a configuration handler in it:

    class Startup
    {
        //  Hack from http://stackoverflow.com/a/17227764/19020 to load controllers in 
        //  another assembly.  Another way to do this is to create a custom assembly resolver
        Type valuesControllerType = typeof(OWINTest.API.ValuesController);
    
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            
            //  Enable attribute based routing
            //  http://www.asp.net/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2
            config.MapHttpAttributeRoutes();
    
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
    
            appBuilder.UseWebApi(config);
        } 
    }
    
Note that:
* You can load API controllers from another assembly by using the hack `Type valuesControllerType = typeof(OWINTest.API.ValuesController);` or by creating a custom assembly resolver
* You can use Attribute based routing by including the line `config.MapHttpAttributeRoutes()` before the default `config.Routes.MapHttpRoute`

