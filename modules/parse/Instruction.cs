using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WakuWakuDramaClub.Timline;

namespace WakuWakuDramaClub.Parse;
public abstract partial class Instruction : RefCounted
{
    public RawInstruction Raw;

    public Dictionary<String, String> Options
    {
        get => Raw.Options;
    }
    protected Instruction(RawInstruction raw)
    {
        Raw = raw;
        BindArguments();
    }

    public abstract void BindArguments();

    public virtual async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport) {
        await Task.Delay(0);
        return new AnimationPack();

    }
   


}
