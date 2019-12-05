using System;
using OMS.Web.Adapter.Mock.Client;

namespace OMS.Web.Adapter.Mock
{
    class Program
    {
        static void Main(string[] args)
        {

            ClientProxy proxy = new ClientProxy("net.tcp://localhost:9990/WebService");

            try
            {
                proxy.UpdateGraph(null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occured during ClientProxy.UpdateGraph(): {e.Message}");
            }

            Console.ReadLine();
        }
    }
}
