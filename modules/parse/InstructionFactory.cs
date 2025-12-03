using Godot;
using System;
namespace WakuWakuDramaClub.Parse;
public abstract partial class InstructionFactory : RefCounted
{
    public abstract string GetName();
    public abstract Instruction Create(RawInstruction raw);

}
