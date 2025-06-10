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

	[Signal]
	public delegate void PassedEventHandler();

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

	public Pokemon Pokemon;
	private List<PokemonStageSlot> _targetQueue = new List<PokemonStageSlot>();

	public bool IsCatchable = false;
	public bool HasRewards = true;
	public GC.Dictionary<int, int> SlotContributionCount = new GC.Dictionary<int, int>();

    public override void _ExitTree()
    {
        PokemonTD.Signals.SpeedToggled -= SpeedToggled;
    }

	public override void _Ready()
	{
		PokemonTD.Signals.SpeedToggled += SpeedToggled;

		if (PokemonTD.AreLevelsRandomized) Pokemon.Level = PokemonTD.GetRandomLevel(PokemonTD.MinPokemonEnemyLevel, PokemonTD.MaxPokemonEnemyLevel);

		if (PokemonTD.IsCaptureModeEnabled)
		{
			EnableCaptureMode();
		}
		else
		{
			float healthIncrease = 1.5f;
			int increasedHealth = Mathf.RoundToInt(Pokemon.Stats.HP * healthIncrease);
			_healthBar.MaxValue = increasedHealth;
			_healthBar.Value = increasedHealth;
		}

		_screenNotifier.ScreenExited += ScreenExited;

		_area.AreaEntered += AddToQueue;
		_area.AreaExited += RemoveFromQueue;

		_attackTimer.Timeout += Attack;

		foreach (PokemonMove pokemonMove in Pokemon.Moves)
		{
			List<StatMove> statIncreasingMoves = PokemonStatMoves.Instance.FindIncreasingStatMoves(pokemonMove);
			PokemonStatMoves.Instance.IncreaseStats(Pokemon, statIncreasingMoves);
		}

		SetWaitTime();
		Pokemon.ApplyEffects();
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		return IsCatchable;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		PokemonTD.AudioManager.PlayPokemonCry(Pokemon, true);
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonEnemyCaptured, this);

		// Print message to console
		string capturedMessage = $"{Pokemon.Name} Has Been Captured";
		PrintRich.PrintLine(TextColor.Yellow, capturedMessage);
	}

	private void DraggingStageSlot(PokemonStageSlot pokemonStageSlot, bool isDragging)
	{
		if (!isDragging) _targetQueue.Remove(pokemonStageSlot);
	}

	private void SpeedToggled(float speed)
	{
		SetWaitTime();
	}

	private void SetWaitTime()
	{
		_attackTimer.WaitTime = 100 / (Pokemon.Stats.Speed * PokemonTD.GameSpeed * 1.25f);
		_attackTimer.WaitTime *= Pokemon.Effects.HasQuickAttack ? 0.7f : 1;
		_attackTimer.Start();
	}

	private void AddToQueue(Area2D area)
	{
		PokemonStageSlot pokemonStageSlot = area.GetParentOrNull<PokemonStageSlot>();
		pokemonStageSlot.AddToQueue(this);

		if (pokemonStageSlot.Pokemon == null || pokemonStageSlot.IsRecovering) return;

		pokemonStageSlot.Fainted += PokemonStageSlotFainted;
		pokemonStageSlot.Dragging += () => _targetQueue.Remove(pokemonStageSlot);
		_targetQueue.Insert(_targetQueue.Count, pokemonStageSlot);
	}

	private void ScreenExited()
	{
		PathFollow2D pathFollow = GetParentOrNull<PathFollow2D>();
		if (Pokemon.Stats.Speed < 0)
		{
			if (Pokemon.HasStatusCondition(StatusCondition.Confuse))
			{
				string confusedMessage = $"{Pokemon.Name} Was Confused And Ran Away";
				PrintRich.PrintLine(TextColor.Yellow, confusedMessage);
			}
			else
			{
				string scaredMessage = $"{Pokemon.Name} Was Scared And Ran Away";
				PrintRich.PrintLine(TextColor.Yellow, scaredMessage);
			}
			PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonEnemyExited, this);
			pathFollow.QueueFree();
			return;
		}

		PokemonTD.AudioManager.PlayPokemonCry(Pokemon, true);
		EmitSignal(SignalName.Passed);
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonEnemyPassed, this);
		pathFollow.QueueFree();

		string passedMessage = $"{Pokemon.Name} Has Breached The Defenses";
		PrintRich.PrintLine(TextColor.Yellow, passedMessage);
	}

	private void PokemonStageSlotFainted(PokemonStageSlot pokemonStageSlot)
	{
		// Remove When Pokemon Faints
		if (pokemonStageSlot.Pokemon.Stats.HP > 0) return;

		_targetQueue.Remove(pokemonStageSlot);
		pokemonStageSlot.Fainted -= PokemonStageSlotFainted;
	}

	private void RemoveFromQueue(Area2D area)
	{
		PokemonStageSlot pokemonStageSlot = area.GetParentOrNull<PokemonStageSlot>();
		pokemonStageSlot.RemoveFromQueue(this);
		_targetQueue.Remove(pokemonStageSlot);
	}

	private void Attack()
	{
		if (_targetQueue.Count <= 0 || PokemonTD.IsGamePaused || Pokemon.IsMoveSkipped()) return;

		PokemonMove pokemonMove = PokemonCombat.Instance.GetCombatMove(Pokemon, _targetQueue[0].Pokemon, Pokemon.Move, -1);

		PokemonStageSlot pokemonStageSlot = (PokemonStageSlot) PokemonCombat.Instance.GetNextTarget(_targetQueue, pokemonMove);
		pokemonStageSlot.Retrieved += UpdatePokemonQueue;
		pokemonStageSlot.Fainted += UpdatePokemonQueue;

		if (pokemonMove.Name == "Roar" || pokemonMove.Name == "Whirlwind")
		{
			PokemonMoveEffect.Instance.UniqueMoves.ApplyUniqueMove(this, pokemonStageSlot, pokemonMove);
		}

		if (Pokemon.Effects.HasConversion) PokemonMoveEffect.Instance.UniqueMoves.Conversion(this, pokemonStageSlot);

		bool hasPokemonMoveHit = PokemonCombat.Instance.HasPokemonMoveHit(Pokemon, pokemonMove, pokemonStageSlot.Pokemon);
		if (!hasPokemonMoveHit) return;
		
		if (pokemonStageSlot.Pokemon.Effects.HasSubstitute) return;

		if (PokemonMoveEffect.Instance.ChargeMoves.IsChargeMove(pokemonMove))
		{
			PokemonMoveEffect.Instance.ChargeMoves.ApplyChargeMove(this);
			PokemonMoveEffect.Instance.ChargeMoves.HasUsedDig(this, pokemonMove);

			if (Pokemon.Effects.IsCharging) return;
		}

		if (Pokemon.Effects.HasHyperBeam) Pokemon.Effects.IsCharging = true;

		AttackPokemon(pokemonStageSlot, pokemonMove);
		PokemonMoveEffect.Instance.ApplyMoveEffect(this, pokemonStageSlot, pokemonMove);
	}

	// For Unique Moves
	public void AttackPokemon(PokemonStageSlot pokemonStageSlot, PokemonMove pokemonMove)
	{
		PokemonCombat.Instance.DealDamage(this, pokemonStageSlot, pokemonMove);

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonStageSlot, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(pokemonStageSlot, statusCondition);

		PokemonStatMoves.Instance.DecreaseStats(pokemonStageSlot, pokemonMove);
	}

	private void UpdatePokemonQueue(PokemonStageSlot pokemonStageSlot)
	{
		pokemonStageSlot.Retrieved -= UpdatePokemonQueue;
		pokemonStageSlot.Fainted -= UpdatePokemonQueue;

		if (!_targetQueue.Contains(pokemonStageSlot)) return;

		_targetQueue.Remove(pokemonStageSlot);
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
		if (Pokemon == null) return;

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
			Pokemon.IsEnraged();
			return;
		}

		PokemonTD.AudioManager.PlayPokemonFaint();
		if (HasRewards)
		{
			PokemonTD.AddPokeDollars(Pokemon);
			CalculateExperienceDistribution();
		}

		EmitSignal(SignalName.Fainted, this);
		QueueFree();

		// Print message to console
		string faintMessage = $"{Pokemon.Name} Has Fainted";
		PrintRich.PrintLine(TextColor.Yellow, faintMessage);
	}

	private void CalculateExperienceDistribution()
	{
		int experience = GetExperience();

		List<int> teamSlotIndexes = SlotContributionCount.Keys.ToList();
		foreach (int pokemonTeamIndex in teamSlotIndexes)
		{
			GiveExperience(pokemonTeamIndex, experience);
		}
	}

	private void GiveExperience(int pokemonTeamIndex, int experience)
	{
		for (int i = 0; i < SlotContributionCount[pokemonTeamIndex]; i++)
		{
			PokemonStage pokemonStage = GetParentOrNull<PathFollow2D>().GetParentOrNull<Path2D>().GetParentOrNull<PokemonStage>();
			StageInterface stageInterface = pokemonStage.StageInterface;
			PokemonTeamSlot pokemonTeamSlot = stageInterface.FindPokemonTeamSlot(pokemonTeamIndex);
			if (pokemonTeamSlot == null) continue;
			pokemonTeamSlot.AddExperience(experience);
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
		foreach (int pokemonTeamIndex in teamSlotIndexes)
		{
			totalContributions += SlotContributionCount[pokemonTeamIndex];
		}
		return totalContributions;
	}

	public void AddStatusCondition(StatusCondition statusCondition)
	{
		_statusConditionContainer.AddStatusCondition(statusCondition);
		Pokemon.AddStatusCondition(statusCondition);
	}

	public void RemoveStatusCondition(StatusCondition statusCondition)
	{
		_statusConditionContainer.RemoveStatusCondition(statusCondition);
		Pokemon.RemoveStatusCondition(statusCondition);
    }

	public void ClearStatusConditions()
	{
		_statusConditionContainer.ClearStatusConditions();
		Pokemon.ClearStatusConditions();
    }
}