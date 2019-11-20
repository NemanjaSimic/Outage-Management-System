using System.Text;

namespace CIM.Model
{
    public class ConcreteModelBuildingResult
    {
        private StringBuilder missingValues = new StringBuilder();
        private StringBuilder report = new StringBuilder();
        private bool success = true;
        private int errorCount = 0;
        private int warrningCount = 0;

        public StringBuilder MissingValues
        {
            get { return missingValues; }
            set { missingValues = value; }
        }

        public int ErrorCount
        {
            get { return errorCount; }
            set { success = false;
                  errorCount = value; }
        }

        public int WarrningCount
        {
            get { return warrningCount; }
            set { warrningCount = value; }
        }

        public bool Success
        {
            get { return success; }
        }

        public StringBuilder Report
        {
            get { return report; }
            set { report = value; }
        }
    }
}
