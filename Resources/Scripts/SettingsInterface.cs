using Godot;

namespace PokemonTD;

public partial class SettingsInterface : CanvasLayer
{
	[Export]
	private CustomButton _exitButton;

	[Export]
	private CustomButton _informationButton;

	[Export]
	private Container _gameSettings;

	[Export]
	private CustomButton _saveButton;

	[Export]
	private CustomButton _loadButton;

	[Export]
	private CustomButton _deleteButton;

	[Export]
	private HSlider _masterSlider;

	[Export]
	private HSlider _musicSlider;

	[Export]
	private HSlider _soundSlider;

	public bool FromMainMenu;

    public override void _Ready()
    {
		_exitButton.Pressed += () => 
		{
			if (FromMainMenu)
			{
				MenuInterface menuInterface = PokemonTD.PackedScenes.GetMenuInterface();
				AddSibling(menuInterface);
			}
			else
			{
				PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPlay);
			}
			QueueFree();
		};
		
		_informationButton.Pressed += () => 
		{
			InformationInterface informationInterface = PokemonTD.PackedScenes.GetInformationInterface();
			informationInterface.FromMainMenu = FromMainMenu;
			AddSibling(informationInterface);
			QueueFree();
		};
		
		_gameSettings.Visible = FromMainMenu;

		_saveButton.Pressed += () => PokemonTD.Signals.EmitSignal(Signals.SignalName.GameSaved);
		_loadButton.Pressed += () => PokemonTD.Signals.EmitSignal(Signals.SignalName.GameLoaded);
		_deleteButton.Pressed += () => PokemonTD.Signals.EmitSignal(Signals.SignalName.GameReset);

        _masterSlider.ValueChanged += (value) => OnValueChanged(BusType.Master, value);
        _musicSlider.ValueChanged += (value) => OnValueChanged(BusType.Music, value);
        _soundSlider.ValueChanged += (value) => OnValueChanged(BusType.Sound, value);

		_masterSlider.Value = AudioServer.GetBusVolumeDb((int) BusType.Master);
		_musicSlider.Value = AudioServer.GetBusVolumeDb((int) BusType.Music);
		_soundSlider.Value = AudioServer.GetBusVolumeDb((int) BusType.Sound);
    }

	private void OnValueChanged(BusType busType, double value)
	{
		float minimumVolume = -50;
		PokemonTD.Signals.EmitSignal(Signals.SignalName.AudioValueChanged, (int) busType, value);

		bool isMuted = value == minimumVolume;
		PokemonTD.Signals.EmitSignal(Signals.SignalName.AudioMuted, (int) busType, isMuted);
	}
}
