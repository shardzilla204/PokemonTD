using Godot;

namespace PokemonTD;

public partial class StageInfo : Container
{
    [Export]
    private Label _waveCount;

    [Export]
    private Label _levelRange;

    [Export]
    private TextureRect _stageThumbnail;

    [Export]
    private Container _pokemonEnemySprites;

    public override void _ExitTree()
    {
        PokemonTD.Signals.StageSelectButtonHovered -= SetStage;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.StageSelectButtonHovered += SetStage;

        PokemonStage pokemonStage = PokemonStages.Instance.GetPokemonStage(1);
        SetStage(pokemonStage);
    }

    private void SetStage(PokemonStage pokemonStage)
    {
        ClearPokemonEnemySprites();
        AddPokemonEnemySprites(pokemonStage.ID);

        SetStageThumbnailTexture(pokemonStage.ID);

        _waveCount.Text = $"{pokemonStage.WaveCount} Waves";
        _levelRange.Text = $"LVL {pokemonStage.PokemonLevels[0]} - {pokemonStage.PokemonLevels[1]}";
    }

    private void ClearPokemonEnemySprites()
	{
		foreach (TextureRect pokemonEnemySprite in _pokemonEnemySprites.GetChildren())
		{
			_pokemonEnemySprites.RemoveChild(pokemonEnemySprite);
			pokemonEnemySprite.QueueFree();
		}
	}

    private void AddPokemonEnemySprites(int pokemonStageID)
	{
		pokemonStageID = pokemonStageID < 1 ? 1 : pokemonStageID; // Default to 1 if lower than 1

		PokemonStage pokemonStage = PokemonStages.Instance.GetPokemonStage(pokemonStageID);
		foreach (string pokemonEnemyName in pokemonStage.PokemonNames)
		{
			TextureRect enemyPokemonSprite = GetEnemyPokemonSprite(pokemonEnemyName);
			_pokemonEnemySprites.AddChild(enemyPokemonSprite);
		}
	}

    private TextureRect GetEnemyPokemonSprite(string pokemonName)
	{
        int sizeValue = 75;
		Vector2 minimumSize = new Vector2(sizeValue, sizeValue);
		TextureRect textureRect = new TextureRect()
		{
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspect,
			CustomMinimumSize = minimumSize
		};

		if (pokemonName is null) return textureRect; // Return an empty TextureRect 

		Pokemon pokemon = PokemonManager.Instance.GetPokemon(pokemonName, PokemonTD.MinPokemonEnemyLevel);
		Texture2D pokemonSprite = pokemon.Sprite;
		textureRect.Texture = pokemonSprite;
		return textureRect;
	}

    private Texture2D GetStageThumbnailTexture(int pokemonStageID)
	{
		string filePath = $"res://Assets/Images/StageThumbnail/Stage{pokemonStageID}Thumbnail.png";
		return ResourceLoader.Load<CompressedTexture2D>(filePath);
	}

	private void SetStageThumbnailTexture(int pokemonStageID)
	{
		pokemonStageID = pokemonStageID < 1 ? 1 : pokemonStageID; // Default to 1 if lower than 1
		_stageThumbnail.Texture = GetStageThumbnailTexture(pokemonStageID);
	}
}
