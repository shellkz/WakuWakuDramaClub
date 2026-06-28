using Godot;
using System;
using System.Threading.Tasks;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Timline;
using WakuWakuDramaClub.Scripting;
namespace WakuWakuDramaClub.Scripting.Instructions;

public partial class WaitInstruction : Instruction
{
	public string Duration {get; set;}
 	public WaitInstruction(RawInstruction raw) : base(raw)
    {
    }
	public override string GetName()
    {
        return Tr(InstructionType.Wait.ToString());
    }
    public override void BindData(RawInstruction raw)
    {
		
		Duration = raw.TryGetStatementArgumentsValue(Tr(InstructionType.Wait.ToString()), "");

    }
 	public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();
		// empty animation clip with only specified length
		Animation animation = new Animation();
		animation.Length = Duration.ToFloat();
		result.Clips.Add(new AnimationClip(viewport.AnimationPlayer, animation));
		
		return result;
	}



}
