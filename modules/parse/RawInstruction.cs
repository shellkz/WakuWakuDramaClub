using Godot;
using System;
using System.Collections.Generic;
namespace WakuWakuDramaClub.Parse;


public partial class RawInstruction : RefCounted
{


    public string Type {  get; set; }
    public string[] Arguments { get; set; }
    public Dictionary<string, string> Options { get; set; }

	public RawInstruction() {
        Type = string.Empty;
		Arguments = [];
		Options = new Dictionary<string, string>();
    }

    public RawInstruction(string type, string[] arguments, Dictionary<string, string> options)
    {
        Type = type;
        Arguments = arguments;
        Options = options;
    }
}
