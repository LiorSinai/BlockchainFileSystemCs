/*
 *  Created on: 3 April 2021
 *      Author: Lior Sinai
 * Description: An immutable blockchain
 */

namespace BlockchainFileSystem
{ 

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

public class InvalidBlockchainException : Exception
{
    public InvalidBlockchainException() { }

    public InvalidBlockchainException(string message)
        : base(message) { }

    public InvalidBlockchainException(string message, Exception inner)
        : base(message, inner) { }
}


public class Blockchain
{   
    // Attributes
    public string Name { get; set ; }

    [JsonIgnore]
    public string BlockchainDirectory { get; set ; }
    public const int Version = 1;

    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime TimeStamp { get; private set ; }

    [JsonIgnore]
    public bool Verified {get; private set;}

    [JsonIgnore]
    public const int MAX_DIFFICULTY = 256;

    public List<Block> Blocks {get; private set;}

    public Blockchain(string directory_="") {
        if (directory_ != ""){
            this.BlockchainDirectory = directory_;
            int idx = directory_.LastIndexOfAny(new char[] { '/', '\\'}); 
            this.Name = directory_.Substring(idx+1); //if -1, take the whole string
        }
        else{
            this.Name = "Blockchain" ;
            this.BlockchainDirectory = System.IO.Directory.GetCurrentDirectory() + "/Blockchain" ;
        }
        //set defaults
        this.TimeStamp = DateTime.UtcNow;
        // Determine whether the directory exists.
        if (!System.IO.Directory.Exists(this.BlockchainDirectory))
        {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(this.BlockchainDirectory);
        }
        this.Blocks = new List<Block>();
    }

    public Blockchain(string directory_, DateTime timeStamp_){
        //for loading
        this.BlockchainDirectory = directory_;
        int idx = directory_.LastIndexOfAny(new char[] { '/', '\\'}); 
        this.Name = directory_.Substring(idx+1); //if -1, take the whole string
        this.TimeStamp = timeStamp_;
        //set defaults
        this.Blocks = new List<Block>();
    }

    // functions
    public Block Front() {return this.Blocks[0];}
    public Block Back() {return this.Blocks[this.Blocks.Count - 1];}
    public Block At(int index) {return this.Blocks[index];}
    public int Height() {return this.Blocks.Count;}
    public int Index() {return this.Blocks.Count - 1;}

    public Block MakeBlock(){
        string previousHash;
        if (this.Height() == 0) {
            previousHash = "0000000000000000000000000000000000000000000000000000000000000000";
        }
        else{
            previousHash = this.Back().Hash();
        }
        return new Block(this.Index() + 1, previousHash,  this.BlockchainDirectory );
    }

    public void CommitBlock(Block block, bool doProofOfWork=true)
    {
        // checks
        if (block.Index != this.Index() + 1){ 
            throw new InvalidBlockchainException("Block index must follow last blockchain index: "+ block.Index + "!=" + (this.Index() + 1));
        }
        if (this.Height() > 0){
            string previousHash = this.Back().Hash();
            if (block.PreviousHash != previousHash){
                throw new InvalidBlockchainException("Previous block hashes do not match:\nProposed block:   " + block.PreviousHash +
                                                                                        "\nBlockchain block: " + previousHash);
            }
        }
        if (Block.Version != Blockchain.Version){ 
            throw new InvalidBlockchainException("Block version differs from Blockchain version: "+ Block.Version + "!=" + Blockchain.Version);
        }
        if (block.TimeStamp >= DateTime.UtcNow){ 
            throw new InvalidBlockchainException("Block cannot be from the future: " + block.TimeStamp);
        }
        if (doProofOfWork){
            uint proof = ProofOfWork(block.getHeader(), block.Target, block.Nonce);
            block.Nonce = proof;
        }
        this.Blocks.Add(block);
    }

    public bool Verify()
    {
        for (int i = 0; i < this.Height(); i++){
            try{
                Blocks[i].Verify();
            }
            catch(InvalidBlockException e){
                throw new InvalidBlockchainException("Blockchain is not valid", e);
            }
            catch(Exception e){
                throw new InvalidBlockchainException("Blockchain is not valid", e);
            }

        }
        return true;
    }

