using LagoVista.Core.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Managers
{
    public class PnPMachineManager
    {
        public PnPMachineManager(PnPMachine machine)
        {
            Machine = machine ?? throw new ArgumentNullException(nameof(machine));
        }

        public PnPMachine Machine
        {
            get;
        }

        public PlaceablePart FindPart(string package, string value)
        {
            foreach (var pack in Machine.Carrier.AvailablePartPacks)
            {
                foreach (var row in pack.Rows)
                {
                    var quantity = row.PartCount - row.CurrentPartIndex;

                    if (quantity > 0 && row.Part.PackageName == package &&
                       row.Part.Value == value)
                    {
                        return new PlaceablePart
                        {
                            PartPack = EntityHeader.Create(pack.Id, pack.Name),
                            Quantity = quantity,
                            Row = row.Display
                        };
                    }
                }
            }

            return null;
        }

        public static Task<PnPMachine> GetPnPMachineAsync(string path)
        {
            return Core.PlatformSupport.Services.Storage.GetAsync<PnPMachine>(path);
        }

        public static Task SavePackagesAsync(PnPMachine machine, string path)
        {
            return Core.PlatformSupport.Services.Storage.StoreAsync(machine, path);
        }
    }
}
