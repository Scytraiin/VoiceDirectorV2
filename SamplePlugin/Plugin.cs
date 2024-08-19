using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using Dalamud.Game.Config;
using Lumina.Excel.GeneratedSheets;
using Maps = Lumina.Excel.GeneratedSheets.Map;
using System;
using System.Linq;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState clientState { get; private set; } = null!;

    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IGameConfig GameConfig {  get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;
    //leaving this somewhere for now
    //CutsceneMovieVoice possible values
    // 0 = JP
    // 1 = EN
    // 2 = GER
    // 3 = FR
    // 42944967295 = Adjust to client,could just be junk/not set if adjust to client is set

    //Test
    //map id:d2fa/00
    //map name: Thok ast Thok


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

        clientState.TerritoryChanged += OnZoneChange;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        clientState.TerritoryChanged -= OnZoneChange;
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
                Logger.Error(e.ToString());
            }

        }else if (command == "/checkcurrlocation"){
            string currId = clientState.MapId.ToString();
            
            var currMap = DataManager.GetExcelSheet<Maps>()!.GetRow(clientState.MapId);
            if (currMap != null)
            {
                try
                {
                    Logger.Debug("Current map id:{0}", currMap.Id.RawString);
                    Logger.Debug("Current map name:{0}", currMap.PlaceName.Value.Name.ToString());
                } catch (Exception e)
                {
                    Logger.Error("Tried to get name or id of map but got exception:{0}",e.ToString());
                }
            }
            else
            {
                Logger.Debug("could not find mapId in database,id searched:{0}", currId);
            }
        }
        
    }

    private void OnZoneChange(ushort e)
    {
        Logger.Debug("Zone changed");

        var currMap = DataManager.GetExcelSheet<Maps>()!.GetRow(clientState.MapId);
        if (currMap != null && currMap.Id == "d2fa/00")
        {
            try
            {
                Logger.Debug("Current map id:{0}", currMap.Id.RawString);
                Logger.Debug("Current map name:{0}", currMap.PlaceName.Value.Name.ToString());
                Logger.Debug("Map is Thok ast Thok yay");
                Logger.Debug("Attempting config change,cutscene voice to EN");
                GameConfig.System.Set("CutsceneMovieVoice", 1);
            }
            catch (Exception ee)
            {
                Logger.Error("Tried to change config but exception:{0}", ee.ToString());
            }
        }
        else
        {
            Logger.Debug("Map is not Thok ast Thok,revert to JP");
            GameConfig.System.Set("CutsceneMovieVoice", 0);
        }
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
