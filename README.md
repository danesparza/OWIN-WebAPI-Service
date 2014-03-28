OWIN WebAPI Service example
===========================

Sometimes, you just need a good example to get started.  The [OWIN-WebAPI-Service project](https://github.com/danesparza/OWIN-WebAPI-Service) came out of a need to create a self-hosted WebAPI 2 service in a Windows service.  Microsoft says that going forward, [OWIN is the way to go](http://www.asp.net/web-api/overview/hosting-aspnet-web-api/self-host-a-web-api).  I wanted to use [attribute routing](http://www.asp.net/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2) in WebAPI 2.  I couldn't find a decent example anywhere, so I created my own. 

*Please be aware that OWIN (and this project template) are not compatible with .NET 4.0 and older projects.* 

## If starting from scratch:

### Create the service project ###
If you're starting from scratch, add a new service project to your solution by selecting **'Windows Service'** in the new project template.

### Add the OWIN Nuget packages ###

From the package manager console: 

```powershell
Install-Package Microsoft.AspNet.WebApi.OwinSelfHost
```

This will install the following dependent packages automatically:
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

```CSharp
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
```
    
Note that:
* You can load API controllers from another assembly by using the hack `Type valuesControllerType = typeof(OWINTest.API.ValuesController);` or by creating a custom assembly resolver
* You can use Attribute based routing by including the line `config.MapHttpAttributeRoutes()` before the default `config.Routes.MapHttpRoute`

### Add API controllers
Add API controllers to the service project by creating classes inherited from `ApiController`.  Here is a simple example that uses attribute based routing:

```CSharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace OWINTest.Service.API
{
    [RoutePrefix("api/testing")]
    public class RoutedController : ApiController
    {
        [Route("getall")]
        public IEnumerable<string> GetAllItems()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
```

Note that:
* Controllers in the service assembly will be loaded automatically.
* If you want to load a controller in another assembly, you'll need to update your `Startup.cs` file (and read the note about loading controllers from other assemblies, above)

### Add code to start/stop the WebAPI listener

Add code to the default service (inherited from `ServiceBase`) that the Visual Studio template created for you.  The finished service class should look something like this:

```CSharp
public partial class APIServiceTest : ServiceBase
{
    public string baseAddress = "http://localhost:9000/";
    private IDisposable _server = null;
    
    public APIServiceTest()
    {
        InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
        _server = WebApp.Start<Startup>(url: baseAddress);
    }

    protected override void OnStop()
    {
        if(_server != null)
        {
            _server.Dispose();
        }
        base.OnStop();
    }
}
```

See how simple that is?  
* In the `OnStart` handler, we start the listener and pass our `Startup` class we created.  That calls our configuration handler.
* In the `OnStop` handler, we just stop the listener
* The service will be listening with a base location of `http://localhost:9000`.

### Install the service
Create a service installer by right-clicking on the service design surface and selecting 'Add installer' from the context menu.  You can update the service name, description, startup mode and default credentials by updating the properties on the 2 new controls that are added.

After you've [added the service installer](http://msdn.microsoft.com/en-us/library/ddhy0byf(v=vs.110).aspx) by updating the service code, install the service using the [.NET installutil.exe](http://msdn.microsoft.com/en-us/library/50614e95(v=vs.110).aspx).  See the sample batch files `install.cmd` and `uninstall.cmd` for an example of making this a little easier on yourself.

### Stuff to try
Now that you've compiled and installed your service, start it up in the 'Services' app in the control panel.  
* If you've added the `RoutedController` example above, try navigating to the following url in [Postman](http://www.getpostman.com/) or your favorite REST service tester: `http://localhost:9000/api/testing/getall` -- you should get a JSON string array back.  
* Try hitting breakpoints in your running service in Visual Studio by selecting 'Debug/Attach to Process'.  Select your service exe, then press 'Attach'.  

