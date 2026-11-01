using Godot;
using CTF.Core;

public partial class Main : Control
{
	private readonly Game _game = Game.StartActionPhase([
		new PlayerId(1), new PlayerId(2)
	]);

	private Label _currentPlayerLabel = null;
	private Button _passButton = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_currentPlayerLabel = GetNode<Label>("TurnPanel/CurrentPlayerLabel");
		_passButton = GetNode<Button>("TurnPanel/PassButton");

		_passButton.Pressed += OnPassPressed;

		GD.Print("Main ready — button connected");

		UpdateView();
	}

	private void OnPassPressed()
	{
		GD.Print("Pass pressed");
		
		if (_game.CurrentPlayer is not PlayerId player)
			return;

		var result = _game.Apply(new PassAction(player));

		if (!result.IsSuccess)
		{
			GD.PushError(result.Error ?? "Pass failed.");
			return;
		}

		UpdateView();
	}

	private void UpdateView()
	{
		_currentPlayerLabel.Text = _game.CurrentPlayer is PlayerId player
			? $"Player {player.Value}'s turn"
			: $"Phase: {_game.Phase}";
		
		_passButton.Disabled = _game.CurrentPlayer is null;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
