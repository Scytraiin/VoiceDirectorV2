using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using Dalamud.Game.Config;
using System;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState clientState { get; private set; } = null!;
    [PluginService] internal static IGameConfig GameConfig {  get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;
    //leaving this somewhere for now
    //CutsceneMovieVoice possible values
    // 0 = JP
    // 1 = EN
    // 2 = GER
    // 3 = FR
    // 42944967295 = Adjust to client,could just be junk/not set if adjust to client is set




    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler("/checkcurrvoice", new CommandInfo(OnCommand)
        {
            HelpMessage = "Check what value is Cutscene Audio right now"
        });
        CommandManager.AddHandler("/checkcurrlocation", new CommandInfo(OnCommand)
        {
            HelpMessage = "Check where the player is right now"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler("/checkcurrvoice");
        CommandManager.RemoveHandler("/checkcurrlocation");
    }

    private void OnCommand(string command, string args)
    {

        if (command == "/checkcurrvoice")
        {
            try
            {
                uint csMovieVoice = 0;
                GameConfig.System.TryGetUInt("CutsceneMovieVoice", out csMovieVoice);
                Logger.Debug("Current voice value:{0}", csMovieVoice);
            }
            catch (Exception e)
            {
                Logger.Debug(e.ToString());
            }

        }else if (command == "/checkcurrlocation"){
            Logger.Debug(clientState.MapId.ToString());
        }
        
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
