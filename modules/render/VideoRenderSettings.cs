namespace WakuWakuDramaClub.Render;

public sealed class VideoRenderSettings
{
	public string OutputDirectory { get; init; } = "";
	public string OutputFileName { get; init; } = "exported_animation.mp4";
	public string VideoCodec { get; init; } = "libx264";
	public int FrameRate { get; init; } = 30;
}
