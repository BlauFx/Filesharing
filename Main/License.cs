using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace BFs
{
    public class License
    {
        private readonly List<string> license = new List<string>();
        
        public License()
        {
            license.Add("BFs");
            license.Add("Updater");

            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            
            //If location is emtpy it means the application is self-contained
            if (string.IsNullOrWhiteSpace(location) || string.IsNullOrEmpty(location))
                return;

            string path = Path.Combine(location, "Licenses");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using HttpClient httpClient = new HttpClient();

            for (int i = 0; i < license.Count; i++)
            {
                if (File.Exists($"{path}\\{license[i]}.txt"))
                    continue;

                try
                {
                    using var fs = new FileStream($"{path}\\{license[i]}.txt", FileMode.CreateNew);
                    httpClient.GetStreamAsync($"https://raw.githubusercontent.com/BlauFx/BFs/master/Licenses/{license[i]}.txt").Result.CopyTo(fs);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
