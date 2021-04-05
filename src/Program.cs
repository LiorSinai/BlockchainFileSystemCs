
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
            
            //demo();
            //Console.Write("Push any key to exit");
            //Console.Read();
        }

        public static void demo()
        {   
            /* blockchain */ 
            Blockchain blockchain = new Blockchain(Directory.GetCurrentDirectory()+"/MyBlockchain"); 
            Block block0 = blockchain.MakeBlock();

            string user = "@admin";
            PseudoToken t;
            string messages;

            // HelloWorld
            t.UserName = user;
            t.FilePath = "../files/Hello.txt";
            t.Author = "Brian Kernighan";
            try{
                messages = block0.StageToken(t);
                Console.WriteLine(messages);
            }
            catch (Exception e){
                Console.WriteLine("ERROR: " + e.Message);
                return;
            }

            // Yoda
            t.UserName = user;
            t.FilePath = "../files/FumiakiKawahata_Yoda_LiorSinai.jpg";
            t.Author = "Lior Sinai";
            messages = block0.StageToken(t);
            Console.WriteLine(messages);

            // Fox
            t.UserName = user;
            t.FilePath = "../files/fox.png";
            t.Author = "Lior Sinai";
            messages = block0.StageToken(t);
            Console.WriteLine(messages);

            // LoremImpsum
            t.UserName = user;
            t.FilePath = "../files/LoremIpsum.txt";
            t.Author = "Marcus Tullius Cicero";
            messages = block0.StageToken(t);
            Console.WriteLine(messages);

            // Console.WriteLine("enter eny key to continue:");
            // Console.Read();

            Console.Write("Verifying block ... ");
            try{
                block0.Verify();
                Console.WriteLine("done");
            }
            catch (InvalidBlockException e){
                Console.WriteLine("ERROR: " + e.InnerException.Message);
                Console.WriteLine(e.Message);
            }
            catch (Exception e){
                Console.WriteLine("ERROR: " + e.InnerException.Message);
                Console.WriteLine(e.Message);
            }
        
            /* Blockchain */
            blockchain.CommitBlock(block0);
            Block block1 = blockchain.MakeBlock();
            blockchain.CommitBlock(block1);

            blockchain.Print(true);

            string jsonName = blockchain.BlockchainDirectory + "/" + blockchain.Name + ".json";
            Console.Write("Writing to " + jsonName + " ... ");
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string jsonString = JsonSerializer.Serialize(blockchain, options);
            File.WriteAllText(jsonName, jsonString);
            Console.WriteLine("done");

            Console.Write("Push any key to exit");
            Console.Read();
        }
    }//class Program
} //namespace BlockchainFileSystem
