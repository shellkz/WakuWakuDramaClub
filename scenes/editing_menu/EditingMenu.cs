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
			GD.Print("Request Code complete");
		
			CallDeferred(MethodName.RequestCodeCompletionDeferred);
			
    	}
	}
	private void RequestCodeCompletionDeferred()
	{
		ScriptEditor.RequestCodeCompletion(true);
	}
	private bool ShouldRequestCompletion()
	{
		int column = ScriptEditor.GetCaretColumn();
		if (column == 0)
			return true;

		int lineIndex = ScriptEditor.GetCaretLine();
		string line = ScriptEditor.GetLine(lineIndex);
		string beforeCursor = line.Substring(0, Math.Min(column, line.Length));

		return !string.IsNullOrWhiteSpace(beforeCursor);
	}

	private void OnCodeCompletionRequested()
	{
		GD.Print(ScriptEditor.GetTextForCodeCompletion());
		int lineIndex = ScriptEditor.GetCaretLine();
		int column = ScriptEditor.GetCaretColumn();
		string line = ScriptEditor.GetLine(lineIndex);
		string beforeCursor = line.Substring(0, Math.Min(column, line.Length));
		GD.Print("Provide Code complete");
		ScriptEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.PlainText, "op1", "op1", Colors.White);
		// ScriptEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.PlainText, "op 2", "op 2", Colors.White);
		// GD.Print("Option count before update: " + ScriptEditor.GetCodeCompletionOptions().Count);
		// ScriptEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.PlainText, "op 3", "op 3", Colors.White);
		ScriptEditor.UpdateCodeCompletionOptions(true);
		GD.Print("Option count: " + ScriptEditor.GetCodeCompletionOptions().Count);

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
