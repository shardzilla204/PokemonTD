using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public partial class PokemonEnemy : TextureRect
{
	[Signal]
	public delegate void AttackedEventHandler();
	
	[Signal]
	public delegate void FaintedEventHandler(PokemonEnemy pokemonEnemy);

	[Export]
	private TextureProgressBar _healthBar;

	[Export]
	private StatusConditionContainer _statusConditionContainer;

	[Export]
	private VisibleOnScreenNotifier2D _screenNotifier;

	[Export]
	private Area2D _area;

	[Export]
	private Timer _attackTimer;

	public VisibleOnScreenNotifier2D ScreenNotifier => _screenNotifier;
	public StatusConditionContainer StatusConditions => _statusConditionContainer;

	public Pokemon Pokemon;
	public List<PokemonStageSlot> PokemonQueue = new List<PokemonStageSlot>();

	public bool IsCatchable = false;
	public GC.Dictionary<int, int> SlotContributionCount = new GC.Dictionary<int, int>();

	public bool IsMovingForward = true;
	public int TeamSlotIndex = -1;
	public PokemonEffects Effects = new PokemonEffects();

    public override void _ExitTree()
    {
		PokemonTD.Signals.DraggingPokemonStageSlot -= DraggingStageSlot;
    }
	
	public override void _Ready()
	{
		PokemonTD.Signals.DraggingPokemonStageSlot += DraggingStageSlot;
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
			if (!IsMovingForward)
			{
				PathFollow2D pathFollow = GetParentOrNull<PathFollow2D>();
				pathFollow.QueueFree();
				return;
			}

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

		foreach (PokemonMove pokemonMove in Pokemon.Moves)
		{
			List<StatMove> statIncreasingMoves = PokemonStats.Instance.FindIncreasingStatMoves(pokemonMove);
			PokemonStats.Instance.IncreaseStats(Pokemon, statIncreasingMoves);
		}

		Effects.HasCounter = Pokemon.Moves[0].Name == "Counter";
		Effects.UsedQuickAttack = Pokemon.Moves[0].Name == "Quick Attack";
		Effects.HasRage = Pokemon.Moves[0].Name == "Rage";

		_attackTimer.WaitTime *= Effects.UsedQuickAttack ? 1.65f : 1f;
		_attackTimer.Start();
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

	private void DraggingStageSlot(PokemonStageSlot pokemonStageSlot, bool isDragging)
	{
		if (!isDragging) PokemonQueue.Remove(pokemonStageSlot);
	}

	private void AddToQueue(Area2D area)
	{
		PokemonStageSlot pokemonStageSlot = area.GetParentOrNull<PokemonStageSlot>();
		pokemonStageSlot.PokemonEnemyQueue.Insert(pokemonStageSlot.PokemonEnemyQueue.Count, this);

		if (pokemonStageSlot.Pokemon == null || !pokemonStageSlot.IsActive) return;

		pokemonStageSlot.Fainted += PokemonStageSlotFainted;
		pokemonStageSlot.Dragging += () => PokemonQueue.Remove(pokemonStageSlot);
		PokemonQueue.Insert(PokemonQueue.Count, pokemonStageSlot);
	}

	private void PokemonStageSlotFainted(PokemonStageSlot pokemonStageSlot)
	{
		// Remove When Pokemon Faints
		if (pokemonStageSlot.Pokemon.HP > 0) return;

		PokemonQueue.Remove(pokemonStageSlot);
		pokemonStageSlot.Fainted -= PokemonStageSlotFainted;
	}

	private void RemoveFromQueue(Area2D area)
	{
		PokemonStageSlot pokemonStageSlot = area.GetParentOrNull<PokemonStageSlot>();
		pokemonStageSlot.PokemonEnemyQueue.Remove(this);
		PokemonQueue.Remove(pokemonStageSlot);
	}

	private void AttackPokemon()
	{
		if (PokemonQueue.Count <= 0 || PokemonTD.IsGamePaused) return;

		if (Effects.HasMoveSkipped)
		{
			Effects.HasMoveSkipped = false;

			string skippedMessage = $"{Pokemon.Name} Has It's Turn Skipped";
            PrintRich.PrintLine(TextColor.Red, skippedMessage);
			return;
		}

		PokemonMove pokemonMove = Pokemon.Moves[0];
		pokemonMove = pokemonMove.Name == "Metronome" ? PokemonMoves.Instance.GetRandomPokemonMove() : pokemonMove;

		PokemonStageSlot pokemonStageSlot = PokemonCombat.Instance.GetNextPokemonStageSlot(PokemonQueue, pokemonMove);
		if (pokemonStageSlot.Effects.HasSubstitute) return;

		if (Effects.IsCharging)
		{
			Effects.IsCharging = false;
			if (pokemonMove.Name != "Hyper Beam") PokemonCombat.Instance.DealDamage(this, pokemonMove, pokemonStageSlot);

			string dischargeMessage = $"{Pokemon.Name} Has Discharged";
			PrintRich.PrintLine(TextColor.Red, dischargeMessage);
		}

		bool hasPokemonMoveHit = PokemonCombat.Instance.HasPokemonMoveHit(this, pokemonMove, pokemonStageSlot);
		if (!hasPokemonMoveHit) return;

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonStageSlot, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(this, pokemonStageSlot, statusCondition);

		if (!Effects.IsCharging) PokemonMoveEffect.Instance.ApplyMoveEffect(this, pokemonMove, pokemonStageSlot);	
		PokemonStats.Instance.CheckStatChanges(pokemonStageSlot, pokemonMove);

		if (!Effects.IsCharging) PokemonCombat.Instance.DealDamage(this, pokemonMove, pokemonStageSlot);
	}

	// For Unique Moves
	public void AttackPokemon(PokemonMove pokemonMove)
	{
		PokemonStageSlot pokemonStageSlot = PokemonCombat.Instance.GetNextPokemonStageSlot(PokemonQueue, pokemonMove);
		PokemonCombat.Instance.DealDamage(this, pokemonMove, pokemonStageSlot);

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonStageSlot, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(this, pokemonStageSlot, statusCondition);

		PokemonStats.Instance.CheckStatChanges(pokemonStageSlot, pokemonMove);
	}

	public void SetPokemon(Pokemon pokemon)
	{
		Pokemon = pokemon;
		Texture = pokemon != null ? pokemon.Sprite : null;
	}

	public void HealPokemon(int health)
	{
		_healthBar.Value += health;
		CheckIsCatchable();
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
		if (_healthBar.Value > 0)
		{
			if (Effects.HasRage)
			{
				StatMove statIncreaseMove = PokemonStats.Instance.FindIncreasingStatMove("Rage");
				PokemonMoveEffect.Instance.ChangeStat(Pokemon, statIncreaseMove); 

				string activatedRageMessage = $"{Pokemon.Name} Has Been Enraged";
				PrintRich.PrintLine(TextColor.Red, activatedRageMessage);
			}
			return;
		}

		PokemonTD.AudioManager.PlayPokemonFaint();
		PokemonTD.AddPokeDollars(Pokemon);

		EmitSignal(SignalName.Fainted, this);

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
			GiveExperience(teamSlotIndex, experience);
		}
	}

	private void GiveExperience(int teamSlotIndex, int experience)
	{
		for (int i = 0; i < SlotContributionCount[teamSlotIndex]; i++)
		{
			PokemonStage pokemonStage = GetParentOrNull<PathFollow2D>().GetParentOrNull<Path2D>().GetParentOrNull<PokemonStage>();
			StageInterface stageInterface = pokemonStage.StageInterface;
			PokemonTeamSlot PokemonTeamSlot = stageInterface.PokemonTeamSlots.FindPokemonTeamSlot(teamSlotIndex);
			PokemonTeamSlot.AddExperience(experience);
		}
	}

	// ? EXP Base Formula
	// ! Will be modified if needed
	// EXP = b * L / 7
	// b = Pokemon Enemy Experience Yield
	// L = Pokemon Enemy Level
	
	// Divide and distribute to who contributed
	public int GetExperience()
	{
		List<int> teamSlotIndexes = SlotContributionCount.Keys.ToList();
		int totalContributions = GetTotalContributions(teamSlotIndexes);
		if (totalContributions == 0) return 0;

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
