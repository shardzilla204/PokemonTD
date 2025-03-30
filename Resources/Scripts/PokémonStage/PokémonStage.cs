using System.Collections.Generic;

using Godot;
using GC = Godot.Collections;

namespace PokémonTD;

public partial class PokémonStage : Node2D
{
	[Export]
	private Path2D _path;

	[Export]
	private Node2D _opacityGroup; // For tile map layers that will turn transparent when moving pokemon from team to slot

	[Export]
	private Control _stageSlots;

	[Export]
	private StageInterface _stageInterface;

	private GC.Array<PathFollow2D> _pathFollows = new GC.Array<PathFollow2D>();

	public int ID = 1;
	public int WaveCount = 10;
	public int PokémonPerWave = 5;
	public List<int> PokémonLevels = new List<int>();

	public List<string> PokémonNames = new List<string>(); // List to spawn from
	
	public int CurrentWave;
	public int RareCandy = 100;

	public bool HasWon;
	public bool HasLost;

	public StageInterface StageInterface;

	public List<StageSlot> StageSlots = new List<StageSlot>();

	private List<PokémonEnemy> _pokémonEnemies = new List<PokémonEnemy>(); // List for enemy scenes

	public override void _EnterTree()
	{
		StageInterface = _stageInterface;

		foreach (Node child in _stageSlots.GetChildren())
		{
			if (child is StageSlot stageSlot) StageSlots.Add(stageSlot);
		}

		PokémonTD.Signals.HasWon += () => HasWon = true;
		PokémonTD.Signals.HasLost += () => HasLost = true;
		PokémonTD.Signals.PokémonEnemyPassed += (pokémonEnemy) => IsWaveFinished();
	}

    public override void _ExitTree()
    {
        PokémonTD.Signals.HasWon -= () => HasWon = true;
    	PokémonTD.Signals.HasLost -= () => HasLost = true;
    	PokémonTD.Signals.PokémonEnemyPassed -= (pokémonEnemy) => IsWaveFinished();
    }

   	public override void _Ready()
	{
		if (PokémonNames.Count == 0)
		{
			PrintRich.PrintLine(TextColor.Red, "Found No Pokémon To Spawn!");
			PokémonTD.AreStagesEnabled = false;
		}
		
		if (PokémonTD.AreStagesEnabled) WaveInterval();
	}

	public override void _Process(double delta)
	{
		if (!PokémonTD.AreStagesEnabled || HasLost || HasWon || PokémonTD.IsGamePaused) return;

		foreach (PathFollow2D pathFollow in _pathFollows)
		{
			PokémonEnemy pokémonEnemy = pathFollow.GetChildOrNull<PokémonEnemy>(0);
			pathFollow.Progress += (float) delta * pokémonEnemy.Pokémon.Speed * PokémonTD.GameSpeed;
		}
	}

	private void OnPokémonEnemyEvent(PokémonEnemy pokémonEnemy)
	{
		_pokémonEnemies.Remove(pokémonEnemy);
		IsWaveFinished();
		RemovePathFollow(pokémonEnemy);
	}

	private async void WaveInterval()
	{
		float timeSeconds = 1f;
		while (CurrentWave < WaveCount)
		{
			StartWave();
			PokémonTD.Signals.EmitSignal(Signals.SignalName.StartedWave);

			string waveMessage = $"Starting Wave {CurrentWave} / {WaveCount}";
			PrintRich.PrintLine(TextColor.Yellow, waveMessage);

			await ToSignal(PokémonTD.Signals, Signals.SignalName.FinishedWave);
			await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout); // Add delay in bewtween waves
		}

		if (RareCandy <= 0) return;
		
		ShowWonInterface();
		PokémonTD.Signals.EmitSignal(Signals.SignalName.HasWon);

