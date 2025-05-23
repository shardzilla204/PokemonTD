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
	private PokemonTeamSlots _PokemonTeamSlots;

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

	public PokemonTeamSlots PokemonTeamSlots => _PokemonTeamSlots;

    public override void _EnterTree()
    {
		PokemonTD.Signals.PokemonEnemyPassed += PokemonEnemyPassed;
		PokemonTD.Signals.PokeDollarsUpdated += PokeDollarsUpdated;
		PokemonTD.Signals.RareCandyUpdated += RareCandyUpdated;
		PokemonTD.Signals.DraggingPokemonTeamSlot += DraggingTeamSlot;
		PokemonTD.Signals.DraggingPokemonStageSlot += DraggingStageSlot;
		PokemonTD.Signals.DraggingPokeBall += SetVisibility;
		PokemonTD.Signals.PokemonUsed += PokemonUsed;
    }

    public override void _ExitTree()
    {
		PokemonTD.Signals.PokemonEnemyPassed -= PokemonEnemyPassed;
		PokemonTD.Signals.PokeDollarsUpdated -= PokeDollarsUpdated;
		PokemonTD.Signals.RareCandyUpdated -= RareCandyUpdated;
		PokemonTD.Signals.DraggingPokemonTeamSlot -= DraggingTeamSlot;
		PokemonTD.Signals.DraggingPokemonStageSlot -= DraggingStageSlot;
		PokemonTD.Signals.DraggingPokeBall -= SetVisibility;
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

			PokemonTD.Signals.EmitSignal(Signals.SignalName.HasLeftStage);
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
		PokemonTeamSlot pokemonTeamSlot = PokemonTeamSlots.FindPokemonTeamSlot(teamSlotIndex);
		pokemonTeamSlot.InUse = inUse;
	}

	public bool IsPokemonTeamSlotInUse(int teamSlotIndex)
	{
		PokemonTeamSlot pokemonTeamSlot = PokemonTeamSlots.FindPokemonTeamSlot(teamSlotIndex);
		return pokemonTeamSlot.InUse;
	}

	private void PokemonEnemyPassed(PokemonEnemy pokemonEnemy)
	{
		RareCandyUpdated();
	}

	private void RareCandyUpdated()
	{
		_rareCandy.Text = $"{_pokemonStage.RareCandy}";
	}

	private void PokeDollarsUpdated()
	{
		_pokeDollars.Text = $"₽ {PokemonTD.PokeDollars}";
	}

	private void DraggingTeamSlot(PokemonTeamSlot pokemonTeamSlot, bool isDragging)
	{
		SetVisibility(isDragging);
	}

	private void DraggingStageSlot(PokemonStageSlot pokemonStageSlot, bool isDragging)
	{
		SetVisibility(isDragging);
	}

	private void SetVisibility(bool isDragging)
	{
		_container.Visible = !isDragging;
	}
}
