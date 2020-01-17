using System;
using OMS.Web.Adapter.WebService;
using OMS.Web.Common;

namespace OMS.Web.Adapter
{
    class Program
    {
        static void Main(string[] args)
        {
            WebServiceHost host = new WebServiceHost(AppSettings.Get<string>("webServiceUrl"));

            try
            {
                host.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occured during WebServiceHost Open(): {e.Message}");
                throw;
            }



            Console.WriteLine("Press enter to close the app.");
            Console.ReadLine();
        }
    }
}
