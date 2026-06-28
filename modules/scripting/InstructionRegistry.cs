using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WakuWakuDramaClub.Scripting.Instructions;
using WakuWakuDramaClub.Scripting.Schema;

namespace WakuWakuDramaClub.Scripting;

public sealed class InstructionRegistry
{
	private readonly Dictionary<string, InstructionRegistryEntry> byId = new Dictionary<string, InstructionRegistryEntry>();
	private readonly Dictionary<string, InstructionRegistryEntry> byKeyword = new Dictionary<string, InstructionRegistryEntry>();

	public IReadOnlyDictionary<string, InstructionRegistryEntry> ById => byId;
	public IReadOnlyDictionary<string, InstructionRegistryEntry> ByKeyword => byKeyword;
	public IReadOnlyCollection<InstructionRegistryEntry> Entries => byId.Values;

	public static InstructionRegistry CreateDefault()
	{
		InstructionRegistry registry = new InstructionRegistry();
		registry.RegisterAllFromAssembly(Assembly.GetExecutingAssembly());
		return registry;
	}

	public void RegisterAllFromAssembly(Assembly assembly)
	{
		IEnumerable<Type> instructionTypes = assembly.GetTypes()
			.Where(type => !type.IsAbstract && typeof(Instruction).IsAssignableFrom(type));

		foreach (Type instructionType in instructionTypes)
		{
			Register(instructionType);
		}
	}

	public void Register(Type instructionType)
	{
		Instruction instruction = (Instruction)Activator.CreateInstance(instructionType, new RawInstruction());
		InstructionRegistryEntry entry = new InstructionRegistryEntry(
			instruction.GetId(),
			instruction.GetKeyword(),
			instructionType,
			instruction.GetSchema());

		byId.Add(entry.Id, entry);
		byKeyword.Add(entry.Keyword, entry);
	}

	public bool TryGetById(string id, out InstructionRegistryEntry entry)
	{
		return byId.TryGetValue(id, out entry);
	}

	public bool TryGetByKeyword(string keyword, out InstructionRegistryEntry entry)
	{
		return byKeyword.TryGetValue(keyword, out entry);
	}

	public InstructionRegistryEntry GetById(string id)
	{
		if (!TryGetById(id, out InstructionRegistryEntry entry))
			throw new InvalidOperationException($"Unknown instruction id: {id}");

		return entry;
	}

	public InstructionRegistryEntry GetByKeyword(string keyword)
	{
		if (!TryGetByKeyword(keyword, out InstructionRegistryEntry entry))
			throw new InvalidOperationException($"Unknown instruction keyword: {keyword}");

		return entry;
	}

	// Instruction keywords
	public IReadOnlyList<string> GetInstructionKeywords()
	{
		return byKeyword.Keys.ToList();
	}
	// Instruction and Options keywords
	public IReadOnlyList<string> GetSupportedKeywords()
	{
		HashSet<string> result = new HashSet<string>();

		foreach (InstructionRegistryEntry entry in Entries)
		{
			result.Add(entry.Keyword);

			foreach (StatementSchema statement in entry.Schema.OptionStatements)
			{
				result.Add(statement.Name);
			}
		}

		return result.ToList();
	}
}
