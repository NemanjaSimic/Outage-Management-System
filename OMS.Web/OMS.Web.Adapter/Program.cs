using OMS.Web.Adapter.WebService;
using System;

namespace OMS.Web.Adapter
{
    class Program
    {
        static void Main(string[] args)
        {
            WebServiceHost host = new WebServiceHost();

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
