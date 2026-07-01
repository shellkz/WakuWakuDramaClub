using System;
using System.Threading.Tasks;
using WakuWakuDramaClub.Completion;
using WakuWakuDramaClub.Timline;

public sealed class EditingMenuServices
{
	public CompletionAnalyzer CompletionAnalyzer { get; init; }
	public CompletionProvider CompletionProvider { get; init; }
	public Stage Stage { get; init; }
	public Func<string, Task<Timeline>> BuildTimeline { get; init; }
}
