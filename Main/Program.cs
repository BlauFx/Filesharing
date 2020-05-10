using System;

namespace BFs
{
    public class Program
    {
        public static void Main()
        {
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

            switch (int.Parse(Console.ReadLine()))
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
