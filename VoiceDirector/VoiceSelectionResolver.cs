using System.Collections.Generic;

namespace VoiceDirector;

public static class VoiceSelectionResolver
{
    public static CutsceneMovieVoiceValue? ResolveDesiredVoice(
        IReadOnlyDictionary<ushort, CutsceneMovieVoiceValue> replacements,
        CutsceneMovieVoiceValue? temporaryOverride,
        ushort? contentId,
        CutsceneMovieVoiceValue defaultLanguage,
        ushort currentVoice)
    {
        if (temporaryOverride is { } temporaryVoice)
        {
            return currentVoice != (ushort)temporaryVoice ? temporaryVoice : null;
        }

        if (contentId is ushort resolvedContentId && replacements.TryGetValue(resolvedContentId, out var replacement))
        {
            return replacement;
        }

        return currentVoice != (ushort)defaultLanguage ? defaultLanguage : null;
    }
}
