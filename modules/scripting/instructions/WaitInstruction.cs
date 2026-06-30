using Godot;
using System;
using System.Threading.Tasks;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Timline;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Schema;
namespace WakuWakuDramaClub.Scripting.Instructions;

public partial class WaitInstruction : Instruction
{
	private const string Id = "Wait";
	private const string Keyword = "等待";

	public string Duration {get; set;}
 	public WaitInstruction(RawInstruction raw) : base(raw)
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
				.Argument("duration", ScriptValueKind.Float);
	}
    public override void BindData(RawInstruction raw)
    {
		RawInstructionBinder bind = Bind(raw);
		Duration = bind.Get("duration");
    }
 	public override Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();
		// empty animation clip with only specified length
		Animation animation = new Animation();
		animation.Length = Duration.ToFloat();
		result.Clips.Add(new AnimationClip(viewport.AnimationPlayer, animation));
		
		return Task.FromResult(result);
	}



}
