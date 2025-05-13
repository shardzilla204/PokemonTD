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

		_playButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
		_settingsButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
		_exitButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;

		if (!PokemonTD.AudioManager.IsPlayingSong(1)) PokemonTD.AudioManager.PlaySong(1); // 01. ~Opening~
	}

	private void OnPlayPressed()
	{
		if (!PokemonTD.HasSelectedStarter)
		{
			StarterSelectionInterface starterSelectionInterface = PokemonTD.PackedScenes.GetStarterSelectionInterface();
			AddSibling(starterSelectionInterface);
		}
		else
		{
			StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
			AddSibling(stageSelectInterface);
		}
		PokemonTD.AudioManager.PlayButtonPressed();
		QueueFree();

		if (!PokemonTD.HasSelectedStarter) PokemonTD.AudioManager.PlayMusic(null);
	}

	private void OnSettingsPressed()
	{
		PokemonTD.AudioManager.PlayButtonPressed();

		SettingsInterface settingsInterface = PokemonTD.PackedScenes.GetSettingsInterface();
		settingsInterface.FromMainMenu = true;
		AddSibling(settingsInterface);

		QueueFree();
	}

	private async void OnExitPressed()
	{
		PokemonTD.AudioManager.PlayButtonPressed();
		PokemonTD.Signals.EmitSignal(Signals.SignalName.GameSaved);

		await ToSignal(GetTree().CreateTimer(0.25f), SceneTreeTimer.SignalName.Timeout);

		GetTree().Quit();
	}
}
