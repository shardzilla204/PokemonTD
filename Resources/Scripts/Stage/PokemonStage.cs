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
	private Control _pokemonStageSlots;

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
	public List<PokemonStageSlot> PokemonStageSlots = new List<PokemonStageSlot>();
	public List<PokemonEnemy> PokemonEnemies = new List<PokemonEnemy>(); // List for enemy scenes

	public override void _EnterTree()
	{
		StageInterface = _stageInterface;

		PokemonTD.Signals.PressedPlay += CheckGameState;
		PokemonTD.Signals.PokemonEnemyPassed += PokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyCaptured += PokemonEnemyEvent;
		PokemonTD.Signals.DraggingPokemonTeamSlot += DraggingTeamSlot;
		PokemonTD.Signals.DraggingPokemonStageSlot += DraggingStageSlot;
		PokemonTD.Signals.DraggingPokeBall += SetStageOpacity;

		foreach (Node child in _pokemonStageSlots.GetChildren())
		{
			if (child is PokemonStageSlot PokemonStageSlot) PokemonStageSlots.Add(PokemonStageSlot);
		}
	}

    public override void _ExitTree()
    {
		PokemonTD.Signals.PressedPlay -= CheckGameState;
		PokemonTD.Signals.PokemonEnemyPassed -= PokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyCaptured -= PokemonEnemyEvent;
		PokemonTD.Signals.DraggingPokemonTeamSlot -= DraggingTeamSlot;
		PokemonTD.Signals.DraggingPokemonStageSlot -= DraggingStageSlot;
		PokemonTD.Signals.DraggingPokeBall -= SetStageOpacity;
    }

   	public override void _Ready()
	{
		if (PokemonNames.Count == 0) PokemonTD.AreStagesEnabled = false;
		if (PokemonTD.AreStagesEnabled) WaveInterval();
	}

	private void CheckGameState()
	{
		if (!HasStarted) PokemonTD.IsGamePaused = true;
	}
	public void DraggingTeamSlot(PokemonTeamSlot pokemonTeamSlot, bool isDragging)
	{
		SetStageOpacity(isDragging);
	}

	public void DraggingStageSlot(PokemonStageSlot pokemonStageSlot, bool isDragging)
	{
		SetStageOpacity(isDragging);
	}

	private void SetStageOpacity(bool isDragging)
	{
		Color transparent = Colors.White;
		transparent.A = 0.65f;
		_transparentLayers.Modulate = isDragging ? transparent : Colors.White;

		foreach (PokemonStageSlot pokemonStageSlot in PokemonStageSlots)
		{
			pokemonStageSlot.SetOpacity(isDragging);
		}
	}

	private void PokemonEnemyEvent(PokemonEnemy pokemonEnemy)
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
		string resultMessage = "You've Won!";
		ShowResultInterface(resultMessage);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.HasWonStage);

		// Print message to console
		if (PrintRich.AreStageMessagesEnabled) PrintRich.PrintLine(TextColor.Yellow, resultMessage);
	}

	private void LostStage()
	{
		string resultMessage = "You've Lost All Your Candy!";
		ShowResultInterface(resultMessage);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.HasLostStage);

		// Print message to console
		if (PrintRich.AreStageMessagesEnabled) PrintRich.PrintLine(TextColor.Yellow, resultMessage);
	}

	private void StartWave()
	{
		CurrentWave++;
		
		SpawnWave();
		EmitSignal(SignalName.StartedWave);

		// Print message to console
		string waveMessage = $"Starting Wave {CurrentWave} / {WaveCount}";
		if (PrintRich.AreStageMessagesEnabled) PrintRich.PrintLine(TextColor.Yellow, waveMessage);
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
		pokemonEnemy.Fainted += PokemonEnemyEvent;
		PokemonEnemies.Add(pokemonEnemy);
		
		PathFollow2D pathFollow = _stagePath.GetPathFollow(false, false, false);
		pathFollow.TreeExiting += () =>
		{
			PokemonEnemies.Remove(pokemonEnemy);
			_stagePath.RemovePathFollow(pathFollow);
		};
		pathFollow.AddChild(pokemonEnemy);
		_stagePath.AddPathFollow(pathFollow);

		// Print message to console
		string spawnMessage = $"Spawning Level {randomPokemon.Level} {randomPokemon.Name}";
		if (PrintRich.AreStageMessagesEnabled) PrintRich.PrintLine(TextColor.Yellow, spawnMessage);
	}

	public void SpawnClone(PokemonEnemy pokemonEnemy, PokemonEnemy pokemonEnemyClone)
	{
		PathFollow2D pathFollow = pokemonEnemy.GetParentOrNull<PathFollow2D>();

		PathFollow2D pathFollowClone = _stagePath.GetPathFollow(false, false, true);
		pathFollowClone.AddChild(pokemonEnemyClone);
		pathFollowClone.ProgressRatio = pathFollow.ProgressRatio;

		_stagePath.AddPathFollow(pathFollowClone);
	}

	public PokemonEnemy GetPokemonClone(PokemonEnemy pokemonEnemy)
	{
		Pokemon pokemonClone = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name, pokemonEnemy.Pokemon.Level);
		pokemonClone.HP /= 3;
		
		PokemonEnemy pokemonEnemyClone = PokemonTD.PackedScenes.GetPokemonEnemy();
		pokemonEnemyClone.SetPokemon(pokemonClone);
		return pokemonEnemyClone;
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
		int MaxLevel = PokemonLevels[1];
		int randomLevel = RNG.RandiRange(minimumLevel, MaxLevel);

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

	public PokemonStageSlot GetRandomPokemonStageSlot()
	{
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		int randomIndex = RNG.RandiRange(0, PokemonStageSlots.Count - 1);
		return PokemonStageSlots[randomIndex];
	}

	public PokemonStageSlot FindPokemonStageSlot(int teamSlotIndex)
	{
		return PokemonStageSlots.Find(PokemonStageSlot => PokemonStageSlot.TeamSlotIndex == teamSlotIndex);
	}
}
