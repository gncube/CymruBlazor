```
CymruBlazor/
├─ CHANGELOG.md
├─ Clean-Solution.ps1
├─ CONTRIBUTING.md
├─ CymruBlazor.slnx
├─ Directory.Build.props
├─ Directory.Packages.props
├─ LICENSE
├─ local-package-versions.props
├─ MIGRATION_CLEANUP.md
├─ New-LocalPackageFeed.ps1
├─ NuGet.CI.Config
├─ nuget.config
├─ NuGet.Local.Config
├─ Pack-Solution.ps1
├─ README.md
├─ test-local.ps1
├─ artifacts/                          (excluded from tree above as generated output)
├─ docs/
│  ├─ CymruBlazor-Scaffold-Guide.md
│  ├─ CymruBlazor-Scaffold-Guide.OLD-for-comparison.md
│  ├─ DEMO_TODOS.md
│  ├─ implementation-plan.md
│  └─ rfc-cysidebar-composable-states-and-mobile-drawer.md
├─ plan/
│  ├─ _template.md
│  ├─ known-issues-and-backlog.md
│  ├─ README.md
│  ├─ active/
│  └─ archive/
│     ├─ architecture-demo-documentation-platform-1.md
│     ├─ implement-core-componet-foundatiomn-base-architecture.md
│     ├─ plan-first-nuget-release.md
│     └─ plan-next-release-components.md
├─ samples/
│  ├─ Dashboard/
│  │  ├─ App.razor
│  │  ├─ Dashboard.csproj
│  │  ├─ Program.cs
│  │  ├─ _Imports.razor
│  │  ├─ Components/
│  │  ├─ Layout/
│  │  ├─ Models/
│  │  ├─ Pages/
│  │  ├─ Properties/
│  │  ├─ Services/
│  │  └─ wwwroot/
│  │
│  ├─ HealthcarePortal/
│  │  ├─ App.razor
│  │  ├─ HealthcarePortal.csproj
│  │  ├─ Program.cs
│  │  ├─ _Imports.razor
│  │  ├─ Layout/
│  │  │  └─ MainLayout.razor
│  │  ├─ Pages/
│  │  ├─ Properties/
│  │  └─ wwwroot/
│  │
│  └─ StarterApp/
│     ├─ App.razor
│     ├─ Program.cs
│     ├─ StarterApp.csproj
│     ├─ _Imports.razor
│     ├─ Features/
│     │  ├─ Dashboard/
│     │  │  ├─ HealthBoardOverview.razor
│     │  │  └─ HealthBoardOverview.razor.css
│     │  └─ Settings/
│     │     └─ AccountSettings.razor
│     ├─ Layout/
│     │  ├─ AppSidebar.razor
│     │  ├─ AppSidebar.razor.cs
│     │  ├─ AppSidebar.razor.css
│     │  ├─ MainLayout.razor
│     │  ├─ MainLayout.razor.css
│     │  ├─ MobileNavMenu.razor
│     │  ├─ MobileNavMenu.razor.cs
│     │  ├─ MobileNavMenu.razor.css
│     │  └─ Models/
│     ├─ Pages/
│     │  └─ NotFound.razor
│     ├─ Properties/
│     └─ wwwroot/
│
├─ src/
│  ├─ CymruBlazor/
│  │  ├─ Accessibility/
│  │  ├─ Components/
│  │  ├─ Contracts/
│  │  ├─ Enums/
│  │  ├─ Extensions/
│  │  ├─ Icons/
│  │  ├─ Services/
│  │  ├─ Themes/
│  │  ├─ wwwroot/
│  │  ├─ _Imports.razor
│  │  └─ CymruBlazor.csproj
│  │
│  └─ CymruBlazor.Demo/
│     ├─ App.razor
│     ├─ CymruBlazor.Demo.csproj
│     ├─ Program.cs
│     ├─ _Imports.razor
│     ├─ Layout/
│     │  ├─ MainLayout.razor
│     │  └─ ... (shared/demo layout files)
│     ├─ Pages/
│     │  ├─ Components/
│     │  │  ├─ Accessibility/
│     │  │  ├─ Branding/
│     │  │  ├─ Button/
│     │  │  ├─ Content/
│     │  │  ├─ Core/
│     │  │  ├─ Feedback/
│     │  │  ├─ Forms/
│     │  │  ├─ Layout/
│     │  │  ├─ Theming/
│     │  │  └─ _Imports.razor
│     │  ├─ GettingStarted/
│     │  ├─ Home.razor
│     │  ├─ Home.razor.cs
│     │  ├─ Home.razor.css
│     │  ├─ Legacy.razor
│     │  └─ NotFound.razor
│     ├─ Properties/
│     ├─ Shared/
│     ├─ wwwroot/
│     └─ ... (demo-specific supporting files)
│
└─ tests/
   ├─ CymruBlazor.AccessibilityTests/
   ├─ CymruBlazor.ApprovalTests/
   └─ CymruBlazor.Tests/
   ```
