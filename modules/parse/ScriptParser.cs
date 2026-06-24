using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace WakuWakuDramaClub.Parse;

[GlobalClass]
public partial class ScriptParser : RefCounted
{



    Dictionary<string, Type> Factory = new Dictionary<string, Type>();
    public ScriptParser()
    {
        RegisterAllFactory();
    }

    public void RegisterAllFactory()
    {
        var factories = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && typeof(Instruction).IsAssignableFrom(t));
        
        foreach (Type factory in factories)
        {
            Instruction instruction = (Instruction)Activator.CreateInstance(factory, new RawInstruction());
            Factory.Add(instruction.GetName(), instruction.GetType());
        }
        
    }


    public List<Instruction> Parse(string script)
    {
        string[] lines = script.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<List<Token>> tokenChains = ConvertToTokenChains(lines);
        List<RawInstruction> rawInstructions = ConvertToRawInstructions(tokenChains);
        


        return ConvertToInstructions(rawInstructions);
    }

    private List<Instruction> ConvertToInstructions(List<RawInstruction> rawInstructions)
    {
        List<Instruction> instructions = new List<Instruction>();
        foreach (RawInstruction rawInstruction in rawInstructions)
        {
            Type factory;

            bool factoryFound = Factory.TryGetValue(rawInstruction.Type, out factory);

            if (!factoryFound)
                throw new InvalidOperationException($"Unknown instruction: {rawInstruction.Type}");

            instructions.Add((Instruction)Activator.CreateInstance(factory, rawInstruction));
        }
        return instructions;
    }


    private List<RawInstruction> ConvertToRawInstructions(List<List<Token>> chains)
    {
        List<RawInstruction> result = new List<RawInstruction>();

        foreach (List<Token> chain in chains)
        {
            RawInstruction raw = new RawInstruction();

            string firstStatemntType = "";
            int i = 0;
            foreach (List<Token> group in SeparateIntoGroup(chain))
            {   

                //group[0] => Statemnt.Type
                //group[1-end] => Staemnt.Argument
                string type = group[0].Value;
                List<string> arguments = group.GetRange(1, group.Count-1).Select(t=>t.Value).ToList();
                Statement statement = new Statement(type, arguments);
                raw.Statements.Add(statement.Type, statement);

                if (i == 0)
                {
                    firstStatemntType = type;   
                }
                i++;
            }
            raw.Type = firstStatemntType;
            result.Add(raw);

        }
        return result;
    }
    public List<List<Token>> SeparateIntoGroup(List<Token> chain)
    {
        // Safety check for null or empty input
        if (chain == null || chain.Count == 0)
        {
            return new List<List<Token>>();
        }

        List<List<Token>> tokenGroups = new List<List<Token>>();
        List<int> keywordIndices = new List<int>();

        // 1. Identify indices of all Keywords
        for (int i = 0; i < chain.Count; i++)
        {
            // The first token in the chain must start a statement group.
            // All subsequent statement groups must be explicitly started by a Keyword.
            if (i == 0 || chain[i].Type == TokenType.Keyword)
            {
                 keywordIndices.Add(i);
            }
        }
        
        // Error handling: If the token chain doesn't start with a Keyword 
        // (after desugaring), the input is invalid.
        if (keywordIndices.Count == 0 || keywordIndices[0] != 0)
        {
             throw new InvalidOperationException("Token chain must start with a Keyword type.");
        }


        // 2. Add the chain's length as the termination index for the final group
        keywordIndices.Add(chain.Count);

        // 3. Iterate over the identified indices to perform the slicing
        for (int i = 0; i < keywordIndices.Count - 1; i++)
        {
            int startIndex = keywordIndices[i];
            int endIndex = keywordIndices[i + 1];
            int length = endIndex - startIndex; 

            // GetRange safely extracts the sub-list (the group/statement)
            // Note: This correctly handles cases where length == 1 (Keyword with no Arguments)
            if (length > 0)
            {
                // List<T>.GetRange is efficient for creating sub-lists
                List<Token> group = chain.GetRange(startIndex, length);
                tokenGroups.Add(group);
            }
        }

        return tokenGroups;
    }
    

    private List<List<Token>> ConvertToTokenChains(string[] lines)
    {
        List<List<Token>> chains = new List<List<Token>>();

        // Raw convert
        foreach (string line in lines) {
            chains.Add(Tokenize(line));
        }

        // Normalize token chain into:
        //  keyword, argument, argument, ..., keyword, argument, ...
        foreach (List<Token> chain in chains)
        {
            if (chain[0].Type == TokenType.Argument && chain[1].Type == TokenType.Dialogue)
            {
                Token argumentToken = chain[0];
                Token dialogueToken = chain[1];

                Token dialogueKeywordToken = new Token(TokenType.Keyword, Tr(TokenType.Dialogue.ToString()));
                Token speakerArgumentToken = new Token(TokenType.Argument, argumentToken.Value);
                Token speechArgumentToken = new Token(TokenType.Argument, dialogueToken.Value);

                chain.RemoveAt(0);
                chain.RemoveAt(0);
                chain.InsertRange(0, new Token[]{dialogueKeywordToken, speakerArgumentToken, speechArgumentToken});
            }
            else if (chain[0].Type == TokenType.Argument && chain[1].Type == TokenType.Keyword)
            {
                Token argumentToken = chain[0];
                Token keywordToken = chain[1];
                chain[0] = keywordToken;
                chain[1] = argumentToken;
            }
        }

        return chains;
    }

    private List<Token> Tokenize(string line)
    {
        List<Token> tokens = new List<Token>();

        // Keyword, Dialogue, Argument
        foreach (string token in line.Split(" "))
        {
            if (token.StartsWith("\"") && token.EndsWith("\""))
            {
                tokens.Add(new Token(TokenType.Dialogue, token.TrimPrefix("\"").TrimSuffix("\"")));
            }
            //GetSupportedInstructions() should be GetSupportedStatements()
            else if (GetSupportedKeywords().Contains(token))
            {
                tokens.Add(new Token(TokenType.Keyword, token));
            }
            else
            {
                tokens.Add(new Token(TokenType.Argument, token));
            }
        }

        return tokens;
    }

   

    public List<string> GetSupportedKeywords()
    {
        List<string> result = new List<string>();

        result.AddRange(GetSupportedInstructions());
        result.AddRange(new string[]{"在", "到", "表情", "動作"});
        return result;
    }   

    public List<string> GetSupportedInstructions()
    {
        return Factory.Keys.ToList();
    }

}
