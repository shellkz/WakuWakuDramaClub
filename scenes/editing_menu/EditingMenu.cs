using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

	private InstructionRegistry completionRegistry;
	private ScriptPreprocessor completionPreprocessor;
	private CompletionAnalyzer completionAnalyzer;



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

		completionRegistry = InstructionRegistry.CreateDefault();
		completionPreprocessor = new ScriptPreprocessor(completionRegistry);
		completionAnalyzer = new CompletionAnalyzer(completionRegistry, completionPreprocessor, ResourceManager.Instance);

		if (ResourceManager.Instance != null)
			ResourceManager.Instance.WhenResourcesLoaded(RunCompletionAnalyzerTests);
		else
			GD.Print("[CompletionAnalyzerTest] ResourceManager.Instance is not available.");
		
		
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
		PrintCompletionAnalysis();

		if (ShouldRequestCompletion()){
			ScriptEditor.RequestCodeCompletion(true);
			//CallDeferred(MethodName.RequestCodeCompletionDeferred);
			
    	}
	}

	private void PrintCompletionAnalysis()
	{
		if (completionAnalyzer == null)
			return;

		int lineIndex = ScriptEditor.GetCaretLine();
		int caretColumn = ScriptEditor.GetCaretColumn();
		string line = ScriptEditor.GetLine(lineIndex);
		CompletionLineResult result = completionAnalyzer.Analyze(line, caretColumn);
		GD.Print(result.ToDebugString());
	}

	private void RunCompletionAnalyzerTests()
	{
		if (completionAnalyzer == null)
			return;

		CompletionAnalyzerTestCase[] cases = new[]
		{
			new CompletionAnalyzerTestCase("|", null, CompletionCursorKind.PrimaryStart, ""),
			new CompletionAnalyzerTestCase("背|", null, CompletionCursorKind.PrimaryStart, "背"),
			new CompletionAnalyzerTestCase("def|", null, CompletionCursorKind.PrimaryStart, "def"),
			new CompletionAnalyzerTestCase("背景 |", "背景", CompletionCursorKind.InstructionArgument, "", "背景", "背景", 0, ScriptValueKind.Background),
			new CompletionAnalyzerTestCase("背景 文藝|", "背景", CompletionCursorKind.InstructionArgument, "文藝", "背景", "背景", 0, ScriptValueKind.Background),
			new CompletionAnalyzerTestCase("音效 |", "音效", CompletionCursorKind.InstructionArgument, "", "音效", "音效", 0, ScriptValueKind.Audio),
			new CompletionAnalyzerTestCase("default/毛豆 |", null, CompletionCursorKind.ActorAction, "", null, null, null, null, true, "default/毛豆"),
			new CompletionAnalyzerTestCase("default/毛豆 移|", null, CompletionCursorKind.ActorAction, "移", null, null, null, null, true, "default/毛豆"),
			new CompletionAnalyzerTestCase("default/毛豆 移動 |", "移動", CompletionCursorKind.StatementName, "", "移動", null, null, null, true, "default/毛豆"),
			new CompletionAnalyzerTestCase("default/毛豆 移動 到 |", "移動", CompletionCursorKind.StatementArgument, "", "移動", "到", 0, ScriptValueKind.Anchor, true, "default/毛豆"),
			new CompletionAnalyzerTestCase("default/毛豆 移動 到 右|", "移動", CompletionCursorKind.StatementArgument, "右", "移動", "到", 0, ScriptValueKind.Anchor, true, "default/毛豆"),
			new CompletionAnalyzerTestCase("default/毛豆 移動 在 |", "移動", CompletionCursorKind.StatementArgument, "", "移動", "在", 0, ScriptValueKind.Float, true, "default/毛豆"),
			new CompletionAnalyzerTestCase("default/毛豆 \"你好\"|", "對話", CompletionCursorKind.DialogueText, "你好", "對話", "對話", 1, ScriptValueKind.DialogueText, true, "default/毛豆"),
			new CompletionAnalyzerTestCase("default/毛豆 \"你好\" |", "對話", CompletionCursorKind.StatementName, "", "對話", null, null, null, true, "default/毛豆"),
			new CompletionAnalyzerTestCase("default/毛豆 \"你好\" 表|", "對話", CompletionCursorKind.StatementName, "表", "對話", null, null, null, true, "default/毛豆"),
			new CompletionAnalyzerTestCase("default/毛豆 \"你好\" 表情 |", "對話", CompletionCursorKind.StatementArgument, "", "對話", "表情", 0, ScriptValueKind.ActorExpression, true, "default/毛豆"),
			new CompletionAnalyzerTestCase("default/毛豆 \"你好\" 動作 |", "對話", CompletionCursorKind.StatementArgument, "", "對話", "動作", 0, ScriptValueKind.ActorPose, true, "default/毛豆"),
			new CompletionAnalyzerTestCase("對話 default/毛豆 |", "對話", CompletionCursorKind.DialogueText, "", "對話", "對話", 1, ScriptValueKind.DialogueText),
			new CompletionAnalyzerTestCase("對話 default/毛豆 你好|", "對話", CompletionCursorKind.DialogueText, "你好", "對話", "對話", 1, ScriptValueKind.DialogueText),
			new CompletionAnalyzerTestCase("移動 default/毛豆 |", "移動", CompletionCursorKind.StatementName, "", "移動"),
			new CompletionAnalyzerTestCase("移動 default/毛豆 到 右 |", "移動", CompletionCursorKind.StatementName, "", "移動"),
		};

		int passCount = 0;
		foreach (CompletionAnalyzerTestCase testCase in cases)
		{
			CompletionLineResult result = completionAnalyzer.AnalyzeTest(testCase.Line, testCase.CaretColumn);
			bool passed = testCase.Matches(result);
			if (passed)
				passCount++;

			GD.Print($"[CompletionAnalyzerTest] {(passed ? "PASS" : "FAIL")} {testCase.Name}");
			if (!passed)
			{
				GD.Print($"  Expected: {testCase.ToExpectedDebugString()}");
				GD.Print($"  Actual:   {result.ToDebugString()}");
			}
		}

		GD.Print($"[CompletionAnalyzerTest] {passCount}/{cases.Length} passed");
	}

	private sealed class CompletionAnalyzerTestCase
	{
		public string Name { get; }
		public string Line { get; }
		public int CaretColumn { get; }
		private readonly string rawType;
		private readonly CompletionCursorKind kind;
		private readonly string prefix;
		private readonly string instructionName;
		private readonly string statementName;
		private readonly int? argumentIndex;
		private readonly ScriptValueKind? valueKind;
		private readonly bool? isActorShorthand;
		private readonly string actorId;

		public CompletionAnalyzerTestCase(
			string markedLine,
			string rawType,
			CompletionCursorKind kind,
			string prefix,
			string instructionName = null,
			string statementName = null,
			int? argumentIndex = null,
			ScriptValueKind? valueKind = null,
			bool? isActorShorthand = null,
			string actorId = null)
		{
			Name = markedLine;
			CaretColumn = markedLine.IndexOf('|');
			Line = markedLine.Replace("|", "");
			this.rawType = rawType;
			this.kind = kind;
			this.prefix = prefix;
			this.instructionName = instructionName;
			this.statementName = statementName;
			this.argumentIndex = argumentIndex;
			this.valueKind = valueKind;
			this.isActorShorthand = isActorShorthand;
			this.actorId = actorId;
		}

		public bool Matches(CompletionLineResult result)
		{
			if (result.Raw.Type != rawType)
				return false;

			if (result.Cursor.Kind != kind)
				return false;

			if (result.Cursor.Prefix != prefix)
				return false;

			if (instructionName != null && result.Cursor.InstructionName != instructionName)
				return false;

			if (statementName != null && result.Cursor.StatementName != statementName)
				return false;

			if (argumentIndex.HasValue && result.Cursor.ArgumentIndex != argumentIndex)
				return false;

			if (valueKind.HasValue && result.Cursor.ValueKind != valueKind)
				return false;

			if (isActorShorthand.HasValue && result.Raw.IsActorShorthand != isActorShorthand)
				return false;

			if (actorId != null && result.Raw.ActorId != actorId)
				return false;

			return true;
		}

		public string ToExpectedDebugString()
		{
			return $"Raw.Type={Format(rawType)}, Kind={kind}, Prefix=\"{prefix}\", InstructionName={Format(instructionName)}, StatementName={Format(statementName)}, ArgumentIndex={Format(argumentIndex)}, ValueKind={Format(valueKind)}, IsActorShorthand={Format(isActorShorthand)}, ActorId={Format(actorId)}";
		}

		private static string Format(string value)
		{
			return value == null ? "null" : $"\"{value}\"";
		}

		private static string Format(int? value)
		{
			return value.HasValue ? value.Value.ToString() : "null";
		}

		private static string Format(bool? value)
		{
			return value.HasValue ? value.Value.ToString() : "null";
		}

		private static string Format(ScriptValueKind? value)
		{
			return value.HasValue ? value.Value.ToString() : "null";
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
		InstructionRegistry registry = InstructionRegistry.CreateDefault();
		ScriptPreprocessor preprocessor = new ScriptPreprocessor(registry);

		ScriptParser parser = new ScriptParser(registry, preprocessor);
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
