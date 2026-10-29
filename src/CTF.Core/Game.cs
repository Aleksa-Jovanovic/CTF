namespace CTF.Core;

public readonly record struct PlayerId(int Value);
public sealed record PassAction(PlayerId PlayerId);
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

    private Game(PlayerId[] turnOrder)
    {
        _turnOrder = turnOrder;
    }

    public GamePhase Phase { get; private set; } = GamePhase.Action;

    public PlayerId? CurrentPlayer => 
        Phase == GamePhase.Action ? _turnOrder[_currentPlayerIndex] : null;

    public static Game StartActionPhase(IEnumerable<PlayerId> players)
    {
        var turnOrder = players.ToArray();
        if (turnOrder.Distinct().Count() != turnOrder.Length)
        {
            throw new ArgumentException("Player IDs must be unique.", nameof(players));
        }

        if (turnOrder.Length < 2)
        {
            throw new ArgumentException("At least two players are required.", nameof(players));
        }

        return new Game(turnOrder);
    }

    public bool HasPassed(PlayerId playerId) => _passed.Contains(playerId);

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

        _passed.Add(action.PlayerId);
        if (_passed.Count == _turnOrder.Length)
        {
            Phase = GamePhase.Production;
            return new ActionResult(true);
        }

        do
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _turnOrder.Length;
        }
        while (_passed.Contains(_turnOrder[_currentPlayerIndex]));

        return new ActionResult(true);
    }

}
