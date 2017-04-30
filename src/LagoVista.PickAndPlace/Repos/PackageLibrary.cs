using LagoVista.PickAndPlace.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Repos
{
    public class PackageLibrary
    {
        public Task<ObservableCollection<Package>> GetPackagesAsync(string path)
        {
            return Core.PlatformSupport.Services.Storage.GetAsync<ObservableCollection<Package>>(path);
        }

        public Task SavePackagesAsync(ObservableCollection<Package> packages, string path)
        {
            return Core.PlatformSupport.Services.Storage.StoreAsync(packages, path);
        }
    }
}
