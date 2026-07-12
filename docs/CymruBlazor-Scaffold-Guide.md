# CymruBlazor — Complete Scaffold Guide

> .NET 10 · C# 14 · NHS Wales Design System · Open Source Component Library

---

## Prerequisites

```bash
# Verify tooling
dotnet --version            # must be 10.x
git --version
node --version              # needed for nhsuk-frontend npm package
dotnet tool list -g         # check existing global tools
```

Install required global tools:

```bash
dotnet tool install --global GitVersion.Tool
dotnet tool install --global dotnet-outdated-tool
dotnet tool update --global dotnet-ef       # optional, future use
```

---

## Phase 1 — Repository Initialisation

### 1.1 Create the repository root

```bash
mkdir CymruBlazor
cd CymruBlazor

git init
git branch -M main
```

### 1.2 Add a root `.gitignore`

```bash
dotnet new gitignore
```

Append NHS / Blazor-specific ignores:

```bash
cat >> .gitignore << 'EOF'

# CymruBlazor specifics
**/wwwroot/css/cymrublazor.min.css
**/wwwroot/lib/
.nuke/
artifacts/
GitVersion.yml.bak
EOF
```

### 1.3 Add initial repo metadata files

````bash
cat > README.md << 'EOF'
# CymruBlazor

An open-source Blazor component library implementing the NHS Wales Design System.

