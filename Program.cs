using System;
using System.Net.Http;

namespace DWMBlurGlassUpdater
{
    class Program
    {
        static async Task<int> Main()
        {
            string latest = await Updater.GetLatestUnstableUrl();
            Console.WriteLine(latest);
            Console.ReadLine();
            return 0;
        }
    }
}