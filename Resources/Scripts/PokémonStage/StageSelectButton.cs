using Godot;

namespace PokémonTD;

public partial class StageSelectButton : CustomButton
{
	public PokémonStage PokémonStage;

	private StageSelectInterface _stageSelectInterface;

    public override void _Ready()
    {
		Text = $"{PokémonStage.ID}";
		_stageSelectInterface = GetParentOrNull<Node>().GetOwnerOrNull<StageSelectInterface>();

        Pressed += SetStage;
		MouseEntered += () => 
		{
			SetEnemyPokémon();
			SetStageThumbnail();
		};
    }

	private void SetStage()
	{
		PokémonStage pokémonStage = GetPokémonStage();

		_stageSelectInterface.AddSibling(pokémonStage);
		_stageSelectInterface.QueueFree();
	}

	// Create a scene and copy and paste the variables
	private PokémonStage GetPokémonStage()
	{
		PokémonStage pokémonStage = PokémonTD.PackedScenes.GetPokémonStage(PokémonStage.ID - 1);
		pokémonStage.WaveCount = PokémonStage.WaveCount;
		pokémonStage.PokémonNames = PokémonStage.PokémonNames;
		pokémonStage.PokémonPerWave = PokémonStage.PokémonPerWave;
		pokémonStage.PokémonLevels = PokémonStage.PokémonLevels;

		return pokémonStage;
	}

	private void SetEnemyPokémon()
	{
		_stageSelectInterface.ClearEnemyPokémonSprites();
		_stageSelectInterface.AddEnemyPokémonSprites(PokémonStage.ID);
	}

	private void SetStageThumbnail()
	{
		_stageSelectInterface.SetStageThumbnailTexture(PokémonStage.ID);
	}
}
