using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Repos
{
    class FeederLibrary
    {
        public Task<ObservableCollection<Feeder>> GetFeederDefinitions(string path)
        {
            return Core.PlatformSupport.Services.Storage.GetAsync<ObservableCollection<Feeder>>(path);
        }

        public Task SaveFeederDefinitions(ObservableCollection<Feeder> feederDefinitions, string path)
        {
            return Core.PlatformSupport.Services.Storage.StoreAsync(feederDefinitions, path);
        }
    }
}
