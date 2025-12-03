using Godot;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using WakuWakuDramaClub.Parse;
using WakuWakuDramaClub.Timline;
public partial class ActorEnterInstruction : Instruction
{
	public String Actor {  get; set; }

    public ActorEnterInstruction(RawInstruction raw) : base(raw)
    {
    }

 	public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();


		Actor actor = viewport.GetActor(Actor);
		result.AddClip(actor.Enter());
		// viewport.GetActor(Actor);
		// result.AddClip(actor.Enter());

		//	TimelineViewport
		//		GetActor() => find actor by name


		return result;
	}
	
    public override void BindArguments()
    {
        Actor = Raw.Arguments[0];
    }
}
