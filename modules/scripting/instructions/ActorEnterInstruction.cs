using Godot;
using System;
using System.Threading.Tasks;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Schema;
using WakuWakuDramaClub.Timline;

namespace WakuWakuDramaClub.Scripting.Instructions;
public partial class ActorEnterInstruction : Instruction
{
	private const string Id = "ActorEnter";
	private const string Keyword = "登場";
	private const string LocationOption = "在";
	public String Actor {  get; set; }
	public String Location {  get; set; }
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
				.Argument("actor", ScriptValueKind.Actor)
			.Option(LocationOption)
				.Argument("location", ScriptValueKind.Anchor);

    }
    public override void BindData(RawInstruction raw)
    {
		RawInstructionBinder bind = Bind(raw);
		Actor = bind.Get("actor");
		Location = bind.Get(LocationOption, "location");
    }
 	public override Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();


		viewport.CreateActorIfNotExist(Actor);
		Actor actor = viewport.GetActor(Actor);

		if (!string.IsNullOrEmpty(Location))
		{
			Vector2 position = viewport.GetAnchorPosition(Location);
			result.AddClip(actor.Teleport(position));
		}

		result.AddClip(actor.FadeIn());

		return Task.FromResult(result);
	}
	
}
