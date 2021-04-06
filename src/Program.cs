
namespace BlockchainFileSystem
{   
    using System;
    using System.IO;
    using System.Text.Json;

    class Program
    {
        static void Main(string[] args)
        {
            CommandLineInterface cli = new CommandLineInterface();
            cli.Run();
        }
    }//class Program
} //namespace BlockchainFileSystem
