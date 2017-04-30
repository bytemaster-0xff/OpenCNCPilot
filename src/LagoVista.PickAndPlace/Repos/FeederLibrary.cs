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
        public Task<ObservableCollection<FeederDefinition>> GetFeederDefinitions(string path)
        {
            return Core.PlatformSupport.Services.Storage.GetAsync<ObservableCollection<FeederDefinition>>(path);
        }

        public Task SaveFeederDefinitions(ObservableCollection<FeederDefinition> feederDefinitions, string path)
        {
            return Core.PlatformSupport.Services.Storage.StoreAsync(feederDefinitions, path);
        }
    }
}
