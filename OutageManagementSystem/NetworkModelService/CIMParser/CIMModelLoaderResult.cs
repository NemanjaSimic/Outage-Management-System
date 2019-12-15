using System.Text;

namespace CIMParser
{
    public class CIMModelLoaderResult
    {
        private StringBuilder report = new StringBuilder();
        private bool success = true;

        public CIMModelLoaderResult()
        {
            this.success = true;
        }

        public CIMModelLoaderResult(bool success) 
        {
            this.success = success;
        }

        public StringBuilder Report
        {
            get { return report; }
            set { report = value; }
        }

        public bool Success
        {
            get { return success; }
            set { success = value; }
        }
    }
}
