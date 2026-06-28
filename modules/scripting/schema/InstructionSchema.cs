using System;
using System.Collections.Generic;
using System.Linq;

namespace WakuWakuDramaClub.Scripting.Schema;

public sealed class InstructionSchema
{
	private readonly List<StatementSchema> optionStatements = new List<StatementSchema>();
	private StatementSchema currentStatement;

	public string Name { get; private set; } = "";
	public StatementSchema PrimaryStatement { get; private set; }
	public IReadOnlyList<StatementSchema> OptionStatements => optionStatements;

	private InstructionSchema()
	{
	}

	public static InstructionSchema Create()
	{
		return new InstructionSchema();
	}

	public InstructionSchema Primary(string statementName)
	{
		PrimaryStatement = new StatementSchema(statementName);
		Name = statementName;
		currentStatement = PrimaryStatement;
		return this;
	}

	public InstructionSchema PrimaryStatementNamed(string statementName)
	{
		return Primary(statementName);
	}

	public InstructionSchema Statement(string statementName, bool allowMultiple = false)
	{
		StatementSchema statement = new StatementSchema(statementName, allowMultiple);
		optionStatements.Add(statement);
		currentStatement = statement;
		return this;
	}

	public InstructionSchema Option(string statementName, bool allowMultiple = false)
	{
		return Statement(statementName, allowMultiple);
	}

	public InstructionSchema Argument(
		string name,
		ScriptValueKind valueKind,
		bool required = true,
		string defaultValue = "")
	{
		if (currentStatement == null)
			throw new InvalidOperationException("Call Primary(...) or Statement(...) before adding arguments.");

		currentStatement.Argument(name, valueKind, required, defaultValue);
		return this;
	}

	public StatementSchema GetStatement(string statementName)
	{
		if (PrimaryStatement != null && PrimaryStatement.Name == statementName)
			return PrimaryStatement;

		return optionStatements.FirstOrDefault(statement => statement.Name == statementName);
	}
}
