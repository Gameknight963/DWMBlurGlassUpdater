using System;
using System.Net.Http;

namespace DWMBlurGlassUpdater
{
    class Program
    {
        static async Task<int> Main()
        {
            Console.WriteLine(await Updater.GetLatestUnstableUrl());
            Console.WriteLine(await Updater.GetVersionUrl("2.3.2"));
            Console.ReadLine();
            return 0;
        }
    }
}