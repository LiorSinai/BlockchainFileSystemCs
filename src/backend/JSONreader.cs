/*
 *  Created on: 3 April 2021
 *      Author: Lior Sinai
 * Description: Pseudo block for loading from JSON
 */

namespace BlockchainFileSystem
{

using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;


public class PseudoBlockchain
{   
    // Attributes
    public string Name { get; set ; }

    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime TimeStamp { get; set ; }

    [JsonIgnore]
    public bool Verified {get; set;}

    public List<PseudoBlock> Blocks {get; set;}
} 


public class PseudoBlock{
    // need to have a fully public class for the JSON reader
    public const int Version = 1;
    public int Index { get;   set; } //First mined block has index = 0, and so on.
    public uint Target { get;  set; }
    public uint Nonce { get; set; }
    public string PreviousHash { get;  set; }
    public string MerkleRoot {get;  set ; }
    
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime TimeStamp { get; set ; }

    public Dictionary<string, PseudoTokenJSON> Tokens  { get; set; } 
}

public class PseudoTokenJSON
{
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime TimeStamp { get;  set; }

    public string Author { get;  set; }
    public string UserName { get;  set; }
    public string FileName { get;  set; }
    public string FileHash { get;  set; }
}



public class BlockchainLoader
{
    public static Blockchain loadFromJson(string fileName, string dirPath){
        if (!File.Exists(fileName)) {throw new FileNotFoundException();}
        string jsonString = File.ReadAllText(fileName);

        PseudoBlockchain pseudoChain = JsonSerializer.Deserialize<PseudoBlockchain>(jsonString);

        Blockchain blockchain = new Blockchain(dirPath, pseudoChain.TimeStamp);

        foreach(PseudoBlock pb in pseudoChain.Blocks){
            // make new block with the pseudo block's information
            string blockDirPath = dirPath + "/Block" + pb.Index; 
            Block block = new Block(pb.Index, pb.PreviousHash, blockDirPath, pb.TimeStamp, pb.Target, pb.Nonce);
            string errormsg = "aborting loading at block " + (block.Index) + "\n";
            foreach(KeyValuePair<string, PseudoTokenJSON> element in pb.Tokens)
            {
                PseudoTokenJSON pt = element.Value;
                Token t = new Token(pt.UserName, pt.FileName, pt.Author, pt.TimeStamp, pt.FileHash);
                block.Tokens.Add(element.Key, t);
            }
            try{
                block.CalcMerkleRoot();
                blockchain.CommitBlock(block, false);
            }
            catch (InvalidBlockchainException e){
                throw new InvalidBlockchainException(errormsg + e.Message);
                
            }
        }
        try{
            blockchain.Verify();
        }
        catch (InvalidBlockchainException e){
            throw new InvalidBlockchainException("aborting loading", e);
        }

        return blockchain;
    }
}

} //BlockchainFileSystem