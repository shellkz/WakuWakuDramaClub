using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WakuWakuDramaClub.Completion;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Parsing;
using WakuWakuDramaClub.Scripting.Instructions;
using WakuWakuDramaClub.Scripting.Schema;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Timline;

public partial class EditingMenu : Control
{
	[Export]
	public TimelinePreview TimelinePreview;

	[Export]
	public ScriptEditor ScriptEditor { get; set; }

	[Export]
	public Button RenderButton { get; set; }

	[Export]
	public Button PlayButton { get; set; }

	[Export]
	public Button PauseButton { get; set; }
	
	private MainWorkspaceServices services;




	public override void _Ready()
	{
	}

	public void Initialize(MainWorkspaceServices services)
	{
		this.services = services;
		TimelinePreview.Initialize(services.Stage);

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
		// Dialogue option selected, move caret between the quote pair.
		if (insertText == LanguageSchema.EmptyDialogueText)
		{
			ScriptEditor.SetCaretColumn(Mathf.Max(0, ScriptEditor.GetCaretColumn() - 1));
		}
	}

    private void OnScriptChanged()
    {
		if (ShouldRequestNewCompletion()){
			ScriptEditor.RequestCodeCompletion(true);
			
    	}
	}

	private bool ShouldRequestNewCompletion()
	{
		return GetCurrentCompletionOptions().Count > 0;
	}

	private void OnCodeCompletionRequested()
	{
		IReadOnlyList<CompletionOption> options = GetCurrentCompletionOptions();
		AddCompletionOptions(options);
		ScriptEditor.UpdateCodeCompletionOptions(true);
	}

	private IReadOnlyList<CompletionOption> GetCurrentCompletionOptions()
	{
		if (services?.CompletionAnalyzer == null || services.CompletionProvider == null)
			return Array.Empty<CompletionOption>();

		int lineIndex = ScriptEditor.GetCaretLine();
		int caretColumn = ScriptEditor.GetCaretColumn();
		string line = ScriptEditor.GetLine(lineIndex);
		CompletionLineResult result = services.CompletionAnalyzer.Analyze(line, caretColumn);
		return services.CompletionProvider.Provide(result);
	}

	private void AddCompletionOptions(IEnumerable<CompletionOption> options)
	{
		foreach (CompletionOption option in options)
		{
			ScriptEditor.AddCodeCompletionOption(
				CodeEdit.CodeCompletionKind.PlainText,
				option.DisplayText,
				option.InsertText,
				Colors.White);
		}
	}

    public async void OnRenderButtonPressed() {

		
		Timeline timeline = await BuildTimeline();
		services.Stage.Timeline = timeline;
		await services.VideoRenderer.ExportAnimationToVideo(timeline.Animation, timeline.Audio);
		
	}
	private async Task<Timeline> BuildTimeline()
	{
		ScriptParser parser = new ScriptParser(services.InstructionRegistry, services.ScriptPreprocessor);
		List<Instruction> instructions = parser.Parse(ScriptEditor.Text);



		//ClearActors
		services.Stage.ClearActors();

		return await services.Stage.BuildTimeline(instructions);
		// TimelineViewport.Timeline = timeline;
		// return timeline;

	}



	public void OnPlayButtonPressed()
	{
		services.Stage.Play();
	}
	public void OnPauseButtonPressed()
	{
		services.Stage.Pause();
	}
}
