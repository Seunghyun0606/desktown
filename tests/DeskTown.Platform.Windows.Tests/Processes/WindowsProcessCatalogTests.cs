using DeskTown.Platform.Windows.Processes;

namespace DeskTown.Platform.Windows.Tests.Processes;

public sealed class WindowsProcessCatalogTests
{
    [Fact]
    public void CatalogReturnsDistinctSortedBareProcessNames()
    {
        var source = new FakeRunningProcessSource
        {
            ProcessNames = new[] { "notion", "Code", " code ", "EXCEL", "excel" }
        };

        var names = new WindowsProcessCatalog(source).GetRunningProcessNames();

        Assert.Equal(new[] { "Code", "EXCEL", "notion" }, names);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("C:\\Program Files\\Code.exe")]
    [InlineData("/usr/bin/code")]
    [InlineData("bad\nname")]
    [InlineData("bad:name")]
    public void CatalogRejectsValuesThatCouldExposePathsOrControlData(string unsafeName)
    {
        var source = new FakeRunningProcessSource
        {
            ProcessNames = new[] { "code", unsafeName }
        };

        var names = new WindowsProcessCatalog(source).GetRunningProcessNames();

        Assert.Equal(new[] { "code" }, names);
    }

    [Fact]
    public void CatalogExcludesNamesThatTheSaveCannotPersist()
    {
        var source = new FakeRunningProcessSource
        {
            ProcessNames = new[] { new string('x', 129), "code" }
        };

        Assert.Equal(new[] { "code" },
            new WindowsProcessCatalog(source).GetRunningProcessNames());
    }

    [Fact]
    public void EmptyCatalogIsAValidAnyAppResult()
    {
        var catalog = new WindowsProcessCatalog(new FakeRunningProcessSource());

        Assert.Empty(catalog.GetRunningProcessNames());
    }

    [Theory]
    [MemberData(nameof(RecoverableFailures))]
    public void CatalogFailureDegradesToEmpty(Exception failure)
    {
        var source = new FakeRunningProcessSource { Exception = failure };

        var names = new WindowsProcessCatalog(source).GetRunningProcessNames();

        Assert.Empty(names);
    }

    public static TheoryData<Exception> RecoverableFailures => new()
    {
        new InvalidOperationException(),
        new UnauthorizedAccessException(),
        new PlatformNotSupportedException()
    };
}
