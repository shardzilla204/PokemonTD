using Godot;

namespace PokemonTD;

public partial class StageSelectButton : CustomButton
{
	public PokemonStage PokemonStage;

	private StageSelectInterface _stageSelectInterface;

    public override void _Ready()
    {
		Text = $"{PokemonStage.ID}";

		_stageSelectInterface = GetParentOrNull<Node>().GetOwnerOrNull<StageSelectInterface>();

        Pressed += SetStage;
		MouseEntered += () => 
		{
			SetPokemonEnemies();
			SetStageThumbnail();
		};
    }

	private void SetStage()
	{
		PokemonStage pokemonStage = GetPokemonStage();

		_stageSelectInterface.AddSibling(pokemonStage);
		_stageSelectInterface.QueueFree();
	}

	// Create a scene and copy and paste the variables
	private PokemonStage GetPokemonStage()
	{
		PokemonStage pokemonStage = PokemonTD.PackedScenes.GetPokemonStage(PokemonStage.ID - 1);
		pokemonStage.WaveCount = PokemonStage.WaveCount;
		pokemonStage.PokemonNames = PokemonStage.PokemonNames;
		pokemonStage.PokemonPerWave = PokemonStage.PokemonPerWave;
		pokemonStage.PokemonLevels = PokemonStage.PokemonLevels;

		return pokemonStage;
	}

	private void SetPokemonEnemies()
	{
		_stageSelectInterface.ClearPokemonEnemySprites();
		_stageSelectInterface.AddPokemonEnemySprites(PokemonStage.ID);
	}

	private void SetStageThumbnail()
	{
		_stageSelectInterface.SetStageThumbnailTexture(PokemonStage.ID);
	}
}
