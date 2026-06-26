using Godot;
using System;
using System.IO;

public partial class ProjectSession : Node
{
    public event Action ProjectLoaded;

    public static ProjectSession Instance { get; private set; }

    public string WorkingDirectory { get; private set; } = "";
    public ProjectData Data { get; private set; }

    private const string ProjectFileName = "project.json";

    public override void _Ready()
    {
        Instance = this;
    }

    public void CreateProject(string name, string path)
    {
        if (File.Exists(Path.Combine(path, ProjectFileName)))
            throw new InvalidOperationException($"目錄已存在專案：{path}");

        Directory.CreateDirectory(path);
        Directory.CreateDirectory(Path.Combine(path, "scripts"));
        Directory.CreateDirectory(Path.Combine(path, "assets", "default", "actors"));
        Directory.CreateDirectory(Path.Combine(path, "assets", "default", "backgrounds"));
        Directory.CreateDirectory(Path.Combine(path, "assets", "default", "audio"));

        WorkingDirectory = path;
        Data = new ProjectData { ProjectName = name, LastOpenedDatetime = DateTime.Now };
        Save();
        ProjectLoaded?.Invoke();
    }

    public void Load(string workingDirectory)
    {
        var projectFilePath = Path.Combine(workingDirectory, ProjectFileName);
        if (!File.Exists(projectFilePath))
            throw new InvalidOperationException($"找不到專案：{workingDirectory}");

        WorkingDirectory = workingDirectory;
        Data = ProjectData.FromJson(File.ReadAllText(projectFilePath));
        Data.LastOpenedDatetime = DateTime.Now;
        Save();
        ProjectLoaded?.Invoke();
    }

    public void Save()
    {
        var path = Path.Combine(WorkingDirectory, ProjectFileName);
        File.WriteAllText(path, Data.ToJson());
    }

    public string ToAbsolutePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(WorkingDirectory))
            throw new InvalidOperationException("Project working directory is not set.");

        return Path.Combine(WorkingDirectory, relativePath);
    }

    public void Close()
    {
        WorkingDirectory = "";
        Data = null;
    }
}
