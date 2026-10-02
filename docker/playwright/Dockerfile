# The image's Playwright version MUST equal the Microsoft.Playwright package
# version in Directory.Packages.props, or the tests fail at browser launch
# ("Executable doesn't exist ... please update docker image"). Don't edit the
# default by hand: scripts/Run-AccessibilityTests.ps1 and
# .github/workflows/accessibility.yml read the version from
# Directory.Packages.props and pass it in as a build argument.
ARG PLAYWRIGHT_VERSION=1.63.0
FROM mcr.microsoft.com/playwright/dotnet:v${PLAYWRIGHT_VERSION}-noble

WORKDIR /workspace

# Install PowerShell and ensure proper SSL certificate handling
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        curl \
        ca-certificates \
        openssl && \
    update-ca-certificates && \
    curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /etc/apt/trusted.gpg.d/microsoft.gpg && \
    echo "deb [arch=amd64,arm64,armhf] https://packages.microsoft.com/repos/microsoft-ubuntu-noble-prod noble main" > /etc/apt/sources.list.d/powershell.list && \
    apt-get update && \
    apt-get install -y --no-install-recommends powershell && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Configure .NET to handle SSL certificate chain validation
# Disable socket handler to use older HTTP implementation for NuGet compatibility
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
ENV NUGET_CERT_REVOCATION_MODE=offline

ENTRYPOINT ["pwsh"]
