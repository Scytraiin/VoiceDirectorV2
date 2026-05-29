using Xunit;

namespace VoiceDirector.Tests;

public sealed class VoiceSelectionResolverTests
{
    [Fact]
    public void ResolveDesiredVoice_ReturnsConfiguredOverride_WhenDutyMatchExists()
    {
        var replacements = new Dictionary<ushort, CutsceneMovieVoiceValue>
        {
            [77] = CutsceneMovieVoiceValue.German,
        };

        var resolved = VoiceSelectionResolver.ResolveDesiredVoice(
            replacements,
            77,
            CutsceneMovieVoiceValue.English,
            (ushort)CutsceneMovieVoiceValue.English);

        Assert.Equal(CutsceneMovieVoiceValue.German, resolved);
    }

    [Fact]
    public void ResolveDesiredVoice_ReturnsDefaultLanguage_WhenNoOverrideExistsAndCurrentVoiceDiffers()
    {
        var resolved = VoiceSelectionResolver.ResolveDesiredVoice(
            new Dictionary<ushort, CutsceneMovieVoiceValue>(),
            88,
            CutsceneMovieVoiceValue.English,
            (ushort)CutsceneMovieVoiceValue.French);

        Assert.Equal(CutsceneMovieVoiceValue.English, resolved);
    }

    [Fact]
    public void ResolveDesiredVoice_ReturnsNull_WhenAlreadyOnDefaultAndNoOverrideExists()
    {
        var resolved = VoiceSelectionResolver.ResolveDesiredVoice(
            new Dictionary<ushort, CutsceneMovieVoiceValue>(),
            null,
            CutsceneMovieVoiceValue.English,
            (ushort)CutsceneMovieVoiceValue.English);

        Assert.Null(resolved);
    }
}
