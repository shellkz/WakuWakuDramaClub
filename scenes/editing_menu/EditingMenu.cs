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
	public ScriptEditor ScriptEditor { get; set; }

	private InstructionRegistry instructionRegistry;
	private ScriptPreprocessor scriptPreprocessor;
	private CompletionAnalyzer completionAnalyzer;
	private CompletionProvider completionProvider;



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

		instructionRegistry = InstructionRegistry.CreateDefault();
		scriptPreprocessor = new ScriptPreprocessor(instructionRegistry);
		completionAnalyzer = new CompletionAnalyzer(instructionRegistry, scriptPreprocessor, ResourceManager.Instance);
		completionProvider = new CompletionProvider(instructionRegistry, ResourceManager.Instance);
		
		
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
		if (completionAnalyzer == null || completionProvider == null)
			return Array.Empty<CompletionOption>();

		int lineIndex = ScriptEditor.GetCaretLine();
		int caretColumn = ScriptEditor.GetCaretColumn();
		string line = ScriptEditor.GetLine(lineIndex);
		CompletionLineResult result = completionAnalyzer.Analyze(line, caretColumn);
		return completionProvider.Provide(result);
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
		TimelineViewport.Timeline = timeline;
		await VideoRenderer.ExportAnimationToVideo(timeline.Animation, timeline.Audio);

	}
	private async Task<Timeline> BuildTimeline()
	{
		ScriptParser parser = new ScriptParser(instructionRegistry, scriptPreprocessor);
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
