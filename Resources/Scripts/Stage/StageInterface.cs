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
	private CustomButton _settingsButton;

	[Export]
	private Container _container;

	[Export]
	private CustomButton _visiblityToggle;

	[Export]
	private PokemonStage _pokemonStage;

	private List<StageTeamSlot> _stageTeamSlots = new List<StageTeamSlot>();
	private bool _isVisible = true;

    public override void _EnterTree()
    {
        PokemonTD.Signals.PokemonTeamUpdated += OnTeamUpdated;
		PokemonTD.Signals.PokemonEnemyPassed += OnPokemonEnemyPassed;
		PokemonTD.Signals.PokeDollarsUpdated += OnPokeDollarsUpdated;
		PokemonTD.Signals.DraggingStageSlot += OnDragging;
		PokemonTD.Signals.DraggingStageTeamSlot += OnDragging;
		PokemonTD.Signals.DraggingPokeBall += OnDragging;
		PokemonTD.Signals.PokemonOnStage += OnPokemonOnStage;
		PokemonTD.Signals.PokemonOffStage += OnPokemonOffStage;
		PokemonTD.Signals.EvolutionFinished += UpdateStageTeamSlot;
    }

    public override void _ExitTree()
    {
        PokemonTD.Signals.PokemonTeamUpdated -= OnTeamUpdated;
		PokemonTD.Signals.PokemonEnemyPassed -= OnPokemonEnemyPassed;
		PokemonTD.Signals.PokeDollarsUpdated -= OnPokeDollarsUpdated;
		PokemonTD.Signals.DraggingStageSlot -= OnDragging;
		PokemonTD.Signals.DraggingStageTeamSlot -= OnDragging;
		PokemonTD.Signals.DraggingPokeBall -= OnDragging;
		PokemonTD.Signals.PokemonOnStage -= OnPokemonOnStage;
		PokemonTD.Signals.PokemonOffStage -= OnPokemonOffStage;
		PokemonTD.Signals.EvolutionFinished -= UpdateStageTeamSlot;
    }

	public override void _Ready()
	{
		Visible = !PokemonTD.IsScreenshotModeOn;

		_visiblityToggle.Pressed += () => 
		{
			OnVisibilityPressed();
			PokemonTD.AudioManager.PlayButtonPressed();
		};
		_visiblityToggle.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
		_visiblityToggle.Text = _isVisible ? "Hide Team" : "Show Team";
		
		ClearStageTeamSlots();
		AddStageTeamSlots();
		AddEmptyStageTeamSlots();

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
		_settingsButton.Pressed += () =>
		{
			SettingsInterface settingsInterface = PokemonTD.PackedScenes.GetSettingsInterface();
			AddSibling(settingsInterface);

			PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPause);
		};

		_exitButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
	}

	private void OnVisibilityPressed()
	{
		_isVisible = !_isVisible;
		_visiblityToggle.Text = _isVisible ? "Hide Team" : "Show Team";

		_stageTeamSlotContainer.Visible = _isVisible;
		Control parent = _stageTeamSlotContainer.GetParentOrNull<Control>();
		parent.MouseFilter = _isVisible ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
	}

	private void OnTeamUpdated()
	{
		int iteration = PokemonTeam.Instance.Pokemon.Count - 1;
		RemoveEmptyStageTeamSlot(iteration);

		StageTeamSlot stageTeamSlot = GetStageTeamSlot(iteration);
		AddStageTeamSlot(stageTeamSlot, iteration);
	}
	
	private void OnPokemonOnStage(int teamSlotIndex)
	{
		StageTeamSlot stageTeamSlot = _stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.TeamSlotIndex == teamSlotIndex);
		stageTeamSlot.InUse = true;
	}

	private void OnPokemonOffStage(int teamSlotIndex)
	{
		StageTeamSlot stageTeamSlot = _stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.TeamSlotIndex == teamSlotIndex);
		stageTeamSlot.InUse = false;
	}

	public bool IsStageTeamSlotInUse(int teamSlotIndex)
	{
		StageTeamSlot stageTeamSlot = _stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.TeamSlotIndex == teamSlotIndex);
		return stageTeamSlot.InUse;
	}

	private void OnPokemonEnemyPassed(PokemonEnemy pokemonEnemy)
	{
		_rareCandy.Text = $"{_pokemonStage.RareCandy}";
	}

	private void OnPokeDollarsUpdated()
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

	private void RemoveEmptyStageTeamSlot(int index)
	{
		Control emptyStageTeamSlot = _stageTeamSlotContainer.GetChildOrNull<Control>(index);
		emptyStageTeamSlot.QueueFree();
	}

	private void AddStageTeamSlots()
	{
		for (int i = 0; i < PokemonTeam.Instance.Pokemon.Count; i++)
		{
			StageTeamSlot stageTeamSlot = GetStageTeamSlot(i);
			AddStageTeamSlot(stageTeamSlot, i);
		}
	}

	private void AddStageTeamSlot(StageTeamSlot stageTeamSlot, int teamSlotIndex)
	{
		_stageTeamSlotContainer.AddChild(stageTeamSlot);
		_stageTeamSlotContainer.MoveChild(stageTeamSlot, teamSlotIndex);
		_stageTeamSlots.Insert(teamSlotIndex, stageTeamSlot);
	}

	public void UpdateStageTeamSlot(Pokemon pokemonEvolution, int teamSlotIndex)
	{
		StageTeamSlot oldStageTeamSlot = _stageTeamSlotContainer.GetChildOrNull<StageTeamSlot>(teamSlotIndex);
		oldStageTeamSlot.QueueFree();

		StageTeamSlot newStageTeamSlot = GetStageTeamSlot(teamSlotIndex);
		AddStageTeamSlot(newStageTeamSlot, teamSlotIndex);
	}

	private StageTeamSlot GetStageTeamSlot(int teamSlotIndex)
	{
		StageTeamSlot stageTeamSlot = PokemonTD.PackedScenes.GetStageTeamSlot();
		stageTeamSlot.TeamSlotIndex = teamSlotIndex;
		
		Pokemon pokemon = PokemonTeam.Instance.Pokemon[teamSlotIndex];
		stageTeamSlot.SetControls(pokemon);

		return stageTeamSlot;
	}

	// Fill the rest of the slots with an empty slot state
	private void AddEmptyStageTeamSlots()
	{
		int emptyTeamSlots = PokemonTD.MaxTeamSize - PokemonTeam.Instance.Pokemon.Count;
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

	public StageTeamSlot FindStageTeamSlot(int teamSlotIndex)
	{
		return _stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.TeamSlotIndex == teamSlotIndex);
	}
}
