using Godot;
using System;
using WakuWakuDramaClub.Render;

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

    private RenderingMenuServices services;
    private bool signalsConnected;

    public void Initialize(RenderingMenuServices services)
    {
        DisconnectSignals();
        this.services = services;
        ConnectSignals();
    }

    private void ConnectSignals()
    {
        if (signalsConnected) return;

        RenderButton.Pressed += OnRenderButtonPressed;
        signalsConnected = true;
    }

    private void DisconnectSignals()
    {
        if (!signalsConnected) return;

        RenderButton.Pressed -= OnRenderButtonPressed;
        signalsConnected = false;
    }

    private async void OnRenderButtonPressed()
    {
        if (services?.BuildTimeline == null || services.VideoRenderer == null) return;

        var timeline = await services.BuildTimeline();
        await services.VideoRenderer.ExportAnimationToVideo(timeline.Animation, timeline.Audio, CollectRenderSettings());
    }

    private VideoRenderSettings CollectRenderSettings()
    {
        return new VideoRenderSettings
        {
            OutputDirectory = services.ExportDirectory,
            OutputFileName = NormalizeOutputFileName(FileNameEdit.Text),
            VideoCodec = GetSelectedVideoCodec(),
            FrameRate = GetSelectedFrameRate()
        };
    }

    private static string NormalizeOutputFileName(string fileName)
    {
        string normalized = string.IsNullOrWhiteSpace(fileName)
            ? "output"
            : fileName.Trim();

        return normalized.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : $"{normalized}.mp4";
    }

    private string GetSelectedVideoCodec()
    {
        return CodecOption.Selected switch
        {
            0 => "libx264",
            _ => "libx264"
        };
    }

    private int GetSelectedFrameRate()
    {
        return FrameRateOption.Selected switch
        {
            0 => 30,
            1 => 60,
            _ => 30
        };
    }
}
