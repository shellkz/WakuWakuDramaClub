using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public enum AssetType
{
	ActorExpression,
	ActorBody,
	Background,
	Audio
}

public sealed class AssetRecord
{
	public string Id { get; init; } = "";
	public string RelativePath { get; init; } = "";
	public AssetType Type { get; init; }
}

public sealed class ActorDefinition
{
	public string PackName { get; init; } = "";
	public string ActorName { get; init; } = "";
	public string ActorId { get; init; } = "";
	public string ActorRootPath { get; init; } = "";
	public Dictionary<string, AssetRecord> Expressions { get; init; } = new Dictionary<string, AssetRecord>();
	public Dictionary<string, AssetRecord> Bodies { get; init; } = new Dictionary<string, AssetRecord>();
}

public partial class ResourceManager : Node
{
	public static ResourceManager Instance { get; private set; }

	public Dictionary<string, AssetRecord> AssetIndex { get; private set; } = new Dictionary<string, AssetRecord>();

	// Legacy actor scene lookup kept until timeline actor creation is migrated to the asset index.
	public Dictionary<string, string> actors = new Dictionary<string, string>();

	private static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		".png",
		".jpg",
		".jpeg"
	};

	private static readonly HashSet<string> AudioExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		".wav",
		".mp3",
		".ogg"
	};

	public override void _Ready()
	{
		Instance = this;
		CallDeferred(MethodName.ConnectProjectSession);
	}

	private void ConnectProjectSession()
	{
		if (ProjectSession.Instance == null)
			return;

		ProjectSession.Instance.ProjectLoaded += Refresh;
		if (!string.IsNullOrWhiteSpace(ProjectSession.Instance.WorkingDirectory))
			Refresh();
	}

	public void Refresh()
	{
		AssetIndex.Clear();

		if (ProjectSession.Instance == null)
			return;

		string workingDirectory = ProjectSession.Instance.WorkingDirectory;
		if (string.IsNullOrWhiteSpace(workingDirectory))
			return;

		string assetsDirectory = Path.Combine(workingDirectory, "assets");
		if (!Directory.Exists(assetsDirectory))
			return;

		foreach (string packDirectory in Directory.GetDirectories(assetsDirectory))
		{
			ScanPack(workingDirectory, packDirectory);
		}
	}

	public ActorDefinition GetActorDefinition(string actorId)
	{
		if (!TryParseQualifiedActorId(actorId, out string packName, out string actorName))
			throw new ArgumentException($"Actor id must use pack/actor format: {actorId}");

		return GetActorDefinition(packName, actorName);
	}

	public ActorDefinition GetActorDefinition(string packName, string actorName)
	{
		if (string.IsNullOrWhiteSpace(packName))
			throw new ArgumentException("Pack name is required.");

		if (string.IsNullOrWhiteSpace(actorName))
			throw new ArgumentException("Actor name is required.");

		string actorId = string.Join('/', packName, actorName);
		string actorRootPath = string.Join('/', "assets", packName, "actors", actorName);
		Dictionary<string, AssetRecord> expressions = new Dictionary<string, AssetRecord>();
		Dictionary<string, AssetRecord> bodies = new Dictionary<string, AssetRecord>();

		foreach (AssetRecord record in AssetIndex.Values)
		{
			if (record.Type != AssetType.ActorExpression && record.Type != AssetType.ActorBody)
				continue;

			if (!TryParseActorAssetPath(record.RelativePath, out string recordPackName, out string recordActorName, out string recordActorRootPath))
				continue;

			if (recordPackName != packName || recordActorName != actorName)
				continue;

			if (recordActorRootPath != actorRootPath)
				continue;

			string name = Path.GetFileNameWithoutExtension(record.RelativePath);

			if (record.Type == AssetType.ActorExpression)
				expressions[name] = record;
			else
				bodies[name] = record;
		}

		if (expressions.Count == 0 && bodies.Count == 0)
			throw new ArgumentException($"Actor not found: {actorId}");

		return new ActorDefinition
		{
			PackName = packName,
			ActorName = actorName,
			ActorId = actorId,
			ActorRootPath = actorRootPath,
			Expressions = expressions,
			Bodies = bodies
		};
	}

	public Texture2D LoadTexture(AssetRecord record)
	{
		if (record.Type != AssetType.ActorExpression && record.Type != AssetType.ActorBody && record.Type != AssetType.Background)
			throw new ArgumentException($"Asset is not a texture: {record.RelativePath}");

		string path = ProjectSession.Instance.ToAbsolutePath(record.RelativePath);
		Image image = Image.LoadFromFile(path);
		if (image == null || image.IsEmpty())
			throw new ArgumentException($"Failed to load texture: {record.RelativePath}");

		return ImageTexture.CreateFromImage(image);
	}

	private void ScanPack(string workingDirectory, string packDirectory)
	{
		ScanActorAssets(workingDirectory, Path.Combine(packDirectory, "actors"));
		ScanFiles(workingDirectory, Path.Combine(packDirectory, "backgrounds"), AssetType.Background, ImageExtensions);
		ScanFiles(workingDirectory, Path.Combine(packDirectory, "audio"), AssetType.Audio, AudioExtensions);
	}

	private void ScanActorAssets(string workingDirectory, string actorsDirectory)
	{
		if (!Directory.Exists(actorsDirectory))
			return;

		foreach (string actorDirectory in Directory.GetDirectories(actorsDirectory))
		{
			ScanFiles(workingDirectory, Path.Combine(actorDirectory, "expressions"), AssetType.ActorExpression, ImageExtensions);
			ScanFiles(workingDirectory, Path.Combine(actorDirectory, "bodies"), AssetType.ActorBody, ImageExtensions);
		}
	}

	private void ScanFiles(string workingDirectory, string directory, AssetType type, HashSet<string> extensions)
	{
		if (!Directory.Exists(directory))
			return;

		foreach (string path in Directory.GetFiles(directory))
		{
			if (!extensions.Contains(Path.GetExtension(path)))
				continue;

			AddAsset(workingDirectory, path, type);
		}
	}

	private void AddAsset(string workingDirectory, string path, AssetType type)
	{
		string relativePath = Path.GetRelativePath(workingDirectory, path).Replace('\\', '/');
		AssetIndex[relativePath] = new AssetRecord
		{
			Id = relativePath,
			RelativePath = relativePath,
			Type = type
		};
	}

	private static bool TryParseActorAssetPath(string relativePath, out string packName, out string actorName, out string actorRootPath)
	{
		packName = "";
		actorName = "";
		actorRootPath = "";

		string[] segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (segments.Length < 6)
			return false;

		if (segments[0] != "assets" || segments[2] != "actors")
			return false;

		packName = segments[1];
		actorName = segments[3];
		actorRootPath = string.Join('/', segments[0], segments[1], segments[2], segments[3]);
		return true;
	}

	private static bool TryParseQualifiedActorId(string actorId, out string packName, out string actorName)
	{
		packName = "";
		actorName = "";

		string[] segments = actorId.Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (segments.Length != 2)
			return false;

		packName = segments[0];
		actorName = segments[1];
		return true;
	}
}
