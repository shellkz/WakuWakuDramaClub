using Godot;
using WakuWakuDramaClub.Timline;
public partial class TimelinePreview : TextureRect
{
	private Stage stage;

	[Export]
	public HSlider PlaybackSlider;

    public override void _Ready()
    {
		PlaybackSlider.ValueChanged += OnCurosrMoved;
    }

	public void Initialize(Stage stage)
	{
		if (this.stage != null)
		{
			this.stage.Playing -= OnTimelinePlaying;
		}

		this.stage = stage;
		Texture = stage?.GetTexture();

		if (this.stage != null)
		{
			this.stage.Playing += OnTimelinePlaying;
		}
	}

	public void Play(Timeline timeline)
	{
		if (stage == null || timeline == null) return;

		stage.Timeline = timeline;
		Play();
	}

	public void Play()
	{
		stage?.Play();
	}

	public void Pause()
	{
		stage?.Pause();
	}

	public void Seek(double value)
	{
		stage?.Seek(value);
	}
    private void OnCurosrMoved(double value)
    {
		Seek(value);
    }


    private void OnTimelinePlaying(double cursor, double duration)
    {
        PlaybackSlider.MaxValue = duration;
		PlaybackSlider.Value = cursor;
    }


}
