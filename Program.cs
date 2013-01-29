/*Program.cs
 * Description: Nagios extention to check the oldest file in a directory.
 * Created: 2013-01-25
 * Author: Keith Olenchak
 * Version 0.9.0.0
 * 
 * 2013-01-29 - Keith Olenchak:
 * -Added calculations for getting the delta, in minutes, between now and the age of the oldest file.
 * -Added getStatus function, converts ReturnCode enum value in to a string.
 * -Added buidOutput function to handle the final output to STDOUT, it builds the string based on the verbosity level.
 * 
 * 2013-01-28 - Keith Olenchak:
 * -Added new ReturnCode "NOFILES" with the integer value of 10, this is for interal use; it should never be used as an application exit code. When ReturnCode.NOFILES is returned
 *      the application should return 'NoFiles' as this will have the user configured ReturnCode.
 * -Added some exception handling to main()
 * -Formatting optimization changes made to print_usage()
 * -Fixed a bug in the parseArguments() loops
 * 
 * Accepted Flags: -t (--target), -v (--verbose), -V (--version), -h (--help), -w (--warning), -c (--critical), -n (--WarnOnNoFiles), -N (--CritOnNoFiles); 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace QuasarQode.NagiosExtentions
{
    public enum ReturnCode { OK = 0, WARNING = 1, CRITICAL = 2, UNKNOWN = 3, NOFILES = 10 };

    class NagiosExtentions
    {
        public static string wThreshold = null, cThreshold = null;
        public static ReturnCode NoFiles = ReturnCode.OK;
        public enum Flag { TARGET, VERBOSE, VERSION, HELP, WARNING, CRITICAL, WARN_ON_NO_FILES, CRIT_ON_NO_FILES };
        public static int Verbose_level = 0;
        public static DirectoryInfo target;
        public static Exception LastException;
        public static Nagios_Thresholds fileageCheck;

        static int Main(string[] args)
        {
            string error = "SUCCESS";
            ReturnCode ExitCode = ReturnCode.OK;
            DateTime AgeOfOldestFile;
            try
            {
                ExitCode = parseArguments(args, out error);
            }
            catch (IndexOutOfRangeException e)
            {
                error = "NO ARGUMENTS PASSED TO APPLICATION.";
                print_usage(true);
                LastException = e;
                return (int)ReturnCode.UNKNOWN;
            }
            if(ExitCode != ReturnCode.OK)
            {
                Console.Out.WriteLine(string.Format("ERR -- {0}", error));
                return (int)ExitCode;
            }
            bool hasNoFiles;
            ExitCode = getOldestFile(target, out error, out AgeOfOldestFile, out hasNoFiles);
            if (hasNoFiles)
            {
                Console.Out.WriteLine(buidOutput(ExitCode,"No files in directory.", null, null, null));
            }
            else
            {
                int timeDelta;
                ExitCode = getTimeDelta(AgeOfOldestFile, out timeDelta, out error);
                if (ExitCode == ReturnCode.UNKNOWN)
                {
                    string[] debugOutput = { string.Format("Exception caught: {0}", LastException.Message), string.Format("Exception Details: {0}", LastException.ToString())};
                    Console.Out.WriteLine(buidOutput(ExitCode, error, null, debugOutput, null));
                    return (int)ExitCode;
                }
                fileageCheck = new Nagios_Thresholds(wThreshold, cThreshold);
                ExitCode = fileageCheck.checkThreshold(timeDelta);
                Console.Out.WriteLine(buidOutput(ExitCode, string.Format("Oldest file is {0} minutes old.",timeDelta.ToString()), null, null, null));
            }
            return (int)ExitCode;
        }

        static ReturnCode getTimeDelta(DateTime OldestFile, out int __TimeDelta, out string error)
        {
            __TimeDelta = 0;
            error = "SUCCESS";
            long l_timeDelta = DateTime.Now.Ticks - OldestFile.Ticks;
            TimeSpan ts_timeDelta = new TimeSpan(l_timeDelta);
            try
            {
                int timeDelta = Convert.ToInt32(ts_timeDelta.TotalMinutes);
            }
            catch (OverflowException e)
            {
                error = "The delta minutes between now and the age of the oldest file is larger than a 32bit integer.";
                LastException = e;
                return ReturnCode.UNKNOWN;
            }
            return ReturnCode.OK;
        }

        static void print_help()
        {
            print_version();
            print_usage(false);
        }

        static void print_version()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.Out.WriteLine(string.Format("CheckFileAge.exe, Version: {0}", version));
        }

        static void print_usage(bool Short_Usage)
        {
            const string shortUsagepreamble = "\r\nUse CheckFileAge.exe -h for help.";
            const string Usage = "\r\nCheckFileAge.exe -t <target> [-v <verbosity level>][-w <warning value>]\r\n[-c <critical value>][-n or -N]\r\n";
            const string TARGET = "-t or --target: Full directory path to be checked.";
            const string VERBOSE = "-v or --verbose: Set verbosity level of output.";
            const string VERSION = "-V or --version: Outputs the version of this executable.";
            const string HELP = "-h or --help: Displays this usage information.";
            const string WARNING = "-w or --warning: Set the warning threshold in minutes.";
            const string CRITICAL = "-c or --critical: Set the critical threshold in minutes.";
            const string WARN_ON_NO_FILES = "-n or --WarnOnNoFiles: If no files are in the directory we exit with WARNING";
            const string CRIT_ON_NO_FILES = "-N or --CritOnNoFiles: If no files are in the directory we exit with CRITICAL";
            const string NOTE1 = "\r\nNote: No files in target returns OK by default";
            if (Short_Usage)
            {
                Console.Out.WriteLine(shortUsagepreamble);
                Console.Out.WriteLine(Usage);
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Usage);
            sb.AppendLine(TARGET);
            sb.AppendLine(VERBOSE);
            sb.AppendLine(VERSION);
            sb.AppendLine(HELP);
            sb.AppendLine(WARNING);
            sb.AppendLine(CRITICAL);
            sb.AppendLine(WARN_ON_NO_FILES);
            sb.AppendLine(CRIT_ON_NO_FILES);
            sb.AppendLine(NOTE1);
            Console.Out.Write(sb.ToString());
        }

        static ReturnCode parseArguments(string[] args, out string error)
        {
            int index = 0, loopcount;
            error = "SUCCESS";
            Dictionary<string, Flag> _flags;
            Exception exception;
            getFlags(out _flags);
            while (index <= args.Length)
            {
                loopcount = 0;
                foreach (var pair in _flags)
                {
                    if (string.Compare(pair.Key, args[index]) == 0)
                    {
                        executeFlag(pair.Value, args, index, out index, out error, out exception);
                        if (pair.Value == Flag.VERSION || pair.Value == Flag.HELP)
                        {
                            return ReturnCode.UNKNOWN;
                        }
                        break;
                    }
                    loopcount++;
                    if (loopcount > _flags.Count)
                    {
                        print_usage(true);
                        error = string.Format("Invalid flag: {0}.", args[index]);
                        return ReturnCode.UNKNOWN;
                    }
                }
                index++;
            }
            return ReturnCode.OK;
        }

        static ReturnCode getOldestFile(DirectoryInfo dir, out string error, out DateTime AgeOfOldestFile, out bool noFiles)
        {
            error = "SUCCESS";
            noFiles = false;
            if (dir.GetFiles().GetLength(0) <= 0)
            {
                error = "NO FILES IN TARGET DIRECTORY.";
                AgeOfOldestFile = DateTime.Now;
                noFiles = true;
                return ReturnCode.NOFILES;
            }
            var files = from f in dir.EnumerateFiles()
                        orderby f.CreationTime
                        select f;
            AgeOfOldestFile = files.ElementAt(0).CreationTime;
            return ReturnCode.OK;
        }

        static void getFlags(out Dictionary<string, Flag> Flags)
        {
            //Accepted Flags: -t (--target), -v (--verbose), -V (--version), -h (--help), -w (--warning), -c (--critical), -n (--WarnOnNoFiles), -N (--CritOnNoFiles);
            Flags = new Dictionary<string,Flag>();
            Flags.Add("-t", Flag.TARGET);
            Flags.Add("--target", Flag.TARGET);
            Flags.Add("-v", Flag.VERBOSE);
            Flags.Add("--verbose", Flag.VERBOSE);
            Flags.Add("-V", Flag.VERSION);
            Flags.Add("--version", Flag.VERSION);
            Flags.Add("-h", Flag.HELP);
            Flags.Add("--help", Flag.HELP);
            Flags.Add("-w", Flag.WARNING);
            Flags.Add("--warning", Flag.WARNING);
            Flags.Add("-c", Flag.CRITICAL);
            Flags.Add("--critical", Flag.CRITICAL);
            Flags.Add("-n", Flag.WARN_ON_NO_FILES);
            Flags.Add("--WarnOnNoFiles", Flag.WARN_ON_NO_FILES);
            Flags.Add("-N", Flag.CRIT_ON_NO_FILES);
            Flags.Add("--CritOnNoFiles", Flag.CRIT_ON_NO_FILES);
        }

        static void executeFlag(Flag flag, string[] args, int index, out int new_index, out string error, out Exception exception)
        {
            index++;
            error = "SUCCESS";
            exception = null;

            switch (flag)
            {
                case Flag.TARGET:
                    {
                        try
                        {
                            target = new DirectoryInfo(args[index]);
                            index++;
                        }
                        catch (ArgumentException e)
                        {
                            error = "Invalid arguemnt for --target.";
                            exception = e;
                        }
                        catch (PathTooLongException e)
                        {
                            error = "The path argument for --target is too long.";
                            exception = e;
                        }
                        break;
                    }
                case Flag.VERBOSE:
                    {
                        try
                        {
                            Verbose_level = Convert.ToInt32(args[index]);
                            index++;
                        }
                        catch (FormatException e)
                        {
                            error = "Argument for Verbose flag not a sequence of digits";
                            exception = e;
                        }
                        catch (OverflowException e)
                        {
                            error = "Argument for verbose flag is too large of a number";
                            exception = e;
                        }
                        break;
                    }
                case Flag.VERSION:
                    {
                        print_version();
                        break;
                    }
                case Flag.HELP:
                    {
                        print_help();
                        break;
                    }
                case Flag.WARN_ON_NO_FILES:
                    {
                        NoFiles = ReturnCode.WARNING;
                        break;
                    }
                case Flag.CRIT_ON_NO_FILES:
                    {
                        NoFiles = ReturnCode.CRITICAL;
                        break;
                    }
                case Flag.WARNING:
                    {
                        wThreshold = args[index];
                        index++;
                        break;
                    }
                case Flag.CRITICAL:
                    {
                        cThreshold = args[index];
                        index++;
                        break;
                    }
                default:
                    {
                        error = "No operation found for this flag.";
                        break;
                    }
            }
            new_index = index;
        }

        static void getStatus(ReturnCode rc, out string status)
        {
            status = "UNKNOWN";
            switch (rc)
            {
                case ReturnCode.OK:
                    {
                        status = "OK";
                        break;
                    }
                case ReturnCode.WARNING:
                    {
                        status = "WARNING";
                        break;
                    }
                case ReturnCode.CRITICAL:
                    {
                        status = "CRITICAL";
                        break;
                    }
                case ReturnCode.UNKNOWN:
                    {
                        status = "UNKNOWN";
                        break;
                    }
            }
        }

        static string buidOutput(ReturnCode FinalReturnCode, string statusText, string perfData1, string[] MultiLineOutput, string[] MultiLinePerfData)
        {
            StringBuilder output = new StringBuilder();
            string status;
            getStatus(FinalReturnCode, out status);
            output.AppendFormat("{0}: {1}", status, statusText);
            if (perfData1 != null)
            {
                output.AppendFormat("| {0}", perfData1);
            }
            if (Verbose_level >= 2 && MultiLineOutput != null)
            {
                foreach (string line in MultiLineOutput)
                {
                    output.AppendFormat("\r\n{0}",line);
                }
            }
            return "";
        }
    }
}
