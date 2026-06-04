FROM mcr.microsoft.com/dotnet/sdk:10.0

SHELL ["/bin/bash", "-lc"]

WORKDIR /src
ENV DALAMUD_HOME=/dalamud

COPY VoiceDirector/ ./VoiceDirector/
COPY VoiceDirector.Tests/ ./VoiceDirector.Tests/
COPY scyt.repo.json ./scyt.repo.json

CMD set -euo pipefail \
    && dotnet restore VoiceDirector.Tests/VoiceDirectorV2.Tests.csproj \
    && dotnet test VoiceDirector.Tests/VoiceDirectorV2.Tests.csproj --no-restore --configuration Release \
    && if [[ ! -d "${DALAMUD_HOME}" ]]; then \
           echo "Tests passed, but plugin build requires a mounted DALAMUD_HOME at ${DALAMUD_HOME}."; \
           echo "Mount a Dalamud dev folder into /dalamud and rerun the container."; \
           exit 2; \
       fi \
    && dotnet restore VoiceDirector/VoiceDirectorV2.csproj -p:EnableWindowsTargeting=true \
    && dotnet build VoiceDirector/VoiceDirectorV2.csproj --no-restore --configuration Release -p:EnableWindowsTargeting=true -o /tmp/plugin-build \
    && if [[ -d /out && -w /out ]]; then \
           rm -rf /out/plugin && mkdir -p /out/plugin && cp -R /tmp/plugin-build/. /out/plugin/; \
           echo "Validation succeeded. Exported build output to /out/plugin."; \
       else \
           echo "Validation succeeded. No artifact export requested."; \
       fi
