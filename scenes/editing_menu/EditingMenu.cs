using Godot;
using System.Collections.Generic;
using System.Diagnostics;
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
	// 	Microsoft Hanhan Desktop
	//  Microsoft Hanhan
	//  Microsoft Yating
	//  Microsoft Zhiwei
		SystemSpeechTTS tts = new SystemSpeechTTS();
	
		// tts.ListVoices();


		

		RenderButton.Pressed += OnRenderButtonPressed;

		//await Task.Delay(1000);

	 
		PlayButton.Pressed += OnPlayButtonPressed;
		PauseButton.Pressed += OnPauseButtonPressed;


	}

	public async void OnRenderButtonPressed() {

		
		ScriptParser parser = new ScriptParser();
		List<Instruction> instructions = parser.Parse(ScriptEditor.Text);

		Timeline timeline = await TimelineBuilder.BuildTimeline(instructions);
		//ResourceSaver.Save(timeline.Animation, "res://test.res");		
		
		// Stopwatch watch = new Stopwatch();
		// watch.Start();
		TimelineViewport.Timeline = timeline;
		await VideoRenderer.ExportAnimationToVideo(timeline.Animation, timeline.Audio);
		// watch.Stop();
		// GD.Print(watch.Elapsed.TotalSeconds);
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
