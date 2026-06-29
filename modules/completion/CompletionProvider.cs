using System;
using System.Collections.Generic;
using System.Linq;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Schema;

namespace WakuWakuDramaClub.Completion;

public sealed class CompletionProvider
{
	private static readonly string[] AnchorOptions = { "左", "中", "右" };

	private readonly InstructionRegistry registry;
	private readonly ResourceManager resourceManager;

	public CompletionProvider(InstructionRegistry registry, ResourceManager resourceManager = null)
	{
		this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
		this.resourceManager = resourceManager;
	}

	public IReadOnlyList<CompletionOption> Provide(CompletionLineResult line)
	{
		if (line == null)
			return Array.Empty<CompletionOption>();

		CompletionCursorContext cursor = line.Cursor;
		IEnumerable<CompletionOption> options = cursor.Kind switch
		{
			CompletionCursorKind.PrimaryStart => ProvidePrimaryStart(),
			CompletionCursorKind.ActorAction => ProvideActorActions(),
			CompletionCursorKind.InstructionArgument => ProvideValueOptions(cursor.ValueKind, ResolveActorId(line.Raw)),
			CompletionCursorKind.StatementName => ProvideStatementNames(cursor.InstructionName, line.Raw),
			CompletionCursorKind.StatementArgument => ProvideValueOptions(cursor.ValueKind, ResolveActorId(line.Raw)),
			_ => Enumerable.Empty<CompletionOption>()
		};

		return options.ToList();
	}

	private IEnumerable<CompletionOption> ProvidePrimaryStart()
	{
		foreach (string keyword in registry.GetInstructionKeywords().OrderBy(keyword => keyword, StringComparer.Ordinal))
			yield return new CompletionOption(CompletionOptionKind.Instruction, keyword);

		foreach (string actorId in GetResourceManager().GetActorIds())
			yield return new CompletionOption(CompletionOptionKind.Resource, actorId);
	}

	private IEnumerable<CompletionOption> ProvideActorActions()
	{
		yield return new CompletionOption(CompletionOptionKind.Literal, LanguageSchema.EmptyDialogueText);

		foreach (InstructionRegistryEntry entry in registry.Entries.OrderBy(entry => entry.Keyword, StringComparer.Ordinal))
		{
			StatementSchema primaryStatement = entry.Schema.PrimaryStatement;
			if (primaryStatement == null || primaryStatement.Arguments.Count == 0)
				continue;

			if (primaryStatement.Arguments[0].ValueKind == ScriptValueKind.Actor)
				yield return new CompletionOption(CompletionOptionKind.Instruction, entry.Keyword);
		}
	}

	private IEnumerable<CompletionOption> ProvideStatementNames(string instructionName, CompletionRawInstruction raw)
	{
		if (string.IsNullOrEmpty(instructionName))
			return Enumerable.Empty<CompletionOption>();

		if (!registry.TryGetByKeyword(instructionName, out InstructionRegistryEntry entry))
			return Enumerable.Empty<CompletionOption>();

		HashSet<string> usedStatements = raw.Statements
			.Skip(1)
			.Select(statement => statement.Type)
			.ToHashSet(StringComparer.Ordinal);

		return entry.Schema.OptionStatements
			.Where(statement => statement.AllowMultiple || !usedStatements.Contains(statement.Name))
			.Select(statement => new CompletionOption(CompletionOptionKind.Statement, statement.Name));
	}

	private IEnumerable<CompletionOption> ProvideValueOptions(ScriptValueKind? valueKind, string actorId)
	{
		if (!valueKind.HasValue)
			return Enumerable.Empty<CompletionOption>();

		ResourceManager manager = GetResourceManager();
		return valueKind.Value switch
		{
			ScriptValueKind.Actor => manager.GetActorIds().Select(ToResourceOption),
			ScriptValueKind.Background => manager.GetBackgroundIds().Select(ToResourceOption),
			ScriptValueKind.Audio => manager.GetAudioIds().Select(ToResourceOption),
			ScriptValueKind.ActorExpression => GetActorExpressions(manager, actorId).Select(ToResourceOption),
			ScriptValueKind.ActorPose => GetActorBodies(manager, actorId).Select(ToResourceOption),
			ScriptValueKind.Anchor => AnchorOptions.Select(value => new CompletionOption(CompletionOptionKind.Value, value)),
			_ => Enumerable.Empty<CompletionOption>()
		};
	}

	private static string ResolveActorId(CompletionRawInstruction raw)
	{
		if (!string.IsNullOrEmpty(raw.ActorId))
			return raw.ActorId;

		CompletionStatement primaryStatement = raw.Statements.FirstOrDefault();
		if (primaryStatement == null || primaryStatement.Arguments.Count == 0)
			return null;

		return primaryStatement.Arguments[0].Value;
	}

	private static CompletionOption ToResourceOption(string value)
	{
		return new CompletionOption(CompletionOptionKind.Resource, value);
	}

	private static IEnumerable<string> GetActorExpressions(ResourceManager manager, string actorId)
	{
		if (string.IsNullOrEmpty(actorId))
			return Enumerable.Empty<string>();

		try
		{
			return manager.GetActorDefinition(actorId).Expressions.Keys.OrderBy(value => value, StringComparer.Ordinal).ToList();
		}
		catch (ArgumentException)
		{
			return Enumerable.Empty<string>();
		}
	}

	private static IEnumerable<string> GetActorBodies(ResourceManager manager, string actorId)
	{
		if (string.IsNullOrEmpty(actorId))
			return Enumerable.Empty<string>();

		try
		{
			return manager.GetActorDefinition(actorId).Bodies.Keys.OrderBy(value => value, StringComparer.Ordinal).ToList();
		}
		catch (ArgumentException)
		{
			return Enumerable.Empty<string>();
		}
	}

	private ResourceManager GetResourceManager()
	{
		return resourceManager ?? ResourceManager.Instance;
	}


}
