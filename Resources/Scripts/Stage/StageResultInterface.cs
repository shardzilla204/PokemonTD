using System.Collections.Generic;
using Godot;

namespace PokemonTD;

public partial class StageResultInterface : CanvasLayer
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
		
		PokemonStage pokemonStageData = PokemonStages.Instance.FindPokemonStage(StageID);
		PokemonStage pokemonStage = GetPokemonStage(pokemonStageData);

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

	private PokemonStage GetPokemonStage(PokemonStage pokemonStageData)
	{
		int stageIndex = StageID - 1;
		PokemonStage pokemonStage = PokemonTD.PackedScenes.GetPokemonStage(stageIndex);
		pokemonStage.ID = stageIndex;
		pokemonStage.WaveCount = pokemonStageData.WaveCount;
		pokemonStage.PokemonNames = pokemonStageData.PokemonNames;
		pokemonStage.PokemonPerWave = pokemonStageData.PokemonPerWave;
		pokemonStage.PokemonLevels = pokemonStageData.PokemonLevels;
		return pokemonStage;
	}
}
