/* Logging.cs
 * This is my custom logging class
 * 
 * 20-04-2015 - Keith Olenchak
 * -Removed messageboxes, now logging exception to the debug list that is printed to stdout when verbose is >=2.
 * 
 * 17-04-2015 - Keith Olenchak
 * -Only pop up messageboxes in debug builds.
 * 
 * 06-12-2013 - Keith Olenchak
 * -Added iLogLevel enum.
 * -Added szLogLevel list to convert loglevel to a string.
 * 
 * 13-12-2012 - Keith Olenchak
 * Added optional argument 'pre_fix' to custom(). This will insert any text prior to the datetime stamp. Primary purpose is for adding 2 carriage returns between previous executions
 * and the current execution's log entries.
 * 
 * 29-07-2012 - Keith Olenchak
 * Logs are now thread safe
 * Logs also now have a size limit of 100KB, they are backed up to a .bak file that gets overwritten every time.
 * 
 * 25-07-2012 - Keith Olenchak
 * I broke the logging class out in to its own CS file and thus its own name space "Quasar.Logs".
 */

using System;
using System.IO;
using QuasarQode.Globals;
using System.Collections.Generic;
using QuasarQode.NagiosExtentions;

namespace QuasarQode.Logs
{

public class Logging
	{
		private static int LOG_LEVEL = 0;
		private static long Log_Size_Limit = 102400;
		private static readonly object _qqsync = new object();
		private static readonly object _custom = new object();
        public enum iLogLevel : int { DEBUG = 0, INFO = 2, WARNING = 4, ERROR = 6, EXCEPTION = 8, FATALEXCEPTION = 10 };
        public static List<string> szLogLevel = new List<string>{"Debug", "1", "Info", "3", "Warning", "5", "Error", "7", "Exception", "9", "FatalException"};

		public static int quasar (int iLog_Level, string strLog_Message)
		{
			/*RetVal deffinitions:
			 * 0 Success
			 * 1001 UnautherizedAccessException
			 * 1002 Generic IOException
			 */
			int RetVal = 0;
			lock (_qqsync) {
				FileInfo fi = new FileInfo("QQ.log");
				StreamWriter sw =fi.AppendText();
				if (iLog_Level >= LOG_LEVEL) {
					//Create main log (QQ.log) in current directory.
					try {
						if (fi.Exists) {
							if(fi.Length > Log_Size_Limit) {
								fi.CopyTo("QQ.log.bak", true);
								fi.Delete();
							}
						}
						if (!fi.Exists) {
							sw.WriteLine(DateTime.Now.ToString () + " - [0] - File Created");
						}
						sw.WriteLine(DateTime.Now.ToString () + " - [" + iLog_Level.ToString () + "] - " + strLog_Message);
						sw.Close();
					} catch (UnauthorizedAccessException e) {
                        string ex = e.Message;
						RetVal = 1001;
					} catch (IOException e) {
                        string ex = e.Message;
						RetVal = 1002;
					}
				}
			}
			return RetVal;
		}

		public static int Custom (string filename, string data, string pre_fix = null, bool overwrite = false)
		{
			int RetVal = 0;
			lock (_custom) {
				FileInfo fi = new FileInfo ("logs/" + filename);
                StreamWriter sw;
				try {
                    if (!Directory.Exists("logs"))
                        Directory.CreateDirectory("logs");
                    if (overwrite)
                    {
                        fi.Delete();
                        sw = fi.CreateText();
                    }
                    else
                    {
                        if (fi.Exists && !overwrite)
                        {
                            if (fi.Length > Log_Size_Limit)
                            {
                                fi.CopyTo("logs/" + filename + ".bak", true);
                                fi.Delete();
                            }
                        }
                        sw = fi.AppendText();
                    }
                    if (!fi.Exists)
                    {
                        sw.WriteLine(DateTime.Now.ToString() + " - [0] - File Created");
                    }
                    if (pre_fix != null)
                        sw.WriteLine(pre_fix + DateTime.Now.ToString() + " - " + data);
                    else
                        sw.WriteLine (DateTime.Now.ToString () + " - " + data);
					sw.Close ();
                    sw.Dispose();
				} catch (UnauthorizedAccessException e) {
                    NagiosExtentions.FileAgeCheck.debugOutput.Add("UnauthorizedAccessException caught in QuasarQode.logs: " + e.Message);
					RetVal = 1001;
				} catch (IOException e) {
                    NagiosExtentions.FileAgeCheck.debugOutput.Add("IOException caught in QuasarQode.logs: " + e.Message);
					RetVal = 1002;
				}
                catch (Exception e)
                {
                    NagiosExtentions.FileAgeCheck.debugOutput.Add("Generic Exception caught in QuasarQode.logs: " + e.Message);
                    RetVal = 1003;
                }
			}
			return RetVal;
		}
	}
}