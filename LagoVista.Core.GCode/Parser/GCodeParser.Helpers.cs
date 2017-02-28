using LagoVista.Core.GCode.Commands;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LagoVista.Core.GCode.Parser
{
    public partial class GCodeParser
    {
        private void Prune(List<Word> words, string line, int lineNumber)
        {
            while (words.Count > 0)
            {
                /* Ignore for now,  
                   From comment that generates parameter:
                        #If we're using pronterface, we need to change raster data / and + in the base64 alphabet to letter 9. This loses a little intensity in pure blacks but keeps pronterface happy.
                    if( self.options.pronterface ):
                        b64 = b64.replace("+", "9").replace("/", "9");
                */
                var prontoFaceDCommand = words.Where(wrd => wrd.Command == 'D').FirstOrDefault();
                if (prontoFaceDCommand != null)
                {
                    words.Remove(prontoFaceDCommand);
                    continue;
                }

                /* ???? */ 
                var unknown = words.Where(wrd => wrd.Command == 'B').FirstOrDefault();
                if (unknown != null)
                {
                    words.Remove(unknown);
                    continue;
                }


                if (words.First().Command == 'G' && !MotionCommands.Contains(words.First().Parameter))
                {
                    #region UnitPlaneDistanceMode

                    double param = words.First().Parameter;

                    if (param == 90)
                    {
                        State.DistanceMode = ParseDistanceMode.Absolute;
                        words.RemoveAt(0);
                        continue;
                    }
                    if (param == 91)
                    {
                        State.DistanceMode = ParseDistanceMode.Incremental;
                        words.RemoveAt(0);
                        continue;
                    }
                    if (param == 90.1)
                    {
                        State.ArcDistanceMode = ParseDistanceMode.Absolute;
                        words.RemoveAt(0);
                        continue;
                    }
                    if (param == 91.1)
                    {
                        State.ArcDistanceMode = ParseDistanceMode.Incremental;
                        words.RemoveAt(0);
                        continue;
                    }
                    if (param == 21)
                    {
                        State.Unit = ParseUnit.Metric;
                        words.RemoveAt(0);
                        continue;
                    }
                    if (param == 20)
                    {
                        State.Unit = ParseUnit.Imperial;
                        words.RemoveAt(0);
                        continue;
                    }
                    if (param == 17)
                    {
                        State.Plane = ArcPlane.XY;
                        words.RemoveAt(0);
                        continue;
                    }
                    if (param == 18)
                    {
                        State.Plane = ArcPlane.ZX;
                        words.RemoveAt(0);
                        continue;
                    }
                    if (param == 19)
                    {
                        State.Plane = ArcPlane.YZ;
                        words.RemoveAt(0);
                        continue;
                    }

                    words.RemoveAt(0);  //unsupported G-Command
                    continue;
                    #endregion
                }

                break;
            }
        }

        private void Validate(List<Word> words)
        {
            for (int i = 0; i < words.Count; i++)
            {
                if (!ValidWords.Contains(words[i].Command.ToString()))
                {
                    words.RemoveAt(i);
                    continue;
                }

                if (words[i].Command != 'F')
                    continue;

                State.Feed = words[i].Parameter;
                words.RemoveAt(i);
                continue;
            }
        }

        private List<Word> FindWords(string line)
        {
            var matches = GCodeSplitter.Matches(line);

            var words = new List<Word>(matches.Count);

            var decimalFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };

            foreach (Match match in matches)
            {
                words.Add(new Word()
                {
                    Command = match.Groups[1].Value[0],
                    Parameter = double.Parse(match.Groups[2].Value, decimalFormat),
                    FullWord = $"{match.Groups[1].Value}{match.Groups[2].Value}"
                });
            }

            return words;
        }
    }
}
