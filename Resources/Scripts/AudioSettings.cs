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

        _masterSlider.Value = PokemonTD.AudioManager.FindAudioBus(BusType.Master).Volume;
        _musicSlider.Value = PokemonTD.AudioManager.FindAudioBus(BusType.Music).Volume;
        _soundSlider.Value = PokemonTD.AudioManager.FindAudioBus(BusType.Sound).Volume;
    }
    
    private void ValueChanged(BusType busType, double value)
	{
		float minVolume = -50;
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.AudioValueChanged, (int) busType, value);

		bool isMuted = value == minVolume;
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.AudioMuted, (int) busType, isMuted);
	}
}
