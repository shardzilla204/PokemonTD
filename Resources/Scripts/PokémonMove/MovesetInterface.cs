using Godot;

namespace PokémonTD;

public partial class MovesetInterface : CanvasLayer
{
	[Signal]
	public delegate void PokémonMoveChangedEventHandler(int teamSlotID, PokémonMove pokémonMove);

	[Export]
	private Container _moveButtonsContainer;

	[Export]
	private Label _pokémonNameLabel;

	[Export]
	private CustomButton _exitButton; 

	[Export]
	private Label _effectLabel;

	public Pokémon Pokémon;
	public int TeamSlotID;

	public override void _Ready()
	{
		_exitButton.Pressed += QueueFree;

		_pokémonNameLabel.Text = $"{Pokémon.Name} Moveset";

		ClearMoveButtons();

		PokémonTD.Signals.ChangeMovesetPressed += QueueFree;

		foreach (PokémonMove pokémonMove in Pokémon.Moves)
		{
			MoveButton moveButton = PokémonTD.PackedScenes.GetMoveButton();
			moveButton.PokémonMove = pokémonMove;
			moveButton.MouseEntered += () => SetEffectText(pokémonMove);
			moveButton.Pressed += () => 
			{
				EmitSignal(SignalName.PokémonMoveChanged, TeamSlotID, pokémonMove);

				if (IsInstanceValid(this)) QueueFree();
			};
			_moveButtonsContainer.AddChild(moveButton);
		}
	}

	public override void _Notification(int what)
	{
		if (what == NotificationExitTree) PokémonTD.Signals.ChangeMovesetPressed -= QueueFree;
	}

	public void ClearMoveButtons()
	{
		foreach (MoveButton moveButton in _moveButtonsContainer.GetChildren())
		{
			_moveButtonsContainer.RemoveChild(moveButton);
			moveButton.QueueFree();
		}
	}

	private void SetEffectText(PokémonMove pokémonMove)
	{
		string powerString = pokémonMove.Power == 0 ? "" : $"Power: {pokémonMove.Power}\n";
		string accuracyString = pokémonMove.Accuracy == 0 ? "" : $"Accuracy: {pokémonMove.Accuracy}%\n\n";
		_effectLabel.Text = $"{powerString}{accuracyString}{pokémonMove.Effect}";
	}
}
