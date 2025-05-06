using System.Collections.Generic;
using System.Linq;
using Godot;

namespace PokemonTD;

public partial class AudioManager : AudioStreamPlayer
{
	[Export]
	private AudioStreamPlayer _musicStreamPlayer;

	[Export]
	private AudioStreamPlayer _soundStreamPlayer;

	private Control _audioSettings;
	private bool _isSettingsCanvasOpen;
	private List<int> _songIDs = new List<int>() { 2, 4, 5, 6, 8, 10 } ;
	
	public override void _EnterTree()
	{
		PokemonTD.AudioManager = this;

		PokemonTD.Signals.AudioValueChanged += OnAudioValueChanged;
		PokemonTD.Signals.AudioMuted += OnAudioMuted;

		PokemonTD.Signals.GameStarted += () => PlaySong(1); // 01. ~Opening~
		PokemonTD.Signals.OnStageSelection += MusicInterval;
		PokemonTD.Signals.PokemonStarterSelected += (pokemon) => MusicInterval();
	}

    public override void _Ready()
    {
        AudioServer.SetBusVolumeDb((int) BusType.Master, -25f);
		AudioServer.SetBusVolumeDb((int) BusType.Music, 0f);
		AudioServer.SetBusVolumeDb((int) BusType.Sound, -25f);
    }
	
	public override void _Input(InputEvent @event)
	{
	  	if (@event is not InputEventKey eventKey) return;

		if (!eventKey.Pressed) return;
	}

	private void OnAudioValueChanged(int busIndex, int volume)
	{
		AudioServer.SetBusVolumeDb(busIndex, volume);
	}

	private void OnAudioMuted(int busIndex, bool isMuted)
	{
		AudioServer.SetBusMute(busIndex, isMuted);
	}

	public void PlayPokemonCry(Pokemon pokemon, bool isOneshot)
	{
		AudioStream pokemonCry = GetPokemonCry(pokemon);
		if (isOneshot) 
		{
			PlayOneshotSound(pokemonCry);
		}
		else
		{
			PlaySound(pokemonCry);
		}
	}

	private AudioStream GetPokemonCry(Pokemon pokemon)
	{
		string filePath = $"res://Assets/Audio/Cry/{pokemon.Name}Cry.wav";
		return ResourceLoader.Load<AudioStream>($"{filePath}");
	}

	// GEN 1 Growl uses the Pokemon's cry
	public void PlayPokemonMove(AudioStreamPlayer pokemonMovePlayer, string pokemonMoveName, Pokemon pokemon)
	{
		if (pokemonMoveName == "Growl")
		{
			pokemonMovePlayer.Stream = GetPokemonCry(pokemon);
			pokemonMovePlayer.Play();
			return;
		}

		string pokemonMoveString = pokemonMoveName.ToString().Replace(" ", "");
		pokemonMoveString = pokemonMoveString.Replace("-", "");

		string filePath = $"res://Assets/Audio/Move/{pokemonMoveString}.wav";
		AudioStream pokemonMove = ResourceLoader.Load<AudioStream>($"{filePath}");
		pokemonMovePlayer.Stream = pokemonMove;
		pokemonMovePlayer.Play();
	}

	// public void PlayPokemonDamage()
	// {
	// 	string filePath = $"res://Assets/Audio/PokemonDamage.wav";
	// 	AudioStream pokemonDamage = ResourceLoader.Load<AudioStream>($"{filePath}");
	// 	PlaySound(pokemonDamage);
	// }

	public void PlayPokemonFaint()
	{
		string filePath = $"res://Assets/Audio/PokemonFainted.wav";
		AudioStream pokemonFaint = ResourceLoader.Load<AudioStream>($"{filePath}");
		PlayOneshotSound(pokemonFaint);
	}

	public void PlayPokemonLeveledUp()
	{
		string filePath = $"res://Assets/Audio/PokemonLeveledUp.wav";
		AudioStream pokemonLeveledUp = ResourceLoader.Load<AudioStream>($"{filePath}");
		PlayOneshotSound(pokemonLeveledUp);
	}

