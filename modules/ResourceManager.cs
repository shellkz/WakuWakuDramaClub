using Godot;
using System;
using System.Collections.Generic;

public partial class ResourceManager : Node
{
	public static ResourceManager Instance { get; private set; } 
    
    // Other global properties
	public Dictionary<string, string> actors = new Dictionary<string, string>();

    public override void _Ready()
    {
        // 2. Set the instance when the node enters the tree (i.e., when it autoloads)
        Instance = this;

		// // Mount ResourcePack from working_directory/packs/xxx.zip or .pck
		// ProjectSetting.LoadResourcePack(path to .pck)
		
		// Setup dictionary to map resource name to resource path
		string PACKS_DIRECTORY = "res://packs";
		foreach (string pack in DirAccess.GetDirectoriesAt(PACKS_DIRECTORY))
		{
			// Actors
			string actorsPath = string.Format("{0}/{1}/actors", PACKS_DIRECTORY, pack);
			foreach (string actor in DirAccess.GetDirectoriesAt(actorsPath))
			{
				string actorPath = string.Format("{0}/{1}/{2}.tscn", actorsPath, actor, actor);
				actors.Add(actor, actorPath);
				
			}
		}
    }
}
