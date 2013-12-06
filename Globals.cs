/*Globals.CS Copyright Keith Olenchak 2012-2013
 * 
 * 11/01/2013 -- Keith Olenchak
 *  -Added new dictionary runtime_Configs to store special runtime settings such as arguments passed to the exe on startup.
 */
using System;
//using System.Windows.Forms;
using System.IO;
using QuasarQode.Logs;
using System.Collections.Generic;
using System.Threading;
using System.Net;
//using QuasarQode.Configuration;

namespace QuasarQode.Globals
{
    public static class Globals
    {
        //public static TextBox tb_progress;
        public static bool Debug = false;
        public static bool ConfigRun = false;
        private static bool firstEntry = true;
        public static bool looper = false;
        public static Thread tLooper;
        public static Dictionary<string, string> dict_Settings, runtime_Configs;
        public delegate void UpdateText_ThreadSafe(string text);

        /*public static int Report(string message, bool debug_text = false)
        {
            int retval = 0;
            
            if (debug_text && !Debug)
            {
                return 901;
            }
            if (debug_text && Debug)
            {
                UpdateText("[DEBUG]" + message + "\r\n");
                if (firstEntry)
                {
                    Logging.Custom("Display.log", "[DEBUG]" + message, "\r\n\r\n");
                    firstEntry = false;
                }
                else
                    Logging.Custom("Display.log", "[DEBUG]" + message);
            }
            else
            {
                UpdateText(message + "\r\n");
                if (firstEntry)
                {
                    Logging.Custom("Display.log", message, "\r\n\r\n");
                    firstEntry = false;
                }
                else
                    Logging.Custom("Display.log", message);
            }
            return retval;
        }

        private static void UpdateText(string text)
        {
            if (tb_progress.InvokeRequired)
            {
                UpdateText_ThreadSafe d = new UpdateText_ThreadSafe(UpdateText);
                tb_progress.Invoke(d, new object[] { text });
            }
            else
            {
                tb_progress.AppendText(text);
            }
        }*/

        /*public static int LogIt(string _LogLevel, string _message, string _log = "Main", bool _fatalerrorLogLevel10 = false)
        {
            int logLevel_Message = 10, logLevel_Setting = 0;
            if (_fatalerrorLogLevel10 == false)
            {
                if (Int32.TryParse(dict_Settings["Log-Level"], out logLevel_Setting) == false)
                {
                    throw new FormatException("Could not parse dict_Settings[Debug-Mode] to Int32.");
                }
                if (Int32.TryParse(_LogLevel, out logLevel_Setting) == false)
                {
                    throw new FormatException("Could not parse _LogLevel Argument to Int32 in LogIt().");
                }
            }
            if (logLevel_Message >= logLevel_Setting)
            {
                Logging.Custom(_log + ".log", string.Format("[{0}] - {1}", _LogLevel, _message));
            }
            return 0;
        }*/

        public static int LogIt(Logging.iLogLevel _LogLevel, string _message)
        {
            Logging.iLogLevel logLevel_Message = _LogLevel;
            string _log = "CheckFileAge_Misc";
            if (logLevel_Message >= Logging.iLogLevel.EXCEPTION)
            {
                _log = "CheckFileAge_Exceptions";
            }
            Logging.Custom(_log + ".log", string.Format("[{0}] - {1}", Logging.szLogLevel[(int)_LogLevel], _message));
            return 0;
        }

        /*public static int LoadSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            Configuration.Configuration.LoadConfiguration(out settings);
            foreach (KeyValuePair<string, string> keyPair in settings)
            {
                if (dict_Settings.ContainsKey(keyPair.Key))
                {
                    dict_Settings[keyPair.Key] = keyPair.Value;
                }
                else
                {
                    dict_Settings.Add(keyPair.Key, keyPair.Value);
                    LogIt("3", string.Format("A setting that does not exist in the default settings dictionary found. Added setting {0} = {1}", keyPair.Key, keyPair.Value));
                }
            }
            if (dict_Settings["Debug-Mode"].ToUpper().StartsWith("T"))
                Debug = true;
            else
                Debug = false;
            LogIt("2", "Settings Loaded.");
            return 0;
        }

        public static int SaveSettings()
        {
            Configuration.Configuration.SaveConfiguration(dict_Settings);
            Report("Settings saved.");
            LogIt("2", "Settings Saved.");
            if (dict_Settings["Debug-Mode"].ToUpper().StartsWith("T"))
                Debug = true;
            else
                Debug = false;
            return 0;
        }

        public static int DefaultSettings()
        {
            Configuration.Configuration.DefaultConfiguration(out dict_Settings);
            Configuration.Configuration.DefaultRuntimeConfiguration(out runtime_Configs);
            Report("Default Settings Loaded");
            return 0;
        }

        public static void CheckForSettings()
        {
            FileInfo fi = new FileInfo("Configuration.xml");
            if (fi.Exists)
            {
                LogIt("5", "Existing configuration.xml found. Loading those settings.");
                LoadSettings();
                Report("Existing Configuration file loaded.");
            }
            else
            {
                LogIt("5", "No existing configuration.xml found. Loading default settings.");
                DefaultSettings();
                Report("Default Settings Loaded");
            }
            if (dict_Settings["Debug-Mode"].ToUpper().StartsWith("T"))
                Debug = true;
            else
                Debug = false;
        }

        public static int Config2Table(out string _table)
        {
            _table = string.Empty;
            foreach (KeyValuePair<string, string> pair in dict_Settings)
            {
                int index = 0;
                string tableEntry = string.Format("{0} = {1} \r\n", pair.Key, pair.Value);
                if (_table.Length - 1 > 0)
                    index = _table.Length - 1;
                _table = _table.Insert(index, tableEntry);
                LogIt("0", string.Format("'{0}' inserted in to _table at index {1}.", tableEntry.TrimEnd(new char[]{'\r','\n'}), index.ToString()));
            }
            return 0;
        }*/

        public static int ReplaceKeywordsInString(string input, out string _newString)
        {
            _newString = input;
            _newString = _newString.Replace("%time%", DateTime.Now.ToString());
            _newString = _newString.Replace("%host%", Dns.GetHostName());
            _newString = _newString.Replace("%ServiceStartType%", dict_Settings["ServiceStartType"]);
            Globals.LogIt(Logging.iLogLevel.DEBUG, string.Format("Replaced keyword is string \"{0}\", final value: \"{1}\".", input, _newString));
            return 0;
        }
    }
}