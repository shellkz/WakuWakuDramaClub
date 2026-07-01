using Godot;
using System;
using System.Threading.Tasks;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Scripting.Schema;
using WakuWakuDramaClub.Timline;
namespace WakuWakuDramaClub.Scripting.Instructions;
public partial class PlaySoundInstruction : Instruction
{
	private const string Id = "PlaySound";
	private const string Keyword = "音效";

	public string Sound {get; set;}
 	public PlaySoundInstruction(RawInstruction raw) : base(raw)
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
				.Argument("audio", ScriptValueKind.Audio);
    }
    public override void BindData(RawInstruction raw)
    {
		RawInstructionBinder bind = Bind(raw);
		Sound = bind.Get("audio");
    }
 	public override Task<AnimationPack> BakeAsAnimation(Stage viewport)
	{
		AnimationPack result = new AnimationPack();

        VideoRenderer.AudioClip voiceClip = new VideoRenderer.AudioClip();

        voiceClip.Format = "wav";
        AssetRecord record = ResourceManager.Instance.GetAudioRecord(Sound);
        voiceClip.Data = ResourceManager.Instance.LoadAudioBytes(record);
        result.Audios.Add(voiceClip);
	
		return Task.FromResult(result);
	}

}
