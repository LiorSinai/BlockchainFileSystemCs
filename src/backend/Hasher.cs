/*
 *  Created on: 3 April 2021
 *      Author: Lior Sinai
 * Description: 
 */


using System;
using System.Text;
using System.Security.Cryptography;
    
namespace BlockchainFileSystem
{  
public static class Hasher
{
    public static string GetSHA256Hash(string rawData)  
    {  
        using (SHA256 sha256Hash = SHA256.Create())  
        {  
            byte[] hash = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));  
            return Bytes2hex(hash);
        }  
    }  

    public static string GetSHA256Hash(byte[] rawData)  
    {  
        using (SHA256 sha256Hash = SHA256.Create())  
        {  
            byte[] hash = sha256Hash.ComputeHash(rawData);  
            return Bytes2hex(hash);
        }  
    }  

    public static string Bytes2hex(byte[] bytes){
        string hexstr = string.Empty;
        foreach (byte x in bytes)
        {
            hexstr += String.Format("{0:x2}", x);
        }
        return hexstr;
    }

    public static byte[] Hex2bytes(string hex) {
        //https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
        if (hex.Length % 2 == 1) hex ="0" + hex;

        byte[] arr = new byte[hex.Length >> 1];
        for (int i = 0; i < hex.Length >> 1; ++i)
        {
            arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
        }
        return arr;
    }

    public static int GetHexVal(char hex) {
        int val = (int)hex;
        if (val >=48 && val <= 57){ //0-9
            return val - 48;
        }
        else if (val >= 65 && val<= 70){ //A-F
            return val - 55; 
        }
        else if (val >= 97 && val<= 102){ //a-f
            return val - 87; 
        }
        else{
            throw new ArgumentException("Invalid hexadecimal digit: " + hex);
        }
    }
}

}//namespace BlockchainFileSystem