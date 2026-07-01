using Godot;
using Godot.Collections;
using System;
using System.IO;
using System.Threading.Tasks;
using WakuWakuDramaClub.Completion;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Scripting;
using WakuWakuDramaClub.Scripting.Parsing;
using WakuWakuDramaClub.Timline;

public partial class App : Panel
{
    private enum MainTab
    {
        Scripting = 0,
        Rendering = 1
    }

    [Export(PropertyHint.GlobalDir)]
    public string DebugProjectDirectory = "";

    [Export]
    PopupMenu projectMenu;

    [Export]
    TabBar tabBar;

    [Export]
    RichTextLabel hintLabel;
    [Export]
    EditingMenu editingMenu;
    [Export]
    RenderingMenu renderingMenu;

    [Export]
    CreateProjectPopup createProjectPopup;
    
    [Export]
    OpenProjectPopup openProjectPopup;
    
    [Export]
    Stage stage;

    [Export]
    VideoRenderer videoRenderer;





    private MainWorkspaceServices workspaceServices;
    
    public override void _Ready()
    {
        RebuildWorkspaceServices();

        tabBar.TabChanged += OnTabChanged;
        projectMenu.IdPressed += OnProjectMenuSelected;
        ProjectSession.Instance.WhenProjectLoaded(OnProjectLoaded);
        UpdateProjectState();

        if (!string.IsNullOrWhiteSpace(DebugProjectDirectory))
        {
            TryOpenProject(DebugProjectDirectory);
        }
    }

    private void CreateWorkspaceServices()
    {
        InstructionRegistry instructionRegistry = InstructionRegistry.CreateDefault();
        ScriptPreprocessor scriptPreprocessor = new ScriptPreprocessor(instructionRegistry);
        CompletionAnalyzer completionAnalyzer = new CompletionAnalyzer(instructionRegistry, scriptPreprocessor, ResourceManager.Instance);
        CompletionProvider completionProvider = new CompletionProvider(instructionRegistry, ResourceManager.Instance);

        workspaceServices = new MainWorkspaceServices
        {
            InstructionRegistry = instructionRegistry,
            ScriptPreprocessor = scriptPreprocessor,
            CompletionAnalyzer = completionAnalyzer,
            CompletionProvider = completionProvider,
            Stage = stage,
            VideoRenderer = videoRenderer
        };
    }

    private void RebuildWorkspaceServices()
    {
        CreateWorkspaceServices();
        editingMenu.Initialize(CreateEditingMenuServices());
        renderingMenu.Initialize(CreateRenderingMenuServices());
    }

    private EditingMenuServices CreateEditingMenuServices()
    {
        return new EditingMenuServices
        {
            CompletionAnalyzer = workspaceServices.CompletionAnalyzer,
            CompletionProvider = workspaceServices.CompletionProvider,
            Stage = workspaceServices.Stage,
            BuildTimeline = BuildTimelineFromScript
        };
    }

    private RenderingMenuServices CreateRenderingMenuServices()
    {
        return new RenderingMenuServices
        {
            VideoRenderer = workspaceServices.VideoRenderer,
            BuildTimeline = BuildTimelineFromCurrentScript,
            ExportDirectory = GetExportDirectory()
        };
    }

    private string GetExportDirectory()
    {
        if (ProjectSession.Instance.Data == null)
            return "";

        return Path.Combine(ProjectSession.Instance.WorkingDirectory, "exports");
    }

    private Task<Timeline> BuildTimelineFromCurrentScript()
    {
        return BuildTimelineFromScript(editingMenu.GetScriptText());
    }

    private async Task<Timeline> BuildTimelineFromScript(string scriptText)
    {
        ScriptParser parser = new ScriptParser(workspaceServices.InstructionRegistry, workspaceServices.ScriptPreprocessor);
        var instructions = parser.Parse(scriptText);

        workspaceServices.Stage.ClearActors();

        return await workspaceServices.Stage.BuildTimeline(instructions);
    }

    private void OnProjectLoaded()
    {
        RebuildWorkspaceServices();
        GetTree().Root.Title = $"WakuWaku Drama Club - {ProjectSession.Instance.Data.ProjectName}";
        UpdateProjectState();
    }

    private void UpdateProjectState()
    {
        var hasProject = ProjectSession.Instance.Data != null;
        tabBar.Visible = hasProject;
        UpdateActiveMenu(hasProject);
        hintLabel.Visible = !hasProject;
        projectMenu.SetItemDisabled(2, !hasProject);
    }

    private void OnTabChanged(long tabIndex)
    {
        UpdateProjectState();
    }

    private void UpdateActiveMenu(bool hasProject)
    {
        editingMenu.Visible = hasProject && tabBar.CurrentTab == (int)MainTab.Scripting;
        renderingMenu.Visible = hasProject && tabBar.CurrentTab == (int)MainTab.Rendering;
    }

    private void OnProjectMenuSelected(long id)
    {
        switch (id)
        {
            case 0:
                NewProject();
                break;
            case 1:
                OpenProject();
                break;
            case 2:
                SaveProject();
                break;
        }
    }

    private void SaveProject()
    {
        if (ProjectSession.Instance.Data == null) return;
        ProjectSession.Instance.Save();
    }

    private void OpenProject()
    {
        openProjectPopup.Open(OnTryOpenProject);
    }

    private void OnTryOpenProject(Dictionary data)
    {
        TryOpenProject((string)data["path"]);
    }

    private void TryOpenProject(string path)
    {
        try
        {
            ProjectSession.Instance.Load(path);
        }
        catch (InvalidOperationException e)
        {
            GD.PrintErr(e.Message);
        }
    }


    private void NewProject()
    {
        GD.Print("New");
        createProjectPopup.Open(OnTryCreateProject);
    }

    private void OnTryCreateProject(Dictionary data)
    {
        try
        {
            ProjectSession.Instance.CreateProject((string)data["name"], (string)data["path"]);
        }
        catch (InvalidOperationException e)
        {
            GD.PrintErr(e.Message);
        }
    }

}
