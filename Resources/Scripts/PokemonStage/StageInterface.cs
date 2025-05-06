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
	private Control _stageTeamSlotContainer;

	[Export]
	private StageControls _stageControls;

	[Export]
	private CustomButton _exitButton;

	[Export]
	private Container _container;

	[Export]
	private PokemonStage _pokemonStage;

	private List<StageTeamSlot> _stageTeamSlots = new List<StageTeamSlot>();

    public override void _EnterTree()
    {
        PokemonTD.Signals.PokemonTeamUpdated += OnTeamUpdated;
		PokemonTD.Signals.PokemonEnemyPassed += OnPokemonEnemyPassed;
		PokemonTD.Signals.PokemonEnemyFainted += OnPokemonEnemyFainted;
		PokemonTD.Signals.DraggingStageSlot += OnDragging;
		PokemonTD.Signals.DraggingStageTeamSlot += OnDragging;
		PokemonTD.Signals.DraggingPokeBall += OnDragging;
		PokemonTD.Signals.PokemonOnStage += OnPokemonOnStage;
		PokemonTD.Signals.PokemonOffStage += OnPokemonOffStage;
    }

    public override void _ExitTree()
    {
        PokemonTD.Signals.PokemonTeamUpdated -= OnTeamUpdated;
		PokemonTD.Signals.PokemonEnemyPassed -= OnPokemonEnemyPassed;
		PokemonTD.Signals.PokemonEnemyFainted -= OnPokemonEnemyFainted;
		PokemonTD.Signals.DraggingStageSlot -= OnDragging;
		PokemonTD.Signals.DraggingStageTeamSlot -= OnDragging;
		PokemonTD.Signals.DraggingPokeBall -= OnDragging;
		PokemonTD.Signals.PokemonOnStage -= OnPokemonOnStage;
		PokemonTD.Signals.PokemonOffStage -= OnPokemonOffStage;
    }

	public override void _Ready()
	{
		Visible = !PokemonTD.IsScreenshotModeOn;
		
		ClearStageTeamSlots();
		AddStageTeamSlots();

		_waveCount.Text = $"Wave {_pokemonStage.CurrentWave} of {_pokemonStage.WaveCount}";
		_pokeDollars.Text = $"₽ {PokemonTD.PokeDollars}";
		_rareCandy.Text = $"{_pokemonStage.RareCandy}";
		
        _pokemonStage.StartedWave += () => _waveCount.Text = $"Wave {_pokemonStage.CurrentWave} of {_pokemonStage.WaveCount}";
		_exitButton.Pressed += () => 
		{
			StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
			_pokemonStage.AddSibling(stageSelectInterface);
			_pokemonStage.QueueFree();

			PokemonTD.AudioManager.PlayButtonPressed();
		};

		_exitButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;

		_stageControls.VisibilityToggled += (isVisible) => _stageTeamSlotContainer.Visible = isVisible;
	}

	private void OnTeamUpdated()
	{
		List<StageTeamSlot> slotsInUse = _stageTeamSlots.FindAll(stageTeamSlot => stageTeamSlot.InUse == true);

		ClearStageTeamSlots();
		AddStageTeamSlots();

		foreach (StageTeamSlot slotInUse in slotsInUse)
		{
			StageTeamSlot stageTeamSlot = _stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.ID == slotInUse.ID);
			stageTeamSlot.InUse = true;
		}
	}

	private void OnPokemonOnStage(int teamSlotID)
	{
		StageTeamSlot stageTeamSlot = _stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.ID == teamSlotID);
		stageTeamSlot.InUse = true;
	}

	private void OnPokemonOffStage(int teamSlotID)
	{
		StageTeamSlot stageTeamSlot = _stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.ID == teamSlotID);
		stageTeamSlot.InUse = false;
	}

	public bool IsStageTeamSlotInUse(int teamSlotID)
	{
		StageTeamSlot stageTeamSlot = _stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.ID == teamSlotID);
		return stageTeamSlot.InUse;
	}

	private void OnPokemonEnemyPassed(PokemonEnemy pokemonEnemy)
	{
		_rareCandy.Text = $"{_pokemonStage.RareCandy}";
	}

	private void OnPokemonEnemyFainted(PokemonEnemy pokemonEnemy, int teamSlotID)
	{
		_pokeDollars.Text = $"₽ {PokemonTD.PokeDollars}";
	}

	private void ClearStageTeamSlots()
	{
		_stageTeamSlots.Clear();
		foreach (Node child in _stageTeamSlotContainer.GetChildren())
		{
			child.QueueFree();
		}
	}

	private void AddStageTeamSlots()
	{
		int emptyTeamSlots = PokemonTD.MaxTeamSize - PokemonTeam.Instance.Pokemon.Count;
		for (int i = 0; i < PokemonTeam.Instance.Pokemon.Count; i++)
		{
			StageTeamSlot stageTeamSlot = PokemonTD.PackedScenes.GetStageTeamSlot();
			stageTeamSlot.ID = i;
			stageTeamSlot.Pokemon = PokemonTeam.Instance.Pokemon[i];

			_stageTeamSlotContainer.AddChild(stageTeamSlot);
			_stageTeamSlots.Add(stageTeamSlot);
		}

		// Fill the rest of the slots with an empty slot state
		for (int i = 0; i < emptyTeamSlots; i++)
		{
			Control emptyStageTeamSlot = PokemonTD.PackedScenes.GetEmptyStageTeamSlot();		
			_stageTeamSlotContainer.AddChild(emptyStageTeamSlot);
		}
	}

	private void OnDragging(bool isDragging)
	{
		_container.Visible = !isDragging;
	}

	public StageTeamSlot FindStageTeamSlot(int id)
	{
		return _stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.ID == id);
	}
}
