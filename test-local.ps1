# test-local.ps1
dotnet restore
dotnet build
dotnet test CymruBlazor.slnx --filter "FullyQualifiedName!~CymruBlazor.AccessibilityTests"
