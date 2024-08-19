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

    public override void Draw()
    {
        var mappies = Plugin.DataManager.GetExcelSheet<Maps>();
        uint i = 0;
        foreach (var item in mappies)
        {
            i++;
            ImGui.Text(item.PlaceName.Value.Name.ToString() + item.Id);
            if (i > 10)
            {
                break;
            }
        }
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


        if (ImGui.BeginTable("fuckinghell", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            ImGui.TableNextColumn();
            ImGui.Text("Maps");
            ImGui.TableNextColumn();
            ImGui.Text("Voice");
            ImGui.TableNextColumn();

        }
        ImGui.EndTable();
    }
}
