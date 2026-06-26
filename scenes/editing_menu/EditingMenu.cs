using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WakuWakuDramaClub.Parse;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Timline;

public partial class EditingMenu : Control
{
	[Export]
	public CodeEdit ScriptEditor { get; set; }



	[Export]
	public Button RenderButton { get; set; }

	[Export]
	public Button PlayButton { get; set; }

	[Export]
	public Button PauseButton { get; set; }
	
	[Export]
	public TimelineBuilder TimelineBuilder { get; set; }

	[Export]
	public TimelineViewport TimelineViewport { get; set; }

	[Export]
	public TimelinePreview TimelinePreview { get; set; }

	[Export]
	public VideoRenderer VideoRenderer { get; set; }




	public override void _Ready()
	{

		RenderButton.Pressed += OnRenderButtonPressed;
		PlayButton.Pressed += OnPlayButtonPressed;
		PauseButton.Pressed += OnPauseButtonPressed;
		ScriptEditor.TextChanged += OnScriptChanged;

	}

    private async void OnScriptChanged()
    {
	
		try
		{
			Timeline tl = await BuildTimeline();
			TimelineViewport.Timeline = tl;
		}
		catch
		{
			GD.PrintErr("Invalid script.");
			return;
		}
    }

    public async void OnRenderButtonPressed() {

		
		Timeline timeline = await BuildTimeline();
		TimelineViewport.Timeline = timeline;
		await VideoRenderer.ExportAnimationToVideo(timeline.Animation, timeline.Audio);

	}
	private async Task<Timeline> BuildTimeline()
	{
		ScriptParser parser = new ScriptParser();
		List<Instruction> instructions = parser.Parse(ScriptEditor.Text);


		//ClearActors
		TimelineViewport.ClearActors();

		return await TimelineBuilder.BuildTimeline(instructions);
		// TimelineViewport.Timeline = timeline;
		// return timeline;

	}
	public void OnPlayButtonPressed()
	{
		TimelineViewport.Play();
	}
	public void OnPauseButtonPressed()
	{
		TimelineViewport.Pause();
	}
	//private static List<VideoRenderer.AudioClip> LoadAudioClipsFromDirectory(List<AudioClipData> clipConfigs)
	//{
	//    List<VideoRenderer.AudioClip> loadedClips = new List<VideoRenderer.AudioClip>();

	//    foreach (var config in clipConfigs)
	//    {
	//        if (!File.Exists(config.FilePath))
	//        {
	//            GD.PrintErr($"Audio file not found: {config.FilePath}");
	//            continue;
	//        }

	//        try
	//        {
	//            byte[] fileData = File.ReadAllBytes(config.FilePath);

	//            // Create a new AudioClip instance for the VideoRenderer
	//            var newClip = new VideoRenderer.AudioClip
	//            {
	//                // The format is hardcoded to "wav" based on your previous code's assumption
	//                // of parsing WAV headers. You might need to make this more dynamic
	//                // if you plan to support other formats.
	//                Format = "wav",
	//                Data = fileData,
	//                StartingTime = config.StartingTime
	//            };

	//            loadedClips.Add(newClip);
	//        }
	//        catch (Exception ex)
	//        {
	//            GD.PrintErr($"Failed to load audio file {config.FilePath}: {ex.Message}");
	//        }
	//    }

	//    return loadedClips;
	//}
	//public class AudioClipData
	//{
	//    public string FilePath { get; set; }
	//    public double StartingTime { get; set; }
	//}

}
