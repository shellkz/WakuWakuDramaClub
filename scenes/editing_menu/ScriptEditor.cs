using Godot;
using System;

public partial class ScriptEditor : CodeEdit
{
	[Signal]
	public delegate void CodeCompletionOptionConfirmedEventHandler(string insertText, string displayText);

	public override void _ConfirmCodeCompletion(bool replace)
	{
		string insertText = "";
		string displayText = "";
		string queryText = "";

		// Get selected option
		int selectedIndex = GetCodeCompletionSelectedIndex();
		if (selectedIndex >= 0)
		{
			var option = GetCodeCompletionOption(selectedIndex);
			insertText = option.TryGetValue("insert_text", out Variant insertTextValue) ? insertTextValue.AsString() : "";
			displayText = option.TryGetValue("display", out Variant displayValue) ? displayValue.AsString() : "";

		}

		// Replace old query text with selected option
		int caretLine = GetCaretLine();
		int caretColumn = GetCaretColumn();
		queryText = GetCurrentCompletionToken(caretLine, caretColumn);
		int queryStartColumn = caretColumn - queryText.Length;


		string lineText = GetLine(caretLine);
		string newLineText = lineText.Substring(0, queryStartColumn)
			+ insertText
			+ lineText.Substring(Mathf.Min(caretColumn, lineText.Length));
		SetLine(caretLine, newLineText);
		SetCaretColumn(queryStartColumn + insertText.Length);


		// Notify outside
		EmitSignal(SignalName.CodeCompletionOptionConfirmed, insertText, displayText);
	}

	private string GetCurrentCompletionToken(int line, int column)
	{
		string lineText = GetLine(line);
		int end = Mathf.Min(column, lineText.Length);
		int start = end;

		while (start > 0 && !char.IsWhiteSpace(lineText[start - 1]))
		{
			start--;
		}

		return lineText.Substring(start, end - start);
	}
}
