using LagoVista.Core.PlatformSupport;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Repos
{
    public class FeederLibrary
    {
        public async Task<ObservableCollection<Feeder>> GetFeedersAsync()
        {
            try
            {
                var feeders = await Services.Storage.GetAsync<ObservableCollection<Feeder>>("Feeders.dat");

                if (feeders == null)
                {
                    return new ObservableCollection<Feeder>();
                }


                return feeders;
            }
            catch (Exception)
            {
                return new ObservableCollection<Feeder>();
            }
        }

        public Task SaveFeederDefinitions(ObservableCollection<Feeder> feederDefinitions)
        {
            return Core.PlatformSupport.Services.Storage.StoreAsync(feederDefinitions, "feeders.dat");
        }
    }
}
