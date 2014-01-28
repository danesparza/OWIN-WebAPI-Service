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
