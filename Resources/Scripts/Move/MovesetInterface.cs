using Godot;

namespace PokemonTD;

public partial class MovesetInterface : CanvasLayer
{
	[Signal]
	public delegate void PokemonMoveChangedEventHandler(int id, PokemonMove pokemonMove);

	[Export]
	private Container _moveOptions;

	[Export]
	private Label _pokemonName;

	[Export]
	private CustomButton _exitButton; 

	[Export]
	private Label _effect;

	public int TeamSlotIndex;
	
	public Pokemon Pokemon;

    public override void _ExitTree()
    {
        PokemonTD.Signals.ChangeMovesetPressed -= QueueFree;
		PokemonTD.Signals.DraggingPokemonTeamSlot -= DraggingTeamSlot;
		PokemonTD.Signals.DraggingPokemonStageSlot -= DraggingStageSlot;
		PokemonTD.Signals.DraggingPokeBall -= Dragging;
    }

	public override void _Ready()
	{
		PokemonTD.Signals.ChangeMovesetPressed += QueueFree;
		PokemonTD.Signals.DraggingPokemonTeamSlot += DraggingTeamSlot;
		PokemonTD.Signals.DraggingPokemonStageSlot += DraggingStageSlot;
		PokemonTD.Signals.DraggingPokeBall += Dragging;

		_pokemonName.Text = $"{Pokemon.Name}'s Moves";

		_exitButton.Pressed += QueueFree;

		ClearMoveOptions();
		AddMoveOptions();
	}

	private void AddMoveOptions()
	{
		foreach (PokemonMove pokemonMove in Pokemon.Moves)
		{
			MoveOption moveOption = PokemonTD.PackedScenes.GetMoveOption();
			moveOption.PokemonMove = pokemonMove;
			moveOption.MouseEntered += () => SetEffectText(pokemonMove);
			moveOption.Pressed += () =>
			{
				EmitSignal(SignalName.PokemonMoveChanged, TeamSlotIndex, pokemonMove);

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

	private void SetEffectText(PokemonMove pokemonMove)
	{
		string power = pokemonMove.Power == 0 ? "" : $"Power: {pokemonMove.Power}\n";
		string accuracy = pokemonMove.Accuracy == 0 ? "" : $"Accuracy: {pokemonMove.Accuracy}%";
		string effect = pokemonMove.Effect == "" ? "" : $"{pokemonMove.Effect}";
        if (effect != "" && accuracy != "") accuracy += "\n\n";

		_effect.Text = $"{power}{accuracy}{effect}";
	}

	private void DraggingTeamSlot(PokemonTeamSlot pokemonTeamSlot, bool isDragging)
	{
		Dragging(isDragging);
	}

	private void DraggingStageSlot(PokemonStageSlot pokemonStageSlot, bool isDragging)
	{
		Dragging(isDragging);
	}

	private void Dragging(bool isDragging)
	{
		QueueFree();
	}
}
