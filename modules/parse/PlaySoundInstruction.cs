using Godot;
using System;
using System.IO;
using System.Threading.Tasks;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Timline;
namespace WakuWakuDramaClub.Parse;
public partial class PlaySoundInstruction : Instruction
{
	public string Sound {get; set;}
 	public PlaySoundInstruction(RawInstruction raw) : base(raw)
    {
    }
	public override string GetName()
    {
        return Tr(InstructionType.PlaySound.ToString());
    }
    public override void BindData(RawInstruction raw)
    {
		
		Sound = String.Format("res://assets/sounds/{0}.wav", raw.TryGetStatementArgumentsValue(Tr(InstructionType.PlaySound.ToString()), ""));

    }
 	public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();

        VideoRenderer.AudioClip voiceClip = new VideoRenderer.AudioClip();

        voiceClip.Format = "wav";
        voiceClip.Data = LoadSound(Sound);
        result.Audios.Add(voiceClip);
	
		return result;
	}

    private byte[] LoadSound(string sound)
    {
        if (!Godot.FileAccess.FileExists(sound))
            throw new ArgumentException($"Sound not found: {sound}");
        return Godot.FileAccess.GetFileAsBytes(sound);
    }

}
