using System;
using System.Linq;

namespace BFs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    if (arg.Equals("--async", StringComparison.OrdinalIgnoreCase))
                    {
                        InternetProtocol.DoAsync = true;
                        Console.WriteLine("Async is enabled!\n");
                    }
                }
            }

           new License();
            // new Updater();

            Console.Title = "BFs";
            Console.WriteLine("Welcome to BFs (BlauFx filesharing)\n" +
                "What do you want to do?\n" +
                "1: Send a file | Port Req.\n" +
                "2: Receive a file | No Port req.\n" +
                "------------------\n" +
                "3: Send a file | No Port req.\n" +
                "4: Receive a file | Port req.\n" +
                "------------------");

            var input = Console.ReadLine();
            foreach (var _ in input!.Where(c => c < '0' || c > '4').Select(c => new { })) return;

            switch (int.Parse(input))
            {
                case 1:
                    new PortReq(false);
                    break;
                case 2:
                    new NoPortReq(false);
                    break;
                case 3:
                    new NoPortReq(true);
                    break;
                case 4:
                    new PortReq(true);
                    break;
            }
        }
    }
}
