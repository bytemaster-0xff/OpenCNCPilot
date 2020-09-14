using System.Globalization;

namespace LagoVista.GCode.Sender
{
    public enum JogDirections
    {
        YPlus,
        YMinus,
        XPlus,
        XMinus,
        ZPlus,
        ZMinus,
        T0Plus,
        T0Minus,
        T1Plus,
        T1Minus,
        CPlus,
        CMinus,
    }

    public enum MachineOrigin
    {
        Top_Left,
        Bottom_Left,
        Top_Right,
        Bottom_Right,
    }

    public enum ConnectionTypes
    {
        Serial_Port,
        Network
    }

    public enum JogGCodeCommand
    {
        G0,
        G1
    }

    public enum Axis
    {
        XY,
        Z
    }

    public enum ResetAxis
    {
        X,
        Y,
        Z,
        T0,
        T1,
        C,
        All
    }

    public enum HomeAxis
    {
        X,
        Y,
        Z,
        T0,
        T1,
        C,
        All
    }


    public enum StepModes
    {
        Micro,
        Small,
        Medium,
        Large,
        XLarge
    }

    public enum FirmwareTypes
    {
        GRBL1_1,
        Marlin,
        Marlin_Laser,
        LagoVista,
        LagoVista_PnP
    }

    public enum OperatingMode
    {
        Manual,
        Alarm,
        SendingGCodeFile,
        PendingToolChange,
        ProbingHeightMap,
        ProbingHeight,
        AligningBoard,
        Disconnected
    }


    public enum StatusMessageTypes
    {
        ReceivedLine,
        SentLine,
        SentLinePriority,
        Info,
        Warning,
        FatalError
    }

    public enum MessageVerbosityLevels
    {
        Diagnostics,
        Verbose,
        Normal,
        Quiet,
    }


    public class Constants
    {
        public static NumberFormatInfo DecimalParseFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };

        public static NumberFormatInfo DecimalOutputFormat
        {
            get
            {
                return new NumberFormatInfo() { NumberDecimalSeparator = "." };
            }
        }

        public const double PixelToleranceEpsilon = 1.5;
        public const int PixelStabilizationToleranceCount = 10;

        public static string PickAndPlaceProject = "Pick and Place (*.pnpjob)|*.pnpjob";
        public static string PCBProject = "PCB Projec (*.pcbproj)|*.pcbproj|All Files|*.*";
        public static string FileFilterPCB = "Eagle|*.brd|All Files|*.*";
        public static string PartsPackages = "Package Library(*.pckgs)|*.pckgs";
        public static string PnPMachine = "PNP Machine(*.pnp)|*.pnp";
        public static string FileFilterGCode = "GCode|*.g;*.tap;*.nc;*.ngc|All Files|*.*";
        public static string FileFilterHeightMap = "Height Maps|*.hmap|All Files|*.*";

        public static string FilePathErrors = "Resources\\GrblErrors.txt";
        public static string FilePathWebsite = "Resources\\index.html";

        public static char[] NewLines = new char[] { '\n', '\r' };

        static Constants()
        {

        }
    }
}
