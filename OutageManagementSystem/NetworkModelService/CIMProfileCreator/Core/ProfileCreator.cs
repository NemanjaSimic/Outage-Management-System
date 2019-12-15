using System.Text;
using FTN.ESI.SIMES.CIM.Model;
using FTN.ESI.SIMES.CIM.Model.Utils;

namespace FTN.ESI.SIMES.CIM.Core
{
    public class ProfileCreator
    {
        private StringBuilder sb;

        public StringBuilder CreateProfile(string namespc, string fileName, string productName, string AssemblyVersion, Profile profile)
        {
            sb = new StringBuilder();
            
            ////GENERATE CLASSES AND ENUMERATIONS
            CodeDOMUtil cdom = new CodeDOMUtil(namespc);
            cdom.Message += new CodeDOMUtil.MessageEventHandler(cdom_Message);
            cdom.GenerateCode(profile);

            ////WRITE FILES
            cdom.WriteFiles(AssemblyVersion);
            
            ////COMPILE
			if(productName.Equals(string.Empty))
			{
				cdom.CompileCode(fileName + "CIMProfile");
			}
			else
			{
				cdom.CompileCode(fileName + "CIMProfile_" + productName);
			}

            return sb;
        }

        void cdom_Message(object sender, string message)
        {
            this.sb.Append(message);
        }
    }    
}
