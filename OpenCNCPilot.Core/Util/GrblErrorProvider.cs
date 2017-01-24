using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace OpenCNCPilot.Core.Util
{
    public class GrblErrorProvider
    {
        Platform.IStorage _storage;
        Platform.ILogger _logger;
        Dictionary<int, string> Errors;

        private GrblErrorProvider(Platform.IStorage storage, Platform.ILogger logger)
        {
            _storage = storage;
            _logger = logger;

            _logger.WriteLine("Loading GRBL Error Database");

            Errors = new Dictionary<int, string>();

            if (!_storage.Exists(Constants.FilePathErrors))
            {
                _logger.WriteLine($"File Missing: Constants.FilePathErrors");
                return;
            }

            string ErrorFile;

            try
            {
                ErrorFile = _storage.ReadAllText(Constants.FilePathErrors);
            }
            catch (Exception ex)
            {
                _logger.WriteLine(ex.Message);
                return;
            }

            Regex LineParser = new Regex(@"([0-9]+)\t([^\n^\r]*)");     //test here https://www.regex101.com/r/hO5zI1/2

            MatchCollection mc = LineParser.Matches(ErrorFile);

            foreach (Match m in mc)
            {
                int errorNo = int.Parse(m.Groups[1].Value);

                Errors.Add(errorNo, m.Groups[2].Value);
            }

            _logger.WriteLine("Loaded GRBL Error Database");
        }

        public string GetErrorMessage(int errorCode)
        {
            if (Errors.ContainsKey(errorCode))
                return Errors[errorCode];
            else
                return $"Unknown Error: {errorCode}";
        }

        static Regex ErrorExp = new Regex(@"Invalid gcode ID:(\d+)");
        private string ErrorMatchEvaluator(Match m)
        {
            return GetErrorMessage(int.Parse(m.Groups[1].Value));
        }

        public string ExpandError(string error)
        {
            return ErrorExp.Replace(error, ErrorMatchEvaluator);
        }

        static GrblErrorProvider _instance;

        public static GrblErrorProvider Instance
        {
            get
            {
                if(_instance == null)
                {
                    throw new Exception("Please call static init method first.");
                }

                return _instance;
            }
        }

        public static void Init(Platform.IStorage storage, Platform.ILogger logger)
        {
            _instance = new GrblErrorProvider(storage, logger);
        }


    }
}