[![NuGet](https://img.shields.io/nuget/v/CymruBlazor)](https://www.nuget.org/packages/CymruBlazor)
[![Build](https://github.com/YOUR_ORG/CymruBlazor/actions/workflows/ci.yml/badge.svg)](https://github.com/YOUR_ORG/CymruBlazor/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Getting Started

```shell
dotnet add package CymruBlazor
````

Add the stylesheet to your `App.razor` or `_Host.cshtml`:

```html
<link href="_content/CymruBlazor/css/cymrublazor.css" rel="stylesheet" />
```

See the [Demo application](src/CymruBlazor.Demo) and [documentation](docs/) for full usage.

## Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request.
EOF

````

```bash
cat > LICENSE << 'EOF'
MIT License

Copyright (c) 2025 CymruBlazor Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
EOF
````

```bash
cat > CONTRIBUTING.md << 'EOF'
# Contributing to CymruBlazor

All contributions are welcome. Please:

1. Fork and create a feature branch from `main`.
2. Follow the Conventional Commits specification for commit messages.
3. Ensure all tests pass: `dotnet test`.
4. Open a pull request with a clear description of changes.

## Commit message format

```

feat(Button): add secondary variant
fix(TextBox): correct focus ring colour
docs(Cards): update accessibility notes

```

Supported types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `ci`.
EOF
```

```bash
cat > CHANGELOG.md << 'EOF'
# Changelog

All notable changes are documented automatically via GitVersion and Conventional Commits.
See [GitHub Releases](https://github.com/YOUR_ORG/CymruBlazor/releases) for the full history.
EOF
```

### 1.4 Configure GitVersion (automated semantic versioning)

```bash
cat > GitVersion.yml << 'EOF'
mode: Mainline
branches:
  main:
    regex: ^main$
    tag: ""
    increment: Patch
    prevent-increment-of-merged-branch-version: true
  feature:
    regex: ^feature[/-]
    tag: alpha
    increment: Minor
  release:
    regex: ^release[/-]
    tag: beta
    increment: Patch
  hotfix:
    regex: ^hotfix[/-]
    tag: ""
    increment: Patch
commit-message-incrementing: Enabled
major-version-bump-message: "^(feat|fix|refactor|perf)(\\(.+\\))?!:"
minor-version-bump-message: "^feat(\\(.+\\))?:"
patch-version-bump-message: "^(fix|refactor|perf)(\\(.+\\))?:"
EOF
```

### 1.5 First commit

```bash
git add .
git commit -m "chore: initialise repository with metadata and versioning config"
```

---

## Phase 2 — Solution and Project Scaffold

### 2.1 Create the solution file

.NET 10 uses the new `.slnx` format (XML-based, cleaner diffs):

```bash
dotnet new sln --name CymruBlazor --format slnx
```

### 2.2 Create the directory structure

```bash
mkdir -p src tests samples docs plans .github/workflows .github/ISSUE_TEMPLATE
```

### 2.3 Scaffold `src/CymruBlazor` — the core Razor Class Library

```bash
dotnet new razorclasslib \
  --name CymruBlazor \
  --output src/CymruBlazor \
  --no-restore
```

Replace the generated csproj with production-ready metadata:

```bash
cat > src/CymruBlazor/CymruBlazor.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <!-- Target -->
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>preview</LangVersion>

    <!-- Assembly identity (GitVersion patches these at build time) -->
    <AssemblyVersion>0.0.0</AssemblyVersion>
    <FileVersion>0.0.0</FileVersion>
    <InformationalVersion>0.0.0</InformationalVersion>

    <!-- NuGet metadata -->
    <PackageId>CymruBlazor</PackageId>
    <Title>CymruBlazor</Title>
    <Description>
      An open-source Blazor component library implementing the NHS Wales Design System.
      Provides accessible, enterprise-grade components aligned with WCAG 2.2 AA.
    </Description>
    <Authors>CymruBlazor Contributors</Authors>
    <Company>Digital Health and Care Wales Community</Company>
    <Copyright>Copyright © 2025 CymruBlazor Contributors</Copyright>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/YOUR_ORG/CymruBlazor</PackageProjectUrl>
    <RepositoryUrl>https://github.com/YOUR_ORG/CymruBlazor</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageTags>blazor;nhs;nhswales;design-system;components;accessibility;wcag</PackageTags>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageIcon>icon.png</PackageIcon>

    <!-- Source link and symbols -->
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
    <EmbedUntrackedSources>true</EmbedUntrackedSources>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>

    <!-- XML docs -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\CymruBlazor.xml</DocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>

    <!-- Trimming / AOT -->
    <IsTrimmable>true</IsTrimmable>
    <EnableTrimAnalyzer>true</EnableTrimAnalyzer>

    <!-- Warnings as errors in CI -->
    <TreatWarningsAsErrors Condition="'$(CI)' == 'true'">true</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <None Include="..\..\README.md" Pack="true" PackagePath="\" />
    <None Include="icon.png" Pack="true" PackagePath="\" Condition="Exists('icon.png')" />
  </ItemGroup>

  <!-- SourceLink (GitHub) -->
  <ItemGroup>
    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="*" PrivateAssets="all" />
    <PackageReference Include="MinVer" Version="*" PrivateAssets="all" />
  </ItemGroup>

</Project>
EOF
```

### 2.4 Scaffold `src/CymruBlazor.Theming` — CSS design token library

```bash
dotnet new razorclasslib \
  --name CymruBlazor.Theming \
  --output src/CymruBlazor.Theming \
  --no-restore

cat > src/CymruBlazor.Theming/CymruBlazor.Theming.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>preview</LangVersion>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <IsTrimmable>true</IsTrimmable>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\CymruBlazor\CymruBlazor.csproj" />
  </ItemGroup>
</Project>
EOF
```

### 2.5 Scaffold `src/CymruBlazor.Icons` — SVG icon library

```bash
dotnet new razorclasslib \
  --name CymruBlazor.Icons \
  --output src/CymruBlazor.Icons \
  --no-restore

cat > src/CymruBlazor.Icons/CymruBlazor.Icons.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>preview</LangVersion>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <IsTrimmable>true</IsTrimmable>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\CymruBlazor\CymruBlazor.csproj" />
  </ItemGroup>
</Project>
EOF
```

### 2.6 Scaffold `src/CymruBlazor.Demo` — Blazor Web App (living docs)

```bash
dotnet new blazorwasm \
  --name CymruBlazor.Demo \
  --output src/CymruBlazor.Demo \
  --auth SingleOrg \
  --pwa --empty \
  --no-restore \
  --force

cat > src/CymruBlazor.Demo/CymruBlazor.Demo.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>preview</LangVersion>
    <ServiceWorkerAssetsManifest>service-worker-assets.js</ServiceWorkerAssetsManifest>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" PrivateAssets="all" />
    <PackageReference Include="Microsoft.Authentication.WebAssembly.Msal" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\CymruBlazor\CymruBlazor.csproj" />
    <ProjectReference Include="..\CymruBlazor.Icons\CymruBlazor.Icons.csproj" />
    <ProjectReference Include="..\CymruBlazor.Theming\CymruBlazor.Theming.csproj" />
  </ItemGroup>
</Project>
EOF
```

### 2.7 Scaffold test projects

```bash
# Component unit tests (bUnit)
dotnet new xunit \
  --name CymruBlazor.Tests \
  --output tests/CymruBlazor.Tests \
  --no-restore

cat > tests/CymruBlazor.Tests/CymruBlazor.Tests.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>preview</LangVersion>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="*" />
    <PackageReference Include="xunit" Version="*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="*" PrivateAssets="all" />
    <PackageReference Include="bunit" Version="*" />
    <PackageReference Include="Shouldly" Version="*" />
    <PackageReference Include="coverlet.collector" Version="*" PrivateAssets="all" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\CymruBlazor\CymruBlazor.csproj" />
  </ItemGroup>
</Project>
EOF
```

```bash
# Approval / snapshot tests
dotnet new xunit \
  --name CymruBlazor.ApprovalTests \
  --output tests/CymruBlazor.ApprovalTests \
  --no-restore

cat > tests/CymruBlazor.ApprovalTests/CymruBlazor.ApprovalTests.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>preview</LangVersion>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="*" />
    <PackageReference Include="xunit" Version="*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="*" PrivateAssets="all" />
    <PackageReference Include="bunit" Version="*" />
    <PackageReference Include="Shouldly" Version="*" />
    <PackageReference Include="ApprovalTests" Version="*" />
    <PackageReference Include="coverlet.collector" Version="*" PrivateAssets="all" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\CymruBlazor\CymruBlazor.csproj" />
  </ItemGroup>
</Project>
EOF
```

```bash
# Accessibility tests
dotnet new xunit \
  --name CymruBlazor.AccessibilityTests \
  --output tests/CymruBlazor.AccessibilityTests \
  --no-restore

cat > tests/CymruBlazor.AccessibilityTests/CymruBlazor.AccessibilityTests.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>preview</LangVersion>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="*" />
    <PackageReference Include="xunit" Version="*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="*" PrivateAssets="all" />
    <PackageReference Include="bunit" Version="*" />
    <PackageReference Include="Deque.AxeCore.Playwright" Version="*" />
    <PackageReference Include="Microsoft.Playwright" Version="*" />
    <PackageReference Include="coverlet.collector" Version="*" PrivateAssets="all" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\CymruBlazor\CymruBlazor.csproj" />
  </ItemGroup>
</Project>
EOF
```

### 2.8 Scaffold sample applications

```bash
# Minimal starter
dotnet new blazorwasm \
  --name StarterApp \
  --output samples/StarterApp \
  --no-restore

# Healthcare portal example
dotnet new blazorwasm \
  --name HealthcarePortal \
  --output samples/HealthcarePortal \
  --no-restore

# Dashboard example
dotnet new blazorwasm \
  --name Dashboard \
  --output samples/Dashboard \
  --no-restore
```

### 2.9 Add all projects to the solution

```bash
# Core library projects
dotnet sln CymruBlazor.slnx add src/CymruBlazor/CymruBlazor.csproj
dotnet sln CymruBlazor.slnx add src/CymruBlazor.Theming/CymruBlazor.Theming.csproj
dotnet sln CymruBlazor.slnx add src/CymruBlazor.Icons/CymruBlazor.Icons.csproj
dotnet sln CymruBlazor.slnx add src/CymruBlazor.Demo/CymruBlazor.Demo.csproj

# Test projects
dotnet sln CymruBlazor.slnx add tests/CymruBlazor.Tests/CymruBlazor.Tests.csproj
dotnet sln CymruBlazor.slnx add tests/CymruBlazor.ApprovalTests/CymruBlazor.ApprovalTests.csproj
dotnet sln CymruBlazor.slnx add tests/CymruBlazor.AccessibilityTests/CymruBlazor.AccessibilityTests.csproj

# Samples
dotnet sln CymruBlazor.slnx add samples/StarterApp/StarterApp.csproj
dotnet sln CymruBlazor.slnx add samples/HealthcarePortal/HealthcarePortal.csproj
dotnet sln CymruBlazor.slnx add samples/Dashboard/Dashboard.csproj
```

### 2.10 Restore all packages

```bash
dotnet restore
```

### 2.11 Commit the solution scaffold

```bash
git add .
git commit -m "chore: scaffold solution with all projects and test structure"
```

---

## Phase 3 — Global Configuration Files

### 3.1 `Directory.Build.props` — shared properties for every project

```bash
cat > Directory.Build.props << 'EOF'
<Project>
  <PropertyGroup>
    <!-- Enforce modern C# across all projects -->
    <LangVersion>preview</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors Condition="'$(CI)' == 'true'">true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <AnalysisLevel>latest-recommended</AnalysisLevel>

    <!-- Deterministic builds for reproducibility -->
    <Deterministic>true</Deterministic>
    <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>

    <!-- SourceLink applies to all packable projects -->
    <EmbedUntrackedSources>true</EmbedUntrackedSources>
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
  </PropertyGroup>
</Project>
EOF
```

### 3.2 `Directory.Build.targets` — shared targets (GitVersion injection)

```bash
cat > Directory.Build.targets << 'EOF'
<Project>
  <Target Name="ApplyGitVersioning"
          BeforeTargets="GetAssemblyVersion"
          Condition="'$(CI)' == 'true'">
    <Exec Command="dotnet gitversion /output json /showvariable NuGetVersionV2"
          ConsoleToMsBuild="true">
      <Output TaskParameter="ConsoleOutput" PropertyName="GitVersion_NuGetVersionV2" />
    </Exec>
    <PropertyGroup>
      <Version>$(GitVersion_NuGetVersionV2)</Version>
    </PropertyGroup>
  </Target>
</Project>
EOF
```

### 3.3 `Directory.Packages.props` — Central Package Management

```bash
cat > Directory.Packages.props << 'EOF'
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup Label="ASP.NET Core / Blazor">
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly" Version="10.0.*" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="10.0.*" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.Web" Version="10.0.*" />
  </ItemGroup>

  <ItemGroup Label="Testing">
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageVersion Include="xunit" Version="2.*" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageVersion Include="bunit" Version="1.*" />
    <PackageVersion Include="Shouldly" Version="4.*" />
    <PackageVersion Include="ApprovalTests" Version="5.*" />
    <PackageVersion Include="coverlet.collector" Version="6.*" />
    <PackageVersion Include="Deque.AxeCore.Playwright" Version="4.*" />
    <PackageVersion Include="Microsoft.Playwright" Version="1.*" />
  </ItemGroup>

  <ItemGroup Label="Source Link">
    <PackageVersion Include="Microsoft.SourceLink.GitHub" Version="8.*" />
    <PackageVersion Include="MinVer" Version="4.*" />
  </ItemGroup>
</Project>
EOF
```

### 3.4 Editor config

```bash
cat > .editorconfig << 'EOF'
root = true

[*]
indent_style = space
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true
charset = utf-8

[*.{cs,razor}]
indent_size = 4
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false
csharp_style_namespace_declarations = file_scoped:error
csharp_using_directive_placement = outside_namespace:error
dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_property = false:suggestion
csharp_prefer_simple_using_statement = true:suggestion
csharp_style_prefer_primary_constructors = true:suggestion
csharp_style_prefer_collection_expressions = true:suggestion
csharp_prefer_static_local_function = true:suggestion
dotnet_diagnostic.CS8600.severity = error
dotnet_diagnostic.CS8601.severity = error
dotnet_diagnostic.CS8602.severity = error
dotnet_diagnostic.CS8603.severity = error
dotnet_diagnostic.CS8604.severity = error
dotnet_diagnostic.CS8618.severity = error

[*.{csproj,props,targets,slnx}]
indent_size = 2

[*.{json,yml,yaml}]
indent_size = 2

[*.md]
trim_trailing_whitespace = false
EOF
```

### 3.5 Commit configuration

```bash
git add .
git commit -m "chore: add Directory.Build.props, central package management, and editor config"
```

---

## Phase 4 — CSS Architecture (NHS Wales Design System)

The NHS Wales Design System extends nhsuk-frontend. CymruBlazor wraps it via CSS custom properties without overriding the upstream design language.

### 4.1 Create the CSS folder structure

```bash
mkdir -p src/CymruBlazor/wwwroot/css
```

### 4.2 NHS colour tokens and custom properties

```bash
cat > src/CymruBlazor/wwwroot/css/tokens.css << 'EOF'
/**
 * CymruBlazor Design Tokens
 * Extends NHS Wales / DHCW design language.
 * Do not conflict with nhsuk-frontend custom properties.
 *
 * Colour palette sourced from:
 * - NHS Design System: https://service-manual.nhs.uk/design-system/styles/colour
 * - DHCW Design System: https://dhcw-digital-health-and-care-wales.github.io/nhsw-component-library/
 */

:root {
  /* ── NHS Core Blue ── */
  --cymru-color-blue:         #005eb8;
  --cymru-color-blue-dark:    #003087;
  --cymru-color-blue-light:   #d8e8f7;

  /* ── NHS Wales / DHCW Dragon Red ── */
  --cymru-color-wales-red:    #d4351c;

  /* ── Neutrals ── */
  --cymru-color-black:        #212b32;
  --cymru-color-dark-grey:    #425563;
  --cymru-color-mid-grey:     #768692;
  --cymru-color-pale-grey:    #f0f4f5;
  --cymru-color-white:        #ffffff;

  /* ── Semantic / Status ── */
  --cymru-color-green:        #007f3b;
  --cymru-color-green-light:  #cde7d1;
  --cymru-color-red:          #d5281b;
  --cymru-color-red-light:    #f9e1df;
  --cymru-color-yellow:       #ffdd00;
  --cymru-color-yellow-light: #fff9c4;
  --cymru-color-orange:       #f47738;
  --cymru-color-pink:         #ae2573;
  --cymru-color-purple:       #330072;

  /* ── Focus state (WCAG 2.2 compliant) ── */
  --cymru-focus-color:          #ffdd00;
  --cymru-focus-text-color:     #212b32;
  --cymru-focus-border-color:   #212b32;
  --cymru-focus-width:          3px;

  /* ── Typography (NHS Frutiger / system fallback) ── */
  --cymru-font-family: "Frutiger W01", Arial, sans-serif;
  --cymru-font-size-base: 1rem;           /* 16px */
  --cymru-font-size-s:    0.875rem;       /* 14px */
  --cymru-font-size-m:    1.125rem;       /* 18px */
  --cymru-font-size-l:    1.5rem;         /* 24px */
  --cymru-font-size-xl:   2rem;           /* 32px */
  --cymru-font-size-xxl:  2.5rem;         /* 40px */
  --cymru-line-height-base: 1.5;

  /* ── Spacing (NHS 8-point grid) ── */
  --cymru-space-1:  0.25rem;   /*  4px */
  --cymru-space-2:  0.5rem;    /*  8px */
  --cymru-space-3:  1rem;      /* 16px */
  --cymru-space-4:  1.5rem;    /* 24px */
  --cymru-space-5:  2rem;      /* 32px */
  --cymru-space-6:  3rem;      /* 48px */
  --cymru-space-7:  4rem;      /* 64px */
  --cymru-space-8:  5rem;      /* 80px */

  /* ── Border ── */
  --cymru-border-width:   1px;
  --cymru-border-radius:  4px;
  --cymru-border-color:   #aeb7bd;

  /* ── Shadows ── */
  --cymru-shadow-sm: 0 2px 4px rgba(33, 43, 50, 0.15);
  --cymru-shadow-md: 0 4px 8px rgba(33, 43, 50, 0.2);

  /* ── Z-index scale ── */
  --cymru-z-dropdown:  100;
  --cymru-z-sticky:    200;
  --cymru-z-modal:     300;
  --cymru-z-toast:     400;

  /* ── Transition ── */
  --cymru-transition-speed: 200ms;
  --cymru-transition-ease:  ease-in-out;
}
EOF
```

### 4.3 Base resets / global styles

```bash
cat > src/CymruBlazor/wwwroot/css/base.css << 'EOF'
/**
 * CymruBlazor Base Styles
 * Provides a clean baseline that layers under nhsuk-frontend.
 */

*, *::before, *::after {
  box-sizing: border-box;
}

html {
  font-size: 100%; /* respects user browser preferences */
}

body {
  font-family: var(--cymru-font-family);
  font-size: var(--cymru-font-size-base);
  line-height: var(--cymru-line-height-base);
  color: var(--cymru-color-black);
  background-color: var(--cymru-color-white);
  margin: 0;
}

/* NHS-standard focus style (WCAG 2.2) */
:focus-visible {
  outline: var(--cymru-focus-width) solid var(--cymru-focus-color);
  outline-offset: 0;
  box-shadow: 0 0 0 var(--cymru-focus-width) var(--cymru-focus-border-color);
}

:focus:not(:focus-visible) {
  outline: none;
}

img, svg {
  max-width: 100%;
  height: auto;
}

/* Skip link — must be first focusable element */
.cymru-skip-link {
  position: absolute;
  left: -9999px;
  top: auto;
  width: 1px;
  height: 1px;
  overflow: hidden;
}

.cymru-skip-link:focus {
  position: static;
  width: auto;
  height: auto;
  overflow: visible;
  padding: var(--cymru-space-2) var(--cymru-space-3);
  background-color: var(--cymru-focus-color);
  color: var(--cymru-focus-text-color);
  text-decoration: none;
  font-weight: bold;
}

/* Visually hidden — screen reader accessible */
.cymru-visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  margin: 0;
  padding: 0;
  overflow: hidden;
  clip: rect(0 0 0 0);
  clip-path: inset(50%);
  white-space: nowrap;
  border: 0;
}
EOF
```

### 4.4 Layout CSS

```bash
cat > src/CymruBlazor/wwwroot/css/layout.css << 'EOF'
/**
 * CymruBlazor Layout
 * NHS-standard 1200px max-width grid.
 */

.cymru-width-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 var(--cymru-space-4);
}

@media (min-width: 40.0625em) {
  .cymru-width-container {
    padding: 0 var(--cymru-space-5);
  }
}

.cymru-main-wrapper {
  padding-top: var(--cymru-space-4);
  padding-bottom: var(--cymru-space-6);
}

.cymru-grid-row {
  display: flex;
  flex-wrap: wrap;
  margin-left: calc(-1 * var(--cymru-space-3));
  margin-right: calc(-1 * var(--cymru-space-3));
}

.cymru-grid-column-full,
.cymru-grid-column-one-half,
.cymru-grid-column-one-third,
.cymru-grid-column-two-thirds {
  padding-left: var(--cymru-space-3);
  padding-right: var(--cymru-space-3);
  width: 100%;
}

@media (min-width: 40.0625em) {
  .cymru-grid-column-one-half   { width: 50%; }
  .cymru-grid-column-one-third  { width: 33.3333%; }
  .cymru-grid-column-two-thirds { width: 66.6666%; }
}
EOF
```

### 4.5 Navigation CSS

```bash
cat > src/CymruBlazor/wwwroot/css/navigation.css << 'EOF'
/**
 * CymruBlazor Navigation
 * Header, top navigation bar, and breadcrumb.
 */

/* Header */
.cymru-header {
  background-color: var(--cymru-color-blue);
  border-bottom: 4px solid var(--cymru-color-blue-dark);
}

.cymru-header__container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--cymru-space-2) var(--cymru-space-4);
  max-width: 1200px;
  margin: 0 auto;
}

.cymru-header__logo {
  display: flex;
  align-items: center;
  color: var(--cymru-color-white);
  text-decoration: none;
  font-weight: 700;
  font-size: var(--cymru-font-size-l);
}

.cymru-header__logo:focus-visible {
  outline-color: var(--cymru-focus-color);
}

/* Breadcrumb */
.cymru-breadcrumb {
  padding: var(--cymru-space-2) 0;
  font-size: var(--cymru-font-size-s);
}

.cymru-breadcrumb__list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-wrap: wrap;
  gap: var(--cymru-space-1);
}

.cymru-breadcrumb__item + .cymru-breadcrumb__item::before {
  content: "›";
  margin-right: var(--cymru-space-1);
  color: var(--cymru-color-mid-grey);
  aria-hidden: true;
}

.cymru-breadcrumb__link {
  color: var(--cymru-color-blue);
  text-decoration: underline;
}

.cymru-breadcrumb__link:hover {
  color: var(--cymru-color-blue-dark);
}
EOF
```

### 4.6 Forms CSS

```bash
cat > src/CymruBlazor/wwwroot/css/forms.css << 'EOF'
/**
 * CymruBlazor Forms
 * Inputs, labels, hints, errors — NHS style.
 */

.cymru-form-group {
  margin-bottom: var(--cymru-space-4);
}

.cymru-label {
  display: block;
  font-weight: 600;
  margin-bottom: var(--cymru-space-1);
  color: var(--cymru-color-black);
}

.cymru-hint {
  display: block;
  font-size: var(--cymru-font-size-s);
  color: var(--cymru-color-dark-grey);
  margin-bottom: var(--cymru-space-2);
}

.cymru-error-message {
  display: block;
  font-weight: 600;
  color: var(--cymru-color-red);
  margin-bottom: var(--cymru-space-2);
}

.cymru-error-message::before {
  content: "Error: ";
  font-weight: 700;
}

/* Text input */
.cymru-input {
  border: var(--cymru-border-width) solid var(--cymru-color-black);
  border-radius: 0;
  padding: var(--cymru-space-2);
  font-size: var(--cymru-font-size-base);
  font-family: var(--cymru-font-family);
  width: 100%;
  max-width: 20em;
  background-color: var(--cymru-color-white);
  color: var(--cymru-color-black);
  -webkit-appearance: none;
  appearance: none;
}

.cymru-input:focus-visible {
  outline: var(--cymru-focus-width) solid var(--cymru-focus-color);
  outline-offset: 0;
  box-shadow: inset 0 0 0 var(--cymru-focus-width) var(--cymru-focus-border-color);
}

.cymru-input--error {
  border-color: var(--cymru-color-red);
  border-width: 3px;
}

/* Select */
.cymru-select {
  border: var(--cymru-border-width) solid var(--cymru-color-black);
  border-radius: 0;
  padding: var(--cymru-space-2);
  font-size: var(--cymru-font-size-base);
  font-family: var(--cymru-font-family);
  background-color: var(--cymru-color-white);
  color: var(--cymru-color-black);
  max-width: 20em;
}

/* Checkbox */
.cymru-checkboxes__item {
  display: flex;
  align-items: flex-start;
  margin-bottom: var(--cymru-space-2);
  gap: var(--cymru-space-2);
}

.cymru-checkboxes__input {
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  cursor: pointer;
  border: 2px solid var(--cymru-color-black);
  background-color: var(--cymru-color-white);
  -webkit-appearance: none;
  appearance: none;
  border-radius: var(--cymru-border-radius);
}

.cymru-checkboxes__input:checked {
  background-color: var(--cymru-color-blue);
  border-color: var(--cymru-color-blue);
}

.cymru-checkboxes__input:checked::after {
  content: "";
  display: block;
  width: 60%;
  height: 40%;
  border: 3px solid var(--cymru-color-white);
  border-top: 0;
  border-right: 0;
  transform: translate(30%, 40%) rotate(-45deg);
}

.cymru-checkboxes__label {
  font-size: var(--cymru-font-size-base);
  padding-top: var(--cymru-space-1);
  cursor: pointer;
}

/* Error summary */
.cymru-error-summary {
  border: 4px solid var(--cymru-color-red);
  padding: var(--cymru-space-3);
  margin-bottom: var(--cymru-space-4);
}

.cymru-error-summary__title {
  font-size: var(--cymru-font-size-m);
  font-weight: 700;
  margin-top: 0;
}

/* Button */
.cymru-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--cymru-space-2);
  padding: var(--cymru-space-2) var(--cymru-space-4);
  font-size: var(--cymru-font-size-base);
  font-family: var(--cymru-font-family);
  font-weight: 600;
  text-decoration: none;
  border: 2px solid transparent;
  border-radius: 0;
  cursor: pointer;
  transition: background-color var(--cymru-transition-speed) var(--cymru-transition-ease),
              color var(--cymru-transition-speed) var(--cymru-transition-ease);
  -webkit-appearance: none;
  appearance: none;
  box-shadow: 0 4px 0 #212b32;
  position: relative;
  top: 0;
}

.cymru-button:active {
  top: 4px;
  box-shadow: none;
}

/* Primary button */
.cymru-button--primary {
  background-color: var(--cymru-color-green);
  color: var(--cymru-color-white);
}

.cymru-button--primary:hover {
  background-color: #00602c;
}

/* Secondary button */
.cymru-button--secondary {
  background-color: var(--cymru-color-pale-grey);
  color: var(--cymru-color-black);
  box-shadow: 0 4px 0 var(--cymru-color-dark-grey);
}

.cymru-button--secondary:hover {
  background-color: #d8dde0;
}

/* Reverse / warning button */
.cymru-button--reverse {
  background-color: var(--cymru-color-white);
  color: var(--cymru-color-blue);
  box-shadow: 0 4px 0 var(--cymru-color-blue-dark);
}

.cymru-button:disabled,
.cymru-button[aria-disabled="true"] {
  opacity: 0.5;
  cursor: not-allowed;
  pointer-events: none;
}
EOF
```

### 4.7 Cards CSS

```bash
cat > src/CymruBlazor/wwwroot/css/cards.css << 'EOF'
/**
 * CymruBlazor Cards
 * NHS-style card component.
 */

.cymru-card {
  background-color: var(--cymru-color-white);
  border: var(--cymru-border-width) solid var(--cymru-border-color);
  border-radius: var(--cymru-border-radius);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.cymru-card__img-wrapper {
  overflow: hidden;
  max-height: 200px;
}

.cymru-card__img-wrapper img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.cymru-card__content {
  padding: var(--cymru-space-4);
  flex: 1;
  display: flex;
  flex-direction: column;
}

.cymru-card__heading {
  font-size: var(--cymru-font-size-m);
  font-weight: 700;
  margin: 0 0 var(--cymru-space-2) 0;
  color: var(--cymru-color-black);
}

.cymru-card__heading a {
  color: var(--cymru-color-blue);
  text-decoration: underline;
}

.cymru-card__heading a:hover {
  color: var(--cymru-color-blue-dark);
}

.cymru-card__description {
  font-size: var(--cymru-font-size-base);
  color: var(--cymru-color-dark-grey);
  margin: 0;
  flex: 1;
}

/* Clickable card variant */
.cymru-card--clickable {
  cursor: pointer;
  transition: box-shadow var(--cymru-transition-speed) var(--cymru-transition-ease);
}

.cymru-card--clickable:hover {
  box-shadow: var(--cymru-shadow-md);
}

.cymru-card--clickable:focus-within {
  outline: var(--cymru-focus-width) solid var(--cymru-focus-color);
}
EOF
```

### 4.8 Alerts CSS

```bash
cat > src/CymruBlazor/wwwroot/css/alerts.css << 'EOF'
/**
 * CymruBlazor Alerts / Callouts
 * NHS-style warning, info, success, and error callouts.
 */

.cymru-alert {
  border-left: 8px solid transparent;
  padding: var(--cymru-space-3) var(--cymru-space-4);
  margin-bottom: var(--cymru-space-4);
}

.cymru-alert__title {
  font-size: var(--cymru-font-size-m);
  font-weight: 700;
  margin: 0 0 var(--cymru-space-2) 0;
}

.cymru-alert__content {
  margin: 0;
}

.cymru-alert--info {
  background-color: #d8e8f7;
  border-color: var(--cymru-color-blue);
}

.cymru-alert--success {
  background-color: var(--cymru-color-green-light);
  border-color: var(--cymru-color-green);
}

.cymru-alert--warning {
  background-color: var(--cymru-color-yellow-light);
  border-color: var(--cymru-color-yellow);
}

.cymru-alert--error {
  background-color: var(--cymru-color-red-light);
  border-color: var(--cymru-color-red);
}
EOF
```

### 4.9 Utilities CSS

```bash
cat > src/CymruBlazor/wwwroot/css/utilities.css << 'EOF'
/**
 * CymruBlazor Utilities
 * Small helper classes. Prefer component-level styles; use these sparingly.
 */

/* Display */
.cymru-u-hidden            { display: none !important; }
.cymru-u-block             { display: block !important; }
.cymru-u-inline-block      { display: inline-block !important; }

/* Text */
.cymru-u-font-bold         { font-weight: 700 !important; }
.cymru-u-text-centre       { text-align: center !important; }
.cymru-u-text-right        { text-align: right !important; }

/* Spacing */
.cymru-u-margin-0          { margin: 0 !important; }
.cymru-u-padding-0         { padding: 0 !important; }

/* Colour */
.cymru-u-color-blue        { color: var(--cymru-color-blue) !important; }
.cymru-u-color-red         { color: var(--cymru-color-red) !important; }
.cymru-u-color-green       { color: var(--cymru-color-green) !important; }

/* Print */
@media print {
  .cymru-u-print-hidden    { display: none !important; }
}
EOF
```

### 4.10 Main bundle entry point

```bash
cat > src/CymruBlazor/wwwroot/css/cymrublazor.css << 'EOF'
/**
 * cymrublazor.css — single import for consuming applications
 *
 *   <link href="_content/CymruBlazor/css/cymrublazor.css" rel="stylesheet" />
 *
 * Layer order matters: tokens → base → layout → components → utilities → overrides
 */

@import url("tokens.css");
@import url("base.css");
@import url("layout.css");
@import url("navigation.css");
@import url("forms.css");
@import url("cards.css");
@import url("alerts.css");
@import url("utilities.css");
EOF
```

### 4.11 Commit the CSS architecture

```bash
git add .
git commit -m "feat(css): implement NHS Wales design token CSS architecture with all component styles"
```

---

## Phase 5 — Core Component Scaffolding

### 5.1 Create folder structure inside the RCL

```bash
mkdir -p src/CymruBlazor/Components/Layout
mkdir -p src/CymruBlazor/Components/Content
mkdir -p src/CymruBlazor/Components/Forms
mkdir -p src/CymruBlazor/Components/Infrastructure

mkdir -p src/CymruBlazor/Models
mkdir -p src/CymruBlazor/Services
mkdir -p src/CymruBlazor/Extensions
```

### 5.2 Global usings for the RCL

```bash
cat > src/CymruBlazor/GlobalUsings.cs << 'EOF'
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.AspNetCore.Components.Forms;
global using System.Text.Json;
global using System.Text.Json.Serialization;
EOF
```

### 5.3 CymruComponentBase — shared base class

```bash
cat > src/CymruBlazor/Components/CymruComponentBase.cs << 'EOF'
namespace CymruBlazor.Components;

/// <summary>
/// Base class for all CymruBlazor components.
/// Provides common infrastructure: additional attributes,
/// CSS class composition, and lifecycle hooks.
/// </summary>
public abstract class CymruComponentBase : ComponentBase
{
    /// <summary>
    /// Additional HTML attributes applied to the root element.
    /// Supports scenarios like <c>aria-*</c>, <c>data-*</c>, and custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Optional additional CSS class names appended to the component's root element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Composes a final CSS class string from base classes and any consumer-provided
    /// <see cref="Class"/> parameter.
    /// </summary>
    /// <param name="baseClasses">The component's own CSS classes.</param>
    /// <returns>A trimmed, whitespace-normalised CSS class string.</returns>
    protected string BuildCssClass(string baseClasses) =>
        string.Join(" ", [baseClasses, Class ?? string.Empty]).Trim();
}
EOF
```

### 5.4 Scaffold the SkipLink component (accessibility-critical)

```bash
cat > src/CymruBlazor/Components/Layout/SkipLink.razor << 'EOF'
@namespace CymruBlazor.Components.Layout
@inherits CymruComponentBase

<a href="@Target" class="@BuildCssClass("cymru-skip-link")" @attributes="AdditionalAttributes">
    @ChildContent
</a>

@code {
    /// <summary>The ID of the main content area to skip to, e.g. "#maincontent".</summary>
    [Parameter, EditorRequired]
    public string Target { get; set; } = "#maincontent";

    /// <summary>The visible skip link text.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
EOF
```

### 5.5 Scaffold the Button component

```bash
cat > src/CymruBlazor/Components/Forms/Button.razor << 'EOF'
@namespace CymruBlazor.Components.Forms
@inherits CymruComponentBase

<button type="@Type"
        class="@CssClass"
        disabled="@Disabled"
        aria-disabled="@(Disabled ? "true" : null)"
        @onclick="OnClickCallback"
        @attributes="AdditionalAttributes">
    @ChildContent
</button>

@code {
    /// <summary>The visual style variant of the button.</summary>
    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;

    /// <summary>The HTML button type attribute.</summary>
    [Parameter]
    public string Type { get; set; } = "button";

    /// <summary>Whether the button is disabled.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Callback invoked when the button is clicked.</summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>The button content (label, icons, etc.).</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string CssClass => BuildCssClass($"cymru-button cymru-button--{Variant.ToString().ToLowerInvariant()}");

    private async Task OnClickCallback(MouseEventArgs args)
    {
        if (!Disabled)
            await OnClick.InvokeAsync(args);
    }
}
EOF

cat > src/CymruBlazor/Components/Forms/ButtonVariant.cs << 'EOF'
namespace CymruBlazor.Components.Forms;

/// <summary>The visual variant of a <see cref="Button"/> component.</summary>
public enum ButtonVariant
{
    /// <summary>Green primary action button.</summary>
    Primary,
    /// <summary>Grey secondary action button.</summary>
    Secondary,
    /// <summary>White reverse button, for use on coloured backgrounds.</summary>
    Reverse,
    /// <summary>Red destructive action button.</summary>
    Warning
}
EOF
```

### 5.6 Scaffold the TextBox component

```bash
cat > src/CymruBlazor/Components/Forms/TextBox.razor << 'EOF'
@namespace CymruBlazor.Components.Forms
@inherits CymruComponentBase

<div class="cymru-form-group @(HasError ? "cymru-form-group--error" : "")">

    @if (!string.IsNullOrWhiteSpace(Label))
    {
        <label class="cymru-label" for="@InputId">@Label</label>
    }

    @if (!string.IsNullOrWhiteSpace(Hint))
    {
        <span class="cymru-hint" id="@HintId">@Hint</span>
    }

    @if (HasError)
    {
        <span class="cymru-error-message" id="@ErrorId" role="alert">@ErrorMessage</span>
    }

    <input id="@InputId"
           type="@Type"
           class="@CssClass"
           value="@Value"
           placeholder="@Placeholder"
           autocomplete="@Autocomplete"
           aria-describedby="@AriaDescribedBy"
           aria-invalid="@(HasError ? "true" : null)"
           @oninput="OnInputChanged"
           @attributes="AdditionalAttributes" />
</div>

@code {
    private static int _idCounter;

    private readonly string _id = $"cymru-input-{Interlocked.Increment(ref _idCounter)}";

    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Hint { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string? Autocomplete { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    private string InputId  => Id ?? _id;
    private string HintId   => $"{InputId}-hint";
    private string ErrorId  => $"{InputId}-error";
    private bool   HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    private string? AriaDescribedBy
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Hint))  parts.Add(HintId);
            if (HasError) parts.Add(ErrorId);
            return parts.Count > 0 ? string.Join(" ", parts) : null;
        }
    }

    private string CssClass =>
        BuildCssClass($"cymru-input{(HasError ? " cymru-input--error" : "")}");

    private async Task OnInputChanged(ChangeEventArgs e) =>
        await ValueChanged.InvokeAsync(e.Value?.ToString());
}
EOF
```

### 5.7 Scaffold the Alert component

```bash
cat > src/CymruBlazor/Components/Content/Alert.razor << 'EOF'
@namespace CymruBlazor.Components.Content
@inherits CymruComponentBase

<div class="@CssClass" role="@Role" @attributes="AdditionalAttributes">
    @if (!string.IsNullOrWhiteSpace(Title))
    {
        <h3 class="cymru-alert__title">@Title</h3>
    }
    <div class="cymru-alert__content">
        @ChildContent
    </div>
</div>

@code {
    [Parameter] public AlertVariant Variant { get; set; } = AlertVariant.Info;
    [Parameter] public string? Title { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string Role => Variant switch
    {
        AlertVariant.Error   => "alert",
        AlertVariant.Warning => "alert",
        _                    => "status"
    };

    private string CssClass =>
        BuildCssClass($"cymru-alert cymru-alert--{Variant.ToString().ToLowerInvariant()}");
}
EOF

cat > src/CymruBlazor/Components/Content/AlertVariant.cs << 'EOF'
namespace CymruBlazor.Components.Content;

/// <summary>Semantic variant for an <see cref="Alert"/> component.</summary>
public enum AlertVariant
{
    Info,
    Success,
    Warning,
    Error
}
EOF
```

### 5.8 Scaffold the Breadcrumb component

```bash
cat > src/CymruBlazor/Components/Layout/Breadcrumb.razor << 'EOF'
@namespace CymruBlazor.Components.Layout
@inherits CymruComponentBase

<nav aria-label="Breadcrumb" class="@BuildCssClass("cymru-breadcrumb")" @attributes="AdditionalAttributes">
    <ol class="cymru-breadcrumb__list">
        @foreach (var item in Items)
        {
            <li class="cymru-breadcrumb__item">
                @if (string.IsNullOrEmpty(item.Href))
                {
                    <span aria-current="page">@item.Label</span>
                }
                else
                {
                    <a class="cymru-breadcrumb__link" href="@item.Href">@item.Label</a>
                }
            </li>
        }
    </ol>
</nav>

@code {
    [Parameter, EditorRequired]
    public IReadOnlyList<BreadcrumbItem> Items { get; set; } = [];
}
EOF

cat > src/CymruBlazor/Models/BreadcrumbItem.cs << 'EOF'
namespace CymruBlazor.Models;

/// <summary>Represents a single item in a breadcrumb trail.</summary>
/// <param name="Label">The display text.</param>
/// <param name="Href">The navigation URL. Null or empty indicates the current page.</param>
public sealed record BreadcrumbItem(string Label, string? Href = null);
EOF
```

### 5.9 Scaffold the Card component

```bash
cat > src/CymruBlazor/Components/Content/Card.razor << 'EOF'
@namespace CymruBlazor.Components.Content
@inherits CymruComponentBase

<article class="@CssClass" @attributes="AdditionalAttributes">
    @if (!string.IsNullOrWhiteSpace(ImageSrc))
    {
        <div class="cymru-card__img-wrapper">
            <img src="@ImageSrc" alt="@ImageAlt" loading="lazy" />
        </div>
    }
    <div class="cymru-card__content">
        @if (!string.IsNullOrWhiteSpace(Heading))
        {
            <h2 class="cymru-card__heading">
                @if (!string.IsNullOrWhiteSpace(Href))
                {
                    <a href="@Href">@Heading</a>
                }
                else
                {
                    @Heading
                }
            </h2>
        }
        @if (ChildContent is not null)
        {
            <p class="cymru-card__description">@ChildContent</p>
        }
    </div>
</article>

@code {
    [Parameter] public string? Heading { get; set; }
    [Parameter] public string? Href { get; set; }
    [Parameter] public string? ImageSrc { get; set; }
    [Parameter] public string? ImageAlt { get; set; }
    [Parameter] public bool Clickable { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string CssClass =>
        BuildCssClass($"cymru-card{(Clickable ? " cymru-card--clickable" : "")}");
}
EOF
```

### 5.10 Infrastructure — ThemeProvider component

```bash
cat > src/CymruBlazor/Components/Infrastructure/CymruBlazorProvider.razor << 'EOF'
@namespace CymruBlazor.Components.Infrastructure

@*
    CymruBlazorProvider — wraps the application, injecting the component library stylesheet
    link and any cascading values (theme, locale) the library needs.

    Place at the root of App.razor:

    <CymruBlazorProvider>
        <Router ...>...</Router>
    </CymruBlazorProvider>
*@

<CascadingValue Value="Theme" Name="CymruTheme" IsFixed="true">
    @ChildContent
</CascadingValue>

@code {
    /// <summary>The active CymruBlazor theme configuration.</summary>
    [Parameter]
    public CymruTheme Theme { get; set; } = CymruTheme.Default;

    /// <summary>Child content — typically the Blazor Router.</summary>
    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;
}
EOF

cat > src/CymruBlazor/Models/CymruTheme.cs << 'EOF'
namespace CymruBlazor.Models;

/// <summary>Defines the active CymruBlazor visual theme.</summary>
public sealed record CymruTheme
{
    /// <summary>The default NHS Wales theme.</summary>
    public static readonly CymruTheme Default = new();

    /// <summary>High-contrast theme for improved accessibility.</summary>
    public static readonly CymruTheme HighContrast = new() { IsHighContrast = true };

    /// <summary>Whether high-contrast mode is active.</summary>
    public bool IsHighContrast { get; init; }

    /// <summary>The name identifier for the theme.</summary>
    public string Name { get; init; } = "default";
}
EOF
```

### 5.11 Create the library's public API surface (`_Imports.razor` and extensions)

```bash
cat > src/CymruBlazor/_Imports.razor << 'EOF'
@using CymruBlazor.Components
@using CymruBlazor.Components.Layout
@using CymruBlazor.Components.Content
@using CymruBlazor.Components.Forms
@using CymruBlazor.Components.Infrastructure
@using CymruBlazor.Models
EOF
```

```bash
cat > src/CymruBlazor/Extensions/ServiceCollectionExtensions.cs << 'EOF'
using Microsoft.Extensions.DependencyInjection;

namespace CymruBlazor.Extensions;

/// <summary>
/// Extension methods for registering CymruBlazor services with the
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all CymruBlazor services required by the component library.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCymruBlazor(this IServiceCollection services)
    {
        // Register library services here as the library grows.
        return services;
    }
}
EOF
```

### 5.12 Commit components

```bash
git add .
git commit -m "feat(components): scaffold core layout, form, and content components with accessibility"
```

---

## Phase 6 — Test Scaffolding

### 6.1 Button unit test

```bash
cat > tests/CymruBlazor.Tests/Components/Forms/ButtonTests.cs << 'EOF'
using Bunit;
using CymruBlazor.Components.Forms;
using Shouldly;

namespace CymruBlazor.Tests.Components.Forms;

public sealed class ButtonTests : TestContext
{
    [Fact]
    public void Button_Renders_With_Primary_Class_By_Default()
    {
        var cut = RenderComponent<Button>(p => p
            .AddChildContent("Click me"));

        cut.Find("button").ClassList.ShouldContain("cymru-button--primary");
    }

    [Fact]
    public void Button_Renders_With_Secondary_Class_When_Variant_Is_Secondary()
    {
        var cut = RenderComponent<Button>(p => p
            .Add(c => c.Variant, ButtonVariant.Secondary)
            .AddChildContent("Secondary"));

        cut.Find("button").ClassList.ShouldContain("cymru-button--secondary");
    }

    [Fact]
    public void Button_Is_Disabled_When_Disabled_Parameter_Is_True()
    {
        var cut = RenderComponent<Button>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("Disabled"));

        var button = cut.Find("button");
        button.HasAttribute("disabled").ShouldBeTrue();
        button.GetAttribute("aria-disabled").ShouldBe("true");
    }

    [Fact]
    public async Task Button_Raises_OnClick_Event_When_Clicked()
    {
        var clicked = false;
        var cut = RenderComponent<Button>(p => p
            .Add(c => c.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicked = true))
            .AddChildContent("Click"));

        await cut.Find("button").ClickAsync(new());

        clicked.ShouldBeTrue();
    }

    [Fact]
    public async Task Button_Does_Not_Raise_OnClick_When_Disabled()
    {
        var clicked = false;
        var cut = RenderComponent<Button>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicked = true))
            .AddChildContent("Click"));

        await cut.Find("button").ClickAsync(new());

        clicked.ShouldBeFalse();
    }
}
EOF
```

### 6.2 Approval test for Button HTML snapshot

```bash
mkdir -p tests/CymruBlazor.ApprovalTests/Snapshots

cat > tests/CymruBlazor.ApprovalTests/Components/Forms/ButtonApprovalTests.cs << 'EOF'
using ApprovalTests;
using ApprovalTests.Reporters;
using Bunit;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.ApprovalTests.Components.Forms;

[UseReporter(typeof(DiffReporter))]
public sealed class ButtonApprovalTests : TestContext
{
    [Fact]
    public void Button_Primary_Renders_Expected_Html()
    {
        var cut = RenderComponent<Button>(p => p
            .Add(c => c.Variant, ButtonVariant.Primary)
            .AddChildContent("Save changes"));

        Approvals.VerifyHtml(cut.Markup);
    }

    [Fact]
    public void Button_Secondary_Renders_Expected_Html()
    {
        var cut = RenderComponent<Button>(p => p
            .Add(c => c.Variant, ButtonVariant.Secondary)
            .AddChildContent("Cancel"));

        Approvals.VerifyHtml(cut.Markup);
    }
}
EOF
```

### 6.3 Alert unit test

```bash
cat > tests/CymruBlazor.Tests/Components/Content/AlertTests.cs << 'EOF'
using Bunit;
using CymruBlazor.Components.Content;
using Shouldly;

namespace CymruBlazor.Tests.Components.Content;

public sealed class AlertTests : TestContext
{
    [Theory]
    [InlineData(AlertVariant.Info,    "cymru-alert--info",    "status")]
    [InlineData(AlertVariant.Success, "cymru-alert--success", "status")]
    [InlineData(AlertVariant.Warning, "cymru-alert--warning", "alert")]
    [InlineData(AlertVariant.Error,   "cymru-alert--error",   "alert")]
    public void Alert_Renders_Correct_Class_And_Role(
        AlertVariant variant, string expectedClass, string expectedRole)
    {
        var cut = RenderComponent<Alert>(p => p
            .Add(c => c.Variant, variant)
            .AddChildContent("Message"));

        var div = cut.Find("div");
        div.ClassList.ShouldContain(expectedClass);
        div.GetAttribute("role").ShouldBe(expectedRole);
    }

    [Fact]
    public void Alert_Renders_Title_When_Provided()
    {
        var cut = RenderComponent<Alert>(p => p
            .Add(c => c.Title, "Important information")
            .AddChildContent("Details here."));

        cut.Find("h3").TextContent.ShouldBe("Important information");
    }
}
EOF
```

### 6.4 Commit tests

```bash
git add .
git commit -m "test(components): add bUnit and approval tests for Button and Alert"
```

---

## Phase 7 — CI/CD with GitHub Actions

### 7.1 Main CI workflow

```bash
cat > .github/workflows/ci.yml << 'EOF'
name: CI

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

env:
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
  DOTNET_NOLOGO: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true

jobs:
  build-and-test:
    name: Build & Test
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          fetch-depth: 0   # required for GitVersion

      - name: Setup .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'

      - name: Install GitVersion
        uses: gittools/actions/gitversion/setup@v1
        with:
          versionSpec: '5.x'

      - name: Determine version
        id: gitversion
        uses: gittools/actions/gitversion/execute@v1

      - name: Display version
        run: echo "Version = ${{ steps.gitversion.outputs.nuGetVersionV2 }}"

      - name: Restore
        run: dotnet restore

      - name: Build
        run: |
          dotnet build --no-restore --configuration Release \
            /p:Version=${{ steps.gitversion.outputs.nuGetVersionV2 }} \
            /p:AssemblyVersion=${{ steps.gitversion.outputs.assemblySemVer }} \
            /p:FileVersion=${{ steps.gitversion.outputs.assemblySemFileVer }}

      - name: Test
        run: |
          dotnet test --no-build --configuration Release \
            --collect:"XPlat Code Coverage" \
            --results-directory coverage \
            --logger "github-actions;report-warnings=false"

      - name: Upload coverage
        uses: codecov/codecov-action@v4
        with:
          directory: coverage
          fail_ci_if_error: false
EOF
```

### 7.2 NuGet publish workflow

```bash
cat > .github/workflows/publish.yml << 'EOF'
name: Publish NuGet

on:
  push:
    tags:
      - 'v*'
  workflow_dispatch:

env:
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
  DOTNET_NOLOGO: true

jobs:
  publish:
    name: Pack & Publish
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Setup .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'

      - name: Install GitVersion
        uses: gittools/actions/gitversion/setup@v1
        with:
          versionSpec: '5.x'

      - name: Determine version
        id: gitversion
        uses: gittools/actions/gitversion/execute@v1

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore
          /p:Version=${{ steps.gitversion.outputs.nuGetVersionV2 }}

      - name: Pack
        run: |
          dotnet pack src/CymruBlazor/CymruBlazor.csproj \
            --configuration Release \
            --no-build \
            --output artifacts \
            /p:Version=${{ steps.gitversion.outputs.nuGetVersionV2 }}

      - name: Push to NuGet.org
        run: |
          dotnet nuget push artifacts/*.nupkg \
            --api-key ${{ secrets.NUGET_API_KEY }} \
            --source https://api.nuget.org/v3/index.json \
            --skip-duplicate

      - name: Create GitHub Release
        uses: softprops/action-gh-release@v2
        with:
          files: artifacts/*
          generate_release_notes: true
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
EOF
```

### 7.3 Dependabot config

```bash
cat > .github/dependabot.yml << 'EOF'
version: 2
updates:
  - package-ecosystem: nuget
    directory: "/"
    schedule:
      interval: weekly
    open-pull-requests-limit: 5
    labels: ["dependencies", "nuget"]

  - package-ecosystem: github-actions
    directory: "/"
    schedule:
      interval: weekly
    labels: ["dependencies", "github-actions"]
EOF
```

### 7.4 PR and issue templates

```bash
cat > .github/pull_request_template.md << 'EOF'
## Summary

<!-- What does this PR change and why? -->

## Type of change

- [ ] Bug fix
- [ ] New feature / component
- [ ] Breaking change
- [ ] Documentation
- [ ] Refactor
- [ ] CI/CD

## Checklist

- [ ] Tests added or updated
- [ ] Documentation updated
- [ ] Accessibility impact considered
- [ ] `dotnet test` passes locally
- [ ] Commit messages follow Conventional Commits
EOF
```

```bash
git add .
git commit -m "ci: add GitHub Actions CI, NuGet publish workflow, and Dependabot config"
```

---

## Phase 8 — Demo Application Setup

### 8.1 Wire up the Demo App's `_Imports.razor`

```bash
cat > src/CymruBlazor.Demo/wwwroot/index.html << 'EOF'
<!DOCTYPE html>
<html lang="en-GB">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>CymruBlazor — Component Demo</title>
    <base href="/" />
    <!-- NHS frontend (upstream) — install via: npm i nhsuk-frontend -->
    <!-- <link rel="stylesheet" href="css/nhsuk-frontend.min.css" /> -->
    <!-- CymruBlazor design system layer -->
    <link href="_content/CymruBlazor/css/cymrublazor.css" rel="stylesheet" />
</head>
<body>
    <div id="app">
        <svg class="cymru-visually-hidden" aria-hidden="true" focusable="false">
            <circle cx="50" cy="50" r="50" />
        </svg>
        Loading CymruBlazor Demo...
    </div>
    <div id="blazor-error-ui">
        An unhandled error has occurred.
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>
    <script src="_framework/blazor.webassembly.js"></script>
</body>
</html>
EOF
```

```bash
cat > src/CymruBlazor.Demo/_Imports.razor << 'EOF'
@using System.Net.Http
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.AspNetCore.Components.Routing
@using CymruBlazor.Components
@using CymruBlazor.Components.Layout
@using CymruBlazor.Components.Content
@using CymruBlazor.Components.Forms
@using CymruBlazor.Components.Infrastructure
@using CymruBlazor.Models
EOF
```

### 8.2 Demo `Program.cs`

```bash
cat > src/CymruBlazor.Demo/Program.cs << 'EOF'
using CymruBlazor.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CymruBlazor.Demo;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register CymruBlazor services
builder.Services.AddCymruBlazor();

await builder.Build().RunAsync();
EOF
```

### 8.3 Commit demo wiring

```bash
git add .
git commit -m "feat(demo): wire up Blazor WASM demo application with CymruBlazor services"
```

---

## Phase 9 — Documentation Structure

````bash
mkdir -p docs/components docs/patterns docs/accessibility docs/getting-started

cat > docs/getting-started/installation.md << 'EOF'
# Installation

## NuGet

```shell
dotnet add package CymruBlazor
````

## Manual reference

Add the following to your `.csproj`:

```xml
<PackageReference Include="CymruBlazor" Version="x.y.z" />
```

## Stylesheet

In `App.razor` or `_Host.cshtml`:

```html
<link href="_content/CymruBlazor/css/cymrublazor.css" rel="stylesheet" />
```

## Provider

Wrap your root component with `CymruBlazorProvider`:

```razor
<CymruBlazorProvider>
    <Router AppAssembly="@typeof(App).Assembly">
        ...
    </Router>
</CymruBlazorProvider>
```

## Service registration

In `Program.cs`:

```csharp
builder.Services.AddCymruBlazor();
```

EOF

````

```bash
cat > docs/components/Button.md << 'EOF'
# Button

Triggers an action. Follows NHS Wales design guidance.

## Usage

```razor
<Button Variant="ButtonVariant.Primary" OnClick="HandleSave">
    Save changes
</Button>
````

## Parameters

| Parameter | Type          | Default  | Description                |
| --------- | ------------- | -------- | -------------------------- |
| Variant   | ButtonVariant | Primary  | Visual style variant       |
| Type      | string        | "button" | HTML button type           |
| Disabled  | bool          | false    | Disables the button        |
| OnClick   | EventCallback | —        | Click event handler        |
| Class     | string?       | null     | Additional CSS class names |

## Variants

- `Primary` — Green, main call to action
- `Secondary` — Grey, supporting action
- `Reverse` — White, for coloured backgrounds
- `Warning` — Red, destructive actions

## Accessibility

- Uses semantic `<button>` element.
- Sets `aria-disabled` alongside the `disabled` attribute.
- Focus visible outline meets WCAG 2.2 AA 3:1 contrast ratio.
  EOF

````

```bash
git add .
git commit -m "docs: add getting-started installation guide and Button component documentation"
````

---

## Phase 10 — Final Verification

### 10.1 Build everything

```bash
dotnet build --configuration Release
```

### 10.2 Run all tests

```bash
dotnet test --configuration Release --verbosity normal
```

### 10.3 Verify the Demo runs

```bash
cd src/CymruBlazor.Demo
dotnet run
# Open https://localhost:5xxx in a browser
cd ../..
```

### 10.4 Pack the NuGet package locally (dry run)

```bash
dotnet pack src/CymruBlazor/CymruBlazor.csproj \
  --configuration Release \
  --output ./local-artifacts \
  /p:Version=0.1.0-alpha.1
```

### 10.5 Final commit

```bash
git add .
git commit -m "chore: verify full solution builds, tests pass, and demo runs"
git tag v0.1.0-alpha.1
```

---

## Summary of git history

```
chore: initialise repository with metadata and versioning config
chore: scaffold solution with all projects and test structure
chore: add Directory.Build.props, central package management, and editor config
feat(css): implement NHS Wales design token CSS architecture with all component styles
feat(components): scaffold core layout, form, and content components with accessibility
test(components): add bUnit and approval tests for Button and Alert
ci: add GitHub Actions CI, NuGet publish workflow, and Dependabot config
feat(demo): wire up Blazor WASM demo application with CymruBlazor services
docs: add getting-started installation guide and Button component documentation
chore: verify full solution builds, tests pass, and demo runs
```

---

## Recommended next iteration (Phase 2)

| Work item                                             | Priority |
| ----------------------------------------------------- | -------- |
| Footer, PageHeader, HeroBanner, Navigation components | High     |
| Select, Checkbox, ValidationSummary form components   | High     |
| Typography and Icons library                          | High     |
| Demo pages for every component (living docs)          | High     |
| Playwright accessibility tests wired to axe-core      | Medium   |
| nhsuk-frontend npm integration via build pipeline     | Medium   |
| Storybook or custom demo pages for each component     | Medium   |
| NuGet icon, SourceLink end-to-end test                | Low      |
| StarterApp and HealthcarePortal sample completion     | Low      |
