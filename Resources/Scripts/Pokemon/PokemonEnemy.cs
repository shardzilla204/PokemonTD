using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public partial class PokemonEnemy : TextureRect
{
	[Signal]
	public delegate void FaintedEventHandler(PokemonEnemy pokemonEnemy);

	[Export]
	private TextureProgressBar _healthBar;

	[Export]
	private VisibleOnScreenNotifier2D _screenNotifier;

	[Export]
	private Area2D _area;

	[Export]
	private Timer _attackTimer;

	public VisibleOnScreenNotifier2D ScreenNotifier => _screenNotifier;
	public Pokemon Pokemon;
	public bool IsCatchable = false;
	public bool IsActive = false;
	public List<StageSlot> PokemonQueue = new List<StageSlot>();
	public GC.Dictionary<int, int> SlotContributionCount = new GC.Dictionary<int, int>();

	public int TeamSlotIndex = -1;
	public int LightScreenCount;
	public int ReflectCount;
	public bool IsMovingForward = true;
	public bool IsCharging = false;
	public bool UsedDig = false;

	public override void _Ready()
	{
		if (PokemonTD.AreLevelsRandomized) Pokemon.Level = PokemonTD.GetRandomLevel(PokemonTD.MinPokemonEnemyLevel, PokemonTD.MaxPokemonEnemyLevel);

		if (PokemonTD.IsCaptureModeEnabled)
		{
			EnableCaptureMode();
		}
		else
		{
			float healthIncrease = 1.5f;
			_healthBar.MaxValue = Pokemon.HP * healthIncrease;
			_healthBar.Value = Pokemon.HP * healthIncrease;
		}

		ScreenNotifier.ScreenExited += () =>
		{
			if (!IsMovingForward) return;

			PokemonTD.AudioManager.PlayPokemonCry(Pokemon, true);
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEnemyPassed, this);
			QueueFree();

			// Print Message To Console
			string passedMessage = $"{Pokemon.Name} Has Breached The Defenses";
			PrintRich.PrintLine(TextColor.Yellow, passedMessage);
		};

		_area.AreaEntered += AddToQueue;
		_area.AreaExited += RemoveFromQueue;
		_attackTimer.Timeout += AttackPokemon;
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		return IsCatchable;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		PokemonTD.AudioManager.PlayPokemonCry(Pokemon, true);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEnemyCaptured, this);

		// Print Message To Console
		string capturedMessage = $"{Pokemon.Name} Has Been Captured";
		PrintRich.PrintLine(TextColor.Yellow, capturedMessage);
	}

	private void AddToQueue(Area2D area)
	{
		StageSlot stageSlot = area.GetParentOrNull<StageSlot>();
		stageSlot.PokemonEnemyQueue.Insert(stageSlot.PokemonEnemyQueue.Count, this);

		if (stageSlot.Pokemon == null || !stageSlot.IsActive) return;

		stageSlot.Fainted += PokemonStageSlotFainted;
		PokemonQueue.Insert(PokemonQueue.Count, stageSlot);
	}

	private void PokemonStageSlotFainted(StageSlot pokemonStageSlot)
	{
		// Remove When Pokemon Faints
		if (pokemonStageSlot.Pokemon.HP <= 0)
		{
			PokemonQueue.Remove(pokemonStageSlot);
			pokemonStageSlot.Fainted -= PokemonStageSlotFainted;
		}
	}

	private void RemoveFromQueue(Area2D area)
	{
		StageSlot stageSlot = area.GetParentOrNull<StageSlot>();
		stageSlot.PokemonEnemyQueue.Remove(this);
		PokemonQueue.Remove(stageSlot);
	}

	private void AttackPokemon()
	{
		if (PokemonQueue.Count <= 0 || PokemonTD.IsGamePaused) return;

		StageSlot pokemonStageSlot = PokemonQueue[0];
		PokemonMove pokemonMove = Pokemon.Moves[0];

		bool hasPokemonMoveHit = PokemonManager.Instance.HasPokemonMoveHit(Pokemon, pokemonMove, pokemonStageSlot.Pokemon);
		if (!hasPokemonMoveHit)
		{
			if (pokemonMove.Accuracy != 0)
			{
				// Print Message To Console
				string missedMessage = $"{Pokemon.Name}'s {pokemonMove.Name} Missed";
				PrintRich.PrintLine(TextColor.Purple, missedMessage);
			}
			return;
		}
		
		PokemonCombat.Instance.ApplyStatusConditions(TeamSlotIndex, pokemonStageSlot, pokemonMove);
		PokemonMoveEffect.Instance.StatMoves.CheckStatChanges(pokemonStageSlot.Pokemon, pokemonMove);

		// Print Message To Console
		string usedMessage = $"{Pokemon.Name} Used {pokemonMove.Name} On {pokemonStageSlot.Pokemon.Name}";
		PrintRich.PrintLine(TextColor.Red, usedMessage);

		if (pokemonMove.Power != 0) PokemonCombat.Instance.ApplyDamage(this, Pokemon.Moves[0], pokemonStageSlot);
	}

	public void AttackPokemon(PokemonMove pokemonMove, StageSlot pokemonStageSlot)
	{
		PokemonCombat.Instance.ApplyStatusConditions(TeamSlotIndex, pokemonStageSlot, pokemonMove);
		PokemonMoveEffect.Instance.StatMoves.CheckStatChanges(pokemonStageSlot.Pokemon, pokemonMove);

		if (pokemonMove.Power != 0) PokemonCombat.Instance.ApplyDamage(this, Pokemon.Moves[0], pokemonStageSlot);
	}

	public void SetPokemon(Pokemon pokemon)
	{
		Pokemon = pokemon;
		Texture = pokemon != null ? pokemon.Sprite : null;
	}

	public void DamagePokemon(int damage)
	{
		_healthBar.Value -= damage;

		CheckIsCatchable();
		CheckHasFainted();
	}

	private void CheckIsCatchable()
	{
		float capturePercentThreshold = 0.25f;

		if (_healthBar.Value > _healthBar.MaxValue * capturePercentThreshold) return;

		IsCatchable = true;
		MouseFilter = MouseFilterEnum.Stop;
		_healthBar.TintProgress = new Color(0.5f, 0, 0); // Red Color

		string catchMessage = $"{Pokemon.Name} Is Now Catchable!";
		PrintRich.PrintLine(TextColor.Yellow, catchMessage);
	}

	private void CheckHasFainted()
	{
		if (_healthBar.Value > 0) return;

		PokemonTD.AudioManager.PlayPokemonFaint();

		PokemonTD.AddPokeDollars(Pokemon);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokeDollarsUpdated);
		EmitSignal(SignalName.Fainted, this);
		
		IsActive = false;
		
		CalculateExperienceDistribution();
		QueueFree();

		// Print Message To Console
		string faintMessage = $"{Pokemon.Name} Has Fainted";
		PrintRich.PrintLine(TextColor.Yellow, faintMessage);
	}

	private void CalculateExperienceDistribution()
	{
		int experience = GetExperience();

		List<int> teamSlotIndexes = SlotContributionCount.Keys.ToList();
		foreach (int teamSlotIndex in teamSlotIndexes)
		{
			for (int i = 0; i < SlotContributionCount[teamSlotIndex]; i++)
			{
				PokemonStage pokemonStage = GetParentOrNull<PathFollow2D>().GetParentOrNull<Path2D>().GetParentOrNull<PokemonStage>();
				StageInterface stageInterface = pokemonStage.StageInterface;
				StageTeamSlot stageTeamSlot = stageInterface.StageTeamSlots.FindStageTeamSlot(teamSlotIndex);
				stageTeamSlot.AddExperience(experience);
			}
		}
	}

	// ? EXP Base Formula
	// ! Will be modified if needed
	// EXP = b * L / 7
	// b = Pokemon Enemy Experience Yield
	// L = Pokemon Enemy Level
	public int GetExperience()
	{
		List<int> teamSlotIndexes = SlotContributionCount.Keys.ToList();
		int totalContributions = GetTotalContributions(teamSlotIndexes);
		return Mathf.RoundToInt(Pokemon.Experience.Yield * Pokemon.Level / 3 / totalContributions);
	}

	private void EnableCaptureMode()
	{
		IsCatchable = true;
		MouseFilter = MouseFilterEnum.Stop;

		_healthBar.Value = 1;
		_healthBar.MaxValue = 1;
		_healthBar.TintProgress = new Color(0.5f, 0, 0); // Red Color
	}

	private int GetTotalContributions(List<int> teamSlotIndexes)
	{
		int totalContributions = 0;
		foreach (int teamSlotIndex in teamSlotIndexes)
		{
			totalContributions += SlotContributionCount[teamSlotIndex];
		}
		return totalContributions;
	}
}
