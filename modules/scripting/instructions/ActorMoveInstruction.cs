using Godot;
using System;
using System.Threading.Tasks;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Timline;
namespace WakuWakuDramaClub.Scripting.Instructions;
public partial class ActorMoveInstruction : Instruction
{
	public String Actor {  get; set; }
	public String Destination {  get; set; }
	public String Duration {  get; set; }
	
    public ActorMoveInstruction(RawInstruction raw) : base(raw)
    {
    }
	public override string GetName()
    {
        return Tr(InstructionType.ActorMove.ToString());
    }
    public override void BindData(RawInstruction raw)
    {
		Actor = raw.TryGetStatementArgumentValue(Tr(InstructionType.ActorMove.ToString()), 0, "");
		Destination = raw.TryGetStatementArgumentValue("到", 0, "");
		Duration = raw.TryGetStatementArgumentValue("在", 0, "");
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
