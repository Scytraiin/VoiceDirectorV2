using System;
using System.Collections.Generic;

using Dalamud.Configuration;
using Dalamud.Plugin;

namespace VoiceDirector;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    private IDalamudPluginInterface? pluginInterface;

    public int Version { get; set; } = 1;

    public CutsceneMovieVoiceValue defaultLanguage { get; set; } = CutsceneMovieVoiceValue.English;

    public Dictionary<ushort, CutsceneMovieVoiceValue> replacements { get; set; } = [];

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        this.pluginInterface?.SavePluginConfig(this);
    }
}
