using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace DWMBlurGlassUpdater
{
    class Program
    {
        public static bool noPause = false;
        public static bool silent = false;

        static async Task<int> Main(string[] args)
        {
            if (args.Length == 0)
            {
                return await InstallLatest();
            }

            List<string> positionalArgs = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                switch (arg)
                {
                    case "--no-pause":
                        noPause = true;
                        break;
                    case "--silent":
                        silent = true;
                        noPause = true;
                        break;
                    default:
                        positionalArgs.Add(args[i]);
                        break;
                }
            }

            string command = positionalArgs[0].ToLower();

            switch (command)
            {
                case "--help":
                case "-h":
                case "/?":
                    PrintHelp();
                    return 0;

                case "--version":
                case "-v":
                case "/v":
                    PrintVersion();
                    return 0;

                case "check":
                    await PrintLatestTag();
                    return 0;

                case "install":
                    if (positionalArgs.Count < 2)
                    {
                        Console.WriteLine("Missing install target. Use --help for usage");
                        return 2;
                    }

                    string target = positionalArgs[1].ToLower();
                    switch (target)
                    {
                        case "latest": return await InstallLatest();
                        case "unstable": return await InstallUnstable();
                        default: return await InstallVersion(target);
                    }

                default:
                    Console.WriteLine("Invalid arguments. Use --help for usage");
                    return 2;
            }
        }

        static async Task<int> Install(string url)
        {
            if (!silent) Console.WriteLine($"Installing {url}");
            bool success = await Updater.InstallFromUrl(url);
            if (!success)
            {
                Console.WriteLine("Destination directory is locked. Make sure uninstall in DWMBlurGlass first.");
                Console.WriteLine("If the issue persists, restart dwm.exe");
                EnterToContinue();
                return 3;
            }

            if (!silent) Console.WriteLine("Installation sucessful.");
            else Console.WriteLine();
            EnterToContinue();
            return 0;
        }

        static async Task<int> InstallVersion(string version)
        {
            string url = await Updater.GetVersionUrl(version);
            return await Install(url);
        }

        static async Task<int> InstallLatest()
        {
            string url = await Updater.GetLatestUrl();
            return await Install(url);
        }

        static async Task<int> InstallUnstable()
        {
            string url = await Updater.GetLatestUnstableUrl();
            return await Install(url);
        }

        static async Task PrintLatestTag()
        {
            string tag = await Updater.GetLatestTag();
            Console.WriteLine(tag);
        }

        static void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  \\.DWMBlurGlassUpdater.exe [command] [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  install latest           Install the latest stable release");
            Console.WriteLine("  install unstable         Install the latest unstable (pre-release) version");
            Console.WriteLine("  install <version>        Install a specific version, e.g. 2.3.1");
            Console.WriteLine("  check                    Print the latest version tag from GitHub");
            Console.WriteLine("  --help, -h, /?           Show this help message");
            Console.WriteLine("  --version, -v, /v        Print updater version");
            Console.WriteLine();
            Console.WriteLine("Flags:");
            Console.WriteLine("  --no-pause               Do not wait for Enter after completion");
            Console.WriteLine("  --silent                 Suppress output messages (implies --no-pause)");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  updater.exe install latest");
            Console.WriteLine("  updater.exe install 2.3.1 --no-pause");
            Console.WriteLine("  updater.exe check --silent");
            Console.WriteLine();
        }


        static void PrintVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Console.WriteLine($"DWMBlurGlassUpdater " +
                $"v{assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version}");
        }

        /// <summary>
        /// Shows a message that says "Press Enter to continue . . ."
        /// and waits for the user to press Enter.
        /// </summary>
        /// <remarks>
        /// If --silent or --no-pause are passed,
        /// this does nothing.
        /// </remarks>
        public static void EnterToContinue()
        {
            if (!noPause)
            {
                Console.WriteLine("\nPress enter to continue . . .");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Shows an exception with the given message, 
        /// then waits for enter. If the --no-pause flag is not given, it
        /// will not wait. If the --silent flag is given,
        /// this will not do anything.
        /// </summary>
        /// <remarks>
        /// This function adds a colon to the message for you, 
        /// do not include one in your messsage.
        /// </remarks>
        /// <param name="message">The message to be shown.</param>
        /// <param name="ex">The exception to be shown.</param>
        /// <param name="enterMessage">The message that is displayed instead of "Press Enter to continue..."</param>
        public static void ShowException(Exception ex, string? message = null, string enterMessage = "Press Enter to continue...")
        {
            if (silent) return;
            string message2 = message == null ? "" : $"{message}: ";
            Console.WriteLine($"\n{message2}{ex.GetType().Name}, {ex.Message}\n" +
                $"{enterMessage}");
            if (!Program.noPause) Console.ReadLine();
        }
    }
}