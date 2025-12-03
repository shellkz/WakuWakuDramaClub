using Godot;
using System;
using WakuWakuDramaClub.Timline;

public partial class Actor : Control
{
	[Export]
	public AnimationPlayer ExpressionPlayer;

	[Export]
	public AnimationPlayer PosePlayer;
	

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
}
