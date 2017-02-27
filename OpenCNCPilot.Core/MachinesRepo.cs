using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public class MachinesRepo
    {
        public string CurrentMachineId { get; set; }
        public List<MachineSettings> Machines { get; set; }

        public MachineSettings CurrentMachine { get; set; }

        public async static Task<MachinesRepo> LoadAsync()
        {
            try
            {
                var machines = await Services.Storage.GetAsync<MachinesRepo>("machines.json");

                if (machines == null)
                {
                    machines.Machines = new List<MachineSettings>();
                    machines.CurrentMachine = MachineSettings.Default;
                    machines.CurrentMachineId = machines.CurrentMachineId;
                }

                return machines;
            }
            catch (Exception)
            {
                var machines = new MachinesRepo();
                machines.Machines = new List<MachineSettings>();
                machines.CurrentMachine = MachineSettings.Default;
                return machines;

            }
        }


        public async Task SaveAsync()
        {
            await Services.Storage.StoreAsync(this, "machines.json");
        }

    }
}
