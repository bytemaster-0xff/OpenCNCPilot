namespace OpenCNCPilot.Core.GCode.GCodeCommands
{
	public class Command
	{
        public const string GCode_Move_On = "G0";
        public const string GCode_Move_Off = "G1";

        public const string GCode_Move_Arch_On = "G2";
        public const string GCode_Move_Arch_Off = "G3";

        public const string GCode_SetToInches = "G20";
        public const string GCode_SetToMillimeters = "G21";
        public const string GCode_MoveToOrigin = "G28";

        public const string GCode_SetAbsoluteMode = "M82";
        public const string GCode_SetRelativeMode = "M83";

        public const string GCode_GetEnpointStatus = "M119";
        public const string GCode_GetCurrentPosition = "M114";
	}
}
