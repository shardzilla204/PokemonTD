using Godot;

namespace PokemonTD;

public enum BusType
{
	Master, 
	Music,
	Sound
}

public partial class AudioBus : Node
{
	public AudioBus(BusType busType)
	{
		BusType = busType;
	}

	public BusType BusType;
	public float Volume = -25;
	public bool IsMuted = false;

	public void ToggleMute()
	{
		IsMuted = !IsMuted;
	}

	public void SetMute(bool isMuted)
	{
		IsMuted = isMuted;
	}
}