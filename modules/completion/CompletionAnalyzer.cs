using System;
using System.Collections.Generic;
using System.Linq;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Parsing;
using WakuWakuDramaClub.Scripting.Schema;

namespace WakuWakuDramaClub.Completion;

public sealed class CompletionAnalyzer
{
	private readonly InstructionRegistry registry;
	private readonly ScriptPreprocessor preprocessor;
	private readonly ResourceManager resourceManager;

	public CompletionAnalyzer(
		InstructionRegistry registry,
		ScriptPreprocessor preprocessor,
		ResourceManager resourceManager = null)
	{
		this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
		this.preprocessor = preprocessor ?? throw new ArgumentNullException(nameof(preprocessor));
		this.resourceManager = resourceManager;
	}

	public CompletionLineResult Analyze(string line, int caretColumn)
	{
		line ??= "";
		caretColumn = Math.Clamp(caretColumn, 0, line.Length);

		List<PositionedToken> tokens = Tokenize(line);
		if (tokens.Count == 0)
			return CreatePrimaryStartResult("");

		ActorShorthandInfo actorShorthand = DetectActorShorthand(tokens);
		if (ShouldReturnActorAction(tokens, actorShorthand, caretColumn))
			return CreateActorActionResult(tokens, actorShorthand, caretColumn);

		List<PositionedToken> normalizedTokens = NormalizeActorShorthand(tokens, actorShorthand);
		List<CompletionStatement> statements = GroupStatements(normalizedTokens);
		string instructionName = statements.Count > 0 ? statements[0].Type : null;
		CompletionRawInstruction raw = new CompletionRawInstruction(
			instructionName,
			statements,
			actorShorthand.IsShorthand,
			actorShorthand.ActorId,
			IsComplete(instructionName, statements));

		CompletionCursorContext cursor = AnalyzeCursor(line, caretColumn, normalizedTokens, statements, instructionName);
		return new CompletionLineResult(raw, cursor);
	}

	public CompletionLineResult AnalyzeTest(string line, int caretColumn)
	{
		return Analyze(line, caretColumn);
	}

	private CompletionLineResult CreatePrimaryStartResult(string prefix)
	{
		CompletionRawInstruction raw = new CompletionRawInstruction(
			null,
			Array.Empty<CompletionStatement>(),
			false,
			null,
			false);

		return new CompletionLineResult(raw, new CompletionCursorContext(CompletionCursorKind.PrimaryStart, prefix));
	}

	private CompletionLineResult CreateActorActionResult(
		List<PositionedToken> tokens,
		ActorShorthandInfo actorShorthand,
		int caretColumn)
	{
		string prefix = "";
		if (tokens.Count > 1)
			prefix = GetPrefix(tokens[1], caretColumn);

		CompletionRawInstruction raw = new CompletionRawInstruction(
			null,
			Array.Empty<CompletionStatement>(),
			true,
			actorShorthand.ActorId,
			false);

		CompletionCursorContext cursor = new CompletionCursorContext(CompletionCursorKind.ActorAction, prefix);
		return new CompletionLineResult(raw, cursor);
	}

	private CompletionCursorContext AnalyzeCursor(
		string line,
		int caretColumn,
		List<PositionedToken> tokens,
		List<CompletionStatement> statements,
		string instructionName)
	{
		if (statements.Count == 0 || string.IsNullOrEmpty(instructionName))
		{
			PositionedToken token = FindTokenAtOrBefore(tokens, caretColumn);
			return new CompletionCursorContext(CompletionCursorKind.PrimaryStart, GetPrefix(token, caretColumn));
		}

		for (int statementIndex = statements.Count - 1; statementIndex >= 0; statementIndex--)
		{
			CompletionStatement statement = statements[statementIndex];
			if (caretColumn < statement.StartColumn)
				continue;

			PositionedToken statementToken = tokens.FirstOrDefault(token => token.StartColumn == statement.StartColumn);
			if (statementToken != null && caretColumn <= statementToken.EndColumn)
			{
				if (statementIndex == 0)
				{
					return new CompletionCursorContext(
						CompletionCursorKind.PrimaryStart,
						GetPrefix(statementToken, caretColumn),
						instructionName,
						statement.Type,
						statementIndex);
				}

				return new CompletionCursorContext(
					CompletionCursorKind.StatementName,
					GetPrefix(statementToken, caretColumn),
					instructionName,
					statement.Type,
					statementIndex);
			}

			return AnalyzeStatementArgumentCursor(
				caretColumn,
				statement,
				statementIndex,
				instructionName,
				statementIndex == 0 ? CompletionCursorKind.InstructionArgument : CompletionCursorKind.StatementArgument);
		}

		return new CompletionCursorContext(CompletionCursorKind.Unknown);
	}

