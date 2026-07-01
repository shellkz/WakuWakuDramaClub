using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WakuWakuDramaClub.Completion;
using WakuWakuDramaClub.Scripting.Schema;
using WakuWakuDramaClub.Timline;

public partial class EditingMenu : Control
{
	[Export]
	public TimelinePreview TimelinePreview;

	[Export]
	public ScriptEditor ScriptEditor { get; set; }

	[Export]
	public Button PreviewButton { get; set; }

	[Export]
	public Button PlayButton { get; set; }

	[Export]
	public Button PauseButton { get; set; }
	
	private EditingMenuServices services;
	private bool signalsConnected;




	public override void _Ready()
	{
	}

	public void Initialize(EditingMenuServices services)
	{
		DisconnectSignals();

		this.services = services;
		TimelinePreview.Initialize(services.Stage);

		ScriptEditor.CodeCompletionEnabled = true;
		ConnectSignals();
	}

	private void ConnectSignals()
	{
		if (signalsConnected) return;

		PreviewButton.Pressed += OnPreviewButtonPressed;
		PlayButton.Pressed += OnPlayButtonPressed;
		PauseButton.Pressed += OnPauseButtonPressed;
		ScriptEditor.TextChanged += OnScriptChanged;
		ScriptEditor.CodeCompletionRequested += OnCodeCompletionRequested;
		ScriptEditor.CodeCompletionOptionConfirmed += OnCodeCompletionOptionConfirmed;

		signalsConnected = true;
	}

	private void DisconnectSignals()
	{
		if (!signalsConnected) return;

		PreviewButton.Pressed -= OnPreviewButtonPressed;
		PlayButton.Pressed -= OnPlayButtonPressed;
		PauseButton.Pressed -= OnPauseButtonPressed;
		ScriptEditor.TextChanged -= OnScriptChanged;
		ScriptEditor.CodeCompletionRequested -= OnCodeCompletionRequested;
		ScriptEditor.CodeCompletionOptionConfirmed -= OnCodeCompletionOptionConfirmed;

		signalsConnected = false;
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

	public string GetScriptText()
	{
		return ScriptEditor.Text;
	}

	private async void OnPreviewButtonPressed()
	{
		await RefreshPreviewTimeline();
	}

	private async Task RefreshPreviewTimeline()
	{
		if (services?.BuildTimeline == null) return;

		Timeline timeline = await services.BuildTimeline(GetScriptText());
		TimelinePreview.Play(timeline);
	}

	public void OnPlayButtonPressed()
	{
		TimelinePreview.Play();
	}
	public void OnPauseButtonPressed()
	{
		TimelinePreview.Pause();
	}
}
