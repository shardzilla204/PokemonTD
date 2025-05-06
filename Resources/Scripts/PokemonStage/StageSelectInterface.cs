using Godot;

namespace PokemonTD;

public partial class StageSelectInterface : Node
{
	[Export]
	private CustomButton _pokemonTeamButton;

	[Export]
	private CustomButton _exitButton;

	[Export]
	private Container _stageSelectButtons;

    public override void _ExitTree()
    {
		PokemonTD.Signals.StageSelectButtonPressed -= OnStageSelectButtonPressed;
    }

	public override void _Ready()
	{	
		// Stop music from resetting if coming back from a stage
		if (PokemonTD.AudioManager.IsPlayingSong(1) /* 01. ~Opening~ */
		|| PokemonTD.AudioManager.IsPlayingSong(11) /* 11. Pokémon Center */) 
		{
			PokemonTD.Signals.EmitSignal(Signals.SignalName.OnStageSelection);
		}

		PokemonTD.Signals.StageSelectButtonPressed += OnStageSelectButtonPressed;

		_pokemonTeamButton.Pressed += () =>
		{
			PokeCenterInterface pokeCenterInterface = PokemonTD.PackedScenes.GetPokeCenterInterface();
			AddSibling(pokeCenterInterface);
			QueueFree();

			PokemonTD.AudioManager.PlayButtonPressed();
		};
		_exitButton.Pressed += () => 
		{
			MenuInterface menuInterface = PokemonTD.PackedScenes.GetMenuInterface();
			AddSibling(menuInterface);
			QueueFree();

			PokemonTD.AudioManager.PlayButtonPressed();
		};

		_exitButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
		_pokemonTeamButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;

		ClearStageSelectButtons();
		AddStageSelectButtons();
	}

	private void OnStageSelectButtonPressed(PokemonStage pokemonStage)
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
		foreach (PokemonStage pokemonStage in PokemonTD.PokemonStages.GetPokemonStages())
		{
			StageSelectButton stageSelectButton = PokemonTD.PackedScenes.GetStageSelectButton();
			stageSelectButton.PokemonStage = pokemonStage;

			_stageSelectButtons.AddChild(stageSelectButton);
		}
	}
}
