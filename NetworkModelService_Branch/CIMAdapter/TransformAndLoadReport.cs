using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.CIMAdapter
{
    public class TransformAndLoadReport
    {
        private StringBuilder report = new StringBuilder();
        private bool success = true;


        public StringBuilder Report
        {
            get
            {
                return report;
            }
            set
            {
                report = value;
            }
        }

        public bool Success
        {
            get
            {
                return success;
            }
            set
            {
                success = value;
            }
        }
    }
}
