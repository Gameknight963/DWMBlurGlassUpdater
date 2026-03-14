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


        public static async Task<string> GetLatestUrl()
        {
            http.DefaultRequestHeaders.Add("User-Agent", "DWMBlurGlassUpdater");
            string resp;
            JObject obj;
            JArray assets;

            try
            {
                resp = await http.GetStringAsync(latestApiUrl);
                obj = JObject.Parse(resp);
                assets = (JArray)obj["assets"]!;
                for (int i = 0; i < assets.Count; i++)
                {
                    if (((string)assets[i]["name"]!).Contains("x64"))
                    {
                        return (string)assets[i]["browser_download_url"]!;
                    }
                }
                throw new InvalidOperationException("Download error: No suitable release found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting or parsing: {ex.GetType().Name}, {ex.Message}");
                Console.ReadLine();
                throw new InvalidOperationException($"Exception parsing releases API: \n{ex}");
            }
        }

        public static async Task<string> GetLatestUnstableUrl()
        {
            http.DefaultRequestHeaders.Add("User-Agent", "DWMBlurGlassUpdater");
            string resp;
            JArray obj;
            JArray assets;

            try
            {
                resp = await http.GetStringAsync(releasesApiUrl);
                obj = JArray.Parse(resp);
                assets = (JArray)obj[0]!["assets"]!;
                for (int i = 0; i < assets.Count; i++)
                {
                    if (((string)assets[i]["name"]!).Contains("x64"))
                    {
                        return (string)assets[i]["browser_download_url"]!;
                    }
                }
                throw new InvalidOperationException("Download error: No suitable release found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting or parsing: {ex.GetType().Name}, {ex.Message}");
                Console.ReadLine();
                throw new InvalidOperationException($"Exception parsing releases API: \n{ex}");
            }
        }
    }
}
