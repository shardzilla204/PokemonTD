using Godot;

using System.Collections.Generic;

namespace PokémonTD;

public partial class StageInterface : CanvasLayer
{
	[Export]
	private Container _pokémonSlotsContainer;

	[Export]
	private Label _waveCount;

	[Export]
	private Label _pokéDollars;

	[Export]
	private Label _rareCandy;

	[Export]
	private Control _stageTeamSlots;

	[Export]
	private CustomButton _exitButton;

	[Export]
	private PokémonStage _pokémonStage;

	private List<StageTeamSlot> _stageTeamSlotsInUse = new List<StageTeamSlot>();

    public override void _EnterTree()
    {
		ClearStageTeamSlots();

        PokémonTD.Signals.StartedWave += UpdateWaveCount;
		PokémonTD.Signals.PokémonEnemyPassed += (pokémonEnemy) => UpdateRareCandy();
		PokémonTD.Signals.PokémonTeamUpdated += () =>
		{
			ClearStageTeamSlots();
			AddStageTeamSlots();
		};

		foreach (StageSlot stageSlot in _pokémonStage.StageSlots)
		{
			stageSlot.Dragging += (isDragging) => _stageTeamSlots.Visible = !isDragging;
		}

		PokémonTD.Signals.PokémonOnStage += OnPokémonOnStage;
		PokémonTD.Signals.PokémonOffStage += OnPokémonOffStage;
		PokémonTD.Signals.VisibilityToggled += (isVisible) => _stageTeamSlots.Visible = isVisible;
    }

	public override void _ExitTree()
    {
        PokémonTD.Signals.StartedWave -= UpdateWaveCount;
		PokémonTD.Signals.PokémonEnemyPassed -= (pokémonEnemy) => UpdateRareCandy();
		PokémonTD.Signals.PokémonTeamUpdated -= () =>
		{
			ClearStageTeamSlots();
			AddStageTeamSlots();
		};
		PokémonTD.Signals.PokémonOnStage -= OnPokémonOnStage;
		PokémonTD.Signals.PokémonOffStage -= OnPokémonOffStage;
		PokémonTD.Signals.VisibilityToggled -= (isVisible) => _stageTeamSlots.Visible = isVisible;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
		{
		}
    }


	public override void _Ready()
	{
		_waveCount.Text = $"Wave {_pokémonStage.CurrentWave} of {_pokémonStage.WaveCount}";
		_pokéDollars.Text = $"₽ {PokémonTD.PokéDollars}";
		_rareCandy.Text = $"{_pokémonStage.RareCandy}";

		AddStageTeamSlots();

		_exitButton.Pressed += () => 
		{
			StageSelectInterface stageSelectInterface = PokémonTD.PackedScenes.GetStageSelectInterface();
			_pokémonStage.AddSibling(stageSelectInterface);

			_pokémonStage.QueueFree();
		};
	}

	private void OnPokémonOnStage(int teamSlotID)
	{
		List<StageTeamSlot> stageTeamSlots = GetStageTeamSlots();
		StageTeamSlot stageTeamSlot = stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.ID == teamSlotID);
		_stageTeamSlotsInUse.Add(stageTeamSlot);
	}

	private void OnPokémonOffStage(int teamSlotID)
	{
		List<StageTeamSlot> stageTeamSlots = GetStageTeamSlots();
		StageTeamSlot stageTeamSlot = stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.ID == teamSlotID);
		_stageTeamSlotsInUse.Remove(stageTeamSlot);
	}

	public List<StageTeamSlot> GetStageTeamSlots()
	{
		List<StageTeamSlot> stageTeamSlots = new List<StageTeamSlot>();
		foreach (Node child in _stageTeamSlots.GetChildren())
		{
			if (child is StageTeamSlot stageTeamSlot) stageTeamSlots.Add(stageTeamSlot);
		}
		return stageTeamSlots;
	}

	public bool IsStageTeamSlotInUse(int id)
	{
		StageTeamSlot stageTeamSlot = _stageTeamSlotsInUse.Find(stageTeamSlot => stageTeamSlot.ID == id);
		return stageTeamSlot != null ? true : false;
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
		int emptyTeamSlots = PokémonTD.MaxTeamSize - PokémonTD.PokémonTeam.Pokémon.Count;
		for (int i = 0; i < PokémonTD.PokémonTeam.Pokémon.Count; i++)
		{
			StageTeamSlot stageTeamSlot = PokémonTD.PackedScenes.GetStageTeamSlot();
			stageTeamSlot.ID = i;
			stageTeamSlot.Pokémon = PokémonTD.PokémonTeam.Pokémon[i];

			_stageTeamSlots.AddChild(stageTeamSlot);
		}

		// Fill the rest of the slots with an empty slot state
		for (int i = 0; i < emptyTeamSlots; i++)
		{
			Control emptyStageTeamSlot = PokémonTD.PackedScenes.GetEmptyStageTeamSlot();		
			_stageTeamSlots.AddChild(emptyStageTeamSlot);
		}

		foreach (StageTeamSlot stageTeamSlot in GetStageTeamSlots())
		{
			stageTeamSlot.Dragging += (isDragging) => _stageTeamSlots.Visible = !isDragging;
		}
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

	private void UpdateWaveCount()
	{
		_waveCount.Text = $"Wave {_pokémonStage.CurrentWave} of {_pokémonStage.WaveCount}";
	}

	public void UpdatePokéDollars()
	{
		_pokéDollars.Text = $"₽ {PokémonTD.PokéDollars}";
	}

	private void UpdateRareCandy()
	{
		_rareCandy.Text = $"{_pokémonStage.RareCandy}";
	}
}
