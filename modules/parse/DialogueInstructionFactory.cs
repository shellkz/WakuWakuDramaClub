using Godot;
using System;
using WakuWakuDramaClub.modules;
namespace WakuWakuDramaClub.Parse;

public partial class DialogueInstructionFactory : InstructionFactory
{
    public override string GetName()
    {
        // return translation entry id
        //Strings.DialogueInstructionName

        return "對話";

    }
    public override Instruction Create(RawInstruction raw)
    {
        return new DialogueInstruction(raw);
    }


}
