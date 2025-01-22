param(
    [string]$version
)

# Strip leading v from version
$version = $version.Substring(1)

# Set build version
$content = Get-Content -Path 'src/LimbusCustomSound/Core/PluginInfo.cs' -Raw
# Replace string Version = "x.y.z" with $version
$updatedContent = $content -replace 'Version = ".*"', "Version = ""$version"""
Set-Content -Path 'src/LimbusCustomSound/Core/PluginInfo.cs' -Value $updatedContent

dotnet build LimbusCustomSound.sln -c Release /p:Version=$version