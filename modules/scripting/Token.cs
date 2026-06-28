
using System;
using Godot;
namespace WakuWakuDramaClub.Scripting;
public enum TokenType
{
    //Normal,   // Content inside nothing
    //Option,    // Content of option inside（）
    Keyword,
    Dialogue, 
    Argument
}
[GlobalClass]
public partial class Token : Godot.RefCounted
{
    public TokenType Type { get; }
    public string Value { get; }

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString()
    {
        return $"[{Type}: '{Value}']";
    }
}