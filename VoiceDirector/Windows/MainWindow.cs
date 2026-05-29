using System;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace VoiceDirector.Windows;

public sealed class MainWindow : Window, IDisposable
{
    public MainWindow()
        : base("Voice Director##MainWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(420, 180),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        ImGui.TextWrapped("Voice Director switches the cutscene voice language per duty.");
        ImGui.Spacing();
        ImGui.BulletText("Open settings with the gear button or run /vodir config.");
        ImGui.BulletText("Set a default voice language used whenever no duty override exists.");
        ImGui.BulletText("Add duty-specific overrides for raids, dungeons, or trials.");
    }
}
