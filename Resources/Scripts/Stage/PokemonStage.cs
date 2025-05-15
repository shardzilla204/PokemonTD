using Godot;
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
	private StagePath _stagePath;

	[Export]
	private Control _stageSlots;

	[Export]
	private Node2D _transparentLayers;

	[Export]
	private StageInterface _stageInterface;

	public int ID = 1;
	public int WaveCount = 10;
	public int PokemonPerWave = 5;
	public List<int> PokemonLevels = new List<int>();
	public List<string> PokemonNames = new List<string>(); // List to spawn from
	
	public int CurrentWave;
	public int RareCandy = 100;

	public bool HasStarted;
	public bool HasFinished;

	public StageInterface StageInterface;
	public List<StageSlot> StageSlots = new List<StageSlot>();
	public List<PokemonEnemy> PokemonEnemies = new List<PokemonEnemy>(); // List for enemy scenes

	public override void _EnterTree()
	{
		StageInterface = _stageInterface;

		PokemonTD.Signals.PressedPlay += CheckGameState;
		PokemonTD.Signals.ForgetMove += PokemonForgettingMove;
		PokemonTD.Signals.PokemonEnemyFainted += OnPokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyPassed += OnPokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyCaptured += OnPokemonEnemyEvent;
		PokemonTD.Signals.DraggingStageSlot += SetStageAlpha;
		PokemonTD.Signals.DraggingStageTeamSlot += SetStageAlpha;
		PokemonTD.Signals.DraggingPokeBall += SetStageAlpha;
		PokemonTD.Signals.PokemonEvolving += PokemonEvolving;

		foreach (Node child in _stageSlots.GetChildren())
		{
			if (child is StageSlot stageSlot) StageSlots.Add(stageSlot);
		}
	}

    public override void _ExitTree()
    {
		PokemonTD.Signals.PressedPlay -= CheckGameState;
		PokemonTD.Signals.ForgetMove -= PokemonForgettingMove;
		PokemonTD.Signals.PokemonEnemyFainted -= OnPokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyPassed -= OnPokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyCaptured -= OnPokemonEnemyEvent;
		PokemonTD.Signals.DraggingStageSlot -= SetStageAlpha;
		PokemonTD.Signals.DraggingStageTeamSlot -= SetStageAlpha;
		PokemonTD.Signals.DraggingPokeBall -= SetStageAlpha;
		PokemonTD.Signals.PokemonEvolving -= PokemonEvolving;
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

	private void CheckGameState()
	{
		if (!HasStarted) PokemonTD.IsGamePaused = true;
	}

	private async void PokemonForgettingMove(Pokemon pokemon, PokemonMove pokemonMove)
	{
		if (!PokemonEvolution.Instance.IsQueueEmpty()) await ToSignal(PokemonEvolution.Instance, PokemonEvolution.SignalName.QueueCleared);
		
		ForgetMoveInterface forgetMoveInterface = PokemonTD.PackedScenes.GetForgetMoveInterface(pokemon, pokemonMove);
		forgetMoveInterface.Finished += () =>
		{
			if (!PokemonMoves.Instance.IsQueueEmpty()) 
			{
				PokemonMoves.Instance.ShowNext(this);
			}
			else
			{
				if (PokemonEvolution.Instance.IsQueueEmpty()) PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPlay);
			}
			forgetMoveInterface.QueueFree();
		};
		PokemonMoves.Instance.AddToQueue(forgetMoveInterface, this);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPause);
	}

	private async void PokemonEvolving(Pokemon pokemon, int teamSlotIndex)
	{
		if (!PokemonMoves.Instance.IsQueueEmpty()) await ToSignal(PokemonMoves.Instance, PokemonMoves.SignalName.QueueCleared);

		EvolutionInterface evolutionInterface = PokemonTD.PackedScenes.GetEvolutionInterface(pokemon, teamSlotIndex);
		evolutionInterface.Finished += (pokemonEvolution) =>
		{
			if (!PokemonEvolution.Instance.IsQueueEmpty()) 
			{
				PokemonEvolution.Instance.ShowNext(this);
			}
			else
			{
				if (PokemonMoves.Instance.IsQueueEmpty()) PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPlay);
			}
			evolutionInterface.QueueFree();
		};
		PokemonEvolution.Instance.AddToQueue(evolutionInterface, this);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPause);
	}

	private void OnEvolutionQueueUpdated(EvolutionInterface evolutionInterface)
	{
		AddSibling(evolutionInterface);
	}

	private void SetStageAlpha(bool isDragging)
	{
		Color transparent = Colors.White;
		transparent.A = 0.65f;
		_transparentLayers.Modulate = isDragging ? transparent : Colors.White;

		foreach (StageSlot stageSlot in StageSlots)
		{
			stageSlot.SetOpacity(isDragging);
		}
	}

	private void OnPokemonEnemyEvent(PokemonEnemy pokemonEnemy)
	{
		PokemonEnemies.Remove(pokemonEnemy);
		_stagePath.RemovePathFollow(pokemonEnemy);
		
		bool isWaveFinished = PokemonEnemies.Count == 0;
		if (isWaveFinished) EmitSignal(SignalName.FinishedWave);
	}

	private async void WaveInterval()
	{
		float timeSeconds = 1f;
		await ToSignal(PokemonTD.Signals, Signals.SignalName.StageStarted);
		HasStarted = true;
		
		while (CurrentWave < WaveCount)
		{
			if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, Signals.SignalName.PressedPlay);

			StartWave();

			await ToSignal(this, SignalName.FinishedWave);
			await ToSignal(GetTree().CreateTimer(timeSeconds / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout); // Add delay in bewtween waves
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
		
		_stagePath.AddPathFollow(pokemonEnemy);

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
			_stagePath.RemovePathFollow(pokemonEnemy);
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

	public StageSlot GetRandomStageSlot()
	{
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		int randomIndex = RNG.RandiRange(0, StageSlots.Count - 1);
		return StageSlots[randomIndex];
	}

	public StageSlot FindStageSlot(int teamSlotIndex)
	{
		return StageSlots.Find(stageSlot => stageSlot.TeamSlotIndex == teamSlotIndex);
	}
}
