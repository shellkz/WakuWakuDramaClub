using System;
using WakuWakuDramaClub.Timline;
using System.Threading.Tasks;
using Godot;
using WakuWakuDramaClub.Render;

namespace WakuWakuDramaClub.Scripting.Instructions;
public partial class DialogueInstruction : Instruction
{
    public String Speaker {  get; set; }
    public String Speech { get; set; }
    public String Expression { get; set; }
    public String Pose { get; set; }

    public DialogueInstruction(RawInstruction raw) : base(raw) {}

    public override string GetName()
    {
        return Tr(InstructionType.Dialogue.ToString());
    }

    public override void BindData(RawInstruction raw)
    {
        Speaker = raw.TryGetStatementArgumentValue(Tr(InstructionType.Dialogue.ToString()), 0, "");
        Speech = raw.TryGetStatementArgumentValue(Tr(InstructionType.Dialogue.ToString()), 1, "");
        Expression = raw.TryGetStatementArgumentValue("表情", 0, "");
        Pose = raw.TryGetStatementArgumentValue("動作", 0, "");
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
