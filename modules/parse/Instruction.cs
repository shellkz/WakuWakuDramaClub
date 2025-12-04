using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using WakuWakuDramaClub.Timline;
namespace WakuWakuDramaClub.Parse;
public abstract partial class Instruction : RefCounted
{
    public string[] Arguments { get; set; }
    public Dictionary<string, string> Options { get; set; }
    public Instruction(RawInstruction raw)
    {
        Arguments = raw.Arguments;
        Options = raw.Options;
        BindData();
    }
    public abstract string GetName();
    public abstract void BindData();
    public virtual async Task<AnimationPack> BakeAsAnimation(TimelineViewport viewport) {
        await Task.Delay(0);
        return new AnimationPack();
    }
    public string TryGetArgument(int index, string defaultValue)
    {
        return (index < Arguments.Length)? Arguments[index] : defaultValue;

    }
    public string TryGetOption(string name, string defaultValue)
    {
        string value;
        if (Options.TryGetValue(name, out value))
        {
            return value;
        }
        return defaultValue;

    }

}

public enum InstructionType
{
    ActorEnter,
    Dialogue
}