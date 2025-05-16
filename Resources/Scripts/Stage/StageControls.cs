using System.Collections.Generic;
using Godot;

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
	}

	public override void _Ready()
	{
		PokemonTD.Signals.PressedPlay += UpdateGameTexture;
		PokemonTD.Signals.PressedPause += UpdateGameTexture;

		// Default to speed of 1
		PokemonTD.Signals.EmitSignal(Signals.SignalName.SpeedToggled, 1);

		_gameToggle.Pressed += OnGamePressed;
		_speedToggle.LeftClick += () => OnSpeedPressed(true);
		_speedToggle.RightClick += () => OnSpeedPressed(false);

		_gameTexture.Texture = !PokemonTD.IsGamePaused ? _pauseTexture : _playTexture;
	}

	private void UpdateGameTexture()
	{
		_gameTexture.Texture = !PokemonTD.IsGamePaused ? _pauseTexture : _playTexture;
	}

	private void OnGamePressed()
	{
		PokemonTD.IsGamePaused = !PokemonTD.IsGamePaused;
		PokemonTD.Signals.EmitSignal(Signals.SignalName.StageStarted);

		UpdateGameTexture();

		if (PokemonTD.IsGamePaused)
		{
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPause);
		}
		else
		{
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPlay);
		}
	}

	private void OnSpeedPressed(bool isLeftClick)
	{
		int indexValue = isLeftClick ? 1 : -1;
		_speedOption += indexValue;

		if (_speedOption >= _speedOptions.Count) _speedOption = 0;
		if (_speedOption < 0) _speedOption = _speedOptions.Count - 1;

		float speed = _speedOptions[_speedOption];

		_speedLabel.Text = speed != 0.5f ? $"{speed}x" : $"½x";

		StageInterface stageInterface = GetParentOrNull<Node>().GetOwnerOrNull<StageInterface>();
		PokemonStage pokemonStage = stageInterface.GetParentOrNull<PokemonStage>();

		foreach (StageSlot stageSlot in pokemonStage.StageSlots)
		{
			stageSlot.SetWaitTime();
		}

		PokemonTD.Signals.EmitSignal(Signals.SignalName.SpeedToggled, speed);
	}
}
