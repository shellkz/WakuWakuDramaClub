
using System;
using Godot;
namespace WakuWakuDramaClub.Parse;
public enum TokenType
{
    Normal,   // Content inside nothing
    Dialogue, // Content of dialogue inside「」
    Option    // Content of option inside（）
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