using System.Collections.Generic;

using Godot;
using GC = Godot.Collections;

namespace PokemonTD;

public partial class PokemonStage : Node2D
{
	[Signal]
	public delegate void StartedWaveEventHandler();

	[Signal]
	public delegate void FinishedWaveEventHandler();

	[Export]
	private Path2D _path;

	[Export]
	private Node2D _transparentLayers; // For tile map layers that will turn transparent when moving pokemon from team to slot

	[Export]
	private Control _stageSlots;

	[Export]
	private StageInterface _stageInterface;

	private GC.Array<PathFollow2D> _pathFollows = new GC.Array<PathFollow2D>();

	public int ID = 1;
	public int WaveCount = 10;
	public int PokemonPerWave = 5;
	public List<int> PokemonLevels = new List<int>();

	public List<string> PokemonNames = new List<string>(); // List to spawn from
	
	public int CurrentWave;
	public int RareCandy = 100;

	public bool HasFinished;

	public StageInterface StageInterface;

	public List<StageSlot> StageSlots = new List<StageSlot>();

	public List<PokemonEnemy> PokemonEnemies = new List<PokemonEnemy>(); // List for enemy scenes

	public override void _EnterTree()
	{
		StageInterface = _stageInterface;

		PokemonTD.Signals.ForgetMove += OnForgetMove;
		PokemonTD.Signals.PokemonEnemyFainted += OnPokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyPassed += OnPokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyCaptured += OnPokemonEnemyEvent;
		PokemonTD.Signals.DraggingStageSlot += SetLayersAlpha;
		PokemonTD.Signals.DraggingTeamStageSlot += SetLayersAlpha;

		foreach (Node child in _stageSlots.GetChildren())
		{
			if (child is StageSlot stageSlot) StageSlots.Add(stageSlot);
		}
	}

    public override void _ExitTree()
    {
		PokemonTD.Signals.ForgetMove -= OnForgetMove;
		PokemonTD.Signals.PokemonEnemyFainted -= OnPokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyPassed -= OnPokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyCaptured -= OnPokemonEnemyEvent;
		PokemonTD.Signals.DraggingStageSlot -= SetLayersAlpha;
		PokemonTD.Signals.DraggingTeamStageSlot -= SetLayersAlpha;
    }

   	public override void _Ready()
	{
		if (PokemonNames.Count == 0)
		{
			PrintRich.PrintLine(TextColor.Red, "Found No Pokemon To Spawn");
			PokemonTD.AreStagesEnabled = false;
		}
		
		if (PokemonTD.AreStagesEnabled) WaveInterval();
	}

	public override void _Process(double delta)
	{
		if (!PokemonTD.AreStagesEnabled || HasFinished || PokemonTD.IsGamePaused) return;

		foreach (PathFollow2D pathFollow in _pathFollows)
		{
			PokemonEnemy pokemonEnemy = pathFollow.GetChildOrNull<PokemonEnemy>(0);
			pathFollow.Progress += (float) delta * pokemonEnemy.Pokemon.Speed * PokemonTD.GameSpeed;
		}
	}

	private void OnForgetMove(Pokemon pokemon, PokemonMove pokemonMove)
	{
		ForgetMoveInterface forgetMoveInterface = PokemonTD.PackedScenes.GetForgetMoveInterface();
		forgetMoveInterface.Pokemon = pokemon;
		forgetMoveInterface.MoveToLearn = pokemonMove;

		AddSibling(forgetMoveInterface);
	}

	private void SetLayersAlpha(bool isDragging)
	{
		Color color = Colors.White;
		color.A = isDragging ? 0.75f : 1; 
		_transparentLayers.Modulate = color;
	}

	private void OnPokemonEnemyEvent(PokemonEnemy pokemonEnemy)
	{
		PokemonEnemies.Remove(pokemonEnemy);
		IsWaveFinished();
		RemovePathFollow(pokemonEnemy);
	}

	private async void WaveInterval()
	{
		float timeSeconds = 1f;
		while (CurrentWave <= WaveCount)
		{
			StartWave();
			EmitSignal(SignalName.StartedWave);

			string waveMessage = $"Starting Wave {CurrentWave} / {WaveCount}";
			PrintRich.PrintLine(TextColor.Yellow, waveMessage);

			await ToSignal(this, SignalName.FinishedWave);
			await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout); // Add delay in bewtween waves
		}

		if (RareCandy <= 0) return;
		
