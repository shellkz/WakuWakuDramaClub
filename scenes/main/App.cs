using Godot;
using Godot.Collections;
using System;

public partial class App : Panel
{
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
    CreateProjectPopup createProjectPopup;
    
    [Export]
    OpenProjectPopup openProjectPopup;
    

    
    public override void _Ready()
    {
        projectMenu.IdPressed += OnProjectMenuSelected;
        ProjectSession.Instance.WhenProjectLoaded(OnProjectLoaded);
        UpdateProjectState();

        if (!string.IsNullOrWhiteSpace(DebugProjectDirectory))
        {
            TryOpenProject(DebugProjectDirectory);
        }
    }

    private void OnProjectLoaded()
    {
        GetTree().Root.Title = $"WakuWaku Drama Club - {ProjectSession.Instance.Data.ProjectName}";
        UpdateProjectState();
    }

    private void UpdateProjectState()
    {
        var hasProject = ProjectSession.Instance.Data != null;
        tabBar.Visible = hasProject;
        editingMenu.Visible = hasProject;
        hintLabel.Visible = !hasProject;
        projectMenu.SetItemDisabled(2, !hasProject);
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
