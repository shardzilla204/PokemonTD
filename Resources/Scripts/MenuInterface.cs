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
		_playButton.Pressed += PlayPressed;
		_settingsButton.Pressed += SettingsPressed;
		_exitButton.Pressed += ExitPressed;

		if (!PokemonTD.AudioManager.IsPlayingSong(1)) PokemonTD.AudioManager.PlaySong(1); // 01. ~Opening~
	}

	private void PlayPressed()
	{
		if (!PokemonSettings.Instance.HasShowedTutorial)
		{
			InformationInterface informationInterface = PokemonTD.PackedScenes.GetInformationInterface();
			informationInterface.FromMainMenu = true;
			AddSibling(informationInterface);
			QueueFree();
			return;
		}

		Node interfaceToAdd = !PokemonTD.HasSelectedStarter ? PokemonTD.PackedScenes.GetStarterSelectionInterface() : PokemonTD.PackedScenes.GetStageSelectInterface();
		AddSibling(interfaceToAdd);
		QueueFree();

		if (!PokemonTD.HasSelectedStarter) PokemonTD.AudioManager.PlayMusic(null);
	}

	private void SettingsPressed()
	{
		SettingsInterface settingsInterface = PokemonTD.PackedScenes.GetSettingsInterface();
		settingsInterface.FromMainMenu = true;
		AddSibling(settingsInterface);
		QueueFree();
	}

	private async void ExitPressed()
	{
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.GameSaved);

		await ToSignal(GetTree().CreateTimer(0.25f), SceneTreeTimer.SignalName.Timeout);

		GetTree().Quit();
	}
}