	public void PlayButtonHovered()
	{
		string filePath = "res://Assets/Audio/ButtonHovered.wav";
		AudioStream buttonHovered = ResourceLoader.Load<AudioStream>($"{filePath}");
		PlayOneshotSound(buttonHovered);
	}

	public void PlayButtonPressed()
	{
		string filePath = "res://Assets/Audio/ButtonPressed.wav";
		AudioStream buttonPressed = ResourceLoader.Load<AudioStream>($"{filePath}");
		PlayOneshotSound(buttonPressed);
	}

	//! Include .import file and remove when loading or else audio will not work when exported
	public void PlaySong(int songID)
	{
		string songIDString = songID.ToString();
		string fileDirectory = $"res://Assets/Audio/Music/";
		List<string> songs = DirAccess.GetFilesAt(fileDirectory).ToList().FindAll(fileName => fileName.Contains(".import"));
		string song = songs.Find(song => song.Contains($"{songID}"));

		string filePath = $"{fileDirectory}{song}".Replace(".import", "");
		AudioStream songStream = ResourceLoader.Load<AudioStream>($"{filePath}");
		PlayMusic(songStream);
	}

	public bool IsPlayingSong(int songID)
	{
		string songIDString = songID.ToString();
		string fileDirectory = $"res://Assets/Audio/Music/";
		List<string> songs = DirAccess.GetFilesAt(fileDirectory).ToList().FindAll(fileName => fileName.Contains(".import"));
		string song = songs.Find(song => song.Contains($"{songID}"));

		string filePath = $"{fileDirectory}{song}".Replace(".import", "");
		AudioStream songStream = ResourceLoader.Load<AudioStream>($"{filePath}");

		AudioStream currentSong = _musicStreamPlayer.Stream;

		return currentSong == songStream;
	}

	private async void MusicInterval()
	{
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		int randomValue = RNG.RandiRange(0, _songIDs.Count - 1);
		int songID = _songIDs[randomValue];

		while (true)
		{
			PlaySong(songID);

			await ToSignal(_musicStreamPlayer, "finished");

			randomValue = RNG.RandiRange(0, _songIDs.Count);
			int nextSongID = _songIDs[randomValue];
			while (nextSongID == songID)
			{
				randomValue = RNG.RandiRange(0, _songIDs.Count);
				nextSongID = _songIDs[randomValue];
			}
			songID = nextSongID;
		}
	}

	private int GetRandomSongID()
	{
		List<AudioStream> music = GetMusic();
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		return RNG.RandiRange(0, music.Count - 1);
	}

	private List<AudioStream> GetMusic()
	{
		string fileDirectory = "res://Assets/Audio/Music/";
		List<AudioStream> music = new List<AudioStream>();
		List<string> musicNames = DirAccess.GetFilesAt(fileDirectory).ToList().FindAll(fileName => fileName.Contains(".import"));
		
		foreach (string musicName in musicNames)
		{
			string filePath = $"{fileDirectory}{musicName}".Replace(".import", "");
			AudioStream song = ResourceLoader.Load<AudioStream>($"{filePath}");
			music.Add(song);
		}
		
		return music;
	}

	public void PlayMusic(AudioStream audioStream)
	{
		_musicStreamPlayer.Stop();
		_musicStreamPlayer.Stream = audioStream;
		_musicStreamPlayer.Play();
	}

	public void PlaySound(AudioStream audioStream)
	{
		_soundStreamPlayer.Stream = audioStream;
		_soundStreamPlayer.Play();
	}

	public void PlayOneshotSound(AudioStream audioStream)
	{
		AudioStreamPlayer soundStreamPlayer = new AudioStreamPlayer()
		{
			Stream = audioStream,
			Bus = "Sound",
			Autoplay = true
		};
		soundStreamPlayer.Finished += soundStreamPlayer.QueueFree;

		AddChild(soundStreamPlayer);
	}
}
