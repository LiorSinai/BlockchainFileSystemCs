/*
 *  Created on: 3 April 2021
 *      Author: Lior Sinai
 * Description: A block which can hold tokens
 */

namespace BlockchainFileSystem
{

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

public class InvalidBlockException : Exception
{
    public InvalidBlockException() { }

    public InvalidBlockException(string message)
        : base(message) { }

    public InvalidBlockException(string message, Exception inner)
        : base(message, inner) { }
}

public class Block
{
    // Attributes of a Block
    [JsonIgnore]
    public string BlockDirectory { get; set ; }
    public const int Version = 1;
    public int Index { get;  private set; } //First mined block has index = 0, and so on.
    public uint Target { get; set; }
    public uint Nonce { get; set; }
    public string PreviousHash { get; private set; }
    public string MerkleRoot {get; private set ; }

    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime TimeStamp { get; private set ; }

    public Dictionary<string, Token> Tokens  { get; private set; } 

    [JsonIgnore]
    public bool Verified {get; private set;}

    public Block(int index_, string previousHash_, string directory_="")
    {
        this.Index = index_;
        this.PreviousHash = previousHash_;
        this.BlockDirectory = directory_ + "/Block" + Index;
        //defaults
        this.TimeStamp = DateTime.UtcNow;
        this.Target = 0;
        this.Nonce = 0;
        // Determine whether the directory exists.
        if (!System.IO.Directory.Exists(this.BlockDirectory))
        {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(this.BlockDirectory);
        }
        this.Tokens = new Dictionary<string, Token>();
    }

    public Block(int index_, string previousHash_, string directory_, DateTime timeStamp_, uint target, uint nonce)
    {
        //used for loading 
        this.Index = index_;
        this.PreviousHash = previousHash_;
        this.BlockDirectory = directory_;
        this.TimeStamp = timeStamp_;
        this.Target = target;
        this.Nonce = nonce;
        this.Tokens = new Dictionary<string, Token>();
    }

    public string StageToken(PseudoToken pseudotoken)
    {
        string messages = String.Empty; //this will be sent to a frontend to manage (the CLI)
        Token token = new Token(pseudotoken);
        string outpath = this.BlockDirectory + "/" + token.FileName ;
        if (File.Exists(outpath)){
            File.Delete(outpath);
            messages += "WARNING: the file at " + outpath + " already exists. Deleting it.\n";	
        }
        File.Copy(pseudotoken.FilePath, outpath);
        messages += "Copied file to " + outpath;
        string id = token.Hash();
        Tokens.Add(id, token);

        this.MerkleRoot = CalcMerkleRoot(); //update MerkleRoot

        return messages;
    }

    public void Print()
    {
        Console.WriteLine("------------------------------------------------------------------------------------------------");
        Console.WriteLine("------------------------------------------------------------------------------------------------");
        Console.WriteLine("");
        Console.WriteLine(this.BlockDirectory);
        Console.WriteLine("version:          " + Block.Version);
        Console.WriteLine("previous hash:    " + this.PreviousHash);
        Console.WriteLine("this hash:        " + this.Hash());
        Console.WriteLine("Merkle root:      " + this.CalcMerkleRoot());
        Console.WriteLine("timestamp UTC:    " + this.TimeStamp);
        Console.WriteLine("target:           " + this.Target);
        Console.WriteLine("nonce:            " + this.Nonce);
        Console.WriteLine("number of tokens: " + this.Tokens.Count);
        Console.WriteLine("");

        Console.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
        Console.WriteLine("");

        foreach(KeyValuePair<string, Token> element in this.Tokens)
        {
            Console.WriteLine("Token "+ element.Key);
            element.Value.Print(this.BlockDirectory);
            Console.WriteLine("");
        }
    }

    public bool Verify(){
        string msgBlock = "Block " + this.Index + " not verified";
        foreach(KeyValuePair<string, Token> element in this.Tokens)
        {
            string hash ;
            try{
                element.Value.Verify(this.BlockDirectory);
                hash = element.Value.Hash();
            }
            catch (Exception e){
                throw new InvalidBlockException(msgBlock, e);
            }
            if (hash != element.Key){
                msgBlock += "\n";
                msgBlock += "Token hashes do not match:\nOriginal:   " + element.Key + "\nCalculated: " + hash;
                throw new InvalidBlockException(msgBlock);
            }
        }
        this.Verified = true;
        return true;
    }

    public string CalcMerkleRoot(){
        List<string> ids = new List<string>();
        foreach(KeyValuePair<string, Token> element in this.Tokens)
        {
            ids.Add(element.Key);
        }
        return MerkleTree.Root(ids);
    }

    public byte[] getHeader(){
        // version (4) | previousHash (32) | MerkleRoot (32) | timestamp (4) | target (4) | nonce (4)
        byte[] header = new byte[] {};
        byte[] bytes;
        // version
        bytes = BitConverter.GetBytes(Block.Version);
        Array.Reverse(bytes); //make big Endian
        header = header.Concat(bytes).ToArray();
        // previous hash
        bytes = Hasher.Hex2bytes(this.PreviousHash);
        header = header.Concat(bytes).ToArray();
        // Merkle Root
        bytes = Hasher.Hex2bytes(this.CalcMerkleRoot());
        header = header.Concat(bytes).ToArray();
        // time stamp
        int timeStamp = Utilities.ConvertToUnixTimestamp(this.TimeStamp);
        bytes = BitConverter.GetBytes(timeStamp);
        Array.Reverse(bytes); //make big Endian
        header = header.Concat(bytes).ToArray();
        // target
        bytes = BitConverter.GetBytes(this.Target);
        Array.Reverse(bytes); //make big Endian
        header = header.Concat(bytes).ToArray();
        // nonce
        bytes = BitConverter.GetBytes(this.Nonce);
        Array.Reverse(bytes); //make big Endian
        header = header.Concat(bytes).ToArray();

        return header;
    }

    public string Hash(){
        return Hasher.GetSHA256Hash(this.getHeader());
    }
        
} // public class Block
 
} //namespace BlockchainFileSystem