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
	private Control _pokemonStageSlotContainer;

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
	private List<PokemonStageSlot> _pokemonStageSlots = new List<PokemonStageSlot>();
	private List<PokemonEnemy> _pokemonEnemies = new List<PokemonEnemy>(); // List for enemy scenes

	public override void _EnterTree()
	{
		StageInterface = _stageInterface;

		PokemonTD.Signals.PressedPlay += CheckGameState;
		PokemonTD.Signals.PokemonEnemyPassed += PokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyCaptured += PokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyExited += PokemonEnemyEvent;
		PokemonTD.Signals.Dragging += SetStageOpacity;

		foreach (Node child in _pokemonStageSlotContainer.GetChildren())
		{
			if (child is PokemonStageSlot pokemonStageSlot) _pokemonStageSlots.Add(pokemonStageSlot);
		}
	}

	public override void _ExitTree()
	{
		PokemonTD.Signals.PressedPlay -= CheckGameState;
		PokemonTD.Signals.PokemonEnemyPassed -= PokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyCaptured -= PokemonEnemyEvent;
		PokemonTD.Signals.PokemonEnemyExited -= PokemonEnemyEvent;
		PokemonTD.Signals.Dragging -= SetStageOpacity;
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

	private void SetStageOpacity(bool isDragging)
	{
		Color transparent = Colors.White;
		transparent.A = 0.65f;
		_transparentLayers.Modulate = isDragging ? transparent : Colors.White;
	}

	private void PokemonEnemyEvent(PokemonEnemy pokemonEnemy)
	{
		_pokemonEnemies.Remove(pokemonEnemy);
		_stagePath.RemovePathFollow(pokemonEnemy);

		bool isWaveFinished = _pokemonEnemies.Count == 0;
		if (isWaveFinished) EmitSignal(SignalName.FinishedWave);
	}

	private async void WaveInterval()
	{
		await ToSignal(PokemonTD.Signals, PokemonSignals.SignalName.StageStarted); // If the player changes settings before starting
		HasStarted = true;

		while (CurrentWave < WaveCount)
		{
			if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, PokemonSignals.SignalName.PressedPlay);

			StartWave();

			await ToSignal(this, SignalName.FinishedWave);
			await ToSignal(GetTree().CreateTimer(1 / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout); // Add delay in bewtween waves
		}

		ShowStageResult();
		HasFinished = true;
	}

	private void ShowStageResult()
	{
		string resultMessage = RareCandy > 0 ? "You've Won!" : "You've Lost All Your Candy!";
		StringName signalName = RareCandy > 0 ? PokemonSignals.SignalName.HasWonStage : PokemonSignals.SignalName.HasLostStage;

		ShowResultInterface(resultMessage);
		PokemonTD.Signals.EmitSignal(signalName);

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
		for (int i = 0; i < PokemonPerWave; i++)
		{
			SpawnPokemon();
			await ToSignal(GetTree().CreateTimer(3 / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout); // Delay spawning Pokemon

			if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, PokemonSignals.SignalName.PressedPlay);
		}
	}

	private void SpawnPokemon()
	{
		string randomPokemonName = GetRandomPokemonName();
		int randomLevel = GetRandomLevel();
		Pokemon randomPokemon = PokemonManager.Instance.GetPokemon(randomPokemonName, randomLevel);

		PokemonEnemy pokemonEnemy = GetPokemonEnemy(randomPokemon);
		_pokemonEnemies.Add(pokemonEnemy);

		PathFollow2D pathFollow = _stagePath.GetPathFollow(false, false, false);
		pathFollow.TreeExiting += () =>
		{
			_pokemonEnemies.Remove(pokemonEnemy);
			_stagePath.RemovePathFollow(pokemonEnemy);
		};
		pathFollow.AddChild(pokemonEnemy);
		_stagePath.AddPathFollow(pathFollow);

		pokemonEnemy.Fainted += PokemonEnemyEvent;

		// Print message to console
		string spawnMessage = $"Spawning Level {randomPokemon.Level} {randomPokemon.Name}";
		if (PrintRich.AreStageMessagesEnabled) PrintRich.PrintLine(TextColor.Yellow, spawnMessage);
	}

	private PokemonEnemy GetPokemonEnemy(Pokemon pokemon)
	{
		PokemonEnemy pokemonEnemy = PokemonTD.PackedScenes.GetPokemonEnemy();
		pokemonEnemy.SetPokemon(pokemon);
		pokemonEnemy.Passed += () =>
		{
			DecreaseRareCandy(pokemon);
			PokemonEnemyEvent(pokemonEnemy);
		};

		return pokemonEnemy;
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
		pokemonClone.Stats.HP /= 3;

		PokemonEnemy pokemonEnemyClone = PokemonTD.PackedScenes.GetPokemonEnemy();
		pokemonEnemyClone.SetPokemon(pokemonClone);
		return pokemonEnemyClone;
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
		RareCandy = Math.Clamp(RareCandy - Mathf.RoundToInt(pokemon.Stats.Attack * percentageAmount), 0, 100);
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.RareCandyUpdated);

		CheckCandy();
	}

	private void CheckCandy()
	{
		if (RareCandy > 0) return;

		ShowStageResult();
		HasFinished = true;
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
		int randomIndex = RNG.RandiRange(0, _pokemonStageSlots.Count - 1);
		return _pokemonStageSlots[randomIndex];
	}

	public PokemonStageSlot FindPokemonStageSlot(int pokemonTeamIndex)
	{
		return _pokemonStageSlots.Find(pokemonStageSlot => pokemonStageSlot.PokemonTeamIndex == pokemonTeamIndex);
	}

	public void AddMovesetInterface(Pokemon pokemon, int pokemonTeamIndex)
	{
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.ChangeMovesetPressed);

		MovesetInterface movesetInterface = GetMovesetInterface(pokemon, pokemonTeamIndex);
		AddChild(movesetInterface);
	}

	private MovesetInterface GetMovesetInterface(Pokemon pokemon, int pokemonTeamIndex)
	{
		MovesetInterface movesetInterface = PokemonTD.PackedScenes.GetMovesetInterface();
		movesetInterface.SetPokemon(pokemon, pokemonTeamIndex);
		movesetInterface.PokemonMoveChanged += PokemonMoveChanged;

		return movesetInterface;
	}

	private void PokemonMoveChanged(Pokemon pokemon, int pokemonTeamIndex, PokemonMove pokemonMove)
	{
		switch (pokemonMove.Name)
		{
			case "Counter":
				pokemon.Effects.HasCounter = true;
				break;
			case "Hyper Beam":
				pokemon.Effects.HasHyperBeam = true;
				break;
		}

		// Print message to console
		string changedPokemonMoveMessage = $"{pokemon.Name}'s Move Is Now {pokemonMove.Name}";
		PrintRich.PrintLine(TextColor.Purple, changedPokemonMoveMessage);

		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonMoveChanged);
	}
}
