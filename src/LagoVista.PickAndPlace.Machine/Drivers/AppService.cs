using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;

namespace LagoViata.PNP.Drivers
{
    public class AppService
    {
        AppServiceConnection _connection;
        public AppService(AppServiceConnection connection)
        {
            _connection = connection;
        }
    }
}
