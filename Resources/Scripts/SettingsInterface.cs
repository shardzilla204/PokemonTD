using Godot;

namespace PokemonTD;

public partial class SettingsInterface : CanvasLayer
{
	[Export]
	private CustomButton _exitButton;

	[Export]
	private HSlider _masterSlider;

	[Export]
	private HSlider _musicSlider;

	[Export]
	private HSlider _soundSlider;

    public override void _Ready()
    {
		_exitButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
		_exitButton.Pressed += () => 
		{
			PokemonTD.AudioManager.PlayButtonPressed();
			MenuInterface menuInterface = PokemonTD.PackedScenes.GetMenuInterface();
			AddSibling(menuInterface);
			QueueFree();
		};

        _masterSlider.ValueChanged += (value) => OnValueChanged(BusType.Master, value);
        _musicSlider.ValueChanged += (value) => OnValueChanged(BusType.Music, value);
        _soundSlider.ValueChanged += (value) => OnValueChanged(BusType.Sound, value);

		_masterSlider.Value = AudioServer.GetBusVolumeDb((int) BusType.Master);
		_musicSlider.Value = AudioServer.GetBusVolumeDb((int) BusType.Music);
		_soundSlider.Value = AudioServer.GetBusVolumeDb((int) BusType.Sound);
    }

	private void OnValueChanged(BusType busType, double value)
	{
		float minVolume = -50;
		PokemonTD.Signals.EmitSignal(Signals.SignalName.AudioValueChanged, (int) busType, value);

		bool isMuted = value == minVolume;
		PokemonTD.Signals.EmitSignal(Signals.SignalName.AudioMuted, (int) busType, isMuted);
	}
}
