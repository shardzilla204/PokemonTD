using Godot;

namespace PokemonTD;

public partial class StageSelectInterface : Node
{
	[Export]
	private CustomButton _exitButton;

	[Export]
	private CustomButton _pokeCenterButton;

	[Export]
	private CustomButton _pokeMartButton;

	[Export]
	private Container _stageSelectButtons;

    public override void _ExitTree()
    {
		PokemonTD.Signals.StageSelectButtonPressed -= StageSelectButtonPressed;
    }

	public override void _Ready()
	{	
		// Stop music from resetting if coming back from a stage
		if (PokemonTD.AudioManager.IsPlayingSong(1) /* 01. ~Opening~ */
		|| PokemonTD.AudioManager.IsPlayingSong(11) /* 11. Pokémon Center */) 
		{
			PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.StageSelected);
		}

		PokemonTD.Signals.StageSelectButtonPressed += StageSelectButtonPressed;

		_exitButton.Pressed += () => 
		{
			MenuInterface menuInterface = PokemonTD.PackedScenes.GetMenuInterface();
			AddSibling(menuInterface);
			QueueFree();
		};
		_pokeCenterButton.Pressed += () =>
		{
			PokeCenterInterface pokeCenterInterface = PokemonTD.PackedScenes.GetPokeCenterInterface();
			AddSibling(pokeCenterInterface);
			QueueFree();
		};
		_pokeMartButton.Pressed += () =>
		{
			PokeMartInterface pokeMartInterface = PokemonTD.PackedScenes.GetPokeMartInterface();
			AddSibling(pokeMartInterface);
			QueueFree();
		};

		ClearStageSelectButtons();
		AddStageSelectButtons();
	}

	private void StageSelectButtonPressed(PokemonStage pokemonStage)
	{
		AddSibling(pokemonStage);
		QueueFree();
	}

	private void ClearStageSelectButtons()
	{
		foreach (StageSelectButton stageSelectButton in _stageSelectButtons.GetChildren())
		{
			_stageSelectButtons.RemoveChild(stageSelectButton);
			stageSelectButton.QueueFree();
		}
	}

	private void AddStageSelectButtons()
	{
		foreach (PokemonStage pokemonStage in PokemonStages.Instance.GetPokemonStages())
		{
			StageSelectButton stageSelectButton = PokemonTD.PackedScenes.GetStageSelectButton();
			stageSelectButton.PokemonStage = pokemonStage;

			_stageSelectButtons.AddChild(stageSelectButton);
		}
	}
}
