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
	public override string GetName()
    {
        return Tr(InstructionType.ActorEnter.ToString());
    }
    public override void BindData()
    {
		Actor = TryGetArgument(0, "");
    }
 	public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();


		Actor actor = viewport.GetActor(Actor);
		result.AddClip(actor.Enter());



		return result;
	}
	
}
