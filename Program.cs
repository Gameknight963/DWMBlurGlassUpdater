using System;
using System.Net.Http;
using System.Collections.Generic;

namespace DWMBlurGlassUpdater
{
    class Program
    {
        static async Task<int> Main()
        {
            Console.WriteLine(await Updater.GetLatestUnstableUrl());

            List<string> versions = await Updater.GetVersionsUrls();
            foreach (string v in versions) Console.WriteLine(v);
            Console.ReadLine();
            return 0;
        }
    }
}