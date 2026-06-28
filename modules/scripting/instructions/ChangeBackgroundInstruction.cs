using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Schema;
using WakuWakuDramaClub.Timline;
namespace WakuWakuDramaClub.Scripting.Instructions;
public partial class ChangeBackgroundInstruction : Instruction
{
	private const string Id = "ChangeBackground";
	private const string Keyword = "背景";

	public String Background {  get; set; }
 	public ChangeBackgroundInstruction(RawInstruction raw) : base(raw)
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
				.Argument("background", ScriptValueKind.Background);
    }
    public override void BindData(RawInstruction raw)
    {
		RawInstructionBinder bind = Bind(raw);
		Background = bind.Get("background");
    }
 	public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();

		AssetRecord record = ResourceManager.Instance.GetBackgroundRecord(Background);
		Texture2D texture = ResourceManager.Instance.LoadTexture(record);
		result.AddClip(viewport.Background.Change(texture));

		return result;
	}
}
