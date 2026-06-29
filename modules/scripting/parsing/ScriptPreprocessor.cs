using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using WakuWakuDramaClub.Scripting.Instructions;
using WakuWakuDramaClub.Scripting.Schema;

namespace WakuWakuDramaClub.Scripting.Parsing;

public sealed class ScriptPreprocessor
{
	private readonly InstructionRegistry registry;

	public ScriptPreprocessor(InstructionRegistry registry)
	{
		this.registry = registry;
	}

	public List<List<Token>> PreprocessLine(string line)
	{
		List<Token> tokens = Tokenize(line);
		List<Token> normalizedTokens = NormalizeActorShorthand(tokens);
		return GroupByKeyword(normalizedTokens);
	}

	public List<Token> Tokenize(string line)
	{
		List<Token> tokens = new List<Token>();

		foreach (ScriptTokenText token in SplitLine(line))
		{
			if (token.IsDialogue)
			{
				tokens.Add(new Token(TokenType.Dialogue, token.Value));
			}
			else if (IsKeyword(token.Value))
			{
				tokens.Add(new Token(TokenType.Keyword, token.Value));
			}
			else
			{
				tokens.Add(new Token(TokenType.Argument, token.Value));
			}
		}

		return tokens;
	}

	private static List<ScriptTokenText> SplitLine(string line)
	{
		List<ScriptTokenText> result = new List<ScriptTokenText>();
		int index = 0;

		while (index < line.Length)
		{
			while (index < line.Length && char.IsWhiteSpace(line[index]))
			{
				index++;
			}

			if (index >= line.Length)
				break;

			if (line[index] == LanguageSchema.DialogueQuoteStart)
			{
				int start = index + 1;
				index = start;

				while (index < line.Length && line[index] != LanguageSchema.DialogueQuoteEnd)
				{
					index++;
				}

				result.Add(new ScriptTokenText(line.Substring(start, index - start), true));

				if (index < line.Length && line[index] == LanguageSchema.DialogueQuoteEnd)
				{
					index++;
				}
			}
			else
			{
				int start = index;

				while (index < line.Length && !char.IsWhiteSpace(line[index]))
				{
					index++;
				}

				result.Add(new ScriptTokenText(line.Substring(start, index - start), false));
			}
		}

		return result;
	}

	public bool IsKeyword(string token)
	{
		return registry.GetSupportedKeywords().Contains(token);
	}

	public List<Token> NormalizeActorShorthand(List<Token> tokens)
	{
		List<Token> result = tokens.ToList();

		if (result.Count < 2)
			return result;

		if (result[0].Type == TokenType.Argument && result[1].Type == TokenType.Dialogue)
		{
			Token argumentToken = result[0];
			Token dialogueToken = result[1];

			Token dialogueKeywordToken = new Token(TokenType.Keyword, registry.GetById(InstructionType.Dialogue.ToString()).Keyword);
			Token speakerArgumentToken = new Token(TokenType.Argument, argumentToken.Value);
			Token speechArgumentToken = new Token(TokenType.Argument, dialogueToken.Value);

			result.RemoveAt(0);
			result.RemoveAt(0);
			result.InsertRange(0, new Token[] { dialogueKeywordToken, speakerArgumentToken, speechArgumentToken });
		}
		else if (result[0].Type == TokenType.Argument && result[1].Type == TokenType.Keyword)
		{
			Token argumentToken = result[0];
			Token keywordToken = result[1];
			result[0] = keywordToken;
			result[1] = argumentToken;
		}

		return result;
	}

	public List<List<Token>> GroupByKeyword(List<Token> tokens)
	{
		if (tokens == null || tokens.Count == 0)
		{
			return new List<List<Token>>();
		}

		List<List<Token>> tokenGroups = new List<List<Token>>();
		List<int> keywordIndices = new List<int>();

		for (int i = 0; i < tokens.Count; i++)
		{
			if (i == 0 || tokens[i].Type == TokenType.Keyword)
			{
				keywordIndices.Add(i);
			}
		}

		if (keywordIndices.Count == 0 || keywordIndices[0] != 0)
		{
			throw new InvalidOperationException("Token chain must start with a Keyword type.");
		}

		keywordIndices.Add(tokens.Count);

		for (int i = 0; i < keywordIndices.Count - 1; i++)
		{
			int startIndex = keywordIndices[i];
			int endIndex = keywordIndices[i + 1];
			int length = endIndex - startIndex;

			if (length > 0)
			{
				List<Token> group = tokens.GetRange(startIndex, length);
				tokenGroups.Add(group);
			}
		}

		return tokenGroups;
	}

	private readonly struct ScriptTokenText
	{
		public string Value { get; }
		public bool IsDialogue { get; }

		public ScriptTokenText(string value, bool isDialogue)
		{
			Value = value;
			IsDialogue = isDialogue;
		}
	}
}
