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
	private const string ToOption = "到";
	private const string DurationOption = "在";

	public String Actor {  get; set; }
	public String Destination {  get; set; }
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
			.Option(ToOption)
				.Argument("destination", ScriptValueKind.Anchor, required: false)
			.Option(DurationOption)
				.Argument("duration", ScriptValueKind.Float, required: false, defaultValue: "0");
    }
    public override void BindData(RawInstruction raw)
    {
		RawInstructionBinder bind = Bind(raw);
		Actor = bind.Get("actor");
		Destination = bind.Get(ToOption, "destination");
		Duration = bind.Get(DurationOption, "duration");
    }
 	public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();

		GD.Print(String.Format("Move to [{0}]", Destination));
		GD.Print(String.Format("in [{0}]", Duration));

		viewport.CreateActorIfNotExist(Actor);
		Actor actor = viewport.GetActor(Actor);
		Vector2 destination = viewport.GetAnchorPosition(Destination);
		double duration = Duration.ToFloat();
		

		result.AddClip(actor.MoveTo(destination, duration));

		return result;
	}
	
}
