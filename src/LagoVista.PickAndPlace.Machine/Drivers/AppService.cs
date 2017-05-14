using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace LagoViata.PNP.Drivers
{
    public class AppService
    {
        AppServiceConnection _connection;
        public AppService(AppServiceConnection connection)
        {
            _connection = connection;
        }

        public async Task StartStep(int axis, int steps, int multiplier)
        {
            var msg = new ValueSet();
            msg.Add("AXIS", Convert.ToInt32(axis));
            msg.Add("MULTIPLIER", Convert.ToInt32(multiplier));
            msg.Add("STEPS", Convert.ToInt32(steps));
            await _connection.SendMessageAsync(msg);
        }

        public async Task Kill()
        {
            var msg = new ValueSet();
            msg.Add("KILL", true);
            await _connection.SendMessageAsync(msg);
        }
    }
}
