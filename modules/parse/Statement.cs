using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Statement : Godot.RefCounted
{
	public string Type {get; set;}
	public List<string> Arguments {get; set;}

    public Statement(string Type, List<string> Arguments)
	{
		this.Type = Type;
		this.Arguments = Arguments;
	}

	public string TryGetArgumentValue(int index, string defaultValue)
	{
		if (index >= 0 && index < Arguments.Count)
		{
			return Arguments[index];
		}
		return defaultValue;
	}

	public string TryGetArgumentsValue(string defaultValue)
	{
		if (Arguments.Count > 0)
		{
			string merged = "";
			foreach (string argument in Arguments)
			{
				merged += argument;
				merged += " ";
			}
			return merged.Trim();
		}
		return defaultValue;
	}
}
