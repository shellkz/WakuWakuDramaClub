using Godot;
using System;
using System.Threading.Tasks;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Schema;
using WakuWakuDramaClub.Timline;
namespace WakuWakuDramaClub.Scripting.Instructions;
public partial class ActorMoveInstruction : Instruction
{
	private const string Id = "ActorMove";
	private const string Keyword = "移動";
	private const string FromOption = "從";
	private const string ToOption = "到";
	private const string DurationOption = "在";

	public String Actor {  get; set; }
	public String From {  get; set; }
	public String To {  get; set; }
	public String Duration {  get; set; }
	
    public ActorMoveInstruction(RawInstruction raw) : base(raw)
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
			.Option(FromOption)
				.Argument("from", ScriptValueKind.Anchor, required: false)
			.Option(ToOption)
				.Argument("to", ScriptValueKind.Anchor)
			.Option(DurationOption)
				.Argument("duration", ScriptValueKind.Float, required: false, defaultValue: "0.5");
    }
    public override void BindData(RawInstruction raw)
    {
		RawInstructionBinder bind = Bind(raw);
		Actor = bind.Get("actor");
		From = bind.Get(FromOption, "from");
		To = bind.Get(ToOption, "to");
		Duration = bind.Get(DurationOption, "duration");
    }
 	public override Task<AnimationPack> BakeAsAnimation(Stage viewport)
	{
		AnimationPack result = new AnimationPack();


		viewport.CreateActorIfNotExist(Actor);
		Actor actor = viewport.GetActor(Actor);
		Vector2 to = viewport.GetAnchorPosition(To);
		double duration = Duration.ToFloat();

		if (string.IsNullOrEmpty(From))
		{
			result.AddClip(actor.MoveTo(to, duration));
		}
		else
		{
			Vector2 from = viewport.GetAnchorPosition(From);
			result.AddClip(actor.MoveTo(from, to, duration));
		}

		return Task.FromResult(result);
	}
	
}
