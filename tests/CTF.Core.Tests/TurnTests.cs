using CTF.Core;
using Xunit;

namespace CTF.Core.Tests;

public sealed class TurnTests
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

    [Fact]
    public void Two_actions_end_the_current_turn()
    {
        var first = new PlayerId(1);
        var second = new PlayerId(2);
        var game = Game.StartActionPhase([
            new PlayerSetup(first, MegaCredits: 22),
            new PlayerSetup(second)
        ]);
        var action = new StandardProjectAction(first, StandardProjectType.PowerPlant);

        Assert.True(game.Apply(action).IsSuccess);
        Assert.True(game.Apply(action).IsSuccess);

        Assert.Equal(0, game.GetPlayer(first).MegaCredits);
        Assert.Equal(2, game.GetPlayer(first).EnergyProduction);
        Assert.Equal(second, game.CurrentPlayer);
        Assert.Equal(0, game.ActionsTakenThisTurn);
    }

    [Fact]
    public void Ending_turn_after_one_action_advances_without_passing()
    {
        var first = new PlayerId(1);
        var second = new PlayerId(2);
        var game = Game.StartActionPhase([
            new PlayerSetup(first, MegaCredits: 11),
            new PlayerSetup(second)
        ]);

        Assert.True(game.Apply(
            new StandardProjectAction(first, StandardProjectType.PowerPlant)
        ).IsSuccess);

        var result = game.Apply(new EndTurnAction(first));

        Assert.True(result.IsSuccess);
        Assert.Equal(second, game.CurrentPlayer);
        Assert.Equal(0, game.ActionsTakenThisTurn);
        Assert.False(game.HasPassed(first));
        Assert.Equal(GamePhase.Action, game.Phase);
    }

    [Fact]
    public void Ending_turn_without_an_action_is_rejected()
    {
        var first = new PlayerId(1);
        var second = new PlayerId(2);
        var game = Game.StartActionPhase([first, second]);

        var result = game.Apply(new EndTurnAction(first));

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(first, game.CurrentPlayer);
        Assert.Equal(0, game.ActionsTakenThisTurn);
        Assert.False(game.HasPassed(first));
        Assert.Equal(GamePhase.Action, game.Phase);
    }

    [Fact]
    public void Passing_after_one_action_is_rejected()
    {
        var first = new PlayerId(1);
        var second = new PlayerId(2);
        var game = Game.StartActionPhase([
            new PlayerSetup(first, MegaCredits: 11),
            new PlayerSetup(second)
        ]);

        Assert.True(game.Apply(
            new StandardProjectAction(first, StandardProjectType.PowerPlant)
        ).IsSuccess);

        var before = game.GetPlayer(first);
        var result = game.Apply(new PassAction(first));

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(first, game.CurrentPlayer);
        Assert.Equal(1, game.ActionsTakenThisTurn);
        Assert.False(game.HasPassed(first));
        Assert.Equal(before, game.GetPlayer(first));
        Assert.Equal(GamePhase.Action, game.Phase);
    }

    [Fact]
    public void Last_unpassed_player_can_continue_until_passing()
    {
        var first = new PlayerId(1);
        var second = new PlayerId(2);
        var game = Game.StartActionPhase([
            new PlayerSetup(first),
            new PlayerSetup(second, MegaCredits: 22)
        ]);

        Assert.True(game.Apply(new PassAction(first)).IsSuccess);

        var action = new StandardProjectAction(
            second, StandardProjectType.PowerPlant);

        Assert.True(game.Apply(action).IsSuccess);
        Assert.True(game.Apply(action).IsSuccess);

        Assert.Equal(second, game.CurrentPlayer);
        Assert.Equal(0, game.ActionsTakenThisTurn);
        Assert.Equal(GamePhase.Action, game.Phase);
        Assert.False(game.HasPassed(second));

        Assert.True(game.Apply(new PassAction(second)).IsSuccess);

        Assert.Equal(GamePhase.Production, game.Phase);
        Assert.Null(game.CurrentPlayer);
    }
}
