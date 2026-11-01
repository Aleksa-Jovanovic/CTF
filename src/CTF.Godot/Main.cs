using Godot;
using CTF.Core;

public partial class Main : Control
{
	private readonly Game _game = Game.StartActionPhase([
		new PlayerSetup(new PlayerId(1), MegaCredits: 22),
		new PlayerSetup(new PlayerId(2), MegaCredits: 22)
	]);

	private Label _currentPlayerLabel = null!;
	private Label _playerInfoLabel = null!;
	private Label _feedbackLabel = null!;
	private Button _powerPlantButton = null!;
	private Button _endTurnButton = null!;
	private Button _passButton = null!;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_currentPlayerLabel =
			GetNode<Label>("TurnPanel/CurrentPlayerLabel");
		_playerInfoLabel =
			GetNode<Label>("TurnPanel/PlayerInfoLabel");
		_feedbackLabel =
			GetNode<Label>("TurnPanel/FeedbackLabel");

		_powerPlantButton =
			GetNode<Button>("TurnPanel/PowerPlantButton");
		_endTurnButton =
			GetNode<Button>("TurnPanel/EndTurnButton");
		_passButton =
			GetNode<Button>("TurnPanel/PassButton");

		_powerPlantButton.Pressed += OnPowerPlantPressed;
		_endTurnButton.Pressed += OnEndTurnPressed;
		_passButton.Pressed += OnPassPressed;

		UpdateView();
	}

	private void OnPowerPlantPressed()
	{
		if (_game.CurrentPlayer is PlayerId player)
		{
			ShowResult(_game.Apply(new StandardProjectAction(
				player, StandardProjectType.PowerPlant)));
		}
	}

	private void OnEndTurnPressed()
	{
		if (_game.CurrentPlayer is PlayerId player)
		{
			ShowResult(_game.Apply(new EndTurnAction(player)));
		}
	}

	private void OnPassPressed()
	{
		if (_game.CurrentPlayer is PlayerId player)
		{
			ShowResult(_game.Apply(new PassAction(player)));
		}
	}

	private void ShowResult(ActionResult result)
	{
		_feedbackLabel.Text = result.IsSuccess
			? ""
			: result.Error ?? "Action failed.";

		UpdateView();
	}

	private void UpdateView()
	{
		if (_game.CurrentPlayer is PlayerId player)
		{
			var state = _game.GetPlayer(player);

			_currentPlayerLabel.Text = $"Player {player.Value}'s turn";
			_playerInfoLabel.Text =
				$"Credits: {state.MegaCredits}\n" +
				$"Energy production: {state.EnergyProduction}\n" +
				$"Actions this turn: {_game.ActionsTakenThisTurn}";
		}
		else
		{
			_currentPlayerLabel.Text = $"Phase: {_game.Phase}";
			_playerInfoLabel.Text = "";
		}

		var finished = _game.CurrentPlayer is null;
		_powerPlantButton.Disabled = finished;
		_endTurnButton.Disabled = finished;
		_passButton.Disabled = finished;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
