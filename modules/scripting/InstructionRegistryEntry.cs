using System;
using WakuWakuDramaClub.Scripting.Schema;

namespace WakuWakuDramaClub.Scripting;

public sealed class InstructionRegistryEntry
{
	public string Id { get; }
	public string Keyword { get; }
	public Type Type { get; }
	public InstructionSchema Schema { get; }

	public InstructionRegistryEntry(string id, string keyword, Type type, InstructionSchema schema)
	{
		Id = id;
		Keyword = keyword;
		Type = type;
		Schema = schema;
	}
}
