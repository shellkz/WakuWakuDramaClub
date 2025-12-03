using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using System.Linq;
using System.Reflection;

namespace WakuWakuDramaClub.Parse;

[GlobalClass]
public partial class ScriptParser : RefCounted
{
    string[] SupportedInstruction = {"對話", "登場", "場景", "移動", "等待"};

    Dictionary<string, InstructionFactory> Factory = new Dictionary<string, InstructionFactory>();

    public ScriptParser()
    {
        RegisterAllFactory();
    }

    public void RegisterAllFactory()
    {
        // Find all InstructionFactory derived solid class
        var factories = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && typeof(InstructionFactory).IsAssignableFrom(t));

        foreach (var factoryType in factories)
        {
            InstructionFactory factory = (InstructionFactory)Activator.CreateInstance(factoryType);
            RegisterFactory(factory);
        }
    }
    public void RegisterFactory(InstructionFactory factory)
    {
       
        //for name in TranslateAll(factory.GetName()) 
        //  Factory.Add(name, factory);
        Factory.Add(factory.GetName(), factory);
    }

    public List<Instruction> Parse(string script)
    {
        string[] lines = script.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<List<Token>> token_chains = new List<List<Token>>();
        foreach (string line in lines) {
            token_chains.Add(Tokenize(line));
        }

        List<RawInstruction> raw_instructions = new List<RawInstruction>();
        foreach (List<Token> token in token_chains) {
            if (token.Count == 0){
                continue;
            }

            RawInstruction intruction = new RawInstruction();

            // find options tokens, inject into instruction options dictionary, and remove 
            List<Token> options= token.Where(t => t.Type == TokenType.Option).ToList();
            
            foreach (Token option in options)
            {
                intruction.Options.Add(option.Value.Split("=")[0],option.Value.Split("=")[1]);

               
            }
            
            token.RemoveAll(t => t.Type == TokenType.Option);


            // interprete token in token chain as intruction

            if (SupportedInstruction.Contains(token[0].Value))
            {
                intruction.Type = token[0].Value;
                token.RemoveAt(0);
                intruction.Arguments = token.Select(t => t.Value).ToArray();

            }
            else if (SupportedInstruction.Contains(token[1].Value))
            {
                intruction.Type = token[1].Value;
                token.RemoveAt(1);
                intruction.Arguments = token.Select(t => t.Value).ToArray();
            }
            else if (token.Any(t => t.Type == TokenType.Dialogue))
            {
                intruction.Type = "對話";
                intruction.Arguments = token.Select(t => t.Value).ToArray();

            }
            else {
                GD.Print($"[Parse] Instruction not supported. ({token})");
                continue;
            }
            raw_instructions.Add(intruction);
        }

        List<Instruction> instructions = new List<Instruction>();
        foreach (RawInstruction rawInstruction in raw_instructions)
        {
            InstructionFactory factory;

            bool factoryFound = Factory.TryGetValue(rawInstruction.Type, out factory);

            if (factoryFound)
            {
                Instruction instruction = factory.Create(rawInstruction);
                instructions.Add(instruction);
            }
        }

        return instructions;
    }
    public List<Token> Tokenize(string line)
    {
        
        List<Token> tokens = new List<Token>();

        // Seperate into 3 group
        // Group 1: Dialogue    「dialogue」      - any string pattern start with 「 end with 」, seperator is kept
        // Group 2: Option      （option）        - any string pattern start with （ end with ）, seperator is kept
        // Group 3: Normal      normal           - any other string pattern
        
        string pattern = @"(「[^」]*」)|(（[^）]*）)|([^「」（）]+)";

        MatchCollection matches = Regex.Matches(line, pattern);

        foreach (Match match in matches)
        {
            if (match.Groups[1].Success) // Dialogue group
            {
                //「content」-> content
                string dialogueContent = match.Groups[1].Value.Substring(1, match.Groups[1].Value.Length - 2).Trim();
                tokens.Add(new Token(TokenType.Dialogue, dialogueContent));
            }
            else if (match.Groups[2].Success) // Option group
            {
                //（option）-> option
                string optionContent = match.Groups[2].Value.Substring(1, match.Groups[2].Value.Length - 2).Trim();
                tokens.Add(new Token(TokenType.Option, optionContent));
            }
            else if (match.Groups[3].Success) // Normal group
            {
                string normalText = match.Groups[3].Value;
                // Futher split with space character as derived normal tokens
                string[] words = normalText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    tokens.Add(new Token(TokenType.Normal, word.Trim()));
                }
            }
        }

        return tokens;
    }

}
