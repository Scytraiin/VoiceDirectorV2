# Voice Director

Workspace for the `Voice Director` Dalamud plugin.

The repo contains the plugin, unit tests, local Docker validation, release notes, and custom repo metadata.

## Projects

- `VoiceDirector/` - the Dalamud plugin.
- `VoiceDirector.Tests/` - unit tests for voice-selection logic and repository metadata.
- `scyt.repo.json` - custom Dalamud repository metadata.
- `release-notes/` - release notes used for GitHub releases.

## Plugin

`Voice Director` changes the `CutsceneMovieVoice` game config value when you enter configured duties and restores your default language elsewhere.

Open it in game with:

```text
/vodir
```

Open settings with:

```text
/vodir config
```

## Local Validation

The Docker workflow keeps local validation reproducible without requiring a global .NET install on the host.

Run from the workspace root:

```bash
docker build -t voice-director-ci .
docker run --rm voice-director-ci
```

To build the actual plugin package, mount a valid Dalamud dev folder. The `FFYIV/15.0.0.2/dev` bundle in this repo can be used as the reference runtime:

```bash
docker run --rm \
  -v "$PWD/FFYIV/15.0.0.2/dev:/dalamud:ro" \
  -v "$PWD/out:/out" \
  voice-director-ci
```

Build output is exported to:

```text
out/plugin/
```

Release-ready artifacts are exported to:

```text
out/release/
```

## Release

Current release target:

- the current release target is `v1.6.1`
- package: `out/release/latest.zip`
- repo metadata: `out/release/scyt.repo.json`

Manual release flow:

1. Update the plugin version and release notes.
2. Run `python3 scripts/prepare_release.py --workspace "$PWD" --version "<version>"`.
3. Smoke test in game.
4. Create the GitHub release tag.
5. Upload `latest.zip`.
6. Publish the updated `scyt.repo.json`.

## Status

- Dalamud API target: 15.
- Current plugin version: `1.6.1`.
- Runtime build should be validated against a Dalamud 15 dev bundle such as `FFYIV/15.0.0.2/dev`.

## Preview

![UI preview](https://raw.githubusercontent.com/noevain/VoiceDirector/master/images/preview1.png)

This repository is forked from the original Voice Director repository because the original project is no longer maintained for recent FFXIV / Dalamud versions.
