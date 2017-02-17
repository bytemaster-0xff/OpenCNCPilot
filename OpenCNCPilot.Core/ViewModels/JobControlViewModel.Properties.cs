
namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class JobControlViewModel
    {
        public bool IsCreatingHeightMap { get { return Machine.Mode == OperatingMode.ProbingHeightMap; } }
        public bool IsProbingHeight { get { return Machine.Mode == OperatingMode.ProbingHeight; } }
        public bool IsRunningJob { get { return Machine.Mode == OperatingMode.SendingGCodeFile; } }
    }
}
