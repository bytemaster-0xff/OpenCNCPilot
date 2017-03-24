using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB.Models
{
    public class PCBProject : ModelBase
    {
        public PCBProject()
        {
            Fiducials = new ObservableCollection<Hole>();
            ConsolidatedDrillRack = new ObservableCollection<ConsolidatedDrillBit>();
        }

        private string _currentFileName;

        private double _scrapSides;
        public double ScrapSides
        {
            get { return _scrapSides; }
            set { Set(ref _scrapSides, value); }
        }

        private double _scrapTopBottom;
        public double ScrapTopBottom 
        {
            get { return _scrapTopBottom; }
            set { Set(ref _scrapTopBottom, value); }
        }

        private string _eagleBRDFilePath;
        public string EagleBRDFilePath
        {
            get { return _eagleBRDFilePath; }
            set { Set(ref _eagleBRDFilePath, value); }
        }

        private string _topEtchingFilePath;
        public string TopEtchingFilePath
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

        public double HoldDownDiameter { get; set; }
        public double HoldDownDrillDiameter { get; set; }
        public double HoldDownDrillDepth { get; set; }
        public double HoldDownBoardOffset { get; set; }

        public double StockWidth { get; set; }
        public double StockHeight { get; set; }
        public double StockThickness { get; set; }


        public bool PauseForToolChange { get; set; }
        public double DrillSpindleDwell { get; set; }
        public int DrillSpindleRPM { get; set; }
        public int SafePlungeRecoverRate { get; set; }
        public int DrillPlungeRate { get; set; }
        public double DrillSafeHeight { get; set; }

        public double HeightMapGridSize { get; set; }
        public double MillSpindleRPM { get; set; }
        public double MillSpindleDwell { get; set; }
        public double MillToolSize { get; set; }
        public double MillCutDepth { get; set; }
        public int MillFeedRate { get; set; }
        public int MillPlungeRate { get; set; }
        public double MillSafeHeight { get; set; }

        public bool FlipBoard { get; set; }

        List<Models.Hole> _fiducualOptions;
        public List<Models.Hole> FiducialOptions
        {
            get { return _fiducualOptions; }
            set { Set(ref _fiducualOptions, value); }
        }

        public ObservableCollection<Hole> Fiducials { get; set; }

        public PCBProject Clone()
        {
            return this.MemberwiseClone() as PCBProject;
        }

        public ObservableCollection<ConsolidatedDrillBit> ConsolidatedDrillRack { get; set; }

        public async Task SaveAsync(String fileName = null)
        {
            if(String.IsNullOrEmpty(fileName))
            {
                if(_currentFileName == null)
                {
                    throw new Exception("Must provide a file name if we aren't editing an existing file.");
                }

                fileName = _currentFileName;
            }
            await Core.PlatformSupport.Services.Storage.StoreAsync(this, fileName);
        }

        public static async Task<PCBProject> OpenAsync(String fileName)
        {
            var project = await Core.PlatformSupport.Services.Storage.GetAsync<PCBProject>(fileName);
            project._currentFileName = fileName;
            return project;
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
                    HoldDownBoardOffset = 3,
                    HoldDownDiameter = 3,
                    HoldDownDrillDiameter = 2,
                    HoldDownDrillDepth = 5,
                    DrillSpindleDwell = 3,
                    DrillSpindleRPM = 20000,
                    DrillPlungeRate = 200,
                    DrillSafeHeight = 5,
                    StockThickness = 1.57,
                    MillCutDepth = 0.5,
                    MillFeedRate = 500,
                    MillPlungeRate = 200,
                    MillSafeHeight = 5,
                    MillSpindleDwell = 3,
                    MillSpindleRPM = 15000,
                    MillToolSize = 3.15,
                    ScrapTopBottom = 5,
                    ScrapSides = 5,
                    SafePlungeRecoverRate = 500,
                    FlipBoard = true,
                    HeightMapGridSize = 10,
               
                };
            }
        }

    }
}