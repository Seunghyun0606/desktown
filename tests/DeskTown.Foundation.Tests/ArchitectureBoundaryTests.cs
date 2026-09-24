using System.Xml.Linq;

namespace DeskTown.Foundation.Tests;

public sealed class ArchitectureBoundaryTests
{
    [Fact]
    public void Domain_has_no_project_dependencies()
    {
        var project = LoadProject("src/DeskTown.Domain/DeskTown.Domain.csproj");

        Assert.Empty(ProjectReferences(project));
        Assert.Empty(PackageReferences(project));
    }

    [Fact]
    public void Application_depends_only_on_domain()
    {
        var project = LoadProject("src/DeskTown.Application/DeskTown.Application.csproj");
        var references = ProjectReferences(project);

        var reference = Assert.Single(references);
        Assert.True(
            reference.Replace('\\', '/').EndsWith(
                "DeskTown.Domain/DeskTown.Domain.csproj",
                StringComparison.Ordinal));
    }

    [Fact]
    public void Domain_and_application_contain_no_Godot_or_native_interop()
    {
        var repository = FindRepositoryRoot();
        var roots = new[]
        {
            Path.Combine(repository.FullName, "src", "DeskTown.Domain"),
            Path.Combine(repository.FullName, "src", "DeskTown.Application")
        };

        foreach (var root in roots)
        {
            foreach (var file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                var source = File.ReadAllText(file);
                Assert.False(source.Contains("using Godot", StringComparison.Ordinal));
                Assert.False(source.Contains("global::Godot", StringComparison.Ordinal));
                Assert.False(source.Contains("DllImport", StringComparison.Ordinal));
                Assert.False(source.Contains("LibraryImport", StringComparison.Ordinal));
            }
        }
    }

    private static XDocument LoadProject(string relativePath)
    {
        return XDocument.Load(Path.Combine(FindRepositoryRoot().FullName, relativePath));
    }

    private static IReadOnlyList<string> ProjectReferences(XDocument project)
    {
        return project
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();
    }

    private static IReadOnlyList<string> PackageReferences(XDocument project)
    {
        return project
            .Descendants("PackageReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DeskTown.sln")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new DirectoryNotFoundException("DeskTown repository root was not found.");
    }
}
