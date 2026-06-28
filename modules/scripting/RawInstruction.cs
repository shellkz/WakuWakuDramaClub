using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;
namespace WakuWakuDramaClub.Scripting;


public partial class RawInstruction : RefCounted
{
    public string Type {  get; set; }
    // public string[] Arguments { get; set; }
    // public Dictionary<string, string> Options { get; set; }

    public Dictionary<string, Statement> Statements { get; set; }
	public RawInstruction() {
        Type = string.Empty;
		// Arguments = [];
		// Options = new Dictionary<string, string>();
        Statements = new Dictionary<string, Statement>();
    }

    // public RawInstruction(string type, string[] arguments, Dictionary<string, string> options)
    // {
    //     Type = type;
    //     Arguments = arguments;
    //     Options = options;
    // }


    public string TryGetStatementArgumentValue(string name, int index, string defaultValue)
    {
        Statement statement;
        if (Statements.TryGetValue(name, out statement))
        {
            // if (name == Tr(InstructionType.ChangeBackground.ToString()))
            //     GD.Print("found no argument");
            return statement.TryGetArgumentValue(index, defaultValue);
        }
        // if (name == Tr(InstructionType.ChangeBackground.ToString()))
        //     GD.Print("found no statement");
        return defaultValue;
    }

        public string TryGetStatementArgumentsValue(string name, string defaultValue)
    {
        Statement statement;
        if (Statements.TryGetValue(name, out statement))
        {
            // if (name == Tr(InstructionType.ChangeBackground.ToString()))
            //     GD.Print("found no argument");
            return statement.TryGetArgumentsValue(defaultValue);
        }
        // if (name == Tr(InstructionType.ChangeBackground.ToString()))
        //     GD.Print("found no statement");
        return defaultValue;
    }
}
