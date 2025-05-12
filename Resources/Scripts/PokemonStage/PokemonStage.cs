using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System;

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
		PokemonTD.Signals.DraggingStageSlot += SetStageSlotsAlpha;
		PokemonTD.Signals.DraggingStageTeamSlot += SetStageSlotsAlpha;
		PokemonTD.Signals.DraggingPokeBall += SetStageSlotsAlpha;
		PokemonTD.Signals.EvolutionStarted += OnEvolutionStarted;

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
		PokemonTD.Signals.DraggingStageSlot -= SetStageSlotsAlpha;
		PokemonTD.Signals.DraggingStageTeamSlot -= SetStageSlotsAlpha;
		PokemonTD.Signals.EvolutionStarted -= OnEvolutionStarted;
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
			MovePokemonEnemy(pathFollow, delta);
		}
	}

	private void MovePokemonEnemy(PathFollow2D pathFollow, double delta)
	{
		PokemonEnemy pokemonEnemy = pathFollow.GetChildOrNull<PokemonEnemy>(0);
		Vector2 previousPosition = pokemonEnemy.GlobalPosition;
		double progressSpeed = delta * pokemonEnemy.Speed * PokemonTD.GameSpeed;
		
		if (!pokemonEnemy.CanMove) return;
		
		pathFollow.Progress += (float) progressSpeed;

		Vector2 direction = previousPosition.DirectionTo(pokemonEnemy.GlobalPosition);
		bool isMovingRight = Math.Round(direction.X, 1) >= 0;
		pokemonEnemy.FlipH = isMovingRight;
	}

	private async void OnForgetMove(Pokemon pokemon, PokemonMove pokemonMove)
	{
		ForgetMoveInterface forgetMoveInterface = PokemonTD.PackedScenes.GetForgetMoveInterface();
		forgetMoveInterface.Pokemon = pokemon;
		forgetMoveInterface.MoveToLearn = pokemonMove;

		if (!PokemonEvolution.Instance.IsQueueEmpty()) 
		{
			await ToSignal(PokemonTD.Signals, Signals.SignalName.EvolutionQueueCleared);
		}

		AddSibling(forgetMoveInterface);

		PokemonMoves.Instance.AddToQueue(forgetMoveInterface);
	}

	private void OnEvolutionStarted(Pokemon pokemon)
	{
		EvolutionInterface evolutionInterface = PokemonTD.PackedScenes.GetEvolutionInterface();
		evolutionInterface.Pokemon = pokemon;
		AddSibling(evolutionInterface);
		GetParentOrNull<Node>().MoveChild(evolutionInterface, GetParentOrNull<Node>().GetChildCount());

		PokemonEvolution.Instance.AddToQueue(evolutionInterface);
	}

	private void SetStageSlotsAlpha(bool isDragging)
	{
		foreach (StageSlot stageSlot in StageSlots)
		{
			stageSlot.SetOpacity(isDragging);
		}
	}

	private void OnPokemonEnemyEvent(PokemonEnemy pokemonEnemy)
	{
		PokemonEnemies.Remove(pokemonEnemy);
		RemovePathFollow(pokemonEnemy);
		
		bool isWaveFinished = PokemonEnemies.Count == 0;
		if (isWaveFinished) EmitSignal(SignalName.FinishedWave);
	}

	private async void WaveInterval()
	{
		float timeSeconds = 1f;
		while (CurrentWave < WaveCount)
		{
			if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, Signals.SignalName.PressedPlay);

			StartWave();

			await ToSignal(this, SignalName.FinishedWave);
			await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout); // Add delay in bewtween waves
		}

		if (RareCandy > 0) 
		{
			WonStage();
		}
		else
		{
			LostStage();
		}

		HasFinished = true;
	}

	private void WonStage()
	{
		string resultMessage = "You've won!";
		ShowResultInterface(resultMessage);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.HasWon);

		string wonMessage = $"You've Won!";
		PrintRich.PrintLine(TextColor.Yellow, wonMessage);
	}

	private void LostStage()
	{
		string resultMessage = "You've lost all your candy!";
		ShowResultInterface(resultMessage);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.HasLost);

		string lostMessage = $"You've Lost All Your Candy!";
		PrintRich.PrintLine(TextColor.Yellow, lostMessage);
	}

	private void StartWave()
	{
		CurrentWave++;
			
		string waveMessage = $"Starting Wave {CurrentWave} / {WaveCount}";
		PrintRich.PrintLine(TextColor.Yellow, waveMessage);
		PokemonTD.AddStageConsoleMessage(TextColor.Yellow, waveMessage);

		SpawnWave();
		EmitSignal(SignalName.StartedWave);
	}

	private async void SpawnWave()
	{		
		float timeSeconds = 3f;
		for (int i = 0; i < PokemonPerWave; i++)
		{
			SpawnPokemon();
			await ToSignal(GetTree().CreateTimer(timeSeconds / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout); // Delay spawning Pokemon
			
			if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, Signals.SignalName.PressedPlay);
		}
	}

	private void SpawnPokemon()
	{
		string randomPokemonName = GetRandomPokemonName();
		int randomLevel = GetRandomLevel();
		Pokemon randomPokemon = PokemonManager.Instance.GetPokemon(randomPokemonName, randomLevel);
		
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
		int minimumLevel = PokemonLevels[0];
		int maximumLevel = PokemonLevels[1];
		int randomLevel = RNG.RandiRange(minimumLevel, maximumLevel);

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
		RareCandy = Math.Clamp(RareCandy - Mathf.RoundToInt(pokemon.Attack * percentageAmount), 0, 100);

		CheckCandy();
	}

	private void CheckCandy()
	{
		if (RareCandy > 0) return;
		
		HasFinished = true;
		LostStage();
	}

	private void ShowResultInterface(string resultMessage)
	{
		StageResultInterface resultInterface = PokemonTD.PackedScenes.GetStageResultInterface();
		resultInterface.StageID = ID;
		resultInterface.Message.Text = resultMessage;
		AddChild(resultInterface);
	}

	private void RemovePathFollow(PokemonEnemy pokemonEnemy)
	{
		PathFollow2D pathFollow = pokemonEnemy.GetParentOrNull<PathFollow2D>();
		pathFollow.QueueFree();
	}

	private PathFollow2D GetPathFollow()
	{
		PathFollow2D pathFollow = new PathFollow2D()
		{
			Rotates = false,
			Loop = false,
			YSortEnabled = true
		};
		pathFollow.TreeExiting += () => _pathFollows.Remove(pathFollow);

		return pathFollow;
	}

	public StageSlot FindStageSlot(int teamSlotIndex)
	{
		return StageSlots.Find(stageSlot => stageSlot.TeamSlotIndex == teamSlotIndex);
	}
}