	private CompletionCursorContext AnalyzeStatementArgumentCursor(
		int caretColumn,
		CompletionStatement statement,
		int statementIndex,
		string instructionName,
		CompletionCursorKind argumentKind)
	{
		for (int argumentIndex = 0; argumentIndex < statement.Arguments.Count; argumentIndex++)
		{
			CompletionArgument argument = statement.Arguments[argumentIndex];
			if (caretColumn >= argument.StartColumn && caretColumn <= argument.EndColumn)
			{
				if (IsBeyondDefinedArguments(instructionName, statement.Type, argumentIndex))
				{
					return new CompletionCursorContext(
						CompletionCursorKind.StatementName,
						GetPrefix(argument, caretColumn),
						instructionName,
						null,
						null);
				}

				ScriptValueKind? valueKind = GetValueKind(instructionName, statement.Type, argumentIndex);
				CompletionCursorKind kind = valueKind == ScriptValueKind.DialogueText
					? CompletionCursorKind.DialogueText
					: argumentKind;

				return new CompletionCursorContext(
					kind,
					GetPrefix(argument, caretColumn),
					instructionName,
					statement.Type,
					statementIndex,
					argumentIndex,
					valueKind);
			}
		}

		int nextArgumentIndex = statement.Arguments.Count;
		ScriptValueKind? nextValueKind = GetValueKind(instructionName, statement.Type, nextArgumentIndex);
		if (nextValueKind == null)
		{
			return new CompletionCursorContext(
				CompletionCursorKind.StatementName,
				"",
				instructionName,
				null,
				null);
		}

		CompletionCursorKind nextKind = nextValueKind == ScriptValueKind.DialogueText
			? CompletionCursorKind.DialogueText
			: argumentKind;

		return new CompletionCursorContext(
			nextKind,
			"",
			instructionName,
			statement.Type,
			statementIndex,
			nextArgumentIndex,
			nextValueKind);
	}

	private List<CompletionStatement> GroupStatements(List<PositionedToken> tokens)
	{
		List<CompletionStatement> statements = new List<CompletionStatement>();
		if (tokens.Count == 0 || tokens[0].Type != CompletionTokenType.Keyword)
			return statements;

		int index = 0;
		while (index < tokens.Count)
		{
			PositionedToken keyword = tokens[index];
			if (keyword.Type != CompletionTokenType.Keyword)
			{
				index++;
				continue;
			}

			int nextIndex = index + 1;
			while (nextIndex < tokens.Count && tokens[nextIndex].Type != CompletionTokenType.Keyword)
			{
				nextIndex++;
			}

			List<CompletionArgument> arguments = tokens
				.GetRange(index + 1, nextIndex - index - 1)
				.Select(token => new CompletionArgument(token.Value, token.StartColumn, token.EndColumn))
				.ToList();

			int endColumn = arguments.Count > 0 ? arguments[^1].EndColumn : keyword.EndColumn;
			statements.Add(new CompletionStatement(keyword.Value, arguments, keyword.StartColumn, endColumn));
			index = nextIndex;
		}

		return statements;
	}

	private List<PositionedToken> NormalizeActorShorthand(
		List<PositionedToken> tokens,
		ActorShorthandInfo actorShorthand)
	{
		if (!actorShorthand.IsShorthand || tokens.Count < 2)
			return tokens;

		List<PositionedToken> result = tokens.ToList();
		if (result[1].Type == CompletionTokenType.Dialogue)
		{
			string dialogueKeyword = registry.GetById(Scripting.Instructions.InstructionType.Dialogue.ToString()).Keyword;
			PositionedToken actor = result[0].AsArgument();
			PositionedToken dialogue = result[1].AsArgument();
			PositionedToken keyword = new PositionedToken(
				CompletionTokenType.Keyword,
				dialogueKeyword,
				result[0].StartColumn,
				result[0].StartColumn);

			result.RemoveRange(0, 2);
			result.InsertRange(0, new[] { keyword, actor, dialogue });
		}
		else if (result[1].Type == CompletionTokenType.Keyword)
		{
			PositionedToken actor = result[0].AsArgument();
			PositionedToken keyword = result[1];
			result.RemoveRange(0, 2);
			result.InsertRange(0, new[] { keyword, actor });
		}

		return result;
	}

	private ActorShorthandInfo DetectActorShorthand(List<PositionedToken> tokens)
	{
		if (tokens.Count == 0 || tokens[0].Type != CompletionTokenType.Argument)
			return ActorShorthandInfo.None;

		if (!IsKnownActor(tokens[0].Value))
			return ActorShorthandInfo.None;

		if (tokens.Count == 1)
			return new ActorShorthandInfo(true, tokens[0].Value);

		if (tokens[1].Type == CompletionTokenType.Keyword || tokens[1].Type == CompletionTokenType.Dialogue)
			return new ActorShorthandInfo(true, tokens[0].Value);

		return new ActorShorthandInfo(true, tokens[0].Value);
	}

