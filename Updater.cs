using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Compression;

namespace DWMBlurGlassUpdater
{
    /// <summary>
    /// Provides functionality for retrieving releases of DWMBlurGlass from GitHub
    /// and installing them locally.
    /// </summary>
    public class Updater
    {
        /// <summary>
        /// Shared <see cref="HttpClient"/> instance used for all GitHub API requests.
        /// </summary>
        private static HttpClient http = new();

        /// <summary>
        /// GitHub API endpoint that returns all releases for the DWMBlurGlass repository.
        /// </summary>
        private static string releasesApiUrl = "https://api.github.com/repos/Maplespe/DWMBlurGlass/releases";

        /// <summary>
        /// GitHub API endpoint that returns the latest stable release.
        /// </summary>
        private static string latestApiUrl => $"{releasesApiUrl}/latest";

        /// <summary>
        /// Searches a GitHub release asset list for a Windows x64 build and returns its download URL.
        /// </summary>
        /// <param name="assets">The JSON array containing release assets from the GitHub API.</param>
        /// <returns>
        /// The <c>browser_download_url</c> of the first asset whose name contains <c>x64</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no Windows x64 asset can be found in the release.
        /// </exception>
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
        /// Retrieves the download URL of the latest stable Windows x64 release.
        /// </summary>
        /// <returns>
        /// A task that resolves to the download URL for the latest stable build.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the GitHub API response cannot be parsed or if no matching asset exists.
        /// </exception>
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
        /// Retrieves the download URL of the latest unstable (pre-release) Windows x64 build.
        /// </summary>
        /// <remarks>
        /// This currently queries the same endpoint as <see cref="GetLatestUrl"/>,
        /// so behavior may depend on how the GitHub repository marks pre-releases.
        /// </remarks>
        /// <returns>
        /// A task resolving to the download URL of the latest unstable build.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the GitHub API response cannot be parsed or no suitable asset exists.
        /// </exception>
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
        /// Retrieves the download URL of a specific version of DWMBlurGlass.
        /// </summary>
        /// <param name="version">
        /// Version string to search for (for example <c>v2.0</c>).
        /// </param>
        /// <param name="forceExact">
        /// If <c>true</c>, the tag name must match exactly.
        /// If <c>false</c>, partial matches are allowed.
        /// </param>
        /// <returns>
        /// A task resolving to the download URL of the matching Windows x64 build.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no matching release is found or the GitHub API response cannot be parsed.
        /// </exception>
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
        /// Retrieves download URLs for all Windows x64 builds available in the repository.
        /// </summary>
        /// <param name="includeUnstable">
        /// If <c>true</c>, pre-release versions are included in the results.
        /// </param>
        /// <returns>
        /// A list of download URLs for all matching assets.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the GitHub API response cannot be parsed.
        /// </exception>
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

        /// <summary>
        /// Downloads a ZIP archive from the specified URL to a local file.
        /// </summary>
        /// <param name="url">The download URL of the ZIP archive.</param>
        /// <param name="outputFile">The path where the downloaded file will be saved.</param>
        /// <returns>A task representing the asynchronous download operation.</returns>
        private static async Task DownloadZip(string url, string outputFile)
        {
            http.DefaultRequestHeaders.Add("User-Agent", "DWMBlurGlassUpdater");

            byte[] data = await http.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(outputFile, data);
        }

        /// <summary>
        /// Extracts a release ZIP archive and installs its contents into the target directory.
        /// </summary>
        /// <param name="zipPath">Path to the downloaded ZIP archive.</param>
        /// <param name="targetDir">Directory where the files should be installed.</param>
        /// <param name="skipFiles">
        /// Optional list of relative file paths that should not be overwritten during installation.
        /// </param>
        /// <returns>A task representing the installation process.</returns>
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

        /// <summary>
        /// Downloads and installs a DWMBlurGlass release from the specified URL.
        /// </summary>
        /// <param name="url">The download URL of the release ZIP file.</param>
        /// <returns>
        /// <c>true</c> if installation succeeded; <c>false</c> if the destination directory is locked.
        /// </returns>
        public static async Task<bool> InstallFromUrl(string url)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string destinationDir = Path.Combine(baseDir, "Release");

            if (Directory.Exists(destinationDir) && IsDirectoryLocked(destinationDir))
                return false;

            Directory.CreateDirectory(destinationDir);

            string zipPath = Path.Combine(baseDir, "DWMBlurGlass.zip");
            await DownloadZip(url, zipPath);
            await InstallZip(zipPath, destinationDir);
            return true;
        }

        /// <summary>
        /// Determines whether a file is locked by another process.
        /// </summary>
        /// <param name="path">Path to the file being checked.</param>
        /// <returns>
        /// <c>true</c> if the file cannot be opened exclusively; otherwise <c>false</c>.
        /// </returns>
        private static bool IsFileLocked(string path)
        {
            try
            {
                using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                {
                    return false;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO lock: {path} ({ex.HResult})");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access issue: {path} ({ex.HResult})");
                return true;
            }
        }

        /// <summary>
        /// Determines whether any file inside a directory is locked.
        /// </summary>
        /// <param name="directoryPath">The directory to check.</param>
        /// <param name="recursive">
        /// If <c>true</c>, subdirectories will also be scanned.
        /// </param>
        /// <returns>
        /// <c>true</c> if any file in the directory is locked; otherwise <c>false</c>.
        /// </returns>
        private static bool IsDirectoryLocked(string directoryPath, bool recursive = true)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException(directoryPath);

            string[] files = Directory.GetFiles(directoryPath, "*",
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            return files.Any(IsFileLocked);
        }
    }
}
