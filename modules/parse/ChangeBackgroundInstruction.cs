using Godot;
using System;
using System.Threading.Tasks;
using WakuWakuDramaClub.Parse;
using WakuWakuDramaClub.Timline;

public partial class ChangeBackgroundInstruction : Instruction
{
	public String Background {  get; set; }
 	public ChangeBackgroundInstruction(RawInstruction raw) : base(raw)
    {
    }
	
	public override string GetName()
    {
        return Tr(InstructionType.ChangeBackground.ToString());
    }
    public override void BindData()
    {
		Background = TryGetArgument(0, "");
    }
 	public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();


		result.AddClip(viewport.Background.Change(Background));

		return result;
	}
}
