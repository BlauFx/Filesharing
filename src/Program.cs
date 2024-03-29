using System;
using System.Linq;

namespace Filesharing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args.Contains("--async"))
                {
                    InternetProtocol.DoAsync = true;
                    Console.WriteLine("Async is enabled!\n");
                }

                if (args.Contains("--ipv6"))
                {
                    InternetProtocol.Ipv6 = true;
                    Console.WriteLine("IPv6 is enabled!\n");
                }
            }

            Console.Title = "Filesharing";
            Console.WriteLine("Welcome\n" +
                "What do you want to do?\n" +
                "1: Send a file | Port Req.\n" +
                "2: Receive a file | No Port req.\n" +
                "------------------\n" +
                "3: Send a file | No Port req.\n" +
                "4: Receive a file | Port req.\n" +
                "------------------");

            var input = Console.ReadLine();

	        if (int.TryParse(input, out int num))
                switch (num)
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
                    default:
                        Console.WriteLine("Please type in a number between 1 and 4!");
                        break;
                }
            else
                Console.WriteLine("Please type in a number!");
        }
    }
}
