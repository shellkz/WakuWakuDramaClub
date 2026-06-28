using System;

namespace WakuWakuDramaClub.Scripting.Schema;

public sealed class ArgumentSchema
{
	public string Name { get; }
	public ScriptValueKind ValueKind { get; }
	public bool Required { get; }
	public string DefaultValue { get; }

	public ArgumentSchema(
		string name,
		ScriptValueKind valueKind,
		bool required = true,
		string defaultValue = "")
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Argument name cannot be empty.", nameof(name));

		if (valueKind == ScriptValueKind.Undefined)
			throw new ArgumentException("Argument value kind cannot be undefined.", nameof(valueKind));

		Name = name;
		ValueKind = valueKind;
		Required = required;
		DefaultValue = defaultValue;
	}
}
