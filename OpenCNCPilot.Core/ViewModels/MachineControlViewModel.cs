using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using OpenCNCPilot.Core.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.ViewModels
{
    public class MachineControlViewModel : ViewModelBase
    {
        IMachine _machine;

        public enum JogDirections
        {
            YPlus,
            YMinus,
            XPlus,
            XMinus,
            ZPlus,
            ZMinus
        }

        public enum Reset
        {
            X,
            Y,
            Z,
            All
        }

        public enum MicroStepModes
        {
            Micro,
            Small,
            Medium,
            Large
        }

        public MachineControlViewModel(IMachine machine, Settings settings)
        {
            _machine = machine;
            
            XYStep = settings.MediumStepSize;
            ZStep = settings.MediumStepSize;

            //JogCommand = new RelayCommand((param) => Jog((JogDirections)param), (param) => { return _machine.Connected; });
            JogCommand = new RelayCommand((param) => Jog((JogDirections)param));
        }
        

        public void Jog(JogDirections direction)
        {
            switch(direction)
            {

            }
        }

        private double _xyStep;
        public double XYStep
        {
            get { return _xyStep; }
            set { Set(ref _xyStep, value); }
        }

        private double _zStep;
        public double ZStep
        {
            get { return _zStep; }
            set { Set(ref _zStep, value); }
        }

        public RelayCommand JogCommand { get; private set; }
        public RelayCommand ResetCommand { get; private set; }


        public RelayCommand SetMicroStepSizeCommand { get; private set; }
        public RelayCommand SetSmallStepSizeCommand { get; private set; }
        public RelayCommand SetMediumSizeStepCommand { get; private set; }
        public RelayCommand SetLargeSizeStepCommand { get; private set; }

        public IMachine Machine { get { return _machine; } }

    }
}
