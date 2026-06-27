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
		
		ScriptEditor.CodeCompletionEnabled = true;
	
		ScriptEditor.TextChanged += OnScriptChanged;
		ScriptEditor.CodeCompletionRequested += OnCodeCompletionRequested;

	}

    private void OnScriptChanged()
    {
		if (ShouldRequestCompletion()){
			ScriptEditor.RequestCodeCompletion(true);
			//CallDeferred(MethodName.RequestCodeCompletionDeferred);
			
    	}
	}
	// private void RequestCodeCompletionDeferred()
	// {
	// 	ScriptEditor.RequestCodeCompletion(true);
	// }
	private bool ShouldRequestCompletion()
	{
		string beforeCursor = GetTextBeforeCursor();
		if (IsCompletedResourceReference(beforeCursor))
			return false;

		return beforeCursor.Length == 0 || !string.IsNullOrWhiteSpace(beforeCursor);
	}

	private void OnCodeCompletionRequested()
	{
		string beforeCursor = GetTextBeforeCursor();
		CompletionContext context = GetCompletionContext(beforeCursor);

		if (context == CompletionContext.FirstToken)
		{
			AddCompletionOptions(new[] { "背景", "音效" });
			AddCompletionOptions(ResourceManager.Instance.GetActorIds());
		}
		else if (context == CompletionContext.Background)
		{
			AddCompletionOptions(ResourceManager.Instance.GetBackgroundIds());
		}
		else if (context == CompletionContext.Audio)
		{
			AddCompletionOptions(ResourceManager.Instance.GetAudioIds());
		}

		ScriptEditor.UpdateCodeCompletionOptions(true);
	}

	private string GetTextBeforeCursor()
	{
		int lineIndex = ScriptEditor.GetCaretLine();
		int column = ScriptEditor.GetCaretColumn();
		string line = ScriptEditor.GetLine(lineIndex);
		return line.Substring(0, Math.Min(column, line.Length));
	}

	private bool IsCompletedResourceReference(string beforeCursor)
	{
		if (ResourceManager.Instance == null)
			return false;

		if (TryGetKeywordRemainder(beforeCursor, "背景", out string backgroundId))
			return ResourceManager.Instance.GetBackgroundIds().Contains(backgroundId);

		if (TryGetKeywordRemainder(beforeCursor, "音效", out string audioId))
			return ResourceManager.Instance.GetAudioIds().Contains(audioId);

		return false;
	}

	private CompletionContext GetCompletionContext(string beforeCursor)
	{
		if (TryGetKeywordRemainder(beforeCursor, "背景", out _))
			return CompletionContext.Background;

		if (TryGetKeywordRemainder(beforeCursor, "音效", out _))
			return CompletionContext.Audio;

		return CompletionContext.FirstToken;
	}

	private static bool TryGetKeywordRemainder(string beforeCursor, string keyword, out string remainder)
	{
		remainder = "";
		string trimmedStart = beforeCursor.TrimStart();

		if (!trimmedStart.StartsWith(keyword, StringComparison.Ordinal))
			return false;

		if (trimmedStart.Length == keyword.Length)
			return false;

		if (!char.IsWhiteSpace(trimmedStart[keyword.Length]))
			return false;

		remainder = trimmedStart.Substring(keyword.Length).TrimStart();
		return true;
	}

	private void AddCompletionOptions(IEnumerable<string> options)
	{
		foreach (string option in options)
		{
			ScriptEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.PlainText, option, option, Colors.White);
		}

	}

	private enum CompletionContext
	{
		FirstToken,
		Background,
		Audio
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
}
