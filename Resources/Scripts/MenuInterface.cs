using Godot;

namespace PokemonTD;

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
		_playButton.Pressed += OnPlayPressed;
		_settingsButton.Pressed += OnSettingsPressed;
		_exitButton.Pressed += OnExitPressed;
	}

	private void OnPlayPressed()
	{
		StarterSelectionInterface starterSelectionInterface = PokemonTD.PackedScenes.GetStarterSelectionInterface();
		AddSibling(starterSelectionInterface);
		QueueFree();
	}

	private void OnSettingsPressed()
	{

	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}
}
