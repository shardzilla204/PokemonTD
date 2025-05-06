using System.Collections.Generic;
using Godot;

namespace PokemonTD;

public partial class StageStateInterface : CanvasLayer
{
	[Export]
	private CustomButton _retryButton;

	[Export]
	private CustomButton _leaveButton;

	[Export]
	private Label _message;

	public Label Message => _message;

	public int StageID;

	public override void _Ready()
	{
		_retryButton.Pressed += OnRetryPressed;
		_leaveButton.Pressed += OnLeavePressed;
	}

	private void OnRetryPressed()
	{
		PokemonTD.IsGamePaused = true;
		
		List<PokemonStage> pokemonStages = PokemonTD.PokemonStages.GetPokemonStages();

		int stageIndex = StageID - 1;

		PokemonStage pokemonStage = PokemonTD.PackedScenes.GetPokemonStage(stageIndex);
		pokemonStage.WaveCount = pokemonStages[stageIndex].WaveCount;
		pokemonStage.PokemonNames = pokemonStages[stageIndex].PokemonNames;
		pokemonStage.PokemonPerWave = pokemonStages[stageIndex].PokemonPerWave;
		pokemonStage.PokemonLevels = pokemonStages[stageIndex].PokemonLevels;

		PokemonStage parent = GetParentOrNull<PokemonStage>();
		parent.AddSibling(pokemonStage);
		parent.QueueFree();
	}

	private void OnLeavePressed()
	{
		PokemonStage pokemonStage = GetParentOrNull<PokemonStage>();

		StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
		pokemonStage.AddSibling(stageSelectInterface);

		pokemonStage.QueueFree();
	}
}
