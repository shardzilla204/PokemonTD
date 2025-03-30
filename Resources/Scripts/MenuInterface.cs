using Godot;

namespace PokémonTD;

public partial class MenuInterface : CanvasLayer
{
	[Export]
	private CustomButton _playButton;

	[Export]
	private CustomButton _settingsButton;

	[Export]
	private CustomButton _exitButton;

	public override void _Ready()
	{
		_playButton.Pressed += OnPressedPlay;
		_settingsButton.Pressed += OnPressedSettings;
		_exitButton.Pressed += OnPressedExit;
	}

	private void OnPressedPlay()
	{
		StarterSelectionInterface starterSelectionInterface = PokémonTD.PackedScenes.GetStarterSelectionInterface();
		AddSibling(starterSelectionInterface);
		QueueFree();
	}

	private void OnPressedSettings()
	{

	}

	private void OnPressedExit()
	{
		GetTree().Quit();
	}
}
