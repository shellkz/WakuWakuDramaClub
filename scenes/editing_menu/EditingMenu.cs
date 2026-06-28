using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WakuWakuDramaClub.Scripting.Parsing;
using WakuWakuDramaClub.Scripting.Instructions;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Timline;

public partial class EditingMenu : Control
{
	[Export]
	public ScriptEditor ScriptEditor { get; set; }



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
		ScriptEditor.CodeCompletionOptionConfirmed += OnCodeCompletionOptionConfirmed;

	}

	private void OnCodeCompletionOptionConfirmed(string insertText, string displayText)
	{
		// Dialouge option selected, insert "", and move caret forward by 1 character
		if (insertText == "\"\"")
		{
			ScriptEditor.SetCaretColumn(Mathf.Max(0, ScriptEditor.GetCaretColumn() - 1));
		}
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
		else if (context == CompletionContext.Actor)
		{
			AddCompletionOptions(new[] { "\"\"", "登場", "移動" });
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

		if (TryGetActorInstructionRemainder(beforeCursor, out _))
			return CompletionContext.Actor;

		return CompletionContext.FirstToken;
	}

	private static bool TryGetActorInstructionRemainder(string beforeCursor, out string remainder)
	{
		remainder = "";

		if (ResourceManager.Instance == null)
			return false;

		string trimmedStart = beforeCursor.TrimStart();
		foreach (string actorId in ResourceManager.Instance.GetActorIds().OrderByDescending(id => id.Length))
		{
			if (!trimmedStart.StartsWith(actorId, StringComparison.Ordinal))
				continue;

			if (trimmedStart.Length == actorId.Length)
				continue;

			if (!char.IsWhiteSpace(trimmedStart[actorId.Length]))
				continue;

			remainder = trimmedStart.Substring(actorId.Length).TrimStart();
			return true;
		}

		return false;
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
		Audio,
		Actor
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
