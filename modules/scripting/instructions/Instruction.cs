using Godot;
using System.Threading.Tasks;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Schema;
using WakuWakuDramaClub.Timline;
namespace WakuWakuDramaClub.Scripting.Instructions;
public abstract partial class Instruction : RefCounted
{
    // public string[] Arguments { get; set; }
    // public Dictionary<string, string> Options { get; set; }
    public Instruction(RawInstruction raw)
    {
        // Arguments = raw.Arguments;
        // Options = raw.Options;
        BindData(raw);
    }
    public abstract string GetId();
    public abstract string GetKeyword();
    public abstract InstructionSchema GetSchema();
    public abstract void BindData(RawInstruction raw);
    protected RawInstructionBinder Bind(RawInstruction raw)
    {
        return new RawInstructionBinder(raw, GetSchema());
    }
    public virtual Task<AnimationPack> BakeAsAnimation(Stage viewport) {
        return Task.FromResult(new AnimationPack());
    }
    
    // public string TryGetArgument(int index, string defaultValue)
    // {
    //     return (index < Arguments.Length)? Arguments[index] : defaultValue;

    // }

    // public string TryGetOption(string name, string defaultValue)
    // {
    //     string value;
    //     if (Options.TryGetValue(name, out value))
    //     {
    //         return value;
    //     }
    //     return defaultValue;

    // }

}

public enum InstructionType
{
    ActorEnter,
    Dialogue,
    ActorMove,
    ChangeBackground,
    PlaySound,
    Wait
}
