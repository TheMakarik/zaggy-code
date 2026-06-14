using System.Reflection;
using FluentAssertions;

namespace ZaggyCode.Data.Tests;

public sealed class SealedClassesTests
{
    [Fact]
    public void AllClasses_MustBeSealed()
    {
        //Arrange
        var classes = Assembly.GetAssembly(typeof(GameCodeStorage))!
            .GetTypes()!
            .Where(type => type is { IsClass: true, IsAbstract: false });

        //Assert
        classes.All(x => x.IsSealed).Should().BeTrue();
    }
}