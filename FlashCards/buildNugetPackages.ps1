nuget pack .\FlashCards.nuspec -IncludeReferencedProjects -Prop Configuration=Release
nuget push *.nupkg