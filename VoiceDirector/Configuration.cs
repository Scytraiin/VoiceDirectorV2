using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace VoiceDirector;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;


    public CutsceneMovieVoiceValue defaultLanguage { get; set; } = CutsceneMovieVoiceValue.English;

    public Dictionary<ushort, CutsceneMovieVoiceValue> replacements = new Dictionary<ushort, CutsceneMovieVoiceValue>();

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
