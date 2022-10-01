using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SoftMatrix.PBIRS.ProxyHost
{
    internal class Program
    {
        static void Main(string[] args) 
        {
            string baseAddress = "http://*:9000/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Console.WriteLine("Prower BI Report Server Proxy Host Started, Listen on " + baseAddress);
                Console.WriteLine("Press Any Key To Exit..." );
                Console.ReadLine();
            }
        }
    }
}
