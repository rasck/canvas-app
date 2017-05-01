using Newtonsoft.Json;
using OfficeOpenXml;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanvasApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CanvasApiHelper helper = new CanvasApiHelper();
                GroupManager groupManager = new GroupManager();

                Console.WriteLine("Please enter course id (e.g. 5988).");
                string courseId = Console.ReadLine();
                Console.WriteLine("Please enter title (e.g. Dmaa0215).");
                string classname = Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine("Using canvas token from CanvasApp.exe.config file...");
                Console.WriteLine("Get your access token from https://ucn.instructure.com/profile/settings");

                Http http = helper.SetupHttpWithAuth();

                Console.WriteLine("Fecthing data and writing data to file...");
                Console.WriteLine("(same folder as this program is executed from, you might need administrator rights)...");
                Console.WriteLine();

                groupManager
                    .CreateExcelSheetWithGroups(http, courseId, classname)
                    .Wait(); // Because we cannot async/await in our Main method

                Console.WriteLine("All data has been written to excel file!");
            }
            catch (AggregateException e)
            {
                Console.WriteLine();
                Console.WriteLine("One or more errors happened...");

                var exList = e.InnerExceptions;
                Console.WriteLine();
                Console.WriteLine("Errors:");
                foreach (var ex in exList)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error with message " + e.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
