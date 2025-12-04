using System;
using WakuWakuDramaClub.Timline;
using System.Threading.Tasks;
using Godot;

namespace WakuWakuDramaClub.Parse;
public partial class DialogueInstruction : Instruction
{
    public String Speaker {  get; set; }
    public String Speech { get; set; }
    public String Expression { get; set; }

    public DialogueInstruction(RawInstruction raw) : base(raw) {}

    public override string GetName()
    {
        return Tr(InstructionType.Dialogue.ToString());
    }

    public override void BindData()
    {
 
        Speaker = TryGetArgument(0, "");
        Speech = TryGetArgument(1, "");
        Expression = TryGetOption("表情", "");
    }

    public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
    {
        AnimationPack result = new AnimationPack();

        // SherpaTTSService tts = viewport.GetNode<SherpaTTSService>("/root/SherpaTTSService");
        // (byte[] voice, TimeSpan duration)= await tts.TTSAsync(Speech, 1.0f, 3);
       


        // VideoRenderer.AudioClip voiceClip = new VideoRenderer.AudioClip();
        // voiceClip.Format = "wav";
        // voiceClip.Data = voice;

        // result.Audios.Add(voiceClip);

        

		Actor actor = viewport.GetActor(Speaker);
        if(!String.IsNullOrEmpty(Expression))
		    result.AddClip(actor.MakeExpression(Expression));

        result.AddClip(viewport.DialoguePanel.ShowDialogue(Speaker, Speech, TimeSpan.FromSeconds(Speech.Length / 5.0)));

        return result;
    }

}
