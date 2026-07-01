using Godot;
using System;

public partial class RenderingMenu : Panel
{
    [Export]
    public LineEdit FileNameEdit;
    [Export]
    public OptionButton CodecOption;

    [Export]
    public OptionButton FrameRateOption;
    [Export]
    public Button RenderButton;
}
