using Godot;

namespace PokemonTD;

public partial class MovesetInterface : CanvasLayer
{
	[Signal]
	public delegate void PokemonMoveChangedEventHandler(Pokemon pokemon, int pokemonTeamIndex, PokemonMove pokemonMove);

	[Export]
	private Container _moveOptions;

	[Export]
	private Label _pokemonName;

	[Export]
	private CustomButton _exitButton; 

	[Export]
	private Label _effect;

	private Pokemon _pokemon;
	private int _pokemonTeamIndex;
	

    public override void _ExitTree()
    {
        PokemonTD.Signals.ChangeMovesetPressed -= QueueFree;
		PokemonTD.Signals.Dragging -= Dragging;
        PokemonTD.Keybinds.ChangePokemonMove -= KeybindPressed;
    }

	public override void _Ready()
	{
		PokemonTD.Signals.ChangeMovesetPressed += QueueFree;
		PokemonTD.Signals.Dragging += Dragging;
		PokemonTD.Keybinds.ChangePokemonMove += KeybindPressed;

		_pokemonName.Text = $"{_pokemon.Name}'s Moves";

		_exitButton.Pressed += QueueFree;

		SetEffectText(_pokemon.Move);

		ClearMoveOptions();
		AddMoveOptions();
	}

	private void AddMoveOptions()
	{
		foreach (PokemonMove pokemonMove in _pokemon.Moves)
		{
			MoveOption moveOption = PokemonTD.PackedScenes.GetMoveOption();
			moveOption.PokemonMove = pokemonMove;
			moveOption.MouseEntered += () => SetEffectText(pokemonMove);
			moveOption.Pressed += () =>
			{
				_pokemon.Move = moveOption.PokemonMove;
				EmitSignal(SignalName.PokemonMoveChanged, _pokemon, _pokemonTeamIndex, pokemonMove);

				if (IsInstanceValid(this)) QueueFree();
			};
			_moveOptions.AddChild(moveOption);
		}
	}

	private void ClearMoveOptions()
	{
		foreach (MoveOption moveOption in _moveOptions.GetChildren())
		{
			moveOption.QueueFree();
		}
	}

	public void SetPokemon(Pokemon pokemon, int pokemonTeamIndex)
	{
		_pokemon = pokemon;
		_pokemonTeamIndex = pokemonTeamIndex;
	}

	private void SetEffectText(PokemonMove pokemonMove)
	{
		string power = pokemonMove.Power == 0 ? "" : $"Power: {pokemonMove.Power}\n";
		string accuracy = pokemonMove.Accuracy == 0 ? "" : $"Accuracy: {pokemonMove.Accuracy}%";
		string effect = pokemonMove.Effect == "" ? "" : $"{pokemonMove.Effect}";
		if (effect != "" && accuracy != "") accuracy += "\n\n";

		_effect.Text = $"{power}{accuracy}{effect}";
	}

	private void KeybindPressed(int pokemonTeamIndex)
	{
		QueueFree();
	}

	private void Dragging(bool isDragging)
	{
		QueueFree();
	}
}
