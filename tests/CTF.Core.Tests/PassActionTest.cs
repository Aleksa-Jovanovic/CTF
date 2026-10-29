using CTF.Core;
using Xunit;

namespace CTF.Core.Tests;

public sealed class PassActionTest
{
    [Fact]
    public void Passing_advances_to_next_player()
    {
        var first = new PlayerId(1);
        var second = new PlayerId(2);
        var game = Game.StartActionPhase([first, second]);

        // API: Game owns the state, and callers can observe it but change it through Apply.
        var result = game.Apply(new PassAction(first));

        Assert.True(result.IsSuccess);
        Assert.True(game.HasPassed(first));
        Assert.Equal(second, game.CurrentPlayer);
    }

    [Fact]
    public void Out_of_turn_pass_is_rejected_without_changing_state()
    {
        var first = new PlayerId(1);
        var second = new PlayerId(2);
        var game = Game.StartActionPhase([first, second]);

        var result = game.Apply(new PassAction(second));

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.False(game.HasPassed(second));
        Assert.Equal(first, game.CurrentPlayer);
    }

    [Fact]
    public void Passing_by_every_player_ends_the_action_phase()
    {
        var first = new PlayerId(1);
        var second = new PlayerId(2);
        var game = Game.StartActionPhase([first, second]);

        game.Apply(new PassAction(first));
        game.Apply(new PassAction(second));

        Assert.Equal(GamePhase.Production, game.Phase);
        Assert.Null(game.CurrentPlayer);
    }
}
