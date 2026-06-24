using Godot;
using System;
using WakuWakuDramaClub.Timline;

public partial class StageBackground : TextureRect
{
	[Export]
	AnimationPlayer animationPlayer;
	public AnimationClip Change(string scene)
	{
		if (!ResourceLoader.Exists(scene))
			throw new ArgumentException($"Background not found: {scene}");

		Animation animation = (Animation)animationPlayer.GetAnimation("black-in-out").Duplicate();
		animation.UpdateTrackKeyValue(".:texture", Animation.TrackType.Value, 0, GD.Load(scene));
		return new AnimationClip(animationPlayer, animation);
	}
}
