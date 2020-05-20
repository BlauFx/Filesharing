﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.IO.Directory;

namespace BFs
{
    class Updater
    {
        public const string ThisRepo = "BFs";
        private const string UpdaterRepo = "Updater";

        private readonly string ExePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        private List<Github_Releases> Github_Releases = new List<Github_Releases>();
        private List<Github_Releases> Github_ReleasesUpdater = new List<Github_Releases>();

        public Updater()
        {
            if (SearchAsyncForUpdates())
            {
                Console.Write("A new version is available");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Console.Write("\nDo you want to download and apply the update? [y/n]: ");
                    if (Console.ReadLine() == "y")
                    {
                        DownloadUpdate("Updater.exe", true);
                        DownloadUpdate("win-x64.zip", false);

                        ApplyUpdate();
                    }
                }
                else
                {
                    Console.WriteLine("Please download the update manually");
                    Console.WriteLine("The Auto update feature is not supported on your platform!");
                }
            }
        }

        private bool SearchAsyncForUpdates()
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                HttpResponseMessage response;
                HttpResponseMessage response2;

                httpClient.DefaultRequestHeaders.Add("user-agent", ".");

                response = httpClient.GetAsync($"https://api.github.com/repos/BlauFx/{ThisRepo}/releases").Result;
                response2 = httpClient.GetAsync($"https://api.github.com/repos/BlauFx/{UpdaterRepo}/releases").Result;

                response.EnsureSuccessStatusCode();
                response2.EnsureSuccessStatusCode();

                Github_Releases = JsonConvert.DeserializeObject<List<Github_Releases>>(response?.Content.ReadAsStringAsync().Result);
                Github_ReleasesUpdater = JsonConvert.DeserializeObject<List<Github_Releases>>(response2?.Content.ReadAsStringAsync().Result);
            }
            catch { /*It can fail due to no internet connection or being rate-limited*/ }

            return CheckIfNewVersionIsAvailable();
        }

        private bool CheckIfNewVersionIsAvailable() => !GetCurrentVersion().Equals(Github_Releases?.FirstOrDefault()?.Tag_name.Trim('v', 'V') ?? GetCurrentVersion());

        private string GetCurrentVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public void DownloadUpdate(string str, bool Update)
        {
            var x = (Update == true ? Github_ReleasesUpdater : Github_Releases)?.FirstOrDefault();
            AssetsClass assets = x?.Assets?.FirstOrDefault(y => y.Name.Equals(str));

            if (Update)
            {
                if (File.Exists("Updater.exe"))
                    File.Delete("Updater.exe");
            }

            if (!Exists(@$"{ExePath}/temp"))
                CreateDirectory(@$"{ExePath}/temp");

            try
            {
                using HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("user-agent", ".");

                using var fs = new FileStream($"{ExePath}//{(str.Contains("Updater", StringComparison.OrdinalIgnoreCase) == true ? "Updater.exe" : "temp//win-x64.zip")}", FileMode.CreateNew);
                httpClient.GetStreamAsync(assets?.Browser_download_url).Result.CopyTo(fs);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to download {(Update == true ? "update.exe" : "win-x64-zip")} ({x?.Tag_name})\nError msg: {e.Message}");

                Console.ReadLine();

                Console.ResetColor();
                Environment.Exit(0);
            }

            if (File.Exists(@$"{ExePath}/temp/win-x64.zip"))
            {
                using StreamWriter strmWriter = new StreamWriter(@$"{ExePath}/temp/win-x64.txt");
                strmWriter.WriteLine("Done");
            }
        }

        public async void ApplyUpdate()
        {
            if (File.Exists($"{ExePath}\\temp\\win-x64.txt"))
            {
                using StreamReader reader = new StreamReader($"{ExePath}\\temp\\win-x64.txt");
                if (!(reader.ReadLine() == "Done"))
                {
                    return;
                }
            }

            static void CreateDir(string path)
            {
                if (!Exists(path))
                    CreateDirectory(path);
                else
                {
                    Delete(path, true);
                    CreateDirectory(path);
                }
            }

            CreateDir($"{ExePath}\\temp\\files");
            CreateDir($"{ExePath}\\temp\\old");

            int PID = Process.GetCurrentProcess().Id;
            ZipFile.ExtractToDirectory($"{ExePath}\\temp\\win-x64.zip", $"{ExePath}\\temp\\files");

            File.Move($"{ExePath}\\Updater.exe", $"{ExePath}\\temp\\Updater.exe");
            new Process()
            {
                StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    Arguments = $"/C start {ExePath}\\temp\\Updater.exe {PID} {ExePath}",
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                },
            }.Start();

            await Task.Delay(5000); //Just wait a lil bit until the Process has started
            Environment.Exit(0);
        }
    }

    internal class Github_Releases
    {
        [JsonPropertyName("tag_name")]
        public string Tag_name { get; set; }

        [JsonPropertyName("assets")]
        public List<AssetsClass> Assets { get; set; }
    }

    internal class AssetsClass
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string Browser_download_url { get; set; }

        [JsonPropertyName("assets")]
        public List<AssetsClass> Assets { get; set; }
    }
}