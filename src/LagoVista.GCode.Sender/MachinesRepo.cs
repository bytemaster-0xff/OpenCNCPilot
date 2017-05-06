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
        public const string FileName = "Machines.json";

        public string CurrentMachineId { get; set; }
        public List<MachineSettings> Machines { get; set; }


        public async static Task<MachinesRepo> LoadAsync()
        {
            try
            {
                var machines = await Services.Storage.GetAsync<MachinesRepo>(MachinesRepo.FileName);

                if (machines == null)
                {
                    machines = MachinesRepo.Default;
                }

                return machines;
            }
            catch (Exception)
            {
                return MachinesRepo.Default;
            }
        }

        public MachineSettings GetCurrentMachine()
        {
            return Machines.Where(machine => machine.Id == CurrentMachineId).First();
        }

        public static MachinesRepo Default
        {
            get
            {
                var repo = new MachinesRepo();
                repo.Machines = new List<MachineSettings>();
                var defaultMachine = MachineSettings.Default;
                repo.Machines.Add(defaultMachine);
                repo.CurrentMachineId = defaultMachine.Id;

                return repo;
            }
        }

        public async Task SaveAsync()
        {
            await Services.Storage.StoreAsync(this, MachinesRepo.FileName);
        }

    }
}
