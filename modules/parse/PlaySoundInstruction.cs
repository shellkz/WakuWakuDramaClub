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
		Sound = raw.TryGetStatementArgumentsValue(Tr(InstructionType.PlaySound.ToString()), "");
    }
 	public override async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport)
	{
		AnimationPack result = new AnimationPack();

        VideoRenderer.AudioClip voiceClip = new VideoRenderer.AudioClip();

        voiceClip.Format = "wav";
        AssetRecord record = ResourceManager.Instance.GetAudioRecord(Sound);
        voiceClip.Data = ResourceManager.Instance.LoadAudioBytes(record);
        result.Audios.Add(voiceClip);
	
		return result;
	}

}
