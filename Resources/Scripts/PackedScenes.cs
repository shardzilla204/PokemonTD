using Godot;
using Godot.Collections;

namespace PokémonTD;

public partial class PackedScenes : Node
{
	[Export]
	private PackedScene _menuInterface;

	[Export]
	private PackedScene _movesetInterface;

	[Export]
	private PackedScene _moveButton;

	[Export]
	private PackedScene _personalComputerInterface;
	
	[Export]
	private PackedScene _inventorySlot;

	[Export]
	private PackedScene _starterSelectionInterface;

	[Export]
	private PackedScene _starterOption;

	[Export]
	private PackedScene _stageSelectInterface;

	[Export]
	private PackedScene _stageSelectButton;

	[Export]
	private PackedScene _stageStateInterface;

	[Export]
	private Array<PackedScene> _pokémonStages;

	[Export]
	private PackedScene _pokémonEnemy;

	[Export]
	private PackedScene _stageTeamSlot;

	[Export]
	private PackedScene _emptyStageTeamSlot;


	public MenuInterface GetMenuInterface()
	{
		return _menuInterface.Instantiate<MenuInterface>();
	}

	public StarterSelectionInterface GetStarterSelectionInterface()
	{
		return _starterSelectionInterface.Instantiate<StarterSelectionInterface>();
	}

	public StarterOption GetStarterOption()
	{
		return _starterOption.Instantiate<StarterOption>();
	}

    public InventorySlot GetInventorySlot()
	{
		return _inventorySlot.Instantiate<InventorySlot>();
	}

	public PokémonEnemy GetPokémonEnemy()
	{
		return _pokémonEnemy.Instantiate<PokémonEnemy>();
	}

	public MovesetInterface GetMovesetInterface()
	{
		return _movesetInterface.Instantiate<MovesetInterface>();
	}

	public MoveButton GetMoveButton()
	{
		return _moveButton.Instantiate<MoveButton>();
	}

	public StageSelectInterface GetStageSelectInterface()
	{
		return _stageSelectInterface.Instantiate<StageSelectInterface>();
	}

	public StageSelectButton GetStageSelectButton()
	{
		return _stageSelectButton.Instantiate<StageSelectButton>();
	}

	public StageStateInterface GetStageStateInterface()
	{
		return _stageStateInterface.Instantiate<StageStateInterface>();
	}

	public PokémonStage GetPokémonStage(int pokémonStageID)
	{
		return _pokémonStages[pokémonStageID].Instantiate<PokémonStage>();
	}
	
	public StageTeamSlot GetStageTeamSlot()
	{
		return _stageTeamSlot.Instantiate<StageTeamSlot>();
	}

	public Control GetEmptyStageTeamSlot()
	{
		return _emptyStageTeamSlot.Instantiate<Control>();
	}

	public PersonalComputerInterface GetPersonalComputerInterface()
	{
		return _personalComputerInterface.Instantiate<PersonalComputerInterface>();
	}
}
