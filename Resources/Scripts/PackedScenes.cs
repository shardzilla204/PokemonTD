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
	private PackedScene _PokemonTeamSlot;

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
			if (PokemonMoves.Instance.IsQueueEmpty() && PokemonEvolution.Instance.IsQueueEmpty()) PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPlay);
			forgetMoveInterface.QueueFree();
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

	public EvolutionInterface GetEvolutionInterface(Pokemon pokemon, int teamSlotIndex)
	{
		EvolutionInterface evolutionInterface = _evolutionInterface.Instantiate<EvolutionInterface>();
		evolutionInterface.Pokemon = pokemon;
		evolutionInterface.Finished += (pokemonEvolution) =>
		{
			if (PokemonEvolution.Instance.IsQueueEmpty() && PokemonMoves.Instance.IsQueueEmpty()) PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPlay);
			PokemonTD.Signals.EmitSignal(Signals.SignalName.EvolutionFinished, pokemonEvolution, teamSlotIndex);
			evolutionInterface.QueueFree();
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
		return _PokemonTeamSlot.Instantiate<PokemonTeamSlot>();
	}

	public Control GetEmptyPokemonTeamSlot()
	{
		return _emptyPokemonTeamSlot.Instantiate<Control>();
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
