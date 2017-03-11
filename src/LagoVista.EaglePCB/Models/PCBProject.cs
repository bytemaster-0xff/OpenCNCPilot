using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB.Models
{
    public class PCBProject
    {
        public double Scrap { get; set; }

        public string BoardFile { get; set; }
        public double BoardDepth { get; set; }

        public bool PauseForToolChange { get; set; }

        public double DrillSpindleDwell { get; set; }
        public int DrillSpindleRPM { get; set; }
        public int SafePlungRecoverRate { get; set; }
        public int DrillPlungRate { get; set; }
        public double DrillSafeHeight { get; set; }

        public double MillSpindleRPM { get; set; }
        public double MillSpindleDwell { get; set; }

        public double MillToolSize { get; set; }
        public double MillCutDepth { get; set; }
        public double MillFeedRate { get; set; }
        public double MillPlungeRate { get; set; }
        public double MillSafeHeight { get; set; }


        public async Task SaveAsync(String fileName)
        {
            await Core.PlatformSupport.Services.Storage.StoreAsync(this, fileName);
        }

        public static Task<PCBProject> OpenAsync(String fileName)
        {
            return Core.PlatformSupport.Services.Storage.GetAsync<PCBProject>(fileName);
        }
    }
}