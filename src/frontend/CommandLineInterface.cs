/*
 *  Created on: 4 April 2021
 *      Author: Lior Sinai
 * Description: A simple command line interface
 */

namespace BlockchainFileSystem
{ 

using System;
using System.IO; 
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class CommandLineInterface
{
    List<CLIOption> Options;
    private string User;
    private string NotInitialisedMessage = "Blockchain not initialised. Use --initialise or --load first";
    private string NoBlockMessage = "No uncommitted blocks";
    private uint Target=0;

    #nullable enable
    private Blockchain? blockchain;
    private Block? block;
    #nullable disable

    public CommandLineInterface()
    {
        Options = new List<CLIOption>();
        Options.Add(new CLIOption("-c", "--commit-block", "", "commit a block to the blockchain", new CLIactionDelegate(CommitBlock)));
        Options.Add(new CLIOption("",   "--header", "[index]", "show the hexadecimal header for a block", new CLIactionDelegate(PrintHeader)));
        Options.Add(new CLIOption("-h", "--help", "", "display this help menu", new CLIactionDelegate(Help)));
        Options.Add(new CLIOption("-i", "--initialise", "name", "initialise a new blockchain", new CLIactionDelegate(InitialiseBlockchain)));
        Options.Add(new CLIOption("-l", "--load", "filePath dirPath", "load an existing blockchain at dirPath from a json file", new CLIactionDelegate(LoadBlockchain)));
        Options.Add(new CLIOption("-p", "--print-blockchain", "[h]", "print the blockchain. If h, print header only", new CLIactionDelegate(PrintBlockChain)));
        Options.Add(new CLIOption("",   "--print-block", "[index]", "print block at index in the blockchain", new CLIactionDelegate(PrintBlock)));
        Options.Add(new CLIOption("",   "--print-token", "[id]", "print token with hash=id", new CLIactionDelegate(PrintToken)));
        Options.Add(new CLIOption("-q", "--quit", "", "quit", new CLIactionDelegate(Quit)));
        Options.Add(new CLIOption("-s", "--save", "", "save the blockchain to a json file", new CLIactionDelegate(SaveBlockchain)));
        Options.Add(new CLIOption("",   "--sha256", "Text", "calculate the SHA256 hash of the text (rest of line)", new CLIactionDelegate(Sha256_Text)));
        Options.Add(new CLIOption("",   "--sha256-hex", "hexdec", "calculate the SHA256 hash of a hexadecimal number", new CLIactionDelegate(Sha256_Hex)));
        Options.Add(new CLIOption("",   "--sha256-file", "fileName", "calculate the SHA256 hash of file contents", new CLIactionDelegate(Sha256_File)));
        Options.Add(new CLIOption("",   "--switch-user", "[username]", "change the active user", new CLIactionDelegate(SwitchUser)));
        Options.Add(new CLIOption("-t", "--stage-token", "", "stage a token into a block", new CLIactionDelegate(StageToken)));
        Options.Add(new CLIOption("",   "--target", "[target]", "set and display the proof of work target", new CLIactionDelegate(SetTarget)));
        Options.Add(new CLIOption("-u", "--unstage", "", "remove the last uncommitted block", new CLIactionDelegate(Unstage)));
        Options.Add(new CLIOption("",   "--verify", "", "verify the blockchain (excludes uncommited blocks)", new CLIactionDelegate(Verify)));
        Options.Add(new CLIOption("-v", "--version", "", "print this version", new CLIactionDelegate(Version)));
    }

    public void Run()
    {
        Console.WriteLine("Welcome to the Blockchain file system");
        Console.Write("What is your name? (enter for default)> ");
        string line = Console.ReadLine();
        SwitchUser(new CLIArguments("", new List<string>(){line}, ""));
        this.Help(new CLIArguments());
        while(true){
            Console.Write(this.User + "> ");
            line = Console.ReadLine();
            if (line == "q" || line == "-q" || line == "--quit") break;
            CLIArguments args = ParseLine(line);
            this.DispatchArgs(args);
        }
        Console.WriteLine("Push any key to exit");
        Console.Read();
    }

    private CLIArguments ParseLine(string line)
    {
        CLIArguments args = new CLIArguments();
        if (String.IsNullOrEmpty(line) || line[0] != '-'){
            return args;
        }
        int idx_space1 = line.IndexOf(' ');
        idx_space1 = (idx_space1 == -1) ? line.Length : idx_space1;
        args.Argc = line.Substring(0, idx_space1 - 0);
        if (idx_space1 < line.Length - 1){
            args.Text = line.Substring(idx_space1 + 1, line.Length -1  - idx_space1);
            args.Argv = args.Text.Split(' ').ToList();
        }
        return args;
    }

    private void DispatchArgs(CLIArguments args)
    {
        string option = args.Argc;
        if (String.IsNullOrEmpty(option)){
            Console.WriteLine("No option entered. For valid options, see -h or --help. q or -q to quit");
            return;
        }
        foreach (CLIOption o in this.Options){
            if (option == o.ShortCmd || option == o.LongCmd){
                o.Action(args);
                return;
            }
        }
        Console.WriteLine(option + " is not a valid option. For valid options, see -h or --help. q or -q to quit");
    }

    private void Help(CLIArguments args)
    {   
        String s = "Blockchain File System v0.1\n\n"; 
        s += "Enter one of the following options:\n";
        foreach(CLIOption o in this.Options){
            if (String.IsNullOrEmpty(o.ShortCmd)){
                s += String.Format("{0,-2}   {1,-30} {2}\n", "", o.LongCmd + " " + o.Paramaters, o.HelpText);
            }
            else if (String.IsNullOrEmpty(o.LongCmd)){
                s +=  String.Format("{0,-35} {1}\n", o.ShortCmd + " " + o.Paramaters, o.HelpText);
            }
            else{
                s +=  String.Format("{0,-2} | {1,-30} {2}\n", o.ShortCmd, o.LongCmd + " " + o.Paramaters, o.HelpText);
            }
        }
        Console.WriteLine(s);
        Console.WriteLine("| => equaivalent commands. [arg] => optional argument");
        Console.WriteLine("If the options [index] or [id] are none, command is executed for the last uncommitted block/token");
        Console.WriteLine("Only one option will be executed per line. Line must start with an option");
    }

    private void Version(CLIArguments args)
    { 
        Console.WriteLine("Blockchain File System v0.1");
    }

    private void SwitchUser(CLIArguments args)
    { 
        string argv =  (args.Argv.Count == 0) ? "" : args.Argv[0];
        if (String.IsNullOrEmpty(argv)){
            this.User = "@admin";
        }
        else{
            this.User = "@" + argv;
        }
        Console.WriteLine("username: " + this.User);
    }

    private void Quit(CLIArguments args)
    { //already break in the while loop
    }

    private void InitialiseBlockchain(CLIArguments args)
    {
        if (blockchain != null)
        {
            Console.WriteLine("Already initialised the blockchain at " + this.blockchain);
            return;
        }
        else if (args.Argv.Count == 0)
        {
            Console.WriteLine("Must include a name in the argument");
        }
        else
        {
            string name = args.Argv[0];
            string dir = Directory.GetCurrentDirectory()+ "/" + name;
            Console.WriteLine("Creating blockchain at " + dir);
            if (System.IO.Directory.Exists(dir)){
                Console.WriteLine("WARNING: Directory already exists at " + dir);
            }
            else{
                try{
                    DirectoryInfo di = System.IO.Directory.CreateDirectory(dir);
                    Console.WriteLine("Directory successfully made");
                }
                catch (Exception e){
                    Console.WriteLine(e.Message);
                    return;
                }
            }
            blockchain = new Blockchain(dir);
            Console.WriteLine("Creating a new directory at " + dir + "/Block0");
            block = blockchain.MakeBlock();
            Console.WriteLine("Done");
        }
    } // InitialiseBlockchain

    private void LoadBlockchain(CLIArguments args)
    {
        string fileName = "";
        string dirPath ="";
        if (blockchain != null)
        {
            Console.WriteLine("Already initialised the blockchain at " + this.blockchain);
            return;
        }
        if (args.Argv.Count == 0)
        {
            Console.WriteLine("Must include a file path and directory path in the argument");
            return;
        }
        else if (args.Argv.Count == 1){
            fileName = args.Argv[0];
            int idx = fileName.LastIndexOfAny(new char[] { '/', '\\'}); 
            if (idx==-1){
                Console.WriteLine("Must include a file path and directory path in the argument");
                return;
            }
            dirPath = fileName.Substring(0, idx); //if -1, take the whole string
        }
        else
        {
            fileName = args.Argv[0];
            dirPath = args.Argv[1];
        }
        try{
            blockchain = BlockchainLoader.loadFromJson(fileName, dirPath);
            int next_idx = blockchain.Index() + 1;
            Console.WriteLine("Creating a new directory at " + blockchain.BlockchainDirectory + "/Block" + next_idx);
            block = blockchain.MakeBlock();
            Console.WriteLine("Done");
        }
        catch (InvalidBlockchainException e){
            Console.WriteLine("ERROR: " + e.ToString());
        }
        catch (Exception e){
            Console.WriteLine("ERROR: " + e.ToString());
        }
    } //LoadBlockchain

    private void PrintBlockChain(CLIArguments args)
    {
        if (blockchain == null)
        {
            Console.WriteLine(NotInitialisedMessage);
            return;
        }
        string argv = (args.Argv.Count < 1) ? "" : args.Argv[0];
        bool printHeader = (argv=="h");
        blockchain.Print(!printHeader);
    }

    private void PrintBlock(CLIArguments args)
    {
        if (blockchain == null)
        {
            Console.WriteLine(NotInitialisedMessage);
            return;
        }
        else if (args.Argv.Count == 0)
        {
            if (block == null) {
                Console.WriteLine(NoBlockMessage);
            }
            else{
                block.Print();
            }
        }
        else
        {
            int index = Convert.ToInt32(args.Argv[0]);
            try{
                blockchain.At(index).Print();
            }
            catch (Exception e){
                Console.WriteLine(e.Message);
            }
        }
    } // PrintBlock

    private void PrintToken(CLIArguments args)
    {
        if (blockchain == null)
        {
            Console.WriteLine(NotInitialisedMessage);
            return;
        }
        else if (args.Argv.Count == 0)
        {
            if (block == null) {
                Console.WriteLine("No staged tokens");
            }
            else{
                Token token = block.Tokens.Values.Last(); //unordered collection, so not guaranteed to be this staged token
                token.Print(block.BlockDirectory);
            }
        }
        else
        {
            string id = args.Argv[0];
            foreach(Block b in blockchain.Blocks){
                if (b.Tokens.ContainsKey(id)){
                    Console.WriteLine("Token found in block " + b.Index);
                    b.Tokens[id].Print();
                    return;
                }
            }
            Console.WriteLine("Token not found with id =" + id);
        }
    } // PrintToken

    private void PrintHeader(CLIArguments args)
    {
        string headerFormat = "version (8) | previousHash (64) | MerkleRoot (64) | timestamp (8) | target (8) | nonce (8)";
        if (blockchain == null)
        {
            Console.WriteLine(NotInitialisedMessage);
            return;
        }
        else if (args.Argv.Count == 0)
        {
            if (block == null) {
                Console.WriteLine(NoBlockMessage);
            }
            else{
                Console.WriteLine(headerFormat);
                Console.WriteLine(Hasher.Bytes2hex(block.getHeader()));
            }
        }
        else
        {
            int index = Convert.ToInt32(args.Argv[0]);
            try{
                Block blk = blockchain.At(index);
                Console.WriteLine(headerFormat);
                Console.WriteLine(Hasher.Bytes2hex(blk.getHeader()));
            }
            catch (Exception e){
                Console.WriteLine(e.Message);
            }
        }
    } // PrintHeader

    private void SaveBlockchain(CLIArguments args)
    {
        string jsonName = blockchain.BlockchainDirectory + "/" + blockchain.Name + ".json";
        Console.WriteLine("Writing to " + jsonName + " ... ");
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        string jsonString = JsonSerializer.Serialize(blockchain, options);
        File.WriteAllText(jsonName, jsonString);
        Console.WriteLine("done");
    } // SaveBlockchain

    private void StageToken(CLIArguments args)
    {
        if (blockchain == null)
        {
            Console.WriteLine(NotInitialisedMessage);
            return;
        }
        
        Console.Write("enter a path to a file> ");
        string filePath = Console.ReadLine();
        Console.Write("enter the author of the file>");
        string author = Console.ReadLine();

        if (this.block == null){
            block = blockchain.MakeBlock();
        }

        try{
            PseudoToken pseudotoken = new PseudoToken(User, filePath, author);
            string msgs = block.StageToken(pseudotoken);
            Console.WriteLine(msgs);
            Console.WriteLine("Staging was successful");
        }
        catch (Exception e){
            Console.WriteLine(e.Message);
        }
    } //StageToken

    private void Unstage(CLIArguments args)
    {
        block = null;
        Console.WriteLine("removed last uncommited block");
    }

    private void CommitBlock(CLIArguments args)
    {
        if (blockchain == null)
        {
            Console.WriteLine(NotInitialisedMessage);
            return;
        }
        else if (this.block == null){
            Console.WriteLine(NoBlockMessage);
            return;
        }
        
        try{
            Console.WriteLine("Committing block ...");
            //set target
            block.Target = this.Target;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            blockchain.CommitBlock(block);
            watch.Stop();

            Console.WriteLine("done");
            Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

            block = blockchain.MakeBlock();
            Console.WriteLine("Made new directory at :" + block.BlockDirectory);
            SaveBlockchain(args);
        }
        catch (InvalidBlockchainException e){
            Console.WriteLine("ERROR: " + e.Message);
        }
        catch (Exception e){
            Console.WriteLine("ERROR: " + e.Message);
        }
    }


    private void Verify(CLIArguments args)
    {
        if (blockchain == null)
        {
            Console.WriteLine(NotInitialisedMessage);
            return;
        }
        try{
            Console.WriteLine("Verifying blockchain ...");
            blockchain.Verify();
            Console.WriteLine("Verified.");
        }
        catch (InvalidBlockchainException e){
            Console.WriteLine("ERROR: " + e.ToString());
        }
        catch (Exception e){
            Console.WriteLine("ERROR: " + e.ToString());
        }
    } // Verify

    private void SetTarget(CLIArguments args)
    {
        if (args.Argv.Count > 0)
        {
            Target = Convert.ToUInt32(args.Argv[0]);
        }
        Console.WriteLine("Target [0-256]: " + Target);
    }

    private void Sha256_Text (CLIArguments args)
    {
        Console.WriteLine(Hasher.GetSHA256Hash(args.Text));
    }

    private void Sha256_Hex (CLIArguments args)
    {
        try{
            byte[] bytes = Hasher.Hex2bytes(args.Argv[0]);
            Console.WriteLine(Hasher.GetSHA256Hash(bytes));
        }
        catch (Exception e){
            Console.WriteLine(e.ToString());
        }
    }

    private void Sha256_File (CLIArguments args)
    {
        try{
            string fileName = args.Argv[0];
            byte[] contents = File.ReadAllBytes(fileName);
            Console.WriteLine(Hasher.GetSHA256Hash(contents));
        }
        catch (Exception e){
            Console.WriteLine(e.ToString());
        }
    }



}

} //namespace BlockchainFileSystem