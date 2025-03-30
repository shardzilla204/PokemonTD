using Godot;

namespace PokémonTD;

public partial class StageSelectInterface : Node
{
	[Export]
	private CustomButton _pokémonTeamButton;

	[Export]
	private CustomButton _exitButton;

	[Export]
	private Container _stageSelectButtons;

	[Export]
	private TextureRect _stageThumbnail;

	[Export]
	private Container _enemyPokémonSprites;

	private PokémonStage _pokémonStage;

	public override void _Ready()
	{
		_pokémonTeamButton.Pressed += () =>
		{
			PersonalComputerInterface personalComputerInterface = PokémonTD.PackedScenes.GetPersonalComputerInterface();
			AddSibling(personalComputerInterface);
			QueueFree();
		};
		_exitButton.Pressed += () => 
		{
			MenuInterface menuInterface = PokémonTD.PackedScenes.GetMenuInterface();
			AddSibling(menuInterface);
			QueueFree();
		};

		ClearEnemyPokémonSprites();
		ClearStageSelectButtons();
		AddEnemyPokémonSprites(1);
		AddStageSelectButtons();
		SetStageThumbnailTexture(1);
	}

	public void ClearEnemyPokémonSprites()
	{
		foreach (TextureRect enemyPokémonSprite in _enemyPokémonSprites.GetChildren())
		{
			_enemyPokémonSprites.RemoveChild(enemyPokémonSprite);
			enemyPokémonSprite.QueueFree();
		}
	}

	public void AddEnemyPokémonSprites(int pokémonStageID)
	{
		pokémonStageID = pokémonStageID < 1 ? 1 : pokémonStageID; // Default to 1 if lower than 1
		PokémonStage pokémonStage = PokémonTD.GetPokémonStage(pokémonStageID);
		foreach (string enemyPokémonName in pokémonStage.PokémonNames)
		{
			TextureRect enemyPokémonSprite = GetEnemyPokémonSprite(enemyPokémonName);
			_enemyPokémonSprites.AddChild(enemyPokémonSprite);
		}
	}

   public TextureRect GetEnemyPokémonSprite(string pokémonName)
	{
		const int MinimumSizeValue = 65;
		TextureRect textureRect = new TextureRect()
		{
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspect,
			CustomMinimumSize = new Vector2(MinimumSizeValue, MinimumSizeValue)
		};

		if (pokémonName is null) return textureRect; // Return an empty TextureRect 

		Texture2D pokémonSprite = PokémonTD.GetPokémonSprite(pokémonName);
		textureRect.Texture = pokémonSprite;
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
		foreach (PokémonStage pokémonStage in PokémonTD.GetPokémonStages())
		{
			StageSelectButton stageSelectButton = PokémonTD.PackedScenes.GetStageSelectButton();
			stageSelectButton.PokémonStage = pokémonStage;

			_stageSelectButtons.AddChild(stageSelectButton);
		}
	}

	private Texture2D GetStageThumbnailTexture(int pokémonStageID)
	{
		string fileDirectory = "res://Assets/Images/StageThumbnail/";
		string fileName = $"Stage{pokémonStageID}Thumbnail";
		string fileExtension = ".png";

		string filePath = $"{fileDirectory}{fileName}{fileExtension}";
		return ResourceLoader.Load<CompressedTexture2D>(filePath);
	}

	public void SetStageThumbnailTexture(int pokémonStageID)
	{
		pokémonStageID = pokémonStageID < 1 ? 1 : pokémonStageID; // Default to 1 if lower than 1
		_stageThumbnail.Texture = GetStageThumbnailTexture(pokémonStageID);
	}
}
