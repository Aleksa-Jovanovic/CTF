using CTF.Core;
using Xunit;

namespace CTF.Core.Tests;

public sealed class GameSetupTests
{
    [Fact]
    public void Duplicate_player_ids_are_rejected()
    {
        var player = new PlayerId(1);

        Assert.Throws<ArgumentException>(
            () => Game.StartActionPhase([player, player]));
    }
}