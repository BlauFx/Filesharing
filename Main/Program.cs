using System;
using System.Linq;

namespace BFs
{
    public class Program
    {
        public static string Version = "1.0.0";

        public static void Main(string[] args)
        {
            new License();

            if (args.Length > 0)
            {
                bool[] IsAvailable = new bool[2];

                foreach (string arg in args)
                {
                    if (arg.Equals("--noupdate", StringComparison.OrdinalIgnoreCase))
                        IsAvailable[0] = true;
                    else if (arg.Equals("--async", StringComparison.OrdinalIgnoreCase))
                        IsAvailable[1] = true;
                }

                if (IsAvailable[0])
                    new Updater();

                if (IsAvailable[1])
                    InternetProtocol.DoAsync = true;
            }
            else
                new Updater();

            Console.Title = "BFs";
            Console.WriteLine("Welcome to BFs (BlauFx filesharing)\n" +
                "What do you want to do?\n" +
                "1: Send a file | Port Req.\n" +
                "2: Receive a file | No Port req.\n" +
                "------------------\n" +
                "3: Send a file | No Port req.\n" +
                "4: Receive a file | Port req.\n" +
                "------------------\n" +
                "5: Connect to a server\n" +
                "6: Create a server\n" +
                "------------------");

            var input = Console.ReadLine();

            foreach (var _ in input.Where(c => c < '0' || c > '9').Select(c => new { })) return;

            switch (int.Parse(input))
            {
                case 1:
                    new SenderWPort();
                    break;
                case 2:
                    new ReceiverWPort();
                    break;
                case 3:
                    new Send();
                    break;
                case 4:
                    new Receive();
                    break;
                case 5:
                    new ShareViaServer(true);
                    break;
                case 6:
                    new ShareViaServer(false);
                    break;
            }
        }
    }
}
