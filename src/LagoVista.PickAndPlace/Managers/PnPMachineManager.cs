using LagoVista.Core.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Managers
{
    public class PnPMachineManager
    {
   

        public static void ResolvePart(PnPMachine machine, PlaceableParts part)
        {
            foreach (var slot in machine.Carrier.PartPackSlots)
            {
                if (!EntityHeader.IsNullOrEmpty(slot.PartPack))
                {
                    var pack = machine.Carrier.AvailablePartPacks.Where(prt => prt.Id == slot.PartPack.Id).First();
                    foreach (var row in pack.Rows)
                    {
                        if (row.Part != null &&
                            row.Part.PackageName?.ToUpper() == part.Package?.ToUpper() &&
                           row.Part.Value?.ToUpper() == part.Value?.ToUpper())
                        {
                            part.Slot = slot;
                            part.PartPack = pack;
                            part.Row = row;
                        }
                    }
                }
            }
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
