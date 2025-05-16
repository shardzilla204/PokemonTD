using Godot;

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
	private StageTeamSlots _stageTeamSlots;

	[Export]
	private StageControls _stageControls;

	[Export]
	private CustomButton _exitButton;

	[Export]
	private CustomButton _settingsButton;

	[Export]
	private Container _container;

	[Export]
	private PokemonStage _pokemonStage;

	private bool _isVisible = true;

	public StageTeamSlots StageTeamSlots => _stageTeamSlots;

    public override void _EnterTree()
    {
		PokemonTD.Signals.PokemonEnemyPassed += PokemonEnemyPassed;
		PokemonTD.Signals.PokeDollarsUpdated += PokeDollarsUpdated;
		PokemonTD.Signals.DraggingStageSlot += Dragging;
		PokemonTD.Signals.DraggingStageTeamSlot += Dragging;
		PokemonTD.Signals.DraggingPokeBall += Dragging;
		PokemonTD.Signals.PokemonUsed += PokemonUsed;
    }

    public override void _ExitTree()
    {
		PokemonTD.Signals.PokemonEnemyPassed -= PokemonEnemyPassed;
		PokemonTD.Signals.PokeDollarsUpdated -= PokeDollarsUpdated;
		PokemonTD.Signals.DraggingStageSlot -= Dragging;
		PokemonTD.Signals.DraggingStageTeamSlot -= Dragging;
		PokemonTD.Signals.DraggingPokeBall -= Dragging;
		PokemonTD.Signals.PokemonUsed -= PokemonUsed;
    }

	public override void _Ready()
	{
		Visible = !PokemonTD.IsScreenshotModeOn;

		_waveCount.Text = $"Wave {_pokemonStage.CurrentWave} of {_pokemonStage.WaveCount}";
		_pokeDollars.Text = $"₽ {PokemonTD.PokeDollars}";
		_rareCandy.Text = $"{_pokemonStage.RareCandy}";
		
        _pokemonStage.StartedWave += () => _waveCount.Text = $"Wave {_pokemonStage.CurrentWave} of {_pokemonStage.WaveCount}";
		_exitButton.Pressed += () => 
		{
			StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
			_pokemonStage.AddSibling(stageSelectInterface);
			_pokemonStage.QueueFree();
		};
		_settingsButton.Pressed += () =>
		{
			SettingsInterface settingsInterface = PokemonTD.PackedScenes.GetSettingsInterface();
			AddSibling(settingsInterface);

			PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPause);
		};
	}

	private void PokemonUsed(bool inUse, int teamSlotIndex)
	{
		StageTeamSlot stageTeamSlot = StageTeamSlots.FindStageTeamSlot(teamSlotIndex);
		stageTeamSlot.InUse = inUse;
	}

	public bool IsStageTeamSlotInUse(int teamSlotIndex)
	{
		StageTeamSlot stageTeamSlot = StageTeamSlots.FindStageTeamSlot(teamSlotIndex);
		return stageTeamSlot.InUse;
	}

	private void PokemonEnemyPassed(PokemonEnemy pokemonEnemy)
	{
		_rareCandy.Text = $"{_pokemonStage.RareCandy}";
	}

	private void PokeDollarsUpdated()
	{
		_pokeDollars.Text = $"₽ {PokemonTD.PokeDollars}";
	}

	private void Dragging(bool isDragging)
	{
		_container.Visible = !isDragging;
	}
}
