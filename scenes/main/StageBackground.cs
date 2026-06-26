using Godot;
using System;
using WakuWakuDramaClub.Timline;

public partial class StageBackground : TextureRect
{
	[Export]
	AnimationPlayer animationPlayer;
	public AnimationClip Change(Texture2D texture)
	{
		if (texture == null)
			throw new ArgumentException("Background texture is null.");

		Animation animation = (Animation)animationPlayer.GetAnimation("black-in-out").Duplicate();
		animation.UpdateTrackKeyValue(".:texture", Animation.TrackType.Value, 0, texture);
		return new AnimationClip(animationPlayer, animation);
	}
}
