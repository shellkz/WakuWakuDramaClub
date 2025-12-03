using Godot;
using System.Collections.Generic;
using System.Linq;
using WakuWakuDramaClub.Render;
namespace WakuWakuDramaClub.Timline;

public partial class AnimationPack : RefCounted
{
    public List<AnimationClip> Clips { get; set; }
    public List<VideoRenderer.AudioClip> Audios { get; set; }
    public AnimationPack() {
        Clips = new List<AnimationClip>();
        Audios = new List<VideoRenderer.AudioClip>(); 
    }

    public void ConvertTo(AnimationPlayer target)
    {
        foreach (var clip in Clips)
        {
            clip.ConvertTo(target);
        }
    }
    public void AddClip(AnimationClip clip)
    {
        Clips.Add(clip);
    }
    public double GetDuration()
    {
        return Clips.Max(c => c.GetDuration());
    }
}
