using Godot;
using WakuWakuDramaClub.Timline;
public partial class TimelinePreview : TextureRect
{
	[Export]
	public Stage Stage;

	[Export]
	public HSlider PlaybackSlider;

    public override void _Ready()
    {
		GD.Print(Stage == null);
        Stage.Playing += OnTimelinePlaying;
		PlaybackSlider.ValueChanged += OnCurosrMoved;
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
        Stage.Seek(value);
    }


    private void OnTimelinePlaying(double cursor, double duration)
    {
        PlaybackSlider.MaxValue = duration;
		PlaybackSlider.Value = cursor;
    }


}
