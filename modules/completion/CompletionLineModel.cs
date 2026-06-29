using System.Collections.Generic;
using System.Linq;
using System.Text;
using WakuWakuDramaClub.Scripting.Schema;

namespace WakuWakuDramaClub.Completion;

public sealed class CompletionLineResult
{
	public CompletionRawInstruction Raw { get; }
	public CompletionCursorContext Cursor { get; }

	public CompletionLineResult(CompletionRawInstruction raw, CompletionCursorContext cursor)
	{
		Raw = raw;
		Cursor = cursor;
	}

	public string ToDebugString()
	{
		StringBuilder builder = new StringBuilder();
		builder.Append("CompletionLineResult ");
		builder.Append("{ Raw = ");
		builder.Append(Raw.ToDebugString());
		builder.Append(", Cursor = ");
		builder.Append(Cursor.ToDebugString());
		builder.Append(" }");
		return builder.ToString();
	}
}

public sealed class CompletionRawInstruction
{
	public string Type { get; }
	public IReadOnlyList<CompletionStatement> Statements { get; }
	public bool IsActorShorthand { get; }
	public string ActorId { get; }
	public bool IsComplete { get; }

	public CompletionRawInstruction(
		string type,
		IReadOnlyList<CompletionStatement> statements,
		bool isActorShorthand,
		string actorId,
		bool isComplete)
	{
		Type = type;
		Statements = statements;
		IsActorShorthand = isActorShorthand;
		ActorId = actorId;
		IsComplete = isComplete;
	}

	public string ToDebugString()
	{
		string statementText = string.Join(", ", Statements.Select(statement => statement.ToDebugString()));
		return $"{{ Type = {Format(Type)}, Statements = [{statementText}], IsActorShorthand = {IsActorShorthand}, ActorId = {Format(ActorId)}, IsComplete = {IsComplete} }}";
	}

	private static string Format(string value)
	{
		return value == null ? "null" : $"\"{value}\"";
	}
}

public sealed class CompletionStatement
{
	public string Type { get; }
	public IReadOnlyList<CompletionArgument> Arguments { get; }
	public int StartColumn { get; }
	public int EndColumn { get; }

	public CompletionStatement(
		string type,
		IReadOnlyList<CompletionArgument> arguments,
		int startColumn,
		int endColumn)
	{
		Type = type;
		Arguments = arguments;
		StartColumn = startColumn;
		EndColumn = endColumn;
	}

	public string ToDebugString()
	{
		string argumentText = string.Join(", ", Arguments.Select(argument => argument.ToDebugString()));
		return $"{{ Type = \"{Type}\", Arguments = [{argumentText}], Range = {StartColumn}..{EndColumn} }}";
	}
}

public sealed class CompletionArgument
{
	public string Value { get; }
	public int StartColumn { get; }
	public int EndColumn { get; }

	public CompletionArgument(string value, int startColumn, int endColumn)
	{
		Value = value;
		StartColumn = startColumn;
		EndColumn = endColumn;
	}

	public string ToDebugString()
	{
		return $"{{ Value = \"{Value}\", Range = {StartColumn}..{EndColumn} }}";
	}
}

public sealed class CompletionCursorContext
{
	public CompletionCursorKind Kind { get; }
	public string Prefix { get; }
	public string InstructionName { get; }
	public string StatementName { get; }
	public int? StatementIndex { get; }
	public int? ArgumentIndex { get; }
	public ScriptValueKind? ValueKind { get; }

	public CompletionCursorContext(
		CompletionCursorKind kind,
		string prefix = "",
		string instructionName = null,
		string statementName = null,
		int? statementIndex = null,
		int? argumentIndex = null,
		ScriptValueKind? valueKind = null)
	{
		Kind = kind;
		Prefix = prefix ?? "";
		InstructionName = instructionName;
		StatementName = statementName;
		StatementIndex = statementIndex;
		ArgumentIndex = argumentIndex;
		ValueKind = valueKind;
	}

	public string ToDebugString()
	{
		return $"{{ Kind = {Kind}, Prefix = \"{Prefix}\", InstructionName = {Format(InstructionName)}, StatementName = {Format(StatementName)}, StatementIndex = {Format(StatementIndex)}, ArgumentIndex = {Format(ArgumentIndex)}, ValueKind = {Format(ValueKind)} }}";
	}

	private static string Format(string value)
	{
		return value == null ? "null" : $"\"{value}\"";
	}

	private static string Format(int? value)
	{
		return value.HasValue ? value.Value.ToString() : "null";
	}

	private static string Format(ScriptValueKind? value)
	{
		return value.HasValue ? value.Value.ToString() : "null";
	}
}

public enum CompletionCursorKind
{
	PrimaryStart,
	InstructionArgument,
	ActorAction,
	StatementName,
	StatementArgument,
	DialogueText,
	Unknown
}
