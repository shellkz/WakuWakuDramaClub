using Godot;

using System;
using WakuWakuDramaClub.Parse;
using WakuWakuDramaClub.modules;
public partial class ActorEnterInstructionFactory : InstructionFactory
{
    public override Instruction Create(RawInstruction raw)
    {
        return new ActorEnterInstruction(raw);
    }

    public override string GetName()
    {
		return "登場";
        //return Strings.ac;
    }
}
