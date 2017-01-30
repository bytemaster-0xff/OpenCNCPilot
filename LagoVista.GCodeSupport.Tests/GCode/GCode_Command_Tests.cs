using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LagoVista.Core.GCode;
using System.Collections.Generic;
using System.Diagnostics;

namespace LagoVista.GCodeSupport.Tests
{
    [TestClass]
    public class GCode_Command_Tests
    {

        [TestInitialize]
        public void Init()
        {
            Mocks.FakePlatformSupport.Register();
        }

        [TestMethod]
        public void Parse_File()
        {
            var file = GCodeFile.FromString(GCodeSampleFile.GCODE_FILE);

            Console.WriteLine("Total Run Time MS: " + file.EstimatedRunTime);

            foreach(var cmd in file.Commands)
            {
                Console.WriteLine(cmd.ToString());
            }
        }
    }
}