		HasFinished = true;
		ShowWonInterface();
		PokemonTD.Signals.EmitSignal(Signals.SignalName.HasWon);

		string wonMessage = $"You've Won!";
		PrintRich.PrintLine(TextColor.Yellow, wonMessage);
	}

	private async void StartWave()
	{
		CurrentWave++;
		
		float timeSeconds = 3f;
		for (int i = 0; i < PokemonPerWave; i++)
		{
			SpawnPokemon();
			await ToSignal(GetTree().CreateTimer(timeSeconds / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout); // Delay spawning Pokemon
			
			if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, Signals.SignalName.PressedPlay);
		}
	}

	private void IsWaveFinished()
	{
		if (PokemonEnemies.Count == 0) EmitSignal(SignalName.FinishedWave);
	}

	private void SpawnPokemon()
	{
		string randomPokemonName = GetRandomPokemonName();
		int randomLevel = GetRandomLevel();
		Pokemon randomPokemon = PokemonTD.PokemonManager.GetPokemon(randomPokemonName, randomLevel);
		
		PokemonEnemy pokemonEnemy = GetPokemonEnemy(randomPokemon);
		PokemonEnemies.Add(pokemonEnemy);
		
		AddPathFollow(pokemonEnemy);

		string spawnMessage = $"Spawning Level {randomPokemon.Level} {randomPokemon.Name}";
		PrintRich.PrintLine(TextColor.Yellow, spawnMessage);
	}

	private PokemonEnemy GetPokemonEnemy(Pokemon pokemon)
	{
		PokemonEnemy pokemonEnemy = PokemonTD.PackedScenes.GetPokemonEnemy();
		pokemonEnemy.SetPokemon(pokemon);
		pokemonEnemy.ScreenNotifier.ScreenExited += () => 
		{
			DecreaseRareCandy(pokemon);
			RemovePathFollow(pokemonEnemy);
			PokemonEnemies.Remove(pokemonEnemy);
		};

		return pokemonEnemy;
	}

	// Get a random Pokemon name based on the stage
	private string GetRandomPokemonName()
	{
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		int randomValue = RNG.RandiRange(0, PokemonNames.Count - 1);

		return PokemonNames[randomValue];
	}

	private int GetRandomLevel()
	{
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		int minLevel = PokemonLevels[0];
		int maxLevel = PokemonLevels[1];
		int randomLevel = RNG.RandiRange(minLevel, maxLevel);

		return randomLevel;
	}

	private void AddPathFollow(PokemonEnemy pokemonEnemy)
	{
		PathFollow2D pathFollow = GetPathFollow();
		pathFollow.AddChild(pokemonEnemy);

		_pathFollows.Add(pathFollow);
		_path.AddChild(pathFollow);
	}

	private void DecreaseRareCandy(Pokemon pokemon)
	{
		float percentageAmount = 0.05f; // 1%
		RareCandy -= Mathf.RoundToInt(pokemon.Attack * percentageAmount);

		HasLostAllCandy();
	}

	private void HasLostAllCandy()
	{
		if (RareCandy > 0) return;
		
		RareCandy = 0; // Prevent a negative value

		HasFinished = true;
		ShowLostInterface();
		PokemonTD.Signals.EmitSignal(Signals.SignalName.HasLost);

		string lostMessage = $"You've Lost All Your Candy!";
		PrintRich.PrintLine(TextColor.Yellow, lostMessage);
	}

	private void ShowWonInterface()
	{
		StageStateInterface wonInterface = PokemonTD.PackedScenes.GetStageStateInterface();
		wonInterface.StageID = ID;
		wonInterface.Message.Text = "You've won!";
		AddChild(wonInterface);
	}

	private void ShowLostInterface()
	{
		StageStateInterface lostInterface = PokemonTD.PackedScenes.GetStageStateInterface();
		lostInterface.StageID = ID;
		lostInterface.Message.Text = "You've lost all your candy!";
		AddChild(lostInterface);
	}

	private void RemovePathFollow(PokemonEnemy pokemonEnemy)
	{
		PathFollow2D pathFollow = pokemonEnemy.GetParentOrNull<PathFollow2D>();
		_pathFollows.Remove(pathFollow);
		_path.RemoveChild(pathFollow);
		pathFollow.QueueFree();
	}

	private PathFollow2D GetPathFollow()
	{
		return new PathFollow2D()
		{
			Rotates = false,
			Loop = false,
			YSortEnabled = true
		};
	}
}
