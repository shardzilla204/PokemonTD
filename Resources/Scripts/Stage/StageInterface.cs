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
	private PokemonTeamSlots _pokemonTeamSlots;

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
	
	public override void _ExitTree()
	{
		PokemonTD.Signals.PokeDollarsUpdated -= SetPokeDollarsText;
		PokemonTD.Signals.RareCandyUpdated -= SetRareCandyText;
		PokemonTD.Signals.Dragging -= SetVisibility;
	}

	public override void _Ready()
	{
		PokemonTD.Signals.PokeDollarsUpdated += SetPokeDollarsText;
		PokemonTD.Signals.RareCandyUpdated += SetRareCandyText;
		PokemonTD.Signals.Dragging += SetVisibility;

		Visible = !PokemonTD.IsScreenshotModeOn;

		SetPokeDollarsText();
		SetRareCandyText();
		SetWaveText();

		_pokemonStage.StartedWave += SetWaveText;
		_exitButton.Pressed += () =>
		{
			StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
			_pokemonStage.AddSibling(stageSelectInterface);
			_pokemonStage.QueueFree();

			PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.HasLeftStage);
		};
		_settingsButton.Pressed += () =>
		{
			SettingsInterface settingsInterface = PokemonTD.PackedScenes.GetSettingsInterface();
			AddSibling(settingsInterface);

			PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PressedPause);
		};
	}

	private void SetRareCandyText()
	{
		_rareCandy.Text = $"{_pokemonStage.RareCandy}";
	}

	private void SetPokeDollarsText()
	{
		_pokeDollars.Text = $"₽ {PokemonTD.PokeDollars}";
	}

	private void SetWaveText()
	{
		_waveCount.Text = $"Wave {_pokemonStage.CurrentWave} of {_pokemonStage.WaveCount}";
	}

	private void SetVisibility(bool isDragging)
	{
		_container.Visible = !isDragging;
	}
	
	public PokemonTeamSlot FindPokemonTeamSlot(int pokemonTeamIndex)
	{
		return _pokemonTeamSlots.FindPokemonTeamSlot(pokemonTeamIndex);
	}
}
