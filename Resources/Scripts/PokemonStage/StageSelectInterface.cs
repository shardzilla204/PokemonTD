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
			PokemonCenterInterface pokemonCenterINterPokemonCenterInterface = PokemonTD.PackedScenes.GetPokemonCenterInterface();
			AddSibling(pokemonCenterINterPokemonCenterInterface);
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
		PokemonStage PokemonStage = PokemonTD.PokemonStages.GetPokemonStage(pokemonStageID);
		foreach (string pokemonEnemyName in PokemonStage.PokemonNames)
		{
			TextureRect enemyPokemonSprite = GetEnemyPokemonSprite(pokemonEnemyName);
			_pokemonEnemySprites.AddChild(enemyPokemonSprite);
		}
	}

   public TextureRect GetEnemyPokemonSprite(string pokemonName)
	{
		const int MinimumSizeValue = 65;
		TextureRect textureRect = new TextureRect()
		{
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspect,
			CustomMinimumSize = new Vector2(MinimumSizeValue, MinimumSizeValue)
		};

		if (pokemonName is null) return textureRect; // Return an empty TextureRect 

		Texture2D PokemonSprite = PokemonTD.GetPokemonSprite(pokemonName);
		textureRect.Texture = PokemonSprite;
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
		string fileDirectory = "res://Assets/Images/StageThumbnail/";
		string fileName = $"Stage{pokemonStageID}Thumbnail";
		string fileExtension = ".png";

		string filePath = $"{fileDirectory}{fileName}{fileExtension}";
		return ResourceLoader.Load<CompressedTexture2D>(filePath);
	}

	public void SetStageThumbnailTexture(int pokemonStageID)
	{
		pokemonStageID = pokemonStageID < 1 ? 1 : pokemonStageID; // Default to 1 if lower than 1
		_stageThumbnail.Texture = GetStageThumbnailTexture(pokemonStageID);
	}
}