		string wonMessage = $"You've Won!";
		PrintRich.PrintLine(TextColor.Yellow, wonMessage);
	}

	private async void StartWave()
	{
		CurrentWave++;
		float timeSeconds = 3f;
		for (int i = 0; i < PokémonPerWave; i++)
		{
			SpawnPokémon();
			await ToSignal(GetTree().CreateTimer(timeSeconds / PokémonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout); // Delay spawning Pokémon
			
			if (PokémonTD.IsGamePaused) await ToSignal(PokémonTD.Signals, Signals.SignalName.PressedPlay);
		}
	}

	public void IsWaveFinished()
	{
		if (_pokémonEnemies.Count == 0) PokémonTD.Signals.EmitSignal(Signals.SignalName.FinishedWave);
	}

	private void SpawnPokémon()
	{
		Pokémon randomPokémon = PokémonTD.PokémonManager.GetPokémon(GetRandomPokémonName());
		
		PokémonEnemy pokémonEnemy = GetPokémonEnemy(randomPokémon);
		pokémonEnemy.Fainted += (pokémonEnemy) => 
		{
			OnPokémonEnemyEvent(pokémonEnemy);
			StageInterface.UpdatePokéDollars();
		};
		pokémonEnemy.Captured += OnPokémonEnemyEvent;

		RandomNumberGenerator RNG = new RandomNumberGenerator();
		int randomLevel = RNG.RandiRange(PokémonLevels[0], PokémonLevels[1]);
		pokémonEnemy.Pokémon.Level = randomLevel;

		_pokémonEnemies.Add(pokémonEnemy);

		AddPathFollow(pokémonEnemy);

		string spawnMessage = $"Spawning Level {randomPokémon.Level} {randomPokémon.Name}";
		PrintRich.PrintLine(TextColor.Yellow, spawnMessage);
	}

	private PokémonEnemy GetPokémonEnemy(Pokémon pokémon)
	{
		PokémonEnemy pokémonEnemy = PokémonTD.PackedScenes.GetPokémonEnemy();
		pokémonEnemy.SetPokémon(pokémon);
		pokémonEnemy.ScreenNotifier.ScreenExited += () => 
		{
			DecreaseRareCandy(pokémon);
			RemovePathFollow(pokémonEnemy);
			_pokémonEnemies.Remove(pokémonEnemy);
		};

		return pokémonEnemy;
	}

	// Get a random Pokémon based on the stage
	private string GetRandomPokémonName()
	{
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		int randomValue = RNG.RandiRange(0, PokémonNames.Count - 1);

		return PokémonNames[randomValue];
	}

	private void AddPathFollow(PokémonEnemy pokémonEnemy)
	{
		PathFollow2D pathFollow = GetPathFollow();
		pathFollow.AddChild(pokémonEnemy);

		_pathFollows.Add(pathFollow);
		_path.AddChild(pathFollow);
	}

	private void DecreaseRareCandy(Pokémon pokémon)
	{
		float percentageAmount = 0.05f; // 1%
		RareCandy -= Mathf.RoundToInt(pokémon.Attack * percentageAmount);

		HasLostAllCandy();
	}

	private void HasLostAllCandy()
	{
		if (RareCandy > 0) return;
		
		RareCandy = 0; // Resets from negative value

		ShowLostInterface();
		PokémonTD.Signals.EmitSignal(Signals.SignalName.HasLost);

		string lostMessage = $"You've Lost All Your Candy!";
		PrintRich.PrintLine(TextColor.Yellow, lostMessage);
	}

	private void ShowWonInterface()
	{
		StageStateInterface wonInterface = PokémonTD.PackedScenes.GetStageStateInterface();
		wonInterface.Message.Text = "You've won!";
		AddChild(wonInterface);
	}

	private void ShowLostInterface()
	{
		StageStateInterface lostInterface = PokémonTD.PackedScenes.GetStageStateInterface();
		lostInterface.Message.Text = "You've lost all your candy!";
		AddChild(lostInterface);
	}

	private void RemovePathFollow(PokémonEnemy pokémonEnemy)
	{
		PathFollow2D pathFollow = pokémonEnemy.GetParentOrNull<PathFollow2D>();
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
