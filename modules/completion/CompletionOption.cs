namespace WakuWakuDramaClub.Completion;

public sealed class CompletionOption
{
	public CompletionOptionKind Kind { get; }
	public string DisplayText { get; }
	public string InsertText { get; }

	public CompletionOption(CompletionOptionKind kind, string displayText, string insertText = null)
	{
		Kind = kind;
		DisplayText = displayText ?? "";
		InsertText = insertText ?? displayText ?? "";
	}

	public string ToDebugString()
	{
		return $"{Kind}:{DisplayText}->{InsertText}";
	}
}

public enum CompletionOptionKind
{
	Instruction,
	Statement,
	Resource,
	Literal,
	Value
}
