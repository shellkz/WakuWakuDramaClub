using Godot;
using System;
using System.Collections.Generic;
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

    [Export]
    public PackedScene ActorTemplate;

    private readonly Dictionary<string, Actor> _actorsById = new Dictionary<string, Actor>();
    
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
        if (_actorsById.TryGetValue(name, out Actor actor))
        {
            if (IsInstanceValid(actor) && !actor.IsQueuedForDeletion())
                return actor;

            _actorsById.Remove(name);
        }

        return null;
    }

    public Vector2 GetAnchorPosition(string name)
    {
        Control anchor = Anchors.FindChild(name) as Control;
        return anchor.Position;
    }
    public void ClearActors()
    {
        foreach (Actor actor in _actorsById.Values)
        {
            if (IsInstanceValid(actor))
                actor.QueueFree();
        }

        _actorsById.Clear();
    }

    public void CreateActorIfNotExist(string id)
    {
        Actor actor = GetActor(id);
        if (actor != null)
            return;

        if (ActorTemplate == null)
            throw new InvalidOperationException("Actor template is not assigned.");

        ActorDefinition definition = ResourceManager.Instance.GetActorDefinition(id);
        Actor node = ActorTemplate.Instantiate<Actor>();
        node.Setup(definition);
        Actors.AddChild(node);
        _actorsById[id] = node;
    }
}
