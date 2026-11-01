using System.ComponentModel;

namespace CTF.Core;

public readonly record struct PlayerId(int Value);

[Description("PlayerSetup - PlayerId, MegaCredits, EnergyProduction")]
public readonly record struct PlayerSetup(PlayerId Id, int MegaCredits = 0, int EnergyProduction = 0);

[Description("PlayerState - PlayerId, MegaCredits, EnergyProduction")]
public sealed record PlayerState(PlayerId Id, int MegaCredits, int EnergyProduction);

// Actions
public enum StandardProjectType { PowerPlant }
public sealed record StandardProjectAction(PlayerId PlayerId, StandardProjectType Type);
public sealed record PassAction(PlayerId PlayerId);
public sealed record EndTurnAction(PlayerId PlayerId);
public sealed record ActionResult(bool IsSuccess, string? Error = null);

public enum GamePhase
{
    Action,
    Production
}

public sealed class Game
{
    private readonly PlayerId[] _turnOrder;
    private readonly HashSet<PlayerId> _passed = [];
    private int _currentPlayerIndex;

    private const int PowerPlantCost = 11;
    private readonly Dictionary<PlayerId, PlayerState> _players;

    private Game(PlayerSetup[] setup)
    {
        _turnOrder = setup.Select(p => p.Id).ToArray();
        _players = setup.ToDictionary(
            p => p.Id,
            p => new PlayerState(p.Id, p.MegaCredits, p.EnergyProduction)
        );
    }

    public GamePhase Phase { get; private set; } = GamePhase.Action;
    public int ActionsTakenThisTurn { get; private set; }
    public PlayerState GetPlayer(PlayerId id) => _players[id];

    public PlayerId? CurrentPlayer => 
        Phase == GamePhase.Action ? _turnOrder[_currentPlayerIndex] : null;

    public static Game StartActionPhase(IEnumerable<PlayerId> players) =>
        StartActionPhase(players.Select(id => new PlayerSetup(id)));
    public static Game StartActionPhase(IEnumerable<PlayerSetup> players)
    {
        var setup = players.ToArray();

        if (setup.Length < 2)
            throw new ArgumentException("At least two players are required.", nameof(players));

        if (setup.Select(p => p.Id).Distinct().Count() != setup.Length)
            throw new ArgumentException("Player IDs must be unique.", nameof(players));

        return new Game(setup);
    }

    public bool HasPassed(PlayerId playerId) => _passed.Contains(playerId);

    private void AdvanceToNextUnpassedPlayer()
    {
        do
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _turnOrder.Length;
        }
        while (_passed.Contains(_turnOrder[_currentPlayerIndex]));

        ActionsTakenThisTurn = 0;
    }

    public ActionResult Apply(PassAction action)
    {
        if (Phase != GamePhase.Action)
        {
            return new ActionResult(false, "The action phase has ended.");
        }

        if (action.PlayerId != CurrentPlayer)
        {
            return new ActionResult(false, "It is not this player's turn.");
        }

        if (ActionsTakenThisTurn == 1)
        {
            return new ActionResult(false, "You can't pass if you already took one action.");
        }

        _passed.Add(action.PlayerId);
        if (_passed.Count == _turnOrder.Length)
        {
            Phase = GamePhase.Production;
            return new ActionResult(true);
        }

        AdvanceToNextUnpassedPlayer();
        return new ActionResult(true);
    }

    public ActionResult Apply(EndTurnAction action)
    {
        if (Phase != GamePhase.Action)
        {
            return new ActionResult(false, "The action phase has ended.");
        }

        if (action.PlayerId != CurrentPlayer)
        {
            return new ActionResult(false, "It is not this player's turn.");
        }

        if (ActionsTakenThisTurn == 0)
        {
            return new ActionResult(false, "You can't end turn as a first action.");
        }
        
        AdvanceToNextUnpassedPlayer();
        return new ActionResult(true);
    }

    public ActionResult Apply(StandardProjectAction action)
    {
        if (Phase != GamePhase.Action)
            return new ActionResult(false, "The action phase has ended.");

        if (action.PlayerId != CurrentPlayer)
            return new ActionResult(false, "It is not this player's turn.");

        if (action.Type != StandardProjectType.PowerPlant)
            return new ActionResult(false, "Unknown standard project.");

        var player = _players[action.PlayerId];

        if (player.MegaCredits < PowerPlantCost)
            return new ActionResult(false, "Not enough MegaCredits.");

        // With makes a shallow copy!
        _players[action.PlayerId] = player with
        {
            MegaCredits = player.MegaCredits - PowerPlantCost,
            EnergyProduction = player.EnergyProduction + 1
        };

        ActionsTakenThisTurn++;

        if (ActionsTakenThisTurn == 2)
        {
            AdvanceToNextUnpassedPlayer();
        }


        return new ActionResult(true);
    }

}
