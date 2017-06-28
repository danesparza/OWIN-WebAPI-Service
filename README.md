OWIN WebAPI Service example [![Build status](https://ci.appveyor.com/api/projects/status/qyo52t5ipvxqh5fb?svg=true)](https://ci.appveyor.com/project/danesparza/owin-webapi-service) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
===========================

Sometimes, you just need a good example to get started.  

The [OWIN-WebAPI-Service project](https://github.com/danesparza/OWIN-WebAPI-Service) came out of a need to create a self-hosted WebAPI 2 service in a Windows service.  Microsoft says that going forward, [OWIN is the way to go](http://www.asp.net/web-api/overview/hosting-aspnet-web-api/self-host-a-web-api).  I wanted to use [attribute routing](http://www.asp.net/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2) in WebAPI 2.  I couldn't find a decent example anywhere, so I created my own. 

*Please be aware that OWIN (and this project template) are only compatible with .NET 4.5 and newer projects.* 

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
* You can load API controllers from another assembly by using the hack `Type valuesControllerType = typeof(OWINTest.API.ValuesController);` or by creating a [custom assembly resolver](http://www.strathweb.com/2013/08/customizing-controller-discovery-in-asp-net-web-api/)
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
Create a service installer by right-clicking on the service design surface and selecting 'Add installer' from the context menu.  You can update the service name, description, [startup mode](http://superuser.com/a/285655/4508) and [default credentials](http://stackoverflow.com/a/510225/19020) by updating the properties on the 2 new controls that are added.  

After you've [added the service installer](http://msdn.microsoft.com/en-us/library/ddhy0byf(v=vs.110).aspx) by updating the service code, install the service using the [.NET installutil.exe](http://msdn.microsoft.com/en-us/library/50614e95(v=vs.110).aspx).  See the sample batch files `install.cmd` and `uninstall.cmd` for an example of making this a little easier on yourself.

### Stuff to try
Now that you've compiled and installed your service, start it up in the 'Services' app in the control panel.  
* If you've added the `RoutedController` example above, try navigating to the following url in [Postman](http://www.getpostman.com/) or your favorite REST service tester: `http://localhost:9000/api/testing/getall` -- you should get a JSON string array back.  
* Try hitting breakpoints in your running service in Visual Studio by selecting 'Debug/Attach to Process'.  Select your running service exe, then press 'Attach'.  
* Try calling the service directly from a browser-based single page application.  (Hint:  You won't be able to until you [enable CORS](http://www.asp.net/web-api/overview/security/enabling-cross-origin-requests-in-web-api))

## Tips

### Building the sample service

So if you just want to take a look at the sample project, you'll need to either grab the zip or [clone the project](https://help.github.com/articles/which-remote-url-should-i-use/) in git.

Before you build and install the service you'll need to do a 'Nuget package restore'. The easiest way to do this is probably to right-click on the solution in Visual Studio and select 'Manage Nuget packages for solution...'

You should see the 'Manage NuGet Packages' screen pop up. At the very top of the screen, you'll probably see a yellow message indicating that 'Some NuGet packages are missing from this solution. Click to restore from your online package sources.' with a Restore button. Go ahead and click Restore and then close the window once the missing packages have been downloaded.

Try your build again after that, and you should be good.

### Installing the service

You'll need to run the `installutil` command as an Administrator.  To do that, you'll need to [run the command prompt itself as Administrator](https://technet.microsoft.com/en-us/library/cc947813%28v=ws.10%29.aspx?f=255&MSPPError=-2147217396), or use [other interesting tricks](http://stackoverflow.com/a/12401075/19020)

### Serving more than just localhost

If you want to listen to all requests coming in a certain port -- not just localhost requests, you'll need to know a few things.  

**First**, understand [there are permission differences between Local System, Local service, Network service](http://stackoverflow.com/a/510225/19020), and a user account.  I recommend you run under 'Local service' because it's a minimal set of permissions. 

**Second**, you'll need to change the code that starts the service.  Instead of listening for requests to `http://localhost:9000`, you'll need to listen for requests to `http://+:9000`.  

**Third**, you'll need to use the command-line tool `netsh` to authorize 'Local Service' to listen for requests.  I usually put this command in the **install.bat** file that installs the service: 

```bash
netsh http add urlacl url=http://+:9000/ user="Local Service"
```

Without this, you'll have problems starting the service and listening to all requests for that port.

### Help -- I'm getting Error 1053 when trying to start the service

If you're getting `Error 1053: The service did not respond to the start or control request in a timely fashion.` there is a good chance you don't have the right version of the .NET framework installed.  Remember: OWIN and WebAPI 2 require .NET 4.5 or a more recent version of the framework to be installed.


