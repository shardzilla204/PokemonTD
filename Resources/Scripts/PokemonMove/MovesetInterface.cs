using Godot;

namespace PokemonTD;

public partial class MovesetInterface : CanvasLayer
{
	[Signal]
	public delegate void PokemonMoveChangedEventHandler(int teamSlotID, PokemonMove pokemonMove);

	[Export]
	private Container _moveOptions;

	[Export]
	private Label _pokemonName;

	[Export]
	private CustomButton _exitButton; 

	[Export]
	private Label _effect;

	public Pokemon Pokemon;
	public int TeamSlotID;

	public override void _Ready()
	{
		_exitButton.Pressed += QueueFree;

		_pokemonName.Text = $"{Pokemon.Name} Moveset";

		ClearMoveButtons();

		PokemonTD.Signals.ChangeMovesetPressed += QueueFree;

		foreach (PokemonMove pokemonMove in Pokemon.Moves)
		{
			MoveOption moveOption = PokemonTD.PackedScenes.GetMoveOption();
			moveOption.PokemonMove = pokemonMove;
			moveOption.MouseEntered += () => SetEffectText(pokemonMove);
			moveOption.Pressed += () => 
			{
				EmitSignal(SignalName.PokemonMoveChanged, TeamSlotID, pokemonMove);

				if (IsInstanceValid(this)) QueueFree();
			};
			_moveOptions.AddChild(moveOption);
		}
	}

	public override void _Notification(int what)
	{
		if (what == NotificationExitTree) PokemonTD.Signals.ChangeMovesetPressed -= QueueFree;
	}

	public void ClearMoveButtons()
	{
		foreach (MoveOption moveOption in _moveOptions.GetChildren())
		{
			moveOption.QueueFree();
		}
	}

	private void SetEffectText(PokemonMove pokemonMove)
	{
		string powerString = pokemonMove.Power == 0 ? "" : $"Power: {pokemonMove.Power}\n";
		string accuracyString = pokemonMove.Accuracy == 0 ? "" : $"Accuracy: {pokemonMove.Accuracy}%\n\n";
		_effect.Text = $"{powerString}{accuracyString}{pokemonMove.Effect}";
	}
}
