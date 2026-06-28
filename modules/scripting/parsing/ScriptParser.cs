using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Instructions;

namespace WakuWakuDramaClub.Scripting.Parsing;

[GlobalClass]
public partial class ScriptParser : RefCounted
{
    private readonly InstructionRegistry registry;
    private readonly ScriptPreprocessor preprocessor;

    public ScriptParser()
        : this(InstructionRegistry.CreateDefault())
    {
    }

    public ScriptParser(InstructionRegistry registry)
        : this(registry, new ScriptPreprocessor(registry))
    {
    }

    public ScriptParser(InstructionRegistry registry, ScriptPreprocessor preprocessor)
    {
        this.registry = registry;
        this.preprocessor = preprocessor ?? new ScriptPreprocessor(registry);
    }


    public List<Instruction> Parse(string script)
    {
        string[] lines = script.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<List<List<Token>>> statementGroups = ConvertToStatementGroups(lines);
        List<RawInstruction> rawInstructions = ConvertToRawInstructions(statementGroups);
        


        return ConvertToInstructions(rawInstructions);
    }

    private List<Instruction> ConvertToInstructions(List<RawInstruction> rawInstructions)
    {
        List<Instruction> instructions = new List<Instruction>();
        foreach (RawInstruction rawInstruction in rawInstructions)
        {
            instructions.Add(CreateInstruction(rawInstruction));
        }
        return instructions;
    }

    private Instruction CreateInstruction(RawInstruction rawInstruction)
    {
        InstructionRegistryEntry entry = registry.GetByKeyword(rawInstruction.Type);
        return (Instruction)Activator.CreateInstance(entry.Type, rawInstruction);
    }


    private List<RawInstruction> ConvertToRawInstructions(List<List<List<Token>>> statementGroups)
    {
        List<RawInstruction> result = new List<RawInstruction>();

        foreach (List<List<Token>> groups in statementGroups)
        {
            RawInstruction raw = new RawInstruction();

            string firstStatemntType = "";
            int i = 0;
            foreach (List<Token> group in groups)
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

    private List<List<List<Token>>> ConvertToStatementGroups(string[] lines)
    {
        List<List<List<Token>>> result = new List<List<List<Token>>>();
        foreach (string line in lines)
        {
            result.Add(preprocessor.PreprocessLine(line));
        }

        return result;
    }

   

    public List<string> GetSupportedKeywords()
    {
        return registry.GetSupportedKeywords().ToList();
    }   

    public List<string> GetSupportedInstructions()
    {
        return registry.GetInstructionKeywords().ToList();
    }

}
