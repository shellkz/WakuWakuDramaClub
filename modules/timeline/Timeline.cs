using Godot;
using System;
using System.Collections.Generic;
using WakuWakuDramaClub.Render;

namespace WakuWakuDramaClub.Timline;

public partial class Timeline : RefCounted
{
    public Animation Animation { get; set; }
    public List<VideoRenderer.AudioClip> Audio {  get; set; }

    public Timeline(Animation animation, List<VideoRenderer.AudioClip> audio)
    {
        Animation = animation;
        Audio = audio;
    }
}
