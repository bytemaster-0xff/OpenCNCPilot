using LagoVista.Core.Models;
using System;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB.Models
{
    public class PCBProject : ModelBase
    {
        public double Scrap { get; set; }

        private string _eagleBRDFilePath;
        public string EagleBRDFilePath
        {
            get { return _eagleBRDFilePath; }
            set { Set(ref _eagleBRDFilePath, value); }
        }

        private string _topEtchingFilePath;
        public string TopEtchignFilePath
        {
            get { return _topEtchingFilePath; }
            set { Set(ref _topEtchingFilePath, value); }
        }

        private string _bottomEtchingFilePath;
        public string BottomEtchingFilePath
        {
            get { return _bottomEtchingFilePath; }
            set { Set(ref _bottomEtchingFilePath, value); }
        }



        public double StockWidth { get; set; }
        public double StockHeight { get; set; }

        public double StockThickness { get; set; }

        public bool PauseForToolChange { get; set; }

        public double DrillSpindleDwell { get; set; }
        public int DrillSpindleRPM { get; set; }
        public int SafePlungeRecoverRate { get; set; }
        public int DrillPlungRate { get; set; }
        public double DrillSafeHeight { get; set; }

        public double HeightMapGridSize { get; set; }

        public double MillSpindleRPM { get; set; }
        public double MillSpindleDwell { get; set; }

        public double MillToolSize { get; set; }
        public double MillCutDepth { get; set; }
        public double MillFeedRate { get; set; }
        public double MillPlungeRate { get; set; }
        public double MillSafeHeight { get; set; }

        public PCBProject Clone()
        {
            return this.MemberwiseClone() as PCBProject;
        }


        public async Task SaveAsync(String fileName)
        {
            await Core.PlatformSupport.Services.Storage.StoreAsync(this, fileName);
        }

        public static Task<PCBProject> OpenAsync(String fileName)
        {
            return Core.PlatformSupport.Services.Storage.GetAsync<PCBProject>(fileName);
        }

        public static PCBProject Default
        {
            get
            {
                return new PCBProject()
                {
                    PauseForToolChange = false,
                    StockWidth = 100,
                    StockHeight = 80,
                    DrillSpindleDwell = 3,
                    DrillSpindleRPM = 20000,
                    DrillPlungRate = 200,
                    DrillSafeHeight = 5,
                    StockThickness = 1.57,
                    MillCutDepth = 0.5,
                    MillFeedRate = 500,
                    MillPlungeRate = 200,
                    MillSafeHeight = 5,
                    MillSpindleDwell = 3,
                    MillSpindleRPM = 15000,
                    MillToolSize = 3.15,
                    Scrap = 5,
                    SafePlungeRecoverRate = 500,
                };
            }
        }

    }
}