using Godot;
using System;
using WakuWakuDramaClub.Timline;

public partial class Actor : Control
{

	[Export]
	public string Id;
	
	[Export]
	public AnimationPlayer MotionPlayer;
	[Export]
	public AnimationPlayer PosePlayer;
	[Export]
	public AnimationPlayer ExpressionPlayer;


	

	//Enter() => AnimationClip that fade actor in
	public AnimationClip Enter()
	{
		
		Animation animation = (Animation)PosePlayer.GetAnimation("fade_in").Duplicate();
				

        double duration = 0.3;
        animation.Length = (float)duration;

   		//animation.UpdateTrackKeyValue(".:visible", Animation.TrackType.Value, 1, duration);
        animation.UpdateTrackKeyTime(".:modulate", Animation.TrackType.Value, 1, duration);



		return new AnimationClip(PosePlayer, animation);
	}

	public AnimationClip MakeExpression(string expression)
	{
		Animation animation = (Animation)ExpressionPlayer.GetAnimation(expression).Duplicate();
		animation.Length = 0;

		return new AnimationClip(ExpressionPlayer, animation);
	}
	public AnimationClip TakePose(string pose)
	{
		Animation animation = (Animation)PosePlayer.GetAnimation(pose).Duplicate();
		return new AnimationClip(PosePlayer, animation);
	}

	public AnimationClip MoveTo(Vector2 destination, double duration)
	{
		Animation animation = (Animation)MotionPlayer.GetAnimation("move").Duplicate();

		// position or global_position?
		
		animation.UpdateTrackKeyValue(".:position",Animation.TrackType.Value, 0, Position);

		animation.UpdateTrackKeyTime(".:position",Animation.TrackType.Value, 1, duration);
		animation.UpdateTrackKeyValue(".:position",Animation.TrackType.Value, 1, destination);

		animation.Length = (float)duration;

		Position = destination;
		
		return new AnimationClip(MotionPlayer, animation);
	}

	//TakePose
}
