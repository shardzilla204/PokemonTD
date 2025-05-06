using System.Collections.Generic;
using Godot;

namespace PokemonTD;

public partial class StageControls : HBoxContainer
{
	[Signal]
	public delegate void VisibilityToggledEventHandler(bool isVisible);

	[Export]
	private CustomButton _gameToggle;

	[Export]
	private CustomButton _speedToggle;

	[Export]
	private CustomButton _visiblityToggle;

	[Export]
	private TextureRect _gameTexture;

	[Export]
	private TextureRect _visibilityTexture;

	[Export]
	private Texture2D _hideTexture;

	[Export]
	private Texture2D _showTexture;

	[Export]
	private Texture2D _playTexture;

	[Export]
	private Texture2D _pauseTexture;

	private int _speedOption;
	private List<float> _speedOptions = new List<float>(){ 1, 2, 4, 0.5f };
	private bool _isVisible = true;

	public override void _Ready()
	{
		// Default to speed of 1
		PokemonTD.Signals.EmitSignal(Signals.SignalName.SpeedToggled, 1);

		_gameToggle.Pressed += () => 
		{
			OnGamePressed();
			PokemonTD.AudioManager.PlayButtonPressed();
		};
		_speedToggle.Pressed += () => 
		{
			OnSpeedPressed();
			PokemonTD.AudioManager.PlayButtonPressed();
		};
		_visiblityToggle.Pressed += () => 
		{
			OnVisibilityPressed();
			PokemonTD.AudioManager.PlayButtonPressed();
		};

		_gameToggle.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
		_speedToggle.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
		_visiblityToggle.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;

		_gameTexture.Texture = !PokemonTD.IsGamePaused ? _pauseTexture : _playTexture;
	}

	private void OnGamePressed()
	{
		PokemonTD.IsGamePaused = !PokemonTD.IsGamePaused;

		_gameTexture.Texture = !PokemonTD.IsGamePaused ? _pauseTexture : _playTexture;

		if (PokemonTD.IsGamePaused)
		{
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPause);
		}
		else
		{
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPlay);
		}
	}

	private void OnSpeedPressed()
	{
		_speedOption++;

		if (_speedOption >= _speedOptions.Count) _speedOption = 0;

		float speed = _speedOptions[_speedOption];

		_speedToggle.Text = speed != 0.5f ? $"{speed}x" : $"½x";

		StageInterface stageInterface = GetParentOrNull<Node>().GetOwnerOrNull<StageInterface>();
		PokemonStage pokemonStage = stageInterface.GetParentOrNull<PokemonStage>();

		foreach (StageSlot stageSlot in pokemonStage.StageSlots)
		{
			stageSlot.SetWaitTime();
		}

		PokemonTD.Signals.EmitSignal(Signals.SignalName.SpeedToggled, speed);
	}

	private void OnVisibilityPressed()
	{
		_isVisible = !_isVisible;
		_visibilityTexture.Texture = _isVisible ? _hideTexture : _showTexture;

		EmitSignal(SignalName.VisibilityToggled, _isVisible);
	}
}
