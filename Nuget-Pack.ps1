$thisDir = (Split-Path $MyInvocation.MyCommand.Path)
Set-Alias nuget "$thisDir/.nuget/Nuget.exe"
nuget update -Self
nuget pack EFHooks.nuspec -Prop Configuration=Release -IncludeReferencedProjects