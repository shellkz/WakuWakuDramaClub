using Godot;
using System;
using System.Collections.Generic;
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
    public override void BindData(RawInstruction raw)
    {
		// foreach (Statement statement in raw.Statements.Values)
		// {
		// 	GD.Print(statement.Type);
		// 	foreach (string argument in statement.Arguments)
		// 	{	
		// 		GD.Print("  -" + argument);
		// 	}	
			
		// 	//GD.Print(raw.TryGetStatementArgumentValue(Tr(InstructionType.ChangeBackground.ToString()), 0, ""));
		// }

		// foreach (string key in raw.Statements.Keys)
		// {
		// 	GD.Print(key + " " + raw.Statements[key].Type);

		// }
		Background = String.Format("res://assets/backgrounds/{0}.jpg", raw.TryGetStatementArgumentsValue(Tr(InstructionType.ChangeBackground.ToString()), ""));
    }
 	public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();


		result.AddClip(viewport.Background.Change(Background));

		return result;
	}
}
