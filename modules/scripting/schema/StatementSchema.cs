using System;
using System.Collections.Generic;
using System.Linq;

namespace WakuWakuDramaClub.Scripting.Schema;

public sealed class StatementSchema
{
	private readonly List<ArgumentSchema> arguments = new List<ArgumentSchema>();

	public string Name { get; }
	public bool AllowMultiple { get; }
	public IReadOnlyList<ArgumentSchema> Arguments => arguments;

	public StatementSchema(string name, bool allowMultiple = false)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Statement name cannot be empty.", nameof(name));

		Name = name;
		AllowMultiple = allowMultiple;
	}

	public StatementSchema Argument(
		string name,
		ScriptValueKind valueKind,
		bool required = true,
		string defaultValue = "")
	{
		arguments.Add(new ArgumentSchema(name, valueKind, required, defaultValue));
		return this;
	}

	public int GetArgumentIndex(string argumentName)
	{
		for (int i = 0; i < arguments.Count; i++)
		{
			if (arguments[i].Name == argumentName)
				return i;
		}

		return -1;
	}

	public ArgumentSchema GetArgument(string argumentName)
	{
		return arguments.FirstOrDefault(argument => argument.Name == argumentName);
	}
}
