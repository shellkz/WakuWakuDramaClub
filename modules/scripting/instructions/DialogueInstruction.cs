using System;
using WakuWakuDramaClub.Timline;
using System.Threading.Tasks;
using Godot;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Scripting.Schema;

namespace WakuWakuDramaClub.Scripting.Instructions;
public partial class DialogueInstruction : Instruction
{
    private const string Id = "Dialogue";
    private const string Keyword = "對話";
    private const string ExpressionOption = "表情";
    private const string PoseOption = "動作";

    public String Speaker {  get; set; }
    public String Speech { get; set; }
    public String Expression { get; set; }
    public String Pose { get; set; }

    public DialogueInstruction(RawInstruction raw) : base(raw) {}

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
                .Argument("dialogue", ScriptValueKind.DialogueText)
            .Option(ExpressionOption)
                .Argument("expression", ScriptValueKind.ActorExpression, required: false)
            .Option(PoseOption)
                .Argument("pose", ScriptValueKind.ActorPose, required: false);
    }

    public override void BindData(RawInstruction raw)
    {
        RawInstructionBinder bind = Bind(raw);
        Speaker = bind.Get("actor");
        Speech = bind.Get("dialogue");
        Expression = bind.Get(ExpressionOption, "expression");
        Pose = bind.Get(PoseOption, "pose");
    }

    public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
    {
        AnimationPack result = new AnimationPack();

        //// TTS logic
        // SherpaTTSService tts = viewport.GetNode<SherpaTTSService>("/root/SherpaTTSService");
        // (byte[] voice, TimeSpan duration)= await tts.TTSAsync(Speech, 1.0f, 3);
        // VideoRenderer.AudioClip voiceClip = new VideoRenderer.AudioClip();
        // voiceClip.Format = "wav";
        // voiceClip.Data = voice;
        // result.Audios.Add(voiceClip);
        SystemSpeechTTS tts= new SystemSpeechTTS();
        (byte[] voice, TimeSpan duration) = tts.SynthesizeSpeechToBytes(Speech, "Microsoft Yating");
        VideoRenderer.AudioClip voiceClip = new VideoRenderer.AudioClip();
        voiceClip.Format = "wav";
        voiceClip.Data = voice;
        result.Audios.Add(voiceClip);
        

		viewport.CreateActorIfNotExist(Speaker);
		Actor actor = viewport.GetActor(Speaker);

        if(!String.IsNullOrEmpty(Expression))
		    result.AddClip(actor.MakeExpression(Expression));
    
        if (!String.IsNullOrEmpty(Pose))
        {
            result.AddClip(actor.TakePose(Pose));
        }

        result.AddClip(viewport.DialoguePanel.ShowDialogue(actor.Definition.DisplayName, Speech, duration));
        //GD.Print("AnimationPack length " + result.GetDuration());
        return result;
    }

}
