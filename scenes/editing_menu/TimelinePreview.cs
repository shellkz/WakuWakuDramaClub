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

	// OnTimelineOperated(TimlineOperation operation)
	//		Play
	//		Pause
	//		CursorDraggingStarted	=>	PAUSE
	//		CursorDragging			=>	SEEK
	//		CursorDraggingEnded		=>  PLAY
	
    private void OnCurosrMoved(double value)
    {
		//TimelineViewport.Pause();
        stage?.Seek(value);
    }


    private void OnTimelinePlaying(double cursor, double duration)
    {
        PlaybackSlider.MaxValue = duration;
		PlaybackSlider.Value = cursor;
    }


}
