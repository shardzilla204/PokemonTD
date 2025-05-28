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
	public GC.Dictionary<int, int> SlotContributionCount = new GC.Dictionary<int, int>();

	public bool IsMovingForward = true;
	public int PokemonTeamIndex = -1;
	public PokemonEffects Effects = new PokemonEffects();

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
			_healthBar.MaxValue = Pokemon.Stats.HP * healthIncrease;
			_healthBar.Value = Pokemon.Stats.HP * healthIncrease;
		}

		_screenNotifier.ScreenExited += () =>
		{
			PathFollow2D pathFollow = GetParentOrNull<PathFollow2D>();
			if (!IsMovingForward)
			{
				pathFollow.QueueFree();
				return;
			}

			PokemonTD.AudioManager.PlayPokemonCry(Pokemon, true);
			EmitSignal(SignalName.Passed);
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEnemyPassed, this);
			pathFollow.QueueFree();

			// Print message to console
			string passedMessage = $"{Pokemon.Name} Has Breached The Defenses";
			PrintRich.PrintLine(TextColor.Yellow, passedMessage);
		};

		_area.AreaEntered += AddToQueue;
		_area.AreaExited += RemoveFromQueue;
		_attackTimer.Timeout += AttackPokemon;

		foreach (PokemonMove pokemonMove in Pokemon.Moves)
		{
			List<StatMove> statIncreasingMoves = PokemonStatMoves.Instance.FindIncreasingStatMoves(pokemonMove);
			PokemonStatMoves.Instance.IncreaseStats(Pokemon, statIncreasingMoves);
		}

		Effects.HasCounter = Pokemon.Moves[0].Name == "Counter";
		Effects.HasQuickAttack = Pokemon.Moves[0].Name == "Quick Attack";
		Effects.HasRage = Pokemon.Moves[0].Name == "Rage";

		_attackTimer.WaitTime *= Effects.HasQuickAttack ? 1.65f : 1f;
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

		// Print message to console
		string capturedMessage = $"{Pokemon.Name} Has Been Captured";
		PrintRich.PrintLine(TextColor.Yellow, capturedMessage);
	}

	private void DraggingStageSlot(PokemonStageSlot pokemonStageSlot, bool isDragging)
	{
		if (!isDragging) _targetQueue.Remove(pokemonStageSlot);
	}

	private void AddToQueue(Area2D area)
	{
		PokemonStageSlot pokemonStageSlot = area.GetParentOrNull<PokemonStageSlot>();
		pokemonStageSlot.AddToQueue(this);

		if (pokemonStageSlot.Pokemon == null || !pokemonStageSlot.IsActive) return;

		pokemonStageSlot.Fainted += PokemonStageSlotFainted;
		pokemonStageSlot.Dragging += () => _targetQueue.Remove(pokemonStageSlot);
		_targetQueue.Insert(_targetQueue.Count, pokemonStageSlot);
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

	private void AttackPokemon()
	{
		if (_targetQueue.Count <= 0 || PokemonTD.IsGamePaused) return;

		if (Effects.HasMoveSkipped)
		{
			Effects.HasMoveSkipped = false;

			string skippedMessage = $"{Pokemon.Name} Had It's Turn Skipped";
			PrintRich.PrintLine(TextColor.Red, skippedMessage);
			return;
		}

		PokemonMove pokemonMove = Pokemon.Move;
		pokemonMove = pokemonMove.Name == "Metronome" ? PokemonMoves.Instance.GetRandomPokemonMove() : pokemonMove;

		PokemonStageSlot pokemonStageSlot = PokemonCombat.Instance.GetNextPokemonStageSlot(_targetQueue, pokemonMove);
		pokemonStageSlot.Retrieved += UpdatePokemonQueue;
		pokemonStageSlot.Fainted += UpdatePokemonQueue;

		bool hasPokemonMoveHit = PokemonCombat.Instance.HasPokemonMoveHit(Pokemon, pokemonMove, pokemonStageSlot.Pokemon);
		if (!hasPokemonMoveHit) return;
		
		if (pokemonStageSlot.Effects.HasSubstitute) return;

		if (PokemonMoveEffect.Instance.ChargeMoves.IsChargeMove(pokemonMove))
		{
			PokemonMoveEffect.Instance.ChargeMoves.ApplyChargeMove(this);
			PokemonMoveEffect.Instance.ChargeMoves.HasUsedDig(this, pokemonMove);

			if (Effects.IsCharging) return;
		}

		PokemonCombat.Instance.DealDamage(this, pokemonStageSlot, pokemonMove);
		PokemonMoveEffect.Instance.ApplyMoveEffect(this, pokemonStageSlot, pokemonMove);

		PokemonStatMoves.Instance.DecreaseStats(pokemonStageSlot, pokemonMove);

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonStageSlot, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(this, pokemonStageSlot, statusCondition);
	}

	private void UpdatePokemonQueue(PokemonStageSlot pokemonStageSlot)
	{
		pokemonStageSlot.Retrieved -= UpdatePokemonQueue;
		pokemonStageSlot.Fainted -= UpdatePokemonQueue;

		if (!_targetQueue.Contains(pokemonStageSlot)) return;

		_targetQueue.Remove(pokemonStageSlot);
	}

	// For Unique Moves
	public void AttackPokemon(PokemonMove pokemonMove)
	{
		PokemonStageSlot pokemonStageSlot = PokemonCombat.Instance.GetNextPokemonStageSlot(_targetQueue, pokemonMove);
		PokemonCombat.Instance.DealDamage(this, pokemonStageSlot, pokemonMove);

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonStageSlot, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(this, pokemonStageSlot, statusCondition);

		PokemonStatMoves.Instance.DecreaseStats(pokemonStageSlot, pokemonMove);
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
				StatMove statIncreaseMove = PokemonStatMoves.Instance.FindIncreasingStatMove("Rage");
				PokemonStatMoves.Instance.ChangeStat(Pokemon, statIncreaseMove);

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
			PokemonTeamSlot PokemonTeamSlot = stageInterface.PokemonTeamSlots.FindPokemonTeamSlot(pokemonTeamIndex);
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
		foreach (int pokemonTeamIndex in teamSlotIndexes)
		{
			totalContributions += SlotContributionCount[pokemonTeamIndex];
		}
		return totalContributions;
	}
	
	public void AddStatusCondition(StatusCondition statusCondition)
	{
		_statusConditionContainer.AddStatusCondition(statusCondition);
	}

    public void RemoveStatusCondition(StatusCondition statusCondition)
    {
        _statusConditionContainer.RemoveStatusCondition(statusCondition);
    }

    public void ClearStatusConditions()
    {
		_statusConditionContainer.ClearStatusConditions(); 
    }
}