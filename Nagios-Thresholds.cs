/*Nagios-Thresholds.cs
 * Description: Class for parsing a dealing with the nagios standard for thresholds.
 * Created: 2013-01-28
 * Author: Keith Olenchak
 * Version 0.5.2.0
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
        private int min, max;
        private bool Range_Inclusive = false, Negative_OK = false, MaxIsInfinity = false;
        public Exception LastExceptionThrown;
        public string LastError;

        /// <summary>
        /// Initiate a new instance of Nagios_Thresholds
        /// </summary>
        public Nagios_Thresholds()
        {
        }

        /// <summary>
        /// Initiate a new instance of Nagios_Thresholds, and set the threshold now.
        /// </summary>
        /// <param name="__threshold">The command line argument containing the threshold string</param>
        /// <exception cref="System.FormatException">Thrown when '~', '@', and ':' are removed and what remains is not numerical.</exception>
        /// <exception cref="System.Exception">Thrown when an unknown exception is caught.</exception>
        public Nagios_Thresholds(string __threshold)
        {
            try
            {
                this.parseThreshold(__threshold);
            }
            catch (Exception e)
            {
                this.LastExceptionThrown = e;
                throw;
            }
        }

        /// <summary>
        /// Parses the threshold string.
        /// </summary>
        /// <param name="__threshold">The command line argument containing the threshold string</param>
        /// <exception cref="System.FormatException">Thrown when '~', '@', and ':' are removed and what remains is not numerical.</exception>
        /// <exception cref="System.Exception">Thrown when an unknown exception is caught.</exception>
        public void parseThreshold(string _threshold)
        {
            if (_threshold.StartsWith("~"))
            {
                this.Negative_OK = true;
                _threshold = _threshold.Substring(1);
            }
            else if (_threshold.StartsWith("@"))
            {
                this.Range_Inclusive = true;
                _threshold = _threshold.Substring(1);
            }
            if (_threshold.Contains(":"))
            {
                try
                {                 
                    if (this.Negative_OK != true)
                    {
                        string szMin = _threshold.Substring(0, _threshold.IndexOf(":"));
                        this.min = Convert.ToInt32(szMin);
                    }
                    if (_threshold.IndexOf(":") < _threshold.Length)
                    {
                        string szMax = _threshold.Substring(_threshold.IndexOf(":") + 1);
                        this.max = Convert.ToInt32(szMax);
                    }
                    else
                    {
                        this.MaxIsInfinity = true;
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
        }
    }
}
