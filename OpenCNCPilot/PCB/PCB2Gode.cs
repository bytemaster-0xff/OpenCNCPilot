using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LagoVista.GCode.Sender.Application.PCB
{
    public class PCB2Gode
    {
        static Process _eagleULPProcess;
        static LagoVista.EaglePCB.Models.PCBProject _project;

        public static void CreateGCode(String boardFile, LagoVista.EaglePCB.Models.PCBProject project)
        {
            _project = project;
            if (String.IsNullOrEmpty(Properties.Settings.Default.EagleConExecutable) ||
                !System.IO.File.Exists(Properties.Settings.Default.EagleConExecutable))
            {
                MessageBox.Show("Please locate the file EagleCon.exe from your Eagle Directory, this is usually installed in C:\\ ");

                var dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = "eaglecon.exe";
                dlg.Filter = "EXE FIles (*.exe)|*.exe";
                var result = dlg.ShowDialog();
                if (String.IsNullOrEmpty(dlg.FileName))
                {
                    return;
                }

                var info = new System.IO.FileInfo(dlg.FileName);

                if (!System.IO.File.Exists(dlg.FileName) || info.Name.ToLower() != "eaglecon.exe")
                {
                    MessageBox.Show("Sorry, please find the file EagleCon.exe");
                    return;
                }

                Properties.Settings.Default.EagleConExecutable = dlg.FileName;
                Properties.Settings.Default.Save();
            }

            if (String.IsNullOrEmpty(Properties.Settings.Default.PCBGCodeULP) ||
                !System.IO.File.Exists(Properties.Settings.Default.PCBGCodeULP))
            {
                MessageBox.Show("Please locate the file pcb-gcode-setup.ulp ");

                var dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Filter = "Eagle ULP (*.ulp)|*.ulp";
                var result = dlg.ShowDialog();
                if (String.IsNullOrEmpty(dlg.FileName))
                {
                    return;
                }

                var info = new System.IO.FileInfo(dlg.FileName);

                if (!System.IO.File.Exists(dlg.FileName) || info.Name.ToLower() != "pcb-gcode-setup.ulp")
                {
                    MessageBox.Show("Sorry, please find the file pcb-gcode-setup.ulp");
                    return;
                }

                Properties.Settings.Default.PCBGCodeULP = dlg.FileName.ToLower();
                Properties.Settings.Default.Save();
            }

            var eagleConFileInfo = new System.IO.FileInfo(Properties.Settings.Default.EagleConExecutable);


            var ulpParams = $@"-C ""RUN '{Properties.Settings.Default.PCBGCodeULP}'; set confirm no; quit"" ";
            var fullArgs = $@"{ulpParams} ""{boardFile}""";

            _eagleULPProcess = new Process();
            _eagleULPProcess.EnableRaisingEvents = true;
            _eagleULPProcess.Exited += EagleULP_Exited;
            _eagleULPProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = eagleConFileInfo.Name,
                WorkingDirectory = eagleConFileInfo.DirectoryName,
                Arguments = fullArgs,
            };

            _eagleULPProcess.Start();
        }

        private static async void EagleULP_Exited(object sender, EventArgs e)
        {
            var boardFileInfo = new System.IO.FileInfo(_project.EagleBRDFilePath);
            var baseBoardName = boardFileInfo.Name.Replace(".brd", "");

            var topEtchFilePath = System.IO.Path.Combine(boardFileInfo.DirectoryName, $"{baseBoardName}.top.etch.tap");
            var bottomEtchFilePath = System.IO.Path.Combine(boardFileInfo.DirectoryName, $"{baseBoardName}.bot.etch.tap");

            if (!System.IO.File.Exists(topEtchFilePath) ||
                !System.IO.File.Exists(topEtchFilePath))
            {
                MessageBox.Show("Sorry, we could not locate the newly created etching files if you save them to a different location or with a different file name, you can manually assign them with the project settings.");
                _project = null;
                return;
            }

            var topEtchFileInfo = new System.IO.FileInfo(topEtchFilePath);
            if ((DateTime.Now - topEtchFileInfo.LastWriteTime) > TimeSpan.FromMinutes(5))
            {
                if (MessageBox.Show("The generated etching files we found are older than 5 minutes, this could be because the process failed or the application sat idle.  Would you like to use these files?", "Use Old Files?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    _project = null;
                    return;
                }
            }

            if(_project == null)
            {
                MessageBox.Show("Sorry, there was an error associating the genreated files to your project.  Please manually attach them under project settings.");
                return;
            }

            _project.TopEtchignFilePath = topEtchFilePath;
            _project.BottomEtchingFilePath = bottomEtchFilePath;
            await _project.SaveAsync();

            _project = null;
        }
    }
}
