using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using Maps = Lumina.Excel.GeneratedSheets.Map;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;
using Lumina.Excel.GeneratedSheets2;
using System.Collections.Generic;
using System.Linq;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private string map_id_sel = string.Empty;
    private CutsceneMovieVoiceValue language_sel = CutsceneMovieVoiceValue.English;
    private Random rand = new Random();
    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("A Wonderful Configuration Window###With a constant ID")
    {
       

        

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {

    }

    public static string GetNameFromEnum(CutsceneMovieVoiceValue csValue)
    {
        switch (csValue)
        {
            case CutsceneMovieVoiceValue.Japanese:return "Japanese";break;
            case CutsceneMovieVoiceValue.English:return "English";break;
            case CutsceneMovieVoiceValue.German:return "German";break;
            case CutsceneMovieVoiceValue.French:return "French";break;
            default:return "How did you do that";break;
        }
    }

    public override void Draw()
    {
        var mappies = Plugin.DataManager.GetExcelSheet<Maps>();
        if (ImGui.BeginCombo("Default Language", GetNameFromEnum(Configuration.defaultLanguage)))
        {
            foreach (CutsceneMovieVoiceValue csVoice in Enum.GetValues(typeof(CutsceneMovieVoiceValue)))
            {
                if (ImGui.Selectable(GetNameFromEnum(csVoice), Configuration.defaultLanguage == csVoice))
                {
                    Plugin.Logger.Debug("selected:" + GetNameFromEnum(csVoice));
                    Configuration.defaultLanguage = csVoice;
                    Configuration.Save();
                }
            }
            ImGui.EndCombo();

        }
        ImGui.Separator();
        if (ImGui.BeginCombo("Location picker",Configuration.previewSelectedMapName))
        {
            foreach (var item in mappies)
            {
                if (ImGui.Selectable(item.PlaceName.Value.Name.ToString() + "||" + item.PlaceNameSub.Value.Name.ToString(), item.Id == map_id_sel)){
                    Plugin.Logger.Debug("selected:" + item.PlaceName.Value.Name.ToString());
                    map_id_sel = item.Id;
                    Configuration.previewSelectedMapName = item.PlaceName.Value.Name.ToString() + "||" + item.PlaceNameSub.Value.Name.ToString();
                }
            }
            ImGui.EndCombo();
        }
        
        if (ImGui.BeginCombo("Language picker", GetNameFromEnum(Configuration.previewSelectedLanguage)))
        {
            foreach (CutsceneMovieVoiceValue csVoice in Enum.GetValues(typeof(CutsceneMovieVoiceValue)))
            {
                    if (ImGui.Selectable(GetNameFromEnum(csVoice), csVoice == Configuration.previewSelectedLanguage))
                    {
                        Plugin.Logger.Debug("selected:" + GetNameFromEnum(csVoice));
                        Configuration.previewSelectedLanguage = csVoice;
                    language_sel = csVoice;
                    }
            }
            ImGui.EndCombo();
        }
        if (ImGui.Button("Add changes"))
        {
            Dictionary<string, CutsceneMovieVoiceValue> rep = Configuration.replacements;
            rep.Add(map_id_sel,language_sel);
            Configuration.replacements = rep;
            Configuration.Save();
            Plugin.Logger.Debug("Added replacement for map id:{0} with language {1}", [map_id_sel,language_sel]);
        }
        if (ImGui.BeginTable("changetable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            ImGui.TableNextColumn();
            ImGui.Text("Maps");
            ImGui.TableNextColumn();
            ImGui.Text("Voice");
            foreach (KeyValuePair<string,CutsceneMovieVoiceValue> entry in Configuration.replacements)
            {
                ImGui.TableNextColumn();
                var item = mappies.Where(x => x.Id == entry.Key).First();
                ImGui.Text(item.PlaceName.Value.Name.ToString() + "||" + item.PlaceNameSub.Value.Name.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(GetNameFromEnum(entry.Value));
                ImGui.SameLine();
                ImGui.PushID(entry.Key);
                if (ImGui.SmallButton("Remove"))
                {
                    Dictionary<string, CutsceneMovieVoiceValue> rep = Configuration.replacements;
                    rep.Remove(entry.Key);
                    Configuration.replacements = rep;
                    Configuration.Save();
                    Plugin.Logger.Debug("Removed replacement for map id:{0} with language {1}", [item.Id, entry.Value]);
                }
                ImGui.PopID();
            }
            ImGui.EndTable();
        }


        }

    }
