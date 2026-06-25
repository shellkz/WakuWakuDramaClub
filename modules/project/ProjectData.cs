using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ProjectData
{
    [JsonPropertyName("project_name")]
    public string ProjectName { get; set; } = "";

    [JsonPropertyName("last_opened_datetime")]
    public DateTime LastOpenedDatetime { get; set; }

    public static ProjectData FromJson(string json) =>
        JsonSerializer.Deserialize<ProjectData>(json) ?? new ProjectData();

    public string ToJson() =>
        JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
}
