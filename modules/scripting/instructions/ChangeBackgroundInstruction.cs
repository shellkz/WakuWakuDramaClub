using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Timline;
namespace WakuWakuDramaClub.Scripting.Instructions;
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
    public override void BindData(RawInstruction raw)
    {
		Background = raw.TryGetStatementArgumentsValue(Tr(InstructionType.ChangeBackground.ToString()), "");
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
