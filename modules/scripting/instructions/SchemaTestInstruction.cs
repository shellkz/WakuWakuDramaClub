using WakuWakuDramaClub.Scripting.Schema;
using Godot;

namespace WakuWakuDramaClub.Scripting.Instructions;

public partial class SchemaTestInstruction : Instruction
{
	private const string Id = "SchemaTest";
	private const string Keyword = "測試";
	private const string ExpressionOption = "表情";
	private const string PoseOption = "動作";
	private const string AnchorOption = "到";
	private const string DurationOption = "在";

	public string Actor { get; private set; }
	public string Dialogue { get; private set; }
	public string Expression { get; private set; }
	public string Pose { get; private set; }
	public string Anchor { get; private set; }
	public string Duration { get; private set; }

	public SchemaTestInstruction(RawInstruction raw) : base(raw)
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
			.Primary(GetKeyword())
				.Argument("actor", ScriptValueKind.Actor)
				.Argument("dialogue", ScriptValueKind.DialogueText)
			.Option(ExpressionOption)
				.Argument("expression", ScriptValueKind.ActorExpression, required: false)
			.Option(PoseOption)
				.Argument("pose", ScriptValueKind.ActorPose, required: false)
			.Option(AnchorOption)
				.Argument("anchor", ScriptValueKind.Anchor, required: false)
			.Option(DurationOption)
				.Argument("duration", ScriptValueKind.Float, required: false, defaultValue: "0");

	}

	public override void BindData(RawInstruction raw)
	{
		RawInstructionBinder bind = Bind(raw);

		Actor = bind.Get("actor");
		Dialogue = bind.Get("dialogue");
		Expression = bind.Get(ExpressionOption, "expression");
		Pose = bind.Get(PoseOption, "pose");
		Anchor = bind.Get(AnchorOption, "anchor");
		Duration = bind.Get(DurationOption, "duration");
	}

	
}
