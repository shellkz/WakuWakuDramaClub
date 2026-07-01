using System;
using System.Threading.Tasks;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Timline;

public sealed class RenderingMenuServices
{
	public VideoRenderer VideoRenderer { get; init; }
	public Func<Task<Timeline>> BuildTimeline { get; init; }
	public string ExportDirectory { get; init; } = "";
}
