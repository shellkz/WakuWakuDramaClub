using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WakuWakuDramaClub.Project;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Scripting.Instructions;
namespace WakuWakuDramaClub.Timline;
public partial class Stage : SubViewport
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


    public async Task<Timeline> BuildTimeline(List<Instruction> instructions)
    {
    
        Animation timeline = new Animation();
        List<VideoRenderer.AudioClip> audios = new List<VideoRenderer.AudioClip>();
        double cursor = 0.0;

        InsertInitialAnimation(timeline);

        // Content
        foreach (Instruction instruction in instructions)
        {
            AnimationPack animationPack = await instruction.BakeAsAnimation(this);
        
            animationPack.ConvertTo(AnimationPlayer); 
       
            // [Time Advancing and insert keyframe]
            foreach (AnimationClip clip in animationPack.Clips)
            {
               
                timeline.InsertAnimation(clip.Animation, cursor, true, ProjectDefaults.FrameDelta);
            }

            foreach (VideoRenderer.AudioClip audio in animationPack.Audios)
            {

                audio.StartingTime += cursor;
                audios.Add(audio);
            }

            cursor += animationPack.GetDuration();
        }

        return  new Timeline(timeline, audios);
    }

    private void InsertInitialAnimation(Animation timeline)
    {
        // Gather stage objects, at the moment there is only DialoguePanel
        if (DialoguePanel == null)
        {
            GD.PrintErr("Cannot insert initial animation because DialoguePanel is null.");
            return;
        }
        // For each  stage object
        //  for intial clip in object.Reset()
        //      convert
        //      insert
        foreach (AnimationClip clip in DialoguePanel.Reset())
        {
            clip.ConvertTo(AnimationPlayer);
            timeline.InsertAnimation(clip.Animation, 0, true, 0);
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
