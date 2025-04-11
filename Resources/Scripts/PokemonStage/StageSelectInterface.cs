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

	[Export]
	private TextureRect _stageThumbnail;

	[Export]
	private Container _pokemonEnemySprites;

	public override void _Ready()
	{
		_pokemonTeamButton.Pressed += () =>
		{
			PokeCenterInterface pokeCenterInterface = PokemonTD.PackedScenes.GetPokeCenterInterface();
			AddSibling(pokeCenterInterface);
			QueueFree();
		};
		_exitButton.Pressed += () => 
		{
			MenuInterface menuInterface = PokemonTD.PackedScenes.GetMenuInterface();
			AddSibling(menuInterface);
			QueueFree();
		};

		ClearPokemonEnemySprites();
		ClearStageSelectButtons();

		AddPokemonEnemySprites(1);
		AddStageSelectButtons();
		
		SetStageThumbnailTexture(1);
	}

	public void ClearPokemonEnemySprites()
	{
		foreach (TextureRect pokemonEnemySprite in _pokemonEnemySprites.GetChildren())
		{
			_pokemonEnemySprites.RemoveChild(pokemonEnemySprite);
			pokemonEnemySprite.QueueFree();
		}
	}

	public void AddPokemonEnemySprites(int pokemonStageID)
	{
		pokemonStageID = pokemonStageID < 1 ? 1 : pokemonStageID; // Default to 1 if lower than 1

		PokemonStage pokemonStage = PokemonTD.PokemonStages.GetPokemonStage(pokemonStageID);
		foreach (string pokemonEnemyName in pokemonStage.PokemonNames)
		{
			TextureRect enemyPokemonSprite = GetEnemyPokemonSprite(pokemonEnemyName);
			_pokemonEnemySprites.AddChild(enemyPokemonSprite);
		}
	}

   	private TextureRect GetEnemyPokemonSprite(string pokemonName)
	{
		Vector2 minimumSize = new Vector2(65, 65);
		TextureRect textureRect = new TextureRect()
		{
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspect,
			CustomMinimumSize = minimumSize
		};

		if (pokemonName is null) return textureRect; // Return an empty TextureRect 

		Pokemon pokemon = PokemonTD.PokemonManager.GetPokemon(pokemonName);
		Texture2D pokemonSprite = pokemon.Sprite;
		textureRect.Texture = pokemonSprite;
		return textureRect;
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

	private Texture2D GetStageThumbnailTexture(int pokemonStageID)
	{
		string filePath = $"res://Assets/Images/StageThumbnail/Stage{pokemonStageID}Thumbnail.png";
		return ResourceLoader.Load<CompressedTexture2D>(filePath);
	}

	public void SetStageThumbnailTexture(int pokemonStageID)
	{
		pokemonStageID = pokemonStageID < 1 ? 1 : pokemonStageID; // Default to 1 if lower than 1
		_stageThumbnail.Texture = GetStageThumbnailTexture(pokemonStageID);
	}
}
