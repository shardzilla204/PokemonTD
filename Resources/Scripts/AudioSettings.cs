using Godot;

namespace PokemonTD;

public partial class AudioSettings : Container
{
    [Export]
    private HSlider _masterSlider;

    [Export]
    private HSlider _musicSlider;

    [Export]
    private HSlider _soundSlider;

    public override void _Ready()
    {
        _masterSlider.ValueChanged += (value) => ValueChanged(BusType.Master, value);
        _musicSlider.ValueChanged += (value) => ValueChanged(BusType.Music, value);
        _soundSlider.ValueChanged += (value) => ValueChanged(BusType.Sound, value);

        _masterSlider.Value = AudioServer.GetBusVolumeDb((int)BusType.Master);
        _musicSlider.Value = AudioServer.GetBusVolumeDb((int)BusType.Music);
        _soundSlider.Value = AudioServer.GetBusVolumeDb((int)BusType.Sound);
    }
    
    private void ValueChanged(BusType busType, double value)
	{
		float minimumVolume = -50;
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.AudioValueChanged, (int) busType, value);

		bool isMuted = value == minimumVolume;
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.AudioMuted, (int) busType, isMuted);
	}
}
