using Godot;
using System;

public partial class CreateProjectPopup : PanelContainer
{
    [Export]
    Button CloseButton;

    [Export]
    LineEdit ProjectNameEdit;

    [Export]
    LineEdit ProjectPathEdit;

    [Export]
    Button PickProjectPathButton;

    [Export]
    Button ConfirmButton;

    [Export]
    Button CancelButton;

    [Export]
    FileDialog FileDialog;

    private Action<Godot.Collections.Dictionary> _onSubmitted;

    public override void _Ready()
    {
        ConfirmButton.Pressed += OnConfirmPressed;
        CancelButton.Pressed += Close;
        CloseButton.Pressed += Close;
        PickProjectPathButton.Pressed += () => FileDialog.PopupCentered();
        FileDialog.DirSelected += dir => ProjectPathEdit.Text = dir;
    }

    private void OnConfirmPressed()
    {
        var data = new Godot.Collections.Dictionary
        {
            ["name"] = ProjectNameEdit.Text,
            ["path"] = ProjectPathEdit.Text
        };
        Close();
        _onSubmitted?.Invoke(data);
    }

    public void Open(Action<Godot.Collections.Dictionary> onSubmitted)
    {
        _onSubmitted = onSubmitted;
        ProjectNameEdit.Text = "";
        ProjectPathEdit.Text = "";
        Show();
    }

    public void Close()
    {
        Hide();
    }
}
