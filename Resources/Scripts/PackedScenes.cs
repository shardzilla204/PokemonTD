using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class PackedScenes : Node
{
	[Export]
	private PackedScene _menuInterface;

	[Export]
	private PackedScene _movesetInterface;

	[Export]
	private PackedScene _moveOption;

	[Export]
	private PackedScene _pokeCenterInterface;
	
	[Export]
	private PackedScene _pokeCenterSlot;

	[Export]
	private PackedScene _starterSelectionInterface;

	[Export]
	private PackedScene _starterOption;

	[Export]
	private PackedScene _stageSelectInterface;

	[Export]
	private PackedScene _stageSelectButton;

	[Export]
	private PackedScene _stageResultInterface;

	[Export]
	private Array<PackedScene> _pokemonStages;

	[Export]
	private PackedScene _pokemonEnemy;

	[Export]
	private PackedScene _stageTeamSlot;

	[Export]
	private PackedScene _emptyStageTeamSlot;

	[Export]
	private PackedScene _forgetMoveInterface;

	[Export]
	private PackedScene _evolutionInterface;

	[Export]
	private PackedScene _stageConsoleLabel;

	[Export]
	private PackedScene _settingsInterface;

	public MenuInterface GetMenuInterface()
	{
		return _menuInterface.Instantiate<MenuInterface>();
	}

	public StarterSelectionInterface GetStarterSelectionInterface()
	{
		return _starterSelectionInterface.Instantiate<StarterSelectionInterface>();
	}

	public ForgetMoveInterface GetForgetMoveInterface()
	{
		return _forgetMoveInterface.Instantiate<ForgetMoveInterface>();
	}

	public StarterOption GetStarterOption()
	{
		return _starterOption.Instantiate<StarterOption>();
	}

	public SettingsInterface GetSettingsInterface()
	{
		return _settingsInterface.Instantiate<SettingsInterface>();
	}

    public PokeCenterSlot GetPokeCenterSlot()
	{
		return _pokeCenterSlot.Instantiate<PokeCenterSlot>();
	}

	public PokemonEnemy GetPokemonEnemy()
	{
		return _pokemonEnemy.Instantiate<PokemonEnemy>();
	}

	public MovesetInterface GetMovesetInterface()
	{
		return _movesetInterface.Instantiate<MovesetInterface>();
	}

	public MoveOption GetMoveOption()
	{
		return _moveOption.Instantiate<MoveOption>();
	}

	public StageSelectInterface GetStageSelectInterface()
	{
		return _stageSelectInterface.Instantiate<StageSelectInterface>();
	}

	public StageSelectButton GetStageSelectButton()
	{
		return _stageSelectButton.Instantiate<StageSelectButton>();
	}

	public StageResultInterface GetStageResultInterface()
	{
		return _stageResultInterface.Instantiate<StageResultInterface>();
	}

	public EvolutionInterface GetEvolutionInterface()
	{
		return _evolutionInterface.Instantiate<EvolutionInterface>();
	}

	public PokemonStage GetPokemonStage(int stageID)
	{
		return _pokemonStages[stageID].Instantiate<PokemonStage>();
	}
	
	public StageTeamSlot GetStageTeamSlot()
	{
		return _stageTeamSlot.Instantiate<StageTeamSlot>();
	}

	public Control GetEmptyStageTeamSlot()
	{
		return _emptyStageTeamSlot.Instantiate<Control>();
	}

	public PokeCenterInterface GetPokeCenterInterface()
	{
		return _pokeCenterInterface.Instantiate<PokeCenterInterface>();
	}

	public StageConsoleLabel GetStageConsoleLabel()
	{
		return _stageConsoleLabel.Instantiate<StageConsoleLabel>();
	}
}
