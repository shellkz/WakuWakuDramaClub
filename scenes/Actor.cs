using Godot;
using System;
using System.Linq;
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
	[Export]
	public TextureRect BodyTextureRect;

	public ActorDefinition Definition { get; private set; }
	
	public void Setup(ActorDefinition definition)
	{
		Definition = definition;
		Id = definition.ActorId;

		BuildExpressionAnimations(definition);
		SetInitialBodyTexture(definition);
	}

	private void BuildExpressionAnimations(ActorDefinition definition)
	{
		if (!ExpressionPlayer.HasAnimation("template"))
			throw new ArgumentException($"Expression template animation not found for actor: {definition.ActorId}");

		AnimationLibrary library = GetOrCreateExpressionLibrary();
		Animation template = ExpressionPlayer.GetAnimation("template");
		foreach ((string name, AssetRecord record) in definition.Expressions.OrderBy(pair => pair.Value.RelativePath, StringComparer.Ordinal))
		{
			Texture2D texture = ResourceManager.Instance.LoadTexture(record);
			Animation animation = (Animation)template.Duplicate();
			animation.TrackSetKeyValue(0, 0, texture);

			if (library.HasAnimation(name))
				library.RemoveAnimation(name);

			library.AddAnimation(name, animation);
		}
	}

	private AnimationLibrary GetOrCreateExpressionLibrary()
	{
		StringName libraryName = "";
		if (!ExpressionPlayer.HasAnimationLibrary(libraryName))
			ExpressionPlayer.AddAnimationLibrary(libraryName, new AnimationLibrary());

		return ExpressionPlayer.GetAnimationLibrary(libraryName);
	}

	private void SetInitialBodyTexture(ActorDefinition definition)
	{
		if (BodyTextureRect == null || definition.Bodies.Count == 0)
			return;

		AssetRecord record = definition.Bodies
			.OrderBy(pair => pair.Value.RelativePath, StringComparer.Ordinal)
			.First()
			.Value;

		BodyTextureRect.Texture = ResourceManager.Instance.LoadTexture(record);
	}


	

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
