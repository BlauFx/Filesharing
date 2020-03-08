using System;

namespace BFs
{
    public class Program
    {
        public static void Main()
        {
            Console.Title = "BFs";

            Console.WriteLine("Welcome to BFs (BlauFx filesharing)");
            Console.WriteLine("What do you want to do?");
            Console.WriteLine("1: Send a file | Port Req.");
            Console.WriteLine("2: Receive a file | No Port req.");
            Console.WriteLine("------------------");
            Console.WriteLine("3: Send a file | No Port req.");
            Console.WriteLine("4: Receive a file | Port req.");
            Console.WriteLine("------------------");

            string x = Console.ReadLine();

            switch (int.Parse(x))
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
            }
        }
    }
}
