using Godot;
using System;
namespace WakuWakuDramaClub.Timline;
public partial class AnimationClip : RefCounted
{
    public AnimationPlayer AnimationPlayer { get; set; }
    public Animation Animation { get; set; }

    public AnimationClip(AnimationPlayer animationPlayer, Animation animation)
    {
        AnimationPlayer = animationPlayer;
        Animation = animation;
    }

    public double GetDuration()
    {
        return Animation.Length;
    }
    public void ConvertTo(AnimationPlayer target)
    {
        Animation.ConvertTo(AnimationPlayer, target);
        AnimationPlayer = target;
    }
    

  
}
