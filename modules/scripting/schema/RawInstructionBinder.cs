using System;

namespace WakuWakuDramaClub.Scripting.Schema;

public sealed class RawInstructionBinder
{
	private readonly RawInstruction raw;
	private readonly InstructionSchema schema;

	public RawInstructionBinder(RawInstruction raw, InstructionSchema schema)
	{
		this.raw = raw ?? throw new ArgumentNullException(nameof(raw));
		this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
	}

	public string Get(string argumentName)
	{
		if (schema.PrimaryStatement == null)
			return "";

		return Get(schema.PrimaryStatement.Name, argumentName);
	}

	public string Get(string statementName, string argumentName)
	{
		StatementSchema statementSchema = schema.GetStatement(statementName);
		if (statementSchema == null)
			return "";

		ArgumentSchema argumentSchema = statementSchema.GetArgument(argumentName);
		if (argumentSchema == null)
			return "";

		int index = statementSchema.GetArgumentIndex(argumentName);
		if (index < 0)
			return argumentSchema.DefaultValue;

		if (!raw.Statements.TryGetValue(statementName, out Statement statement))
			return argumentSchema.DefaultValue;

		return statement.TryGetArgumentValue(index, argumentSchema.DefaultValue);
	}
}
