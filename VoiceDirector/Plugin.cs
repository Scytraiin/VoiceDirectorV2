using System;

using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using Lumina.Excel.Sheets;

using VoiceDirector.Windows;

namespace VoiceDirector;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/vodir";
    private const string CutsceneMovieVoiceKey = "CutsceneMovieVoice";

    private readonly IDalamudPluginInterface pluginInterface;
    private readonly ICommandManager commandManager;
    private readonly IClientState clientState;
    private readonly IDataManager dataManager;
    private readonly IGameConfig gameConfig;
    private readonly IPluginLog log;
    private readonly WindowSystem windowSystem = new("VoiceDirector");
    private readonly ConfigWindow configWindow;
    private readonly MainWindow mainWindow;
    private readonly Configuration configuration;

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        IDataManager dataManager,
        IGameConfig gameConfig,
        IPluginLog log)
    {
        this.pluginInterface = pluginInterface;
        this.commandManager = commandManager;
        this.clientState = clientState;
        this.dataManager = dataManager;
        this.gameConfig = gameConfig;
        this.log = log;

        this.configuration = this.pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.configuration.Initialize(this.pluginInterface);

        this.configWindow = new ConfigWindow(this.configuration, this.dataManager, this.log);
        this.mainWindow = new MainWindow();

        this.windowSystem.AddWindow(this.configWindow);
        this.windowSystem.AddWindow(this.mainWindow);

        this.commandManager.AddHandler(CommandName, new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Open the Voice Director window. Use '/vodir config' for settings.",
        });

        this.pluginInterface.UiBuilder.Draw += this.DrawUi;
        this.pluginInterface.UiBuilder.OpenMainUi += this.OpenMainUi;
        this.pluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;
        this.clientState.TerritoryChanged += this.OnTerritoryChanged;

        this.log.Information("Voice Director initialized.");
    }

    public void Dispose()
    {
        this.clientState.TerritoryChanged -= this.OnTerritoryChanged;
        this.pluginInterface.UiBuilder.Draw -= this.DrawUi;
        this.pluginInterface.UiBuilder.OpenMainUi -= this.OpenMainUi;
        this.pluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUi;

        this.commandManager.RemoveHandler(CommandName);

        this.windowSystem.RemoveAllWindows();
        this.configWindow.Dispose();
        this.mainWindow.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        if (string.Equals(args.Trim(), "config", StringComparison.OrdinalIgnoreCase))
        {
            this.OpenConfigUi();
            return;
        }

        this.OpenMainUi();
    }

    private void OnTerritoryChanged(uint territoryType)
    {
        try
        {
            this.ApplyConfiguredVoice(this.ResolveContentId(territoryType));
        }
        catch (Exception ex)
        {
            this.log.Error(ex, "Failed to update cutscene voice for territory change {TerritoryType}.", territoryType);
        }
    }

    private ushort? ResolveContentId(uint territoryType)
    {
        if (territoryType == 0)
        {
            return null;
        }

        if (!this.dataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryType, out var territory))
        {
            this.log.Warning("Could not resolve TerritoryType row {TerritoryType}.", territoryType);
            return null;
        }

        var contentId = territory.ContentFinderCondition.ValueNullable?.Content.RowId;
        if (contentId is null or 0 || contentId > ushort.MaxValue)
        {
            return null;
        }

        return (ushort)contentId.Value;
    }

    private void ApplyConfiguredVoice(ushort? contentId)
    {
        var currentVoice = (ushort)this.gameConfig.System.GetUInt(CutsceneMovieVoiceKey);
        var desiredVoice = VoiceSelectionResolver.ResolveDesiredVoice(
            this.configuration.replacements,
            contentId,
            this.configuration.defaultLanguage,
            currentVoice);

        if (desiredVoice is null)
        {
            return;
        }

        this.gameConfig.System.Set(CutsceneMovieVoiceKey, (uint)desiredVoice.Value);
        this.log.Debug(
            "Set {ConfigKey} to {Voice} for content {ContentId}.",
            CutsceneMovieVoiceKey,
            desiredVoice.Value,
            contentId?.ToString() ?? "default");
    }

    private void DrawUi()
    {
        try
        {
            this.windowSystem.Draw();
        }
        catch (Exception ex)
        {
            this.mainWindow.IsOpen = false;
            this.configWindow.IsOpen = false;
            this.log.Error(ex, "Unhandled UI exception in Voice Director. Closed plugin windows to protect the host.");
        }
    }

    private void OpenMainUi()
    {
        this.mainWindow.IsOpen = true;
    }

    private void OpenConfigUi()
    {
        this.configWindow.IsOpen = true;
    }
}
