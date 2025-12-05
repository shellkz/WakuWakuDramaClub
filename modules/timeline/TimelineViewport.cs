using Godot;
using System;
namespace WakuWakuDramaClub.Timline;
public partial class TimelineViewport : SubViewport
{
    [Signal]
    public delegate void PlayingEventHandler(double cursor, double duration);

    [Export]
    public AnimationPlayer AnimationPlayer;


    [Export]
    public Control Actors;
    [Export]
    public Control Anchors;
    [Export]
    public DialoguePanel DialoguePanel;

    private Timeline _timeline;
    public Timeline Timeline
    {
        get => _timeline;
        set
        {
            _timeline = value;

            AnimationLibrary animationLibrary = new AnimationLibrary();
            animationLibrary.AddAnimation("timeline", _timeline.Animation);

            if (AnimationPlayer.HasAnimationLibrary("custom"))
            {
                AnimationPlayer.RemoveAnimationLibrary("custom");
            }
            AnimationPlayer.AddAnimationLibrary("custom", animationLibrary);
            
        }
    }

    public override void _Process(double delta)
    {
        
        if (AnimationPlayer.IsPlaying())
        {
            EmitSignal(SignalName.Playing, AnimationPlayer.CurrentAnimationPosition, AnimationPlayer.CurrentAnimationLength);    
        }

    }

    public void Play()
    {
        if(!AnimationPlayer.HasAnimation("custom/timeline"))
            return;

        AnimationPlayer.Play("custom/timeline");
		// AnimationPlayer.Pause();
		// AnimationPlayer.Play();
    }

    public void Pause()
    {
		AnimationPlayer.Pause();
    }

    public void Seek(double cursor)
    {
		AnimationPlayer.Seek(cursor, true);
    }


    public Actor GetActor(string name)
    {
        return Actors.FindChild(name) as Actor;
    }

    public Vector2 GetAnchorPosition(string name)
    {
        Control anchor = Anchors.FindChild(name) as Control;
        return anchor.Position;
    }
}
