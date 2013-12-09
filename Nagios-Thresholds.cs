/*Nagios-Thresholds.cs
 * Description: Class for parsing a dealing with the nagios standard for thresholds.
 * Created: 2013-01-28
 * Author: Keith Olenchak
 * Version 1.0.0.2
 * 
 * 2013-01-30 -- Keith Olenchak:
 * -Minor bug fixes, wrong varible names, else if where should have been just if.
 * 
 * 2013-01-29 -- Keith Olenchak:
 * -Converted class to hold threshold values for warning and crit, rather than relying on the program to keep track.
 * -Class initiation now requires both a warning threshold and critical threshold, either can be null but we throw an exception if both are null.
 * -ReturnCode enum made a public member of the namespace rather than the nagiosextentions class.
 * -Added error handling to parseThreshold to throw an exception if the threshold is marked with '@' but no range is defined with ':'.
 * -Finished checkThreshold()
 * 
 * 2013-01-28 -- Keith Olenchak:
 * -Added parseThreshold(string _threshold) function to parse the string and set class variables accordingly.
 * -Added new bool "MaxIsInfinity" to account for occasions when nothing proceeds the ':'
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuasarQode.NagiosExtentions
{
    class Nagios_Thresholds
    {
        private int W_Min, W_Max, C_Min, C_Max;
        private bool W_Range_Inclusive, W_Negative_OK, W_MaxIsInfinity, C_Range_Inclusive, C_Negative_OK, C_MaxIsInfinity, 
            CritIsRange, WarnIsRange, WarningThresholdPresent = true, CriticalThresholdPresent = true;
        public Exception LastExceptionThrown;
        public string LastError;

        /// <summary>
        /// Initiate a new instance of Nagios_Thresholds, and set the threshold now.
        /// </summary>
        /// <param name="WarningThreshold">Warning Threshold string, pass null if none was received.</param>
        /// <param name="CriticalThreshold">Critical Threshold string, pass null if none was received.</param>
        public Nagios_Thresholds(string WarningThreshold, string CriticalThreshold)
        {
            Globals.Globals.LogIt(Logs.Logging.iLogLevel.INFO, "Nagios_Thresholds() Initiated.");
            try
            {
                if (WarningThreshold == null && CriticalThreshold == null)
                {
                    this.LastError = "BOTH THRESHOLDS CANNOT BE NULL";
                    throw new Exception("BOTH THRESHOLDS CANNOT BE NULL");
                }
                if (WarningThreshold == null)
                {
                    this.WarningThresholdPresent = false;
                }
                else
                {
                    this.parseThreshold(WarningThreshold, out this.W_Min, out this.W_Max, out this.W_Negative_OK, out this.W_Range_Inclusive, 
                        out this.W_MaxIsInfinity, out this.WarnIsRange);
                }
                if (CriticalThreshold == null)
                {
                    this.CriticalThresholdPresent = false;
                }
                else
                {
                    this.parseThreshold(CriticalThreshold, out this.C_Min, out this.C_Max, out this.C_Negative_OK, out this.C_Range_Inclusive, 
                        out this.C_MaxIsInfinity, out this.CritIsRange);
                }
            }
            catch (Exception e)
            {
                this.LastExceptionThrown = e;
                throw;
            }
            Globals.Globals.LogIt(Logs.Logging.iLogLevel.DEBUG, "Hit the end of Nagios_Thresholds().");
        }

        /// <summary>
        /// Parses threshold string and returns all the values extracted.
        /// </summary>
        /// <param name="_threshold"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="Negative_OK"></param>
        /// <param name="Range_Inclusive"></param>
        /// <param name="MaxIsInfinity"></param>
        /// <exception cref="System.FormatException">Thrown when '~', '@', and ':' are removed and what remains is not numerical.</exception>
        /// <exception cref="System.Exception">Thrown when an unknown exception is caught.</exception>
        private void parseThreshold(string _threshold, out int min, out int max, out bool Negative_OK, out bool Range_Inclusive, out bool MaxIsInfinity, out bool IsRange)
        {
            min = -1;
            max = -1;
            Negative_OK = false;
            Range_Inclusive = false;
            MaxIsInfinity = false;
            IsRange = false;
            if (_threshold.StartsWith("~"))
            {
                Negative_OK = true;
                _threshold = _threshold.Substring(1);
            }
            else if (_threshold.StartsWith("@"))
            {
                Range_Inclusive = true;
                _threshold = _threshold.Substring(1);
                if (_threshold.Contains(":") == false)
                {
                    this.LastError = "Threshold cannot be marked with '@' without specifying a range using ':'.";
                    throw new Exception("Threshold string is invalid");
                }
            }
            if (_threshold.Contains(":"))
            {
                IsRange = true;
                try
                {
                    if (Negative_OK != true)
                    {
                        string szMin = _threshold.Substring(0, _threshold.IndexOf(":"));
                        min = Convert.ToInt32(szMin);
                    }
                    if (_threshold.IndexOf(":") < _threshold.Length)
                    {
                        string szMax = _threshold.Substring(_threshold.IndexOf(":") + 1);
                        max = Convert.ToInt32(szMax);
                    }
                    else
                    {
                        MaxIsInfinity = true;
                    }
                }
                catch (FormatException e)
                {
                    this.LastError = "Invalid sequence of characters in argument.";
                    this.LastExceptionThrown = e;
                    throw;
                }
                catch (OverflowException e)
                {
                    this.LastError = "Integer overflow, number is larger than an int32.";
                    this.LastExceptionThrown = e;
                    throw;
                }
                catch (Exception e)
                {
                    this.LastError = "An unexpected exception was caught.";
                    this.LastExceptionThrown = e;
                    throw;
                }
            }
            else
            {
                IsRange = false;
                try
                {
                    string szMin = _threshold.Substring(0);
                    min = Convert.ToInt32(szMin);
                }
                catch (FormatException e)
                {
                    this.LastError = "Invalid sequence of characters in argument.";
                    this.LastExceptionThrown = e;
                    throw;
                }
                catch (OverflowException e)
                {
                    this.LastError = "Integer overflow, number is larger than an int32.";
                    this.LastExceptionThrown = e;
                    throw;
                }
                catch (Exception e)
                {
                    this.LastError = "An unexpected exception was caught.";
                    this.LastExceptionThrown = e;
                    throw;
                }
            }
        }

        public ReturnCode checkThreshold(int currentValue)
        {
            if (currentValue < 0 && !this.W_Negative_OK && !this.C_Negative_OK)
            {
                return ReturnCode.CRITICAL;
            }
            if (currentValue < 0 && (this.W_Negative_OK || this.C_Negative_OK))
            {
                return ReturnCode.OK;
            }

            if (this.CriticalThresholdPresent)
            {
                if (this.CritIsRange)
                {
                    if (this.C_Range_Inclusive && currentValue < this.C_Max && currentValue > this.C_Min)
                    {
                        return ReturnCode.CRITICAL;
                    }
                    else if (this.C_MaxIsInfinity && currentValue < this.C_Min)
                    {
                        return ReturnCode.CRITICAL;
                    }
                    else if (currentValue > this.C_Max || currentValue < this.C_Min)
                    {
                        return ReturnCode.CRITICAL;
                    }
                }
                else if (!this.CritIsRange && currentValue > this.C_Min)
                {
                    return ReturnCode.CRITICAL;
                }
            }
            if (this.WarningThresholdPresent)
            {
                if (this.WarnIsRange)
                {
                    if (this.W_Range_Inclusive && currentValue < this.W_Max && currentValue > this.W_Min)
                    {
                        return ReturnCode.WARNING;
                    }
                    else if (this.W_MaxIsInfinity && currentValue < this.W_Min)
                    {
                        return ReturnCode.WARNING;
                    }
                    else if (currentValue > this.W_Max || currentValue < this.W_Min)
                    {
                        return ReturnCode.WARNING;
                    }
                }
                else if (!this.WarnIsRange && currentValue > this.W_Min)
                {
                    return ReturnCode.WARNING;
                }
            }
            return ReturnCode.OK;
        }
    }
}
