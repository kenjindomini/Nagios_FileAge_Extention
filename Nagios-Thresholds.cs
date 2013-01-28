/*Nagios-Thresholds.cs
 * Description: Class for parsing a dealing with the nagios standard for thresholds.
 * Created: 2013-01-28
 * Author: Keith Olenchak
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuasarQode.NagiosExtentions
{
    class Nagios_Thresholds
    {
        private int min = 0, max = 0;
        private bool Range_Inclusive = false, Negative_OK = false;

        public Nagios_Thresholds()
        {
        }

        public Nagios_Thresholds(string __threshold)
        {
            parseThreshold(__threshold);
        }

        public int parseThreshold(string _threshold)
        {
            return 0;
        }
    }
}
