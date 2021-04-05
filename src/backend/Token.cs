/*
 *  Created on: 3 April 2021
 *      Author: Lior Sinai
 * Description: Token metadata and file interaction
 */

namespace BlockchainFileSystem
{

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.Json.Serialization;

[Serializable]
public class InvalidTokenException : Exception
{
    public InvalidTokenException() { }

    public InvalidTokenException(string message)
        : base(message) { }

    public InvalidTokenException(string message, Exception inner)
        : base(message, inner) { }
}

public class Token 
{
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime TimeStamp { get; private set; }
    public string Author { get; private set; }
    public string UserName { get; private set; }
    public string FileName { get; private set; }
    public string FileHash { get; private set; }
    //std::vector<uint8_t> fileData;

    public Token(string userName_, string filePath_, string author_)
    {
        this.UserName = userName_;
        int idx = filePath_.LastIndexOfAny(new char[] { '/', '\\'}); 
        this.FileName = filePath_.Substring(idx+1); //if -1, take the whole string
        this.Author = author_;
        this.TimeStamp = DateTime.UtcNow;
        this.FileHash = GetFileHash(filePath_);
    }
    public Token(PseudoToken t)
    {
        this.UserName = t.UserName;
        int idx = t.FilePath.LastIndexOfAny(new char[] { '/', '\\'}); 
        this.FileName = t.FilePath.Substring(idx+1); //if -1, take the whole string
        this.Author = t.Author;
        this.TimeStamp = DateTime.UtcNow;
        this.FileHash = GetFileHash(t.FilePath);
    }
    public Token(string userName_, string fileName_, string author_, DateTime timeStamp, string fileHash_)
    {   
        //for loading
        this.UserName = userName_;
        this.FileName = fileName_;
        this.Author = author_;
        this.TimeStamp = timeStamp;
        this.FileHash = fileHash_;
    }
    public override string ToString() => $"BlockchainFileSystem.Token(userName={UserName}, fileName={FileName}, author={Author}, timeStamp={TimeStamp}, fileHash={FileHash})";

    public static string GetFileHash(string filePath)
    {
        byte[] contents = File.ReadAllBytes(filePath);
        string hash = Hasher.GetSHA256Hash(contents);
        return hash;    
    } 

    public void Print(string dir ="")
    {
        string outstring = "";
        outstring += "User name: " + this.UserName + "\n";
        outstring += "file name: " + this.FileName + "\n";
        outstring += "file author: " + this.Author + "\n";
        outstring += "file hash: " + this.FileHash + "\n";
        outstring += "file size: ";
        string filePath = dir != "" ? dir + "/" + this.FileName : this.FileName;  
        int show_max   = 51;
        int show_start = 24;
        int show_end   = 24;
        try{
            byte[] contents = File.ReadAllBytes(filePath);
            int fileSize = contents.Length;
            if (fileSize > 1_000_000_000)  { outstring += (fileSize/1000000000) + "GB\n";}
            else if (fileSize > 1_000_000) { outstring += (fileSize/1000000) + "MB\n";}
            else if (fileSize > 1_000)     { outstring += (fileSize/1000) + "kB\n";}
            else { outstring += fileSize + "B\n";}
            outstring += "file data: ";
            if (fileSize * 2 > show_max){
                for (int i = 0; i < show_start; i++) {
                    outstring += String.Format("{0:x2}", contents[i]);
                }
                outstring += "...";
                for (int i = fileSize - show_end; i < fileSize; i++) {
                    outstring += String.Format("{0:x2}", contents[i]);
                }
            }
            else{
                outstring += Hasher.Bytes2hex(contents);
            }
            outstring += "\n";
        }
        catch (FileNotFoundException) {
            outstring += "0\n";
            outstring += "file data: WARNING: no file loaded from " + filePath + "\n";
        }
        outstring += "token creation time UTC: " + this.TimeStamp;

        Console.WriteLine(outstring);
    }

    public byte[] Serialise()
    {   
        // userName | fileName | author | fileHash(64) | timeStamp (4)
        byte[] header = new byte[] {};
        byte[] bytes;
        bytes = Encoding.UTF8.GetBytes(this.UserName + this.FileName + this.Author);
        header = header.Concat(bytes).ToArray();
        bytes = Hasher.Hex2bytes(this.FileHash);
        header = header.Concat(bytes).ToArray();
        int timeStamp = Utilities.ConvertToUnixTimestamp(this.TimeStamp);
        bytes = BitConverter.GetBytes(timeStamp);
        header = header.Concat(bytes).ToArray();
       
        return header;
    }

    public string Hash()
    {   
        return Hasher.GetSHA256Hash(this.Serialise());
    }

    public bool Verify(string dir ="")
    {   
        /* load token data and check hash */
        string path = dir + "/" + this.FileName;
        string hash = GetFileHash(path);
        if (hash != this.FileHash){
            string msg = this.FileName + " hashes do not match:\nOriginal:   " + this.FileHash + "\nCalculated: " + hash;
            throw new InvalidTokenException(msg);
        }
        return true;
    }



} 

} //namespace BlockchainFileSystem

