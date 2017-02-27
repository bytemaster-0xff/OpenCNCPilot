using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Models
{
    public partial class HeightMap
    {
        public async Task SaveAsync()
        {
            if (!String.IsNullOrEmpty(_fileName))
            {
                var popupService = SLWIOC.Get<IPopupServices>();
                _fileName = await popupService.ShowSaveFileAsync(Constants.FileFilterHeightMap);
            }

            if (!String.IsNullOrEmpty(_fileName))
            {
                await Core.PlatformSupport.Services.Storage.StoreAsync(this, _fileName);
            }
        }

        public async Task SaveAsAsync()
        {
            var popupService = SLWIOC.Get<IPopupServices>();
            _fileName = await popupService.ShowSaveFileAsync(Constants.FileFilterHeightMap);
            if (!String.IsNullOrEmpty(_fileName))
            {
                await Core.PlatformSupport.Services.Storage.StoreAsync(this, _fileName);
            }
        }

        public static async Task<HeightMap> OpenAsync(MachineSettings settings)
        {
            var popupService = SLWIOC.Get<IPopupServices>();
            var fileName = await popupService.ShowSaveFileAsync(Constants.FileFilterHeightMap);
            if (fileName != null)
            {
                var heightMap = await Core.PlatformSupport.Services.Storage.GetAsync<HeightMap>(fileName);
                heightMap.Refresh();
                return heightMap;
            }
            else
            {
                return null;
            }
        }
    }
}
