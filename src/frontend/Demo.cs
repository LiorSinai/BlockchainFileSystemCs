namespace BlockchainFileSystem
{
using System;
using System.IO;
using System.Text.Json;

class Demos{
public static void RunDemo(CLIArguments args)
    {   
        /* blockchain */ 
        Blockchain blockchain = new Blockchain(Directory.GetCurrentDirectory()+"\\..\\MyBlockchain"); 
        Block block0 = blockchain.MakeBlock();

        string user = "@admin";
        PseudoToken t;
        string messages;

        Console.WriteLine("Going to stage tokens");
        Console.WriteLine("Push enter to continue:");
        Console.ReadLine();

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

        Console.WriteLine("\nGoing to verify block");
        Console.WriteLine("Push enter to continue:");
        Console.Read();

        Console.Write("Verifying block ... ");
        try{
            block0.Verify();
            Console.WriteLine("done");
        }
        catch (InvalidBlockException e){
            Console.WriteLine("ERROR: " + e.InnerException.Message);
            Console.WriteLine(e.Message);
            return;
        }
        catch (Exception e){
            Console.WriteLine("ERROR: " + e.InnerException.Message);
            Console.WriteLine(e.Message);
            return;
        }

        Console.WriteLine("\nInsert into a blockchain");
        Console.WriteLine("Push enter to continue:");
        Console.ReadLine();

        /* Blockchain */
        blockchain.CommitBlock(block0);
        blockchain.Print(true);

        Console.WriteLine("\nSave to a file");
        Console.WriteLine("Push enter to continue:");
        Console.ReadLine();

        string jsonName = blockchain.BlockchainDirectory + "/" + blockchain.Name + ".json";
        Console.Write("Writing to " + jsonName + " ... ");
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        string jsonString = JsonSerializer.Serialize(blockchain, options);
        File.WriteAllText(jsonName, jsonString);
        Console.WriteLine("done");
    }
}
}