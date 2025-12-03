using Godot;
using WakuWakuDramaClub.Timline;
public partial class TimelinePreview : TextureRect
{
	[Export]
	public TimelineViewport TimelineViewport;

	[Export]
	public HSlider PlaybackSlider;

    public override void _Ready()
    {
        TimelineViewport.Playing += OnTimelinePlaying;
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
        TimelineViewport.Seek(value);
    }


    private void OnTimelinePlaying(double cursor, double duration)
    {
        PlaybackSlider.MaxValue = duration;
		PlaybackSlider.Value = cursor;
    }


}
