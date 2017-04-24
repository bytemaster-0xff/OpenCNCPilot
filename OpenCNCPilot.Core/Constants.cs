﻿using System.Globalization;

namespace LagoVista.GCode.Sender
{
    public enum JogDirections
    {
        YPlus,
        YMinus,
        XPlus,
        XMinus,
        ZPlus,
        ZMinus
    }

    public enum MachineOrigin
    {
        Top_Left,
        Bottom_Left,
        Top_Right,
        Bottom_Right,
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
        Marlin_Laser
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

        public static string PCBProject = "PCB Project|*.pcbproj|All Files|*.*";
        public static string FileFilterPCB = "Eagle|*.brd|All Files|*.*";
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
