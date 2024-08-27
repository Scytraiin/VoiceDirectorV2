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
using Dalamud.Utility;
using Dalamud.Interface.Utility;
using Lumina.Excel;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private string map_id_sel = string.Empty;
    private CutsceneMovieVoiceValue language_sel = CutsceneMovieVoiceValue.English;
    public string _filter = string.Empty;
    public ExcelSheet<Maps> mappies = Plugin.DataManager.GetExcelSheet<Maps>();
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

    private void DrawSelectableInternal(Maps map)
    {
        using var id = ImRaii.PushId(map.Id);
        var name = map.PlaceName.Value.Name.ToString();
        if(ImGui.Selectable(name, false))
        {
            Plugin.Logger.Debug("Selected map:{0}",map.PlaceName.Value.Name.ToString());
        }
    }
    public override void Draw()
    {
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
        using var combo = ImRaii.Combo("Search locations",Configuration.previewSelectedMapName);
        Func<Maps, bool> selector = map => map.PlaceName.Value.Name.ToString().Contains(_filter);
        if (combo)
        {
            if (ImGui.InputTextWithHint("##filter", "Filter...", ref _filter, 30))
            {
                
            }

            ImRaii.Child("ChildL");
            ImGuiClip.FilteredClippedDraw(mappies, 0, selector, DrawSelectableInternal);
        }
        /*
        if (ImGui.BeginCombo("Location picker",Configuration.previewSelectedMapName))
        {
            byte[] searchTerm = [];
            if(ImGui.InputText("Search locations",searchTerm,30)){
                Plugin.Logger.Debug("search term:{0}",searchTerm.ToString());
            }
            foreach (var item in mappies)
            {
                var resolvedName = item.PlaceNameSub.Value.Name.ToString().IsNullOrEmpty() ? item.PlaceName.Value.Name.ToString() : item.PlaceName.Value.Name.ToString() + "||" + item.PlaceNameSub.Value.Name.ToString();
                if (ImGui.Selectable(resolvedName, item.Id == map_id_sel)){
                    Plugin.Logger.Debug("selected:" + item.PlaceName.Value.Name.ToString());
                    map_id_sel = item.Id;
                    Configuration.previewSelectedMapName = item.PlaceName.Value.Name.ToString() + "||" + item.PlaceNameSub.Value.Name.ToString();
                }
            }
            ImGui.EndCombo();
        }
        */
        
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
        ImGui.Separator();
        ImGui.Text("List of current changes");
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
