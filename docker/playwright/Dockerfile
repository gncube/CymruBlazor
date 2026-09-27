FROM mcr.microsoft.com/playwright/dotnet:v1.62.0-noble

WORKDIR /workspace

# Install PowerShell for cross-platform automation execution
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        curl \
        ca-certificates && \
    curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /etc/apt/trusted.gpg.d/microsoft.gpg && \
    echo "deb [arch=amd64,arm64,armhf] https://packages.microsoft.com/repos/microsoft-ubuntu-noble-prod noble main" > /etc/apt/sources.list.d/powershell.list && \
    apt-get update && \
    apt-get install -y --no-install-recommends powershell && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["pwsh"]
