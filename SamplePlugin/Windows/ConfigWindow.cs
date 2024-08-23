using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using Maps = Lumina.Excel.GeneratedSheets.Map;
using Dalamud.IoC;
using Dalamud.Plugin.Services;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
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

    private string GetNameFromEnum(CutsceneMovieVoiceValue csValue)
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
        if (ImGui.BeginCombo("picker", "location"))
        {
            foreach (var item in mappies)
            {
                bool is_sel = false;
                string map_sel = item.Id;
                if (ImGui.Selectable(item.PlaceName.Value.Name.ToString(),is_sel)){
                    Plugin.Logger.Debug("selected:" + item.PlaceName.Value.Name.ToString());
                    is_sel = true;
                    map_sel = item.Id;
                }
            }
        }
        ImGui.EndCombo();
        if (ImGui.BeginCombo("Language picker", "language"))
        {
            foreach (CutsceneMovieVoiceValue csVoice in Enum.GetValues(typeof(CutsceneMovieVoiceValue)))
            {
                bool is_sel = false;
                CutsceneMovieVoiceValue language_sel = CutsceneMovieVoiceValue.English;
                    if (ImGui.Selectable(GetNameFromEnum(csVoice), is_sel))
                    {
                        Plugin.Logger.Debug("selected:" + GetNameFromEnum(csVoice));
                        is_sel = true;
                        language_sel = csVoice;
                    }
            }
        }
        ImGui.EndCombo();
        if (ImGui.BeginTable("fuckinghell", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            ImGui.TableNextColumn();
            ImGui.Text("Maps");
            ImGui.TableNextColumn();
            ImGui.Text("Voice");
            foreach (var item in mappies)
            {
                ImGui.TableNextColumn();
                ImGui.Text(item.PlaceName.Value.Name.ToString() + "||Subsection:" + item.PlaceNameSub.Value.Name.ToString());
                ImGui.TableNextColumn();
                ImGui.Text("replaced language here");
            }
        }
            ImGui.EndTable();
            // can't ref a property, so use a local copy
            var configValue = Configuration.SomePropertyToBeSavedAndWithADefault;
            if (ImGui.Checkbox("Random Config Bool", ref configValue))
            {
                Configuration.SomePropertyToBeSavedAndWithADefault = configValue;
                // can save immediately on change, if you don't want to provide a "Save and Close" button
                Configuration.Save();
            }

            var movable = Configuration.IsConfigWindowMovable;
            if (ImGui.Checkbox("Movable Config Window", ref movable))
            {
                Configuration.IsConfigWindowMovable = movable;
                Configuration.Save();
            }


        }

    }
