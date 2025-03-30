using System.Collections.Generic;
using Godot;

namespace PokémonTD;

public partial class PokémonStageControls : HBoxContainer
{
	[Export]
	private CustomButton _gameToggle;

	[Export]
	private CustomButton _speedToggle;

	[Export]
	private CustomButton _visiblityToggle;

	private bool _isPaused;
	private int _speedOption;
	private List<int> _speedOptions = new List<int>(){ 1, 2, 4 };
	private float _maxSpeed = 3;
	private bool _isVisible = true;

	public override void _Ready()
	{
		_gameToggle.Pressed += OnGamePressed;
		_speedToggle.Pressed += OnSpeedPressed;
		_visiblityToggle.Pressed += OnVisibilityPressed;
	}

	private void OnGamePressed()
	{
		_isPaused = !_isPaused;

		_gameToggle.Text = _isPaused ? "Pause" : "Play";

		if (_isPaused)
		{
			PokémonTD.Signals.EmitSignal(Signals.SignalName.PressedPause);
		}
		else
		{
			PokémonTD.Signals.EmitSignal(Signals.SignalName.PressedPlay);
		}
	}

	private void OnSpeedPressed()
	{
		_speedOption++;

		if (_speedOption >= _speedOptions.Count) _speedOption = 0;

		float speed = _speedOptions[_speedOption];

		_speedToggle.Text = $"{speed}x";

		PokémonTD.Signals.EmitSignal(Signals.SignalName.SpeedToggled, speed);
	}

	private void OnVisibilityPressed()
	{
		_isVisible = !_isVisible;
		_visiblityToggle.Text = _isVisible ? "Hide" : "Show";

		PokémonTD.Signals.EmitSignal(Signals.SignalName.VisibilityToggled, _isVisible);
	}
}
