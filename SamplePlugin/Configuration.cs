using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace SamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

    public CutsceneMovieVoiceValue defaultLanguage { get; set; } = CutsceneMovieVoiceValue.English;

    public string previewSelectedMapName { get; set; } = "Location";
    public CutsceneMovieVoiceValue previewSelectedLanguage { get; set; } = CutsceneMovieVoiceValue.English;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
