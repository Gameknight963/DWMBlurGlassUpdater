using System;
using System.Net.Http;
using System.Collections.Generic;

namespace DWMBlurGlassUpdater
{
    class Program
    {
        static async Task<int> Main()
        {
            string url = await Updater.GetLatestUnstableUrl();
            Console.WriteLine(url);
            bool sucess = await Updater.InstallFromUrl(url);
            if (!sucess)
            {
                Console.WriteLine("Destination directory is locked. Make sure to press uninstall in DWMBlurGlassGUI first");
                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();
                return 1;
            }

            Console.WriteLine("Installation sucessful.");
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
            return 0;
        }
    }
}