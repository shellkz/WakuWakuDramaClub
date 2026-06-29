namespace WakuWakuDramaClub.Scripting.Schema;

public static class LanguageSchema
{
	public const char DialogueQuoteStart = '「';
	public const char DialogueQuoteEnd = '」';

	public static string EmptyDialogueText => $"{DialogueQuoteStart}{DialogueQuoteEnd}";
}
