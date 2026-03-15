using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Compression;

namespace DWMBlurGlassUpdater
{
    /// <summary>
    /// Provides methods to fetch release download URLs from the DWMBlurGlass GitHub repository.
    /// </summary>
    public class Updater
    {
        /// <summary>
        /// Shared HttpClient instance used for GitHub API requests.
        /// </summary>
        private static HttpClient http = new();

        /// <summary>
        /// GitHub API URL for all releases.
        /// </summary>
        private static string releasesApiUrl = "https://api.github.com/repos/Maplespe/DWMBlurGlass/releases";

        /// <summary>
        /// GitHub API URL for the latest release.
        /// </summary>
        private static string latestApiUrl => $"{releasesApiUrl}/latest";

        /// <summary>
        /// Searches the provided assets array for a Windows x64 build and returns its download URL.
        /// </summary>
        /// <param name="assets">A JArray containing release assets.</param>
        /// <returns>The browser download URL of the first x64 asset found.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no suitable release asset is found.</exception>
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

        /// <summary>
        /// Gets the download URL for the latest stable Windows x64 release.
        /// </summary>
        /// <returns>A string containing the download URL.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the GitHub API response cannot be parsed or no suitable asset is found.</exception>
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

        /// <summary>
        /// Gets the download URL for the latest unstable (pre-release) Windows x64 release.
        /// </summary>
        /// <returns>A string containing the download URL.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the GitHub API response cannot be parsed or no suitable asset is found.</exception>
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

        /// <summary>
        /// Gets the download URL for a specific release version.
        /// </summary>
        /// <param name="version">The version string to search for (e.g., "v1.0.0").</param>
        /// <param name="forceExact">If true, only exact matches are considered; if false, partial matches are allowed.</param>
        /// <returns>The browser download URL of the first matching x64 asset.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no suitable release is found or the API response cannot be parsed.</exception>
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

        /// <summary>
        /// Gets download URLs for all Windows x64 releases.
        /// </summary>
        /// <param name="includeUnstable">If true, pre-release versions are included; otherwise, only stable releases are returned.</param>
        /// <returns>A list of strings containing browser download URLs for all matching assets.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the GitHub API response cannot be parsed.</exception>
        public static async Task<List<string>> GetVersionsUrls(bool includeUnstable = false)
        {
            http.DefaultRequestHeaders.Add("User-Agent", "DWMBlurGlassUpdater");
            List<string> versionUrls = new List<string>();

            try
            {
                string resp = await http.GetStringAsync(releasesApiUrl);
                JArray releases = JArray.Parse(resp);

                for (int i = 0; i < releases.Count; i++)
                {
                    JObject release = (JObject)releases[i];

                    bool isPreRelease = (bool)release["prerelease"]!;
                    if (!includeUnstable && isPreRelease) continue;

                    JArray assets = (JArray)release["assets"]!;
                    foreach (JObject asset in assets)
                    {
                        string url = (string)asset["browser_download_url"]!;
                        versionUrls.Add(url);
                    }
                }

                return versionUrls;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting releases: {ex.GetType().Name}, {ex.Message}\n" +
                    $"Press Enter to see full exception...");
                Console.ReadLine();
                throw new InvalidOperationException($"Exception parsing releases API: \n{ex}");
            }
        }

        private static async Task DownloadZip(string url, string outputFile)
        {
            http.DefaultRequestHeaders.Add("User-Agent", "DWMBlurGlassUpdater");

            byte[] data = await http.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(outputFile, data);
        }

        private static async Task InstallZip(string zipPath, string targetDir, string[]? skipFiles = null)
        {
            string folderInZip = "Release";

            skipFiles ??= Array.Empty<string>();

            string tempDir = Path.Combine(Path.GetTempPath(), "DWMBlurGlassUpdater");

            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.FullName.StartsWith(folderInZip + "/", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string relativePath = entry.FullName.Substring(folderInZip.Length + 1);
                    string tempPath = Path.Combine(tempDir, relativePath);

                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);
                    entry.ExtractToFile(tempPath, true);
                }
            }

            foreach (var file in Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(tempDir, file);
                if (Array.Exists(skipFiles, s => string.Equals(s, relativePath, StringComparison.OrdinalIgnoreCase)))
                    continue;

                string destPath = Path.Combine(targetDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                File.Copy(file, destPath, true);
            }

            Directory.Delete(tempDir, true);
        }

        public static async Task<bool> InstallFromUrl(string url)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string destinationDir = Path.Combine(baseDir, "Release");
            if (Directory.Exists(destinationDir) && IsDirectoryLocked(destinationDir)) return false;
            Directory.CreateDirectory(destinationDir);

            string zipPath = Path.Combine(baseDir, "DWMBlurGlass.zip");
            await DownloadZip(url, zipPath);
            await InstallZip(zipPath, destinationDir);
            return true;
        }

        private static bool IsFileLocked(string path)
        {
            try
            {
                using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                {
                    // If we get here, the file is not locked
                    return false;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO lock: {path} ({ex.HResult})");
                return true; // file is locked
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access issue: {path} ({ex.HResult})");
                return true; // file might be read-only
            }
        }

        private static bool IsDirectoryLocked(string directoryPath, bool recursive = true)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException(directoryPath);

            string[] files = Directory.GetFiles(directoryPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            return files.Any(IsFileLocked);
        }
    }
}
