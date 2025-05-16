using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class ForgetMoveInterface : CanvasLayer
{
	[Signal]
	public delegate void FinishedEventHandler();

	[Export]
	private Label _pokemonName;

	[Export]
	private MoveOption _moveToLearnOption;

	[Export]
	private MoveOption _moveToForgetOption;

	[Export]
	private Container _moveForgetOptions;

	[Export]
	private Label _moveDetails;

	[Export]
	private CustomButton _swap;

	public Pokemon Pokemon;
	public PokemonMove MoveToLearn;

	private PokemonMove _moveToForget;

	private List<MoveOption> _moveOptions = new List<MoveOption>();

	public override void _Ready()
	{
		_pokemonName.Text = $"Your {Pokemon.Name} wants to learn";

		ClearMoves();
		SetMovesToSwap();
		_moveToLearnOption.UpdateOption(MoveToLearn);
		_moveToForgetOption.UpdateOption(_moveToForget);

		// Resume game after deciding
		_swap.Pressed += () => 
		{
			if (_moveToForget != MoveToLearn) ForgetMove();

			QueueFree();
		};

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPause);
	}

	private void ClearMoves()
	{
		// Clear Current Move To Learn
		foreach (Node child in _moveToLearnOption.GetChildren())
		{
			if (child is CustomButton moveOption) moveOption.QueueFree();
		}

		// Clear Moves You Can Forget
		foreach (Node child in _moveForgetOptions.GetChildren())
		{
			child.QueueFree();
		}
	}

	private MoveOption GetMoveOption(PokemonMove pokemonMove)
	{
		Vector2 minimumSize = new Vector2(400, 75);
		int fontSize = 30;

		MoveOption moveOption = PokemonTD.PackedScenes.GetMoveOption();
		moveOption.PokemonMove = pokemonMove;
		moveOption.CustomMinimumSize = minimumSize;
		moveOption.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter; 
		moveOption.SetFontSize(fontSize);
		moveOption.MouseEntered += () => OnMoveOptionMouseEntered(pokemonMove);

		return moveOption;
	}

	private void SetMovesToSwap()
	{
		// Add move options based on move count
		foreach (PokemonMove pokemonMove in Pokemon.Moves)
		{
			MoveOption moveOption = GetMoveOption(pokemonMove);
			moveOption.UpdateOption(pokemonMove);

			_moveForgetOptions.AddChild(moveOption);
			_moveOptions.Add(moveOption);
		}

		MoveOption moveLearnOption = GetMoveOption(MoveToLearn);
		_moveForgetOptions.AddChild(moveLearnOption);
		_moveOptions.Add(moveLearnOption);

		// Add ability to toggle between options
		foreach (MoveOption moveOption in _moveOptions)
		{
			moveOption.Pressed += () => OnMoveOptionPressed(moveOption);
		}

		// ! Use a button group instead
		float darknessPercentage = 0.25f;
		MoveOption firstMoveOption = _moveOptions[0];
		firstMoveOption.Modulate = Colors.White.Darkened(darknessPercentage);

		_moveToForget = Pokemon.Moves[0];
		_moveToForgetOption.UpdateOption(_moveToForget);
	}

	private void OnMoveOptionMouseEntered(PokemonMove pokemonMove)
	{
		_moveDetails.Text = "";
		_moveDetails.Text += pokemonMove.Power == 0 ? "" : $"Power: {pokemonMove.Power}\n";
		_moveDetails.Text += pokemonMove.Accuracy == 0 ? "" : $"Accuracy: {pokemonMove.Accuracy}\n";
		_moveDetails.Text += pokemonMove.Effect == "" ? "" : $"\n{pokemonMove.Effect}";
	}

	private void OnMoveOptionPressed(MoveOption moveOption)
	{
		Color colorWhite = Colors.White;
		float darknessPercentage = 0.25f;

		List<MoveOption> otherMoveOptions = _moveOptions.FindAll(otherMoveOption => otherMoveOption != moveOption);
		foreach (MoveOption otherMoveOption in otherMoveOptions)
		{
			otherMoveOption.Modulate = colorWhite;
		}

		moveOption.Modulate = colorWhite.Darkened(darknessPercentage);

		_moveToForget = moveOption.PokemonMove;
		_moveToForgetOption.UpdateOption(moveOption.PokemonMove);
	}

	private void ForgetMove()
	{
		int moveIndex = Pokemon.Moves.IndexOf(_moveToForget);
		Pokemon.Moves.Remove(_moveToForget);
		Pokemon.Moves.Insert(moveIndex, MoveToLearn);

		// Print Message To Console
		string forgotMoveMessage = $"{Pokemon.Name} Forgot {_moveToForget.Name} For {MoveToLearn.Name}";
		PrintRich.PrintLine(TextColor.Purple, forgotMoveMessage);

		PokemonMoves.Instance.RemoveFromQueue(this);
		PokemonMoves.Instance.IsQueueEmpty();
		EmitSignal(SignalName.Finished);
	}
}