    public void Print(bool printBlocks=true)
    {
        Console.WriteLine("------------------------------------------------------------------------------------------------");
        Console.WriteLine("------------------------------------------------------------------------------------------------");
        Console.WriteLine("");
        Console.WriteLine("Blockchain VERSION " + Blockchain.Version);
        Console.WriteLine("name:          " + this.Name);
        Console.WriteLine("directory:     " + this.BlockchainDirectory);
        Console.WriteLine("timestamp UTC: " + this.TimeStamp);
        Console.WriteLine("height:        " + this.Height());
        Console.WriteLine("");

        if (printBlocks){
            foreach (Block block in this.Blocks){
                block.Print();
            }
        }

        Console.WriteLine("------------------------------------------------------------------------------------------------");
        Console.WriteLine("------------------------------------------------------------------------------------------------");
    }

    public static bool IsValidProof(byte[] bytes, int target){
        int n = target/8; // a byte has 8 bits
        for (int i = 0; i < n; i++){
            if (bytes[i] != 0 ) {return false;}
        }
        int rem = target - n * 8; // remainder
        if (rem == 0){return true;}
        else {
            int i = n;
            // e.g Convert.ToString((int)Math.Pow(2, 8-3)-1, 2) = 0001_1111 = 31
            return bytes[i] < Math.Pow(2, 8 - rem); 
        }
    }

    public static bool IsValidProof(string hash, int target){
        int n = target/4; // a hex char has 4 bits
        for (int i = 0; i < n; i++){
            if (Hasher.GetHexVal(hash[i]) != 0 ) {return false;}
        }
        int rem = target - n * 4;
        if (rem == 0){return true;}
        else {
            int i = n;
            // e.g Convert.ToString((int)Math.Pow(2, 4-3)-1, 2) = 0001 = 1
            return Hasher.GetHexVal(hash[i]) < Math.Pow(2, 4 - rem); 
        }
    }

    uint ProofOfWork(byte[] bytes, uint target, uint start = 0)
    {
    /*
    * Simple proof of work
    * First n bits of the 256 bits of the SHA256 hash must be 0
    * Increasing the target by 1 doubles the difficulty and average time taken
    * Max difficulty = 256 (only valid solution is 256 zeros)
    * -- A standard pc has 2GHz ~= 2^30 calcs/s of processing power. 
    * -- Hashing power of my CPU is 1.4m hashes/s ~= 2^20.4 hashes/s
    *    -> a target of 20 will take 1 seconds
    *    -> a target of 21 will take 2 seconds
    *    -> a target of 22 will take 4 seconds
    *    .... 
    *    -> a target of 30 will take 1024 seconds ~= 17 min
    *    ...
    *    -> a target of 40 will take 12.1 days
    *    ... 
    *    -> a target of 45 will take 1 year
    *    -> current bitcoin target is 76. This will take 2.3 billion years
    */
        SHA256Managed hasher = new SHA256Managed();

        target = Math.Min(MAX_DIFFICULTY, target);
        
        byte[] nonceBytes = BitConverter.GetBytes(start);
        Array.Reverse(nonceBytes); //make big Endian

        byte[] bytes_WIP = bytes;
        int first = bytes.Length - nonceBytes.Length;
        Array.Copy(nonceBytes, 0, bytes_WIP, first, nonceBytes.Length);

        byte[] hash = hasher.ComputeHash(bytes_WIP);

        while (!IsValidProof(hash, (int)target)){
            Utilities.Bytes_add_1(bytes_WIP, first);
            hash = hasher.ComputeHash(bytes_WIP);
        }

        Array.Copy(bytes_WIP, first, nonceBytes, 0, nonceBytes.Length);
        Array.Reverse(nonceBytes); //BitConverter is IsLittleEndian
        uint nonce = BitConverter.ToUInt32(nonceBytes, 0);

        return nonce;
    }
     
} //class Blockchain
 
} //namespace BlockchainFileSystem