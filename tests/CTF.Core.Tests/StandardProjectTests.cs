using CTF.Core;
using Xunit;

namespace CTF.Core.Tests;

public sealed class StandardProjectTests
{
    [Fact]
    public void Power_plant_spends_credits_and_increases_energy_production()
    {
        var first = new PlayerId(1);
        var second = new PlayerId(2);
        var game = Game.StartActionPhase([
            new PlayerSetup(first, MegaCredits: 11),
            new PlayerSetup(second, MegaCredits: 0)
        ]);

        var result = game.Apply(
            new StandardProjectAction(first, StandardProjectType.PowerPlant));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, game.GetPlayer(first).MegaCredits);
        Assert.Equal(1, game.GetPlayer(first).EnergyProduction);
        Assert.Equal(first, game.CurrentPlayer);
        Assert.Equal(1, game.ActionsTakenThisTurn);
    }

    [Fact]
    public void Unaffordable_power_plant_changes_nothing()
    {
        var first = new PlayerId(1);
        var second = new PlayerId(2);
        var game = Game.StartActionPhase([
            new PlayerSetup(first, MegaCredits: 10),
            new PlayerSetup(second)
        ]);

        var result = game.Apply(
            new StandardProjectAction(first, StandardProjectType.PowerPlant));

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(10, game.GetPlayer(first).MegaCredits);
        Assert.Equal(0, game.GetPlayer(first).EnergyProduction);
        Assert.Equal(first, game.CurrentPlayer);
        Assert.Equal(0, game.ActionsTakenThisTurn);
    }
}
