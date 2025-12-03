using System;
using WakuWakuDramaClub.Timline;
using System.Threading.Tasks;
using Godot;

namespace WakuWakuDramaClub.Parse;
public partial class DialogueInstruction : Instruction
{
    public DialogueInstruction(RawInstruction raw) : base(raw)
    {
    }

    public String Speaker {  get; set; }
    public String Speech { get; set; }

    public String Expression { get; set; }

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

        result.AddClip(viewport.DialoguePanel.ShowDialogue(Speaker, Speech, new TimeSpan(0, 0, 1)));

        return result;
    }

    public override void BindArguments()
    {
 
        Speaker = Raw.Arguments[0];
        Speech = Raw.Arguments[1];

        // foreach (string key in Options.Keys)
        // {
        //     GD.Print(key + " " + Options[key]);
        // }
        
        string expression = "";
        if (Options.TryGetValue("表情", out expression))
        {
            Expression = expression;
        }

        // //Expression not null
        


    }

}
