using Godot;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Schema;
using WakuWakuDramaClub.Timline;

namespace WakuWakuDramaClub.Scripting.Instructions;
public partial class ActorEnterInstruction : Instruction
{
	private const string Id = "ActorEnter";
	private const string Keyword = "登場";

	public String Actor {  get; set; }
    public ActorEnterInstruction(RawInstruction raw) : base(raw)
    {
    }
	public override string GetId()
	{
		return Id;
	}
	public override string GetKeyword()
    {
        return Keyword;
    }
	public override InstructionSchema GetSchema()
	{
		return InstructionSchema.Create()
			.Primary(Keyword)
				.Argument("actor", ScriptValueKind.Actor);
    }
    public override void BindData(RawInstruction raw)
    {
		RawInstructionBinder bind = Bind(raw);
		Actor = bind.Get("actor");
    }
 	public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();


		viewport.CreateActorIfNotExist(Actor);
		Actor actor = viewport.GetActor(Actor);
		result.AddClip(actor.Enter());



		return result;
	}
	
}
