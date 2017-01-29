using System.Globalization;

namespace LagoVista.GCode.Sender
{
    public enum FirmwareTypes
    {
        GRBL1_1,
        Marlin,
        Marlin_Laser
    }

    public enum OperatingMode
    {
        Manual,
        SendingJob,
        ProbingHeightMap,
        ProbingHeight,
        Disconnected
    }


    public enum StatusMessageTypes
    {
        ReceviedLine,
        SentLine,
        Info,
        Warning,
        FatalError
    }

    public class Constants
	{
		public static NumberFormatInfo DecimalParseFormat = new NumberFormatInfo() { NumberDecimalSeparator = "."};

        public static NumberFormatInfo DecimalOutputFormat
		{
			get
			{
				return new NumberFormatInfo() { NumberDecimalSeparator = "."};
			}
		}

		public static string FileFilterGCode = "GCode|*.g;*.tap;*.nc;*.ngc|All Files|*.*";
		public static string FileFilterHeightMap = "Height Maps|*.hmap|All Files|*.*";

		public static string FilePathErrors = "Resources\\GrblErrors.txt";
		public static string FilePathWebsite = "Resources\\index.html";

		public static char[] NewLines = new char[] {'\n', '\r'};

		static Constants()
		{
			
		}
	}
}
