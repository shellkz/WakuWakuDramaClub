using Godot;
using System;
using System.Threading.Tasks;
using WakuWakuDramaClub.Parse;
using WakuWakuDramaClub.Timline;

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
    public override void BindData()
    {
		Actor = TryGetArgument(0, "");
		Destination = TryGetArgument(1, "");
		Duration = TryGetArgument(2, "");
    }
 	public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();


		Actor actor = viewport.GetActor(Actor);
		Vector2 destination = viewport.GetAnchorPosition(Destination);
		double duration = Duration.ToFloat();
		
		result.AddClip(actor.MoveTo(destination, duration));

		return result;
	}
	
}
