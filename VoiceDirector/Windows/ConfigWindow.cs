using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace VoiceDirector.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly IPluginLog log;
    private readonly ExcelSheet<ContentFinderCondition> contents;
    private readonly IReadOnlyList<ContentFinderCondition> selectableDuties;
    private readonly IReadOnlyDictionary<ushort, string> dutyNamesByContentId;
    private CutsceneMovieVoiceValue selectedLanguage = CutsceneMovieVoiceValue.English;
    private string filter = string.Empty;
    private uint selectedContentFinderConditionId;
    private string? statusMessage;
    private bool statusIsError;

    public ConfigWindow(Configuration configuration, IDataManager dataManager, IPluginLog log)
        : base("Voice Director Config###VoiceDirectorConfig")
    {
        this.configuration = configuration;
        this.log = log;
        this.contents = dataManager.GetExcelSheet<ContentFinderCondition>();
        this.selectableDuties = this.contents
            .Where(content => content.Content.RowId > 0)
            .Where(content => !string.IsNullOrWhiteSpace(content.Name.ToString()))
            .OrderBy(content => content.Name.ToString(), StringComparer.OrdinalIgnoreCase)
            .ToList();
        this.dutyNamesByContentId = BuildDutyNameMap(this.selectableDuties);

        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(520, 420),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public void Dispose()
    {
    }

    public static string GetNameFromEnum(CutsceneMovieVoiceValue value) =>
        value switch
        {
            CutsceneMovieVoiceValue.Japanese => "Japanese",
            CutsceneMovieVoiceValue.English => "English",
            CutsceneMovieVoiceValue.German => "German",
            CutsceneMovieVoiceValue.French => "French",
            _ => $"Unknown ({(ushort)value})",
        };

    public override void Draw()
    {
        this.DrawDefaultLanguagePicker();
        ImGui.Separator();
        this.DrawDutyPicker();
        this.DrawVoicePicker();
        this.DrawSaveButton();
        this.DrawStatusMessage();
        ImGui.Separator();
        this.DrawReplacementTable();
    }

    private static IReadOnlyDictionary<ushort, string> BuildDutyNameMap(IEnumerable<ContentFinderCondition> duties)
    {
        var names = new Dictionary<ushort, string>();
        foreach (var duty in duties)
        {
            var contentId = duty.Content.RowId;
            if (contentId is 0 or > ushort.MaxValue)
            {
                continue;
            }

            names.TryAdd((ushort)contentId, duty.Name.ToString());
        }

        return names;
    }

    private void DrawDefaultLanguagePicker()
    {
        if (!ImGui.BeginCombo("Default Language", GetNameFromEnum(this.configuration.defaultLanguage)))
        {
            return;
        }

        foreach (CutsceneMovieVoiceValue voice in Enum.GetValues(typeof(CutsceneMovieVoiceValue)))
        {
            if (!ImGui.Selectable(GetNameFromEnum(voice), this.configuration.defaultLanguage == voice))
            {
                continue;
            }

            this.configuration.defaultLanguage = voice;
            this.configuration.Save();
            this.log.Debug("Updated default language to {Language}.", voice);
        }

        ImGui.EndCombo();
    }

    private void DrawDutyPicker()
    {
        if (!ImGui.BeginCombo("Duty Picker", this.GetSelectedDutyName(), ImGuiComboFlags.HeightLarge))
        {
            return;
        }

        ImGui.InputTextWithHint("##ContentSearchFilter", "Search duties...", ref this.filter, 300);
        ImGui.Separator();

        var filteredDuties = this.selectableDuties
            .Where(content => content.Name.ToString().Contains(this.filter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (filteredDuties.Count == 0)
        {
            ImGui.TextUnformatted("No matches found.");
        }
        else
        {
            foreach (var duty in filteredDuties)
            {
                if (ImGui.Selectable(duty.Name.ToString(), duty.RowId == this.selectedContentFinderConditionId))
                {
                    this.selectedContentFinderConditionId = duty.RowId;
                }
            }
        }

        ImGui.EndCombo();
    }

    private void DrawVoicePicker()
    {
        if (!ImGui.BeginCombo("Language Picker", GetNameFromEnum(this.selectedLanguage)))
        {
            return;
        }

        foreach (CutsceneMovieVoiceValue voice in Enum.GetValues(typeof(CutsceneMovieVoiceValue)))
        {
            if (ImGui.Selectable(GetNameFromEnum(voice), voice == this.selectedLanguage))
            {
                this.selectedLanguage = voice;
            }
        }

        ImGui.EndCombo();
    }

    private void DrawSaveButton()
    {
        if (!ImGui.Button("Save Duty Override"))
        {
            return;
        }

        if (this.selectedContentFinderConditionId == 0)
        {
            this.SetStatus("Select a duty before saving a voice override.", true);
            return;
        }

        if (!this.contents.TryGetRow(this.selectedContentFinderConditionId, out var duty))
        {
            this.SetStatus("Could not resolve the selected duty row.", true);
            return;
        }

        var contentId = duty.Content.RowId;
        if (contentId is 0 or > ushort.MaxValue)
        {
            this.SetStatus("The selected duty does not expose a valid content identifier.", true);
            return;
        }

        var replacementKey = (ushort)contentId;
        var updatedExisting = this.configuration.replacements.ContainsKey(replacementKey);
        this.configuration.replacements[replacementKey] = this.selectedLanguage;
        this.configuration.Save();

        var action = updatedExisting ? "Updated" : "Saved";
        this.SetStatus($"{action} override for {duty.Name} -> {GetNameFromEnum(this.selectedLanguage)}.", false);
        this.log.Debug(
            "{Action} replacement for content id {ContentId} with language {Language}.",
            action,
            replacementKey,
            this.selectedLanguage);
    }

    private void DrawStatusMessage()
    {
        if (string.IsNullOrWhiteSpace(this.statusMessage))
        {
            return;
        }

        if (this.statusIsError)
        {
            ImGui.TextColored(new Vector4(0.9f, 0.3f, 0.3f, 1f), this.statusMessage);
            return;
        }

        ImGui.TextColored(new Vector4(0.4f, 0.85f, 0.45f, 1f), this.statusMessage);
    }

    private void DrawReplacementTable()
    {
        ImGui.TextUnformatted("Current Duty Overrides");

        if (!ImGui.BeginTable("changeTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg))
        {
            return;
        }

        ImGui.TableSetupColumn("Duty");
        ImGui.TableSetupColumn("Voice");
        ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed, 80f);
        ImGui.TableHeadersRow();

        foreach (var entry in this.configuration.replacements.OrderBy(entry => this.ResolveDutyName(entry.Key), StringComparer.OrdinalIgnoreCase))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(this.ResolveDutyName(entry.Key));

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(GetNameFromEnum(entry.Value));

            ImGui.TableNextColumn();
            ImGui.PushID((int)entry.Key);
            if (ImGui.SmallButton("Remove"))
            {
                this.configuration.replacements.Remove(entry.Key);
                this.configuration.Save();
                this.SetStatus($"Removed override for {this.ResolveDutyName(entry.Key)}.", false);
                this.log.Debug("Removed replacement for content id {ContentId}.", entry.Key);
            }

            ImGui.PopID();
        }

        ImGui.EndTable();
    }

    private string GetSelectedDutyName()
    {
        if (this.selectedContentFinderConditionId == 0)
        {
            return "Duty name";
        }

        if (!this.contents.TryGetRow(this.selectedContentFinderConditionId, out var duty))
        {
            return "Duty name";
        }

        return duty.Name.ToString();
    }

    private string ResolveDutyName(ushort contentId)
    {
        if (this.dutyNamesByContentId.TryGetValue(contentId, out var dutyName))
        {
            return dutyName;
        }

        return $"Unknown or removed duty ({contentId})";
    }

    private void SetStatus(string message, bool isError)
    {
        this.statusMessage = message;
        this.statusIsError = isError;
    }
}