	private bool ShouldReturnActorAction(
		List<PositionedToken> tokens,
		ActorShorthandInfo actorShorthand,
		int caretColumn)
	{
		if (!actorShorthand.IsShorthand)
			return false;

		if (tokens.Count == 1)
			return caretColumn >= tokens[0].EndColumn;

		return tokens[1].Type == CompletionTokenType.Argument;
	}

	private bool IsComplete(string instructionName, List<CompletionStatement> statements)
	{
		if (string.IsNullOrEmpty(instructionName) || !registry.TryGetByKeyword(instructionName, out InstructionRegistryEntry entry))
			return false;

		foreach (CompletionStatement statement in statements)
		{
			StatementSchema schema = entry.Schema.GetStatement(statement.Type);
			if (schema == null)
				return false;

			int requiredCount = schema.Arguments.Count(argument => argument.Required);
			if (statement.Arguments.Count < requiredCount)
				return false;
		}

		return statements.Count > 0;
	}

	private ScriptValueKind? GetValueKind(string instructionName, string statementName, int argumentIndex)
	{
		if (string.IsNullOrEmpty(instructionName))
			return null;

		if (!registry.TryGetByKeyword(instructionName, out InstructionRegistryEntry entry))
			return null;

		StatementSchema statement = entry.Schema.GetStatement(statementName);
		if (statement == null || argumentIndex < 0 || argumentIndex >= statement.Arguments.Count)
			return null;

		return statement.Arguments[argumentIndex].ValueKind;
	}

	private bool IsBeyondDefinedArguments(string instructionName, string statementName, int argumentIndex)
	{
		if (string.IsNullOrEmpty(instructionName))
			return false;

		if (!registry.TryGetByKeyword(instructionName, out InstructionRegistryEntry entry))
			return false;

		StatementSchema statement = entry.Schema.GetStatement(statementName);
		return statement != null && argumentIndex >= statement.Arguments.Count;
	}

	private bool IsKnownActor(string value)
	{
		ResourceManager manager = resourceManager ?? ResourceManager.Instance;
		if (manager == null)
			return false;

		return manager.GetActorIds().Contains(value);
	}

	private List<PositionedToken> Tokenize(string line)
	{
		_ = preprocessor;
		List<PositionedToken> tokens = new List<PositionedToken>();
		int index = 0;

		while (index < line.Length)
		{
			while (index < line.Length && char.IsWhiteSpace(line[index]))
			{
				index++;
			}

			if (index >= line.Length)
				break;

			if (line[index] == '"')
			{
				int startColumn = index;
				int valueStart = index + 1;
				index = valueStart;

				while (index < line.Length && line[index] != '"')
				{
					index++;
				}

				string value = line.Substring(valueStart, index - valueStart);
				if (index < line.Length && line[index] == '"')
					index++;

				tokens.Add(new PositionedToken(CompletionTokenType.Dialogue, value, startColumn, index));
			}
			else
			{
				int startColumn = index;
				while (index < line.Length && !char.IsWhiteSpace(line[index]))
				{
					index++;
				}

				string value = line.Substring(startColumn, index - startColumn);
				CompletionTokenType type = registry.GetSupportedKeywords().Contains(value)
					? CompletionTokenType.Keyword
					: CompletionTokenType.Argument;

				tokens.Add(new PositionedToken(type, value, startColumn, index));
			}
		}

		return tokens;
	}

	private static PositionedToken FindTokenAtOrBefore(List<PositionedToken> tokens, int caretColumn)
	{
		return tokens.LastOrDefault(token => caretColumn >= token.StartColumn);
	}

	private static string GetPrefix(PositionedToken token, int caretColumn)
	{
		if (token == null)
			return "";

		int length = Math.Clamp(caretColumn - token.StartColumn, 0, token.Value.Length);
		return token.Value.Substring(0, length);
	}

	private static string GetPrefix(CompletionArgument argument, int caretColumn)
	{
		int length = Math.Clamp(caretColumn - argument.StartColumn, 0, argument.Value.Length);
		return argument.Value.Substring(0, length);
	}

	private enum CompletionTokenType
	{
		Keyword,
		Dialogue,
		Argument
	}

	private sealed class PositionedToken
	{
		public CompletionTokenType Type { get; }
		public string Value { get; }
		public int StartColumn { get; }
		public int EndColumn { get; }

		public PositionedToken(CompletionTokenType type, string value, int startColumn, int endColumn)
		{
			Type = type;
			Value = value;
			StartColumn = startColumn;
			EndColumn = endColumn;
		}

		public PositionedToken AsArgument()
		{
			return new PositionedToken(CompletionTokenType.Argument, Value, StartColumn, EndColumn);
		}
	}

	private readonly struct ActorShorthandInfo
	{
		public static ActorShorthandInfo None => new ActorShorthandInfo(false, null);

		public bool IsShorthand { get; }
		public string ActorId { get; }

		public ActorShorthandInfo(bool isShorthand, string actorId)
		{
			IsShorthand = isShorthand;
			ActorId = actorId;
		}
	}
}
