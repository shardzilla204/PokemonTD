using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class StageControls : HBoxContainer
{
	[Export]
	private CustomButton _gameToggle;

	[Export]
	private CustomButton _speedToggle;

	[Export]
	private TextureRect _gameTexture;

	[Export]
	private Label _speedLabel;

	[Export]
	private Texture2D _playTexture;

	[Export]
	private Texture2D _pauseTexture;

	private int _speedOption;
	private List<float> _speedOptions = new List<float>(){ 1, 2, 4, 0.5f };

	public override void _ExitTree()
	{
		PokemonTD.Signals.PressedPlay -= UpdateGameTexture;
		PokemonTD.Signals.PressedPause -= UpdateGameTexture;
		PokemonTD.Keybinds.GameState -= GamePressed;
		PokemonTD.Keybinds.GameSpeed -= SpeedPressed;
	}

	public override void _Ready()
	{
		PokemonTD.Signals.PressedPlay += UpdateGameTexture;
		PokemonTD.Signals.PressedPause += UpdateGameTexture;
		PokemonTD.Keybinds.GameState += GamePressed;
		PokemonTD.Keybinds.GameSpeed += SpeedPressed;

		// Default to speed of 1
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.SpeedToggled, 1);

		_gameToggle.Pressed += GamePressed;
		_speedToggle.LeftClick += () => SpeedPressed(true);
		_speedToggle.RightClick += () => SpeedPressed(false);

		UpdateGameTexture();
	}

	private void UpdateGameTexture()
	{
		_gameTexture.Texture = !PokemonTD.IsGamePaused ? _pauseTexture : _playTexture;
	}

	private void GamePressed()
	{
		PokemonTD.IsGamePaused = !PokemonTD.IsGamePaused;
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.StageStarted);

		StringName signalName = PokemonTD.IsGamePaused ? PokemonSignals.SignalName.PressedPause : PokemonSignals.SignalName.PressedPlay;
		PokemonTD.Signals.EmitSignal(signalName);
	}

	private void SpeedPressed(bool isLeftClick)
	{
		int indexValue = isLeftClick ? 1 : -1;
		_speedOption += indexValue;

		if (_speedOption >= _speedOptions.Count) _speedOption = 0;
		if (_speedOption < 0) _speedOption = _speedOptions.Count - 1;

		float speed = _speedOptions[_speedOption];

		_speedLabel.Text = speed != 0.5f ? $"{speed}x" : $"½x";

		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.SpeedToggled, speed);
	}
}
