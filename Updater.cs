using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DWMBlurGlassUpdater
{
    public class Updater
    {
        private static HttpClient http = new();

        private static string releasesApiUrl = "https://api.github.com/repos/Maplespe/DWMBlurGlass/releases";
        private static string latestApiUrl => $"{releasesApiUrl}/latest";

        private static string GetWindowsBuildUrl(JArray assets)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                if (((string)assets[i]["name"]!).Contains("x64"))
                {
                    return (string)assets[i]["browser_download_url"]!;
                }
            }
            throw new InvalidOperationException("No suitable release found.");
        }

        public static async Task<string> GetLatestUrl()
        {
            http.DefaultRequestHeaders.Add("User-Agent", "DWMBlurGlassUpdater");

            try
            {
                string resp = await http.GetStringAsync(latestApiUrl);
                JObject obj = JObject.Parse(resp);
                JArray assets = (JArray)obj["assets"]!;
                return GetWindowsBuildUrl(assets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting or parsing: {ex.GetType().Name}, {ex.Message}\n" +
                    $"Press enter for full exception...");
                Console.ReadLine();
                throw new InvalidOperationException($"Exception parsing releases API: \n{ex}");
            }
        }

        public static async Task<string> GetLatestUnstableUrl()
        {
            http.DefaultRequestHeaders.Add("User-Agent", "DWMBlurGlassUpdater");

            try
            {
                string resp = await http.GetStringAsync(latestApiUrl);
                JObject obj = JObject.Parse(resp);
                JArray assets = (JArray)obj["assets"]!;
                return GetWindowsBuildUrl(assets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting or parsing: {ex.GetType().Name}, {ex.Message}\n" +
                    $"Press enter for full exception...");
                Console.ReadLine();
                throw new InvalidOperationException($"Exception parsing releases API: \n{ex}");
            }
        }

        public static async Task<string> GetVersionUrl(string version, bool forceExact = false)
        {
            http.DefaultRequestHeaders.Add("User-Agent", "DWMBlurGlassUpdater");

            try
            {
                string resp = await http.GetStringAsync(releasesApiUrl);
                JArray releases = JArray.Parse(resp);

                for (int i = 0; i < releases.Count; i++)
                {
                    string tag = (string)releases[i]["tag_name"]!;
                    if (forceExact)
                    {
                        if (tag == version) return GetWindowsBuildUrl((JArray)releases[i]!["assets"]!);
                    }
                    else
                    {
                        if (tag.Contains(version)) return GetWindowsBuildUrl((JArray)releases[i]!["assets"]!);
                    }
                }

                throw new InvalidOperationException("No suitable release found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting version {version}: {ex.GetType().Name}, {ex.Message}\n" +
                    $"Press enter for full exception...");
                Console.ReadLine();
                throw new InvalidOperationException($"Exception parsing releases API: \n{ex}");
            }
        }
    }
}