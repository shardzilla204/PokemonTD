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

		if (!PokemonTD.AudioManager.IsPlayingSong(1)) PokemonTD.AudioManager.PlaySong(1); // 01. ~Opening~
	}

	private void OnPlayPressed()
	{
		Node interfaceToAdd = !PokemonTD.HasSelectedStarter ? PokemonTD.PackedScenes.GetStarterSelectionInterface() : PokemonTD.PackedScenes.GetStageSelectInterface();
		AddSibling(interfaceToAdd);
		QueueFree();

		if (!PokemonTD.HasSelectedStarter) PokemonTD.AudioManager.PlayMusic(null);
	}

	private void OnSettingsPressed()
	{
		SettingsInterface settingsInterface = PokemonTD.PackedScenes.GetSettingsInterface();
		settingsInterface.FromMainMenu = true;
		AddSibling(settingsInterface);
		QueueFree();
	}

	private async void OnExitPressed()
	{
		PokemonTD.Signals.EmitSignal(Signals.SignalName.GameSaved);

		await ToSignal(GetTree().CreateTimer(0.25f), SceneTreeTimer.SignalName.Timeout);

		GetTree().Quit();
	}
}
