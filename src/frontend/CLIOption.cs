/*
 *  Created on: 4 April 2021
 *      Author: Lior Sinai
 * Description: Option for a command line interface
 */


namespace BlockchainFileSystem
{ 

using System.Collections.Generic;

public delegate void CLIactionDelegate (CLIArguments args);

public class CLIOption
{
    public string ShortCmd {get; set;}
    public string LongCmd {get; set;}
    public string Paramaters {get; set;}
    public string HelpText {get; set;}
    public CLIactionDelegate Action;

    public CLIOption(string shortCmd, string longCmd, string pars, string helpText, CLIactionDelegate action)
    {
        this.ShortCmd = shortCmd;
        this.LongCmd  = longCmd;
        this.Paramaters = pars;
        this.HelpText = helpText;
        this.Action = action;
    }
}

public class CLIArguments
{
    public string Argc {get; set;}
    public List<string> Argv {get; set;}
    public string Text {get; set;}
    public CLIArguments(string argc, List<string> argv, string text)
    {
        this.Argc = argc;
        this.Argv = argv;
        this.Text = text;
    }
    public CLIArguments(){
        this.Argc = "";
        this.Argv = new List<string>();
        this.Text = "";
    }
}

} //namespace BlockchainFileSystem