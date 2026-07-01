using WakuWakuDramaClub.Completion;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Parsing;
using WakuWakuDramaClub.Timline;

public sealed class MainWorkspaceServices
{
	public InstructionRegistry InstructionRegistry { get; init; }
	public ScriptPreprocessor ScriptPreprocessor { get; init; }
	public CompletionAnalyzer CompletionAnalyzer { get; init; }
	public CompletionProvider CompletionProvider { get; init; }
	public Stage Stage { get; init; }
	public VideoRenderer VideoRenderer { get; init; }
}
