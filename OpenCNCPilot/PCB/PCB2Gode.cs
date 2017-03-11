using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.PCB
{
    public class PCB2Gode
    {
        static Process _eagleULPProcess;

        public static void CreateGCode()
        {
            var ulpParams = @"-C'RUN C:\EAGLE-7.7.0\ulp\pcb-gcode-3.6.2.4\pcb-gcode.ulp; QUIT' ";
            var brdParams = @"'D:\OpenCNCPilot\Tests\LagoVista.EaglePCB.Tests\KegeratorController.brd'";
            var eagleDir = @"C:\Eagle-7.7.0";
            var eagleBin = "EagleCon.exe";
            var fullArgs = $@"{ulpParams} {brdParams}";

            Debug.WriteLine(fullArgs);

            var startupFile = $@"{eagleDir}\bin\{eagleBin}";

            _eagleULPProcess = new Process();
            _eagleULPProcess.Exited += EagleULP_Exited;
            _eagleULPProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = startupFile,
                WorkingDirectory = $@"{eagleDir}\bin",
                Arguments = fullArgs

            };

            _eagleULPProcess.Start();
        }

        private static void EagleULP_Exited(object sender, EventArgs e)
        {
            Debug.WriteLine(_eagleULPProcess.ExitCode);
        }
    }
}
