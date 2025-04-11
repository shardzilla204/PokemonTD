using Godot;

using System.Collections.Generic;

namespace PokemonTD;

public partial class StageInterface : CanvasLayer
{
	[Export]
	private Label _waveCount;

	[Export]
	private Label _pokeDollars;

	[Export]
	private Label _rareCandy;

	[Export]
	private Control _stageTeamSlots;

	[Export]
	private StageControls _stageControls;

	[Export]
	private CustomButton _exitButton;

	[Export]
	private PokemonStage _pokemonStage;

	private List<StageTeamSlot> _activeStageTeamSlots = new List<StageTeamSlot>(); // Pokemon that are currently on the stage

    public override void _EnterTree()
    {
        PokemonTD.Signals.PokemonTeamUpdated += OnTeamUpdated;
		PokemonTD.Signals.PokemonEnemyPassed += OnPokemonEnemyPassed;
		PokemonTD.Signals.PokemonEnemyFainted += OnPokemonEnemyFainted;
		PokemonTD.Signals.DraggingStageSlot += OnDraggingSlot;
		PokemonTD.Signals.DraggingTeamStageSlot += OnDraggingSlot;
		PokemonTD.Signals.PokemonOnStage += OnPokemonOnStage;
		PokemonTD.Signals.PokemonOffStage += OnPokemonOffStage;
    }

    public override void _ExitTree()
    {
        PokemonTD.Signals.PokemonTeamUpdated -= OnTeamUpdated;
		PokemonTD.Signals.PokemonEnemyPassed -= OnPokemonEnemyPassed;
		PokemonTD.Signals.PokemonEnemyFainted -= OnPokemonEnemyFainted;
		PokemonTD.Signals.DraggingStageSlot -= OnDraggingSlot;
		PokemonTD.Signals.DraggingTeamStageSlot -= OnDraggingSlot;
		PokemonTD.Signals.PokemonOnStage -= OnPokemonOnStage;
		PokemonTD.Signals.PokemonOffStage -= OnPokemonOffStage;
    }

	public override void _Ready()
	{
		ClearStageTeamSlots();
		AddStageTeamSlots();

		_waveCount.Text = $"Wave {_pokemonStage.CurrentWave - 1} of {_pokemonStage.WaveCount}";
		_pokeDollars.Text = $"₽ {PokemonTD.PokeDollars}";
		_rareCandy.Text = $"{_pokemonStage.RareCandy}";
		
        _pokemonStage.StartedWave += () => _waveCount.Text = $"Wave {_pokemonStage.CurrentWave - 1} of {_pokemonStage.WaveCount}";
		_exitButton.Pressed += () => 
		{
			StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
			_pokemonStage.AddSibling(stageSelectInterface);
			_pokemonStage.QueueFree();
		};

		_stageControls.VisibilityToggled += (isVisible) => _stageTeamSlots.Visible = isVisible;
	}

	private void OnTeamUpdated()
	{
		ClearStageTeamSlots();
		AddStageTeamSlots();
	}

	private void OnPokemonOnStage(int teamSlotID)
	{
		List<StageTeamSlot> stageTeamSlots = GetStageTeamSlots();
		StageTeamSlot stageTeamSlot = stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.ID == teamSlotID);
		_activeStageTeamSlots.Add(stageTeamSlot);
	}

	private void OnPokemonOffStage(int teamSlotID)
	{
		List<StageTeamSlot> stageTeamSlots = GetStageTeamSlots();
		StageTeamSlot stageTeamSlot = stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.ID == teamSlotID);
		_activeStageTeamSlots.Remove(stageTeamSlot);
	}

	public List<StageTeamSlot> GetStageTeamSlots()
	{
		List<StageTeamSlot> stageTeamSlots = new List<StageTeamSlot>();
		foreach (Node child in _stageTeamSlots.GetChildren())
		{
			if (child is StageTeamSlot stageTeamSlot) 
			{
				stageTeamSlots.Add(stageTeamSlot);
			}
		}
		return stageTeamSlots;
	}

	public bool IsStageTeamSlotInUse(int id)
	{
		StageTeamSlot stageTeamSlot = _activeStageTeamSlots.Find(stageTeamSlot => stageTeamSlot.ID == id);
		return stageTeamSlot != null ? true : false;
	}

	private void OnPokemonEnemyPassed(PokemonEnemy pokemonEnemy)
	{
		_rareCandy.Text = $"{_pokemonStage.RareCandy}";
	}

	private void OnPokemonEnemyFainted(PokemonEnemy pokemonEnemy)
	{
		_pokeDollars.Text = $"₽ {PokemonTD.PokeDollars}";
	}

	private void ClearStageTeamSlots()
	{
		foreach (Node child in _stageTeamSlots.GetChildren())
		{
			_stageTeamSlots.RemoveChild(child);
			child.QueueFree();
		}
	}

	private void AddStageTeamSlots()
	{
		int emptyTeamSlots = PokemonTD.MaxTeamSize - PokemonTD.PokemonTeam.Pokemon.Count;
		for (int i = 0; i < PokemonTD.PokemonTeam.Pokemon.Count; i++)
		{
			StageTeamSlot stageTeamSlot = PokemonTD.PackedScenes.GetStageTeamSlot();
			stageTeamSlot.ID = i;
			stageTeamSlot.Pokemon = PokemonTD.PokemonTeam.Pokemon[i];

			_stageTeamSlots.AddChild(stageTeamSlot);
		}

		// Fill the rest of the slots with an empty slot state
		for (int i = 0; i < emptyTeamSlots; i++)
		{
			Control emptyStageTeamSlot = PokemonTD.PackedScenes.GetEmptyStageTeamSlot();		
			_stageTeamSlots.AddChild(emptyStageTeamSlot);
		}
	}

	private void OnDraggingSlot(bool isDragging)
	{
		_stageTeamSlots.Visible = !isDragging;
	}

	public StageTeamSlot FindStageTeamSlot(int id)
	{
		List<StageTeamSlot> stageTeamSlots = new List<StageTeamSlot>();
		foreach (Node child in _stageTeamSlots.GetChildren())
		{
			if (child is StageTeamSlot stageTeamSlot) stageTeamSlots.Add(stageTeamSlot);
		}
		return stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.ID == id);
	}
}
