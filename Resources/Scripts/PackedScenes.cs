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
	private PackedScene _pokemonTeamSlot;

	[Export]
	private PackedScene _emptyPokemonTeamSlot;

	[Export]
	private PackedScene _forgetMoveInterface;

	[Export]
	private PackedScene _evolutionInterface;

	[Export]
	private PackedScene _stageConsoleLabel;

	[Export]
	private PackedScene _settingsInterface;

	[Export]
	private PackedScene _informationInterface;

	[Export]
	private PackedScene _statusConditionIcon;

	[Export]
	private PackedScene _pokeMartInterface;

	[Export]
	private PackedScene _pokeMartItem;

	[Export]
	private PackedScene _pokeMartSlot;

	public MenuInterface GetMenuInterface()
	{
		return _menuInterface.Instantiate<MenuInterface>();
	}

	public StarterSelectionInterface GetStarterSelectionInterface()
	{
		return _starterSelectionInterface.Instantiate<StarterSelectionInterface>();
	}

	public ForgetMoveInterface GetForgetMoveInterface(Pokemon pokemon, PokemonMove pokemonMove)
	{
		ForgetMoveInterface forgetMoveInterface = _forgetMoveInterface.Instantiate<ForgetMoveInterface>();
		forgetMoveInterface.Pokemon = pokemon;
		forgetMoveInterface.MoveToLearn = pokemonMove;
		forgetMoveInterface.Finished += () =>
		{
			if (PokemonMoves.Instance.IsQueueEmpty()) PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PressedPlay);
		};

		return forgetMoveInterface;
	}

	public StatusConditionIcon GetStatusConditionIcon(StatusCondition statusCondition)
	{
		StatusConditionIcon statusConditionIcon = _statusConditionIcon.Instantiate<StatusConditionIcon>();
		statusConditionIcon.SetIcon(statusCondition);
		return statusConditionIcon;
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

	public EvolutionInterface GetEvolutionInterface(Pokemon pokemon, Pokemon pokemonEvolution, int pokemonTeamIndex)
	{
		EvolutionInterface evolutionInterface = _evolutionInterface.Instantiate<EvolutionInterface>();
		evolutionInterface.SetPokemon(pokemon, pokemonEvolution);
		evolutionInterface.Finished += (evolution) =>
		{
			if (PokemonEvolution.Instance.IsQueueEmpty() && PokemonMoves.Instance.IsQueueEmpty()) PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PressedPlay);
			
			if (evolution != null) PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.EvolutionFinished, evolution, pokemonTeamIndex);
		};
		return evolutionInterface;
	}

	public InformationInterface GetInformationInterface()
	{
		return _informationInterface.Instantiate<InformationInterface>();
	}

	public PokemonStage GetPokemonStage(int stageID)
	{
		return _pokemonStages[stageID].Instantiate<PokemonStage>();
	}

	public PokemonTeamSlot GetPokemonTeamSlot()
	{
		return _pokemonTeamSlot.Instantiate<PokemonTeamSlot>();
	}

	public Control GetEmptyPokemonTeamSlot()
	{
		return _emptyPokemonTeamSlot.Instantiate<Control>();
	}

	public PokeCenterInterface GetPokeCenterInterface()
	{
		return _pokeCenterInterface.Instantiate<PokeCenterInterface>();
	}

	public PokeMartInterface GetPokeMartInterface()
	{
		return _pokeMartInterface.Instantiate<PokeMartInterface>();
	}

	public PokeMartItem GetPokeMartItem(PokeMartItem pokeMartItemData)
	{
		PokeMartItem pokeMartItem = _pokeMartItem.Instantiate<PokeMartItem>();
		pokeMartItem.Name = pokeMartItemData.Name;
		pokeMartItem.Description = pokeMartItemData.Description;
		pokeMartItem.Price = pokeMartItemData.Price;
		pokeMartItem.Sprite = pokeMartItemData.Sprite;
		return pokeMartItem;
	}

	public PokeMartSlot GetPokeMartSlot(PokeMartItem pokeMartItem)
	{
		PokeMartSlot pokeMartSlot = _pokeMartSlot.Instantiate<PokeMartSlot>();
		pokeMartSlot.SetPokeMartItem(pokeMartItem);
		return pokeMartSlot;
	}
}
