/*
 *  Created on: 3 April 2021
 *      Author: Lior Sinai
 * Description: 
 */

namespace BlockchainFileSystem
{

public struct PseudoToken 
{
    public string UserName;
    public string FilePath;
    public string Author;

    public PseudoToken(string userName, string filePath, string author)
    {
        this.UserName = userName;
        this.FilePath = filePath;
        this.Author = author;
    }
    public override string ToString() => $"BlockchainFileSystem.PseudoToken(userName={UserName}, filePath={FilePath}, author={Author})";

} 

} //namespace BlockchainFileSystem