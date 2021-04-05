# BlockchainFileSystemCpp

A blockchain file system. This program moves files into a directory which is registered on a private blockchain.
The blockchain meta data is stored in JSON format in the directory.
This blockchain is "immutable". 
That is, any change to any file registered inside a block will make that block and all subsequent blocks after it invalid.

This project is essentially a very basic version control system.
Hence the terminology used is based on version control software, such "stage" and "commit" instead of crypto-currency terminology such as "mine" and  "confirm". 
Ironically it itself is versioned and hosted GitHub using Git, a much more powerful version control software. 
This project however is not meant to be a production ready system; it is simply to make the concept of a blockchain more tangible for teaching purposes.

## Blockchain Features

1. Immutable: any change to any file registered inside a block will make that block and all subsequent blocks after it invalid
2. <s>De</s>Centralized
	1. <s>Consenesus mechanism </s>
	2. <s>Propagation mechanism </s>
	2. <s>Central Waiting list </s>
3. Proof of work
	1. Manual setting of difficulty from level 0 to 256. Default is 0.
	2. <s> Automatic difficulty adjustment mechanism </s>
4. Storage: the blockchain can be saved in JSON format and reloaded from JSON format. Files are stored in a standard OS directory.
5. <s>Cryptographic security </s>

It was a design choice to store the files in a normal directory, instead of lumping them into a single binary file.
This makes the blockchain more tangible and interactive.
It is STRONGLY ADVISEABLE TO NOT CHANGE ANYTHING in the blockchain directory because this will probably set the blockchain into an invalid state.

## Command Line Interface

This program was made using .NET 5.0 and requires it to run.
Go to [dotnet.microsoft.com/download/dotnet/5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
to download it and install it.

Once .NET is installed, in a terminal navigate to the /src folder and type ```dotnet run```.
See [docs.microsoft.com/en-us/dotnet/core/tools/dotnet-run](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-run) for more information.
This should start the CLI and you'll be greated with 
> Welcome to the Blockchain file system

### Commands
Available commands:
```
-c | --commit-block                 commit a block to the blockchain
     --header [index]               show the hexadecimal header for a block
-h | --help                         display this help menu
-i | --initialise name              initialise a new blockchain
-l | --load filePath dirPath        load an existing blockchain at dirPath from a json file
-p | --print-blockchain [h]         print the blockchain. If h, print header only
     --print-block [index]          print block at index in the blockchain
     --print-token [id]             print token with hash=id
-q | --quit                         quit
-s | --save                         save the blockchain to a json file
     --sha256 Text                  calculate the SHA256 hash of the text (rest of line)
     --sha256-hex hexdec            calculate the SHA256 hash of a hexadecimal number
     --sha256-file fileName         calculate the SHA256 hash of file contents
     --switch-user [username]       change the active user
-t | --stage-token                  stage a token into a block
     --target [target]              set and display the proof of work target
-u | --unstage                      remove the last uncommitted block
     --verify                       verify the blockchain (excludes uncommited blocks)
-v | --version                      print this version
 ```
 
 ### Examples
 #### Initialise
 Once the CLI has started, run the following:
 ```
 @admin > --initialise MyPersonalBlockChain  //follow prompts to initialise a new block chain
 @admin > --stage-token  //follow prompts to stage a new token
 @admin > --print-block  
 @admin > --commit-block  
 @admin > --print-blockchain
 ```
 
 #### Load
 ```
 @admin > --load MyPersonalBlockChain/MyPersonalBlockChain.json //extracts MyPersonalBlockChain as the directory
 //... final line is:
 //verified!
 @admin > --stage-token   //follow prompts to stage a new token
 @admin > --print-block   
 @admin > --commit-block 
@admin > --print-blockchain 
 ```

## Proof of work

Proof of work is not necessary for a centralised system. 
It is included here for completeness.

The proof of work is identical to the Bitcoin proof of work.
That is, the SHA256 of the block header must be less than ```2^(256-n)``` for a target ```n```.
Equivalent ways of stating this are: the 256 bit hash must start with ```n``` zeros, or the 64 character hexadecimal hash must start with ```n/4``` zeros.
Unlike Bitcoin, ```n``` cannot have decimals, so it is an integer from ```0``` to ```256```.

Increasing the target by 1 doubles the difficulty and average time taken.
A standard pc has 2GHz ~= 2^30 calcs/s of processing power.
For my CPU, this means it can do about 1.4 million hashes/s ~= 2^20.4 hashes/s
This means:
* a target of 20 takes 1 seconds
* a target of 21 takes 2 seconds
* a target of 22 takes 4 seconds
*    .... 
* a target of 30  will take 1024 seconds ~= 17 min
* a target of 40 will take 12.1 days
* a target of 45 will take 1 year
* a target of 76 will take 2.3 billion years

A target of 76 was the Bitcoin difficulty at the time of this commit. (Conversion: 256 - 208 - log2((2^16-1)/D) where D is the difficulty.)
It is strongly recommended to keep the target at 0.

## To do list:
- Incremental saving to JSON. The current code saves the whole blockchain each time.
- Threading, so the program can send messages back to the CLI for outputting interval messages during the proof of work. 
