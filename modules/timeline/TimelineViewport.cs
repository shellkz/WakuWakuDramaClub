using Godot;
using System;
using System.Linq;
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

    [Export]
    public StageBackground Background;
    
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
        
        foreach (Node child in Actors.GetChildren()){
            if (child is Actor)
            {
                Actor actor = child as Actor;
                GD.Print(actor.Id);
                if (actor.Id == name)
                {
                    return actor;
                }
            }
        }
        return null;

        //return Actors.FindChildren("*", "Actor", true, false).Select(node=>node as Actor).First(actor=>actor.Id == name);
    }

    public Vector2 GetAnchorPosition(string name)
    {
        Control anchor = Anchors.FindChild(name) as Control;
        return anchor.Position;
    }
    public void ClearActors()
    {
        foreach (Node child in Actors.GetChildren())
        {
            child.QueueFree();
        }
    }

    public void CreateActorIfNotExist(string id)
    {
        Actor actor = GetActor(id);
        if (actor != null)
            return;

        if (!ResourceManager.Instance.actors.ContainsKey(id))
            throw new ArgumentException($"Actor not found: {id}");

        string path = ResourceManager.Instance.actors[id];
        if (!ResourceLoader.Exists(path))
            throw new ArgumentException($"Actor scene not found: {path}");

        PackedScene scene = GD.Load<PackedScene>(path);
        Actor node = scene.Instantiate<Actor>();
        Actors.AddChild(node);
    }
}
