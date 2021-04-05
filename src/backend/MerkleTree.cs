/*
 *  Created on: 3 April 2021
 *      Author: Lior Sinai
 * Description: A simple Merkle Tree implementation
 * 
 * sources: 
 *  - https://en.wikipedia.org/wiki/Merkle_tree
 *  - https://101blockchains.com/merkle-trees/ 
 */

namespace BlockchainFileSystem
{

using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

public class MerkleTree
{
    public static string Root(List<string> hashes)
    {
        List<byte[]> layer = new List<byte[]>();
        foreach(string h in hashes)
        {
            layer.Add(Hasher.Hex2bytes(h));
        }
        if (layer.Count == 0)
        { 
            return Hasher.GetSHA256Hash("");
        }
        return HashLayer(layer);
    }

    private static string HashLayer(List<byte[]> layer)
    {
        int layerSize = layer.Count();
        if (layerSize == 1) return Hasher.Bytes2hex(layer[0]); //found the root
        List<byte[]> upperLayer = new List<byte[]> { };

        SHA256Managed hasher = new SHA256Managed();
        for (int i = 0; i < (layerSize -1); i = i + 2)
        {
            byte[] node = layer[i];
            node = node.Concat(layer[i+1]).ToArray();
            byte[] hash = hasher.ComputeHash(node);
            upperLayer.Add(hash);
        }
        if (layerSize % 2 == 1)
        {
            //odd number of nodes, therefore the last node needs to be paired with itself
            int i = layerSize - 1;
            byte[] node = layer[i];
            node = node.Concat(layer[i]).ToArray();
            byte[] hash = hasher.ComputeHash(node);
            upperLayer.Add(hash);
        }
        return HashLayer(upperLayer);
    }
     
}
 
} 