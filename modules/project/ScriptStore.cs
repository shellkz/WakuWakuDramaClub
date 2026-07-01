using System;
using System.IO;

namespace WakuWakuDramaClub.Project;

public sealed class ScriptStore
{
	private string scriptDirectory = "";

	public void LoadProject(string scriptDirectory)
	{
		this.scriptDirectory = scriptDirectory;
	}

	public string LoadScript()
	{
		string path = GetMainScriptPath();
		return File.Exists(path) ? File.ReadAllText(path) : "";
	}

	public void SaveScript(string text)
	{
		File.WriteAllText(GetMainScriptPath(), text);
	}

	private string GetMainScriptPath()
	{
		if (string.IsNullOrWhiteSpace(scriptDirectory))
			throw new InvalidOperationException("Script directory is not set.");

		return Path.Combine(scriptDirectory, ProjectDefaults.MainScriptFileName);
	}
}
