using System.IO;
using System.Net.Http;
using System.Reflection;

namespace BFs
{
    public class License
    {
        private readonly string[] license = new string[3];
        
        public License()
        {
            license[0] = "BFs";
            license[1] = "Newtonsoft.Json";
            license[2] = "Updater";

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Licenses");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using HttpClient httpClient = new HttpClient();

            for (int i = 0; i < license.Length; i++)
            {
                if (!File.Exists($"{path}\\{license[i]}.txt"))
                {
                    try
                    {
                        using var fs = new FileStream($"{path}\\{license[i]}.txt", FileMode.CreateNew);
                        httpClient.GetStreamAsync($"https://raw.githubusercontent.com/BlauFx/BFs/master/Licenses/{license[i]}.txt").Result.CopyTo(fs);
                    } catch { }
                }
            }
        }
    }
}
