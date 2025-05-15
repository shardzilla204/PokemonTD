using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public partial class PokemonEnemy : TextureRect
{
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
	public List<StageSlot> PokemonQueue = new List<StageSlot>();
	public GC.Dictionary<int, int> SlotContributionCount = new GC.Dictionary<int, int>();

	public int TeamSlotIndex = -1;
	public int LightScreenCount;
	public int ReflectCount;
	public bool IsMovingForward = true;

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
			
			string passedMessage = $"{Pokemon.Name} Has Breached The Defenses";
			PrintRich.PrintLine(TextColor.Yellow, passedMessage);

			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEnemyPassed, this);
			QueueFree();

			PokemonTD.AudioManager.PlayPokemonCry(Pokemon, true);
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
		string capturedMessage = $"{Pokemon.Name} Has Been Captured";
		PrintRich.PrintLine(TextColor.Yellow, capturedMessage);
		PokemonTD.AddStageConsoleMessage(TextColor.Yellow, capturedMessage);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEnemyCaptured, this);

		PokemonTD.AudioManager.PlayPokemonCry(Pokemon, true);
	}

    private void AddToQueue(Area2D area)
	{
		StageSlot stageSlot = area.GetParentOrNull<StageSlot>();
		stageSlot.PokemonEnemyQueue.Insert(stageSlot.PokemonEnemyQueue.Count, this);

		if (stageSlot.Pokemon == null || !stageSlot.IsActive) return;

		stageSlot.Fainted += OnStageSlotFainted;
		PokemonQueue.Insert(PokemonQueue.Count, stageSlot);
	}

	private void OnStageSlotFainted(StageSlot pokemonStageSlot)
	{
		// Remove when pokemon becomes downed
		if (pokemonStageSlot.Pokemon.HP <= 0) 
		{
			PokemonQueue.Remove(pokemonStageSlot);
			pokemonStageSlot.Fainted -= OnStageSlotFainted;
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
				string missedMessage = $"{Pokemon.Name}'s {pokemonMove.Name} Missed";
				PrintRich.PrintLine(TextColor.Purple, missedMessage);
			}
			return;
		}

		string usedMessage = $"{Pokemon.Name} Used {pokemonMove.Name} On {pokemonStageSlot.Pokemon.Name}";
		PrintRich.PrintLine(TextColor.Red, usedMessage);
		PokemonTD.AddStageConsoleMessage(TextColor.Red, usedMessage);

		PokemonCombat.Instance.ApplyStatusConditions(TeamSlotIndex, pokemonStageSlot, pokemonMove);
		PokemonMoveEffect.Instance.StatMoves.CheckStatChanges(pokemonStageSlot.Pokemon, pokemonMove);

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

		PokemonTD.AddPokeDollars(Pokemon);

		string faintMessage = $"{Pokemon.Name} Has Fainted";
		PrintRich.PrintLine(TextColor.Yellow, faintMessage);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEnemyFainted, this);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokeDollarsUpdated);
		CalculateExperienceDistribution();
		QueueFree();

		PokemonTD.AudioManager.PlayPokemonFaint();
	}

	// ! Fix
	private void CalculateExperienceDistribution()
	{
		int totalContributions = 0;
		List<int> teamSlotIndexes = SlotContributionCount.Keys.ToList();
		foreach (int teamSlotIndex in teamSlotIndexes)
		{
			totalContributions += SlotContributionCount[teamSlotIndex];
		}

		int experience = GetExperience();
		float experienceAmount = experience / totalContributions;

		foreach (int teamSlotIndex in teamSlotIndexes)
		{
			for (int i = 0; i < SlotContributionCount[teamSlotIndex]; i++)
			{
				PokemonStage pokemonStage = GetParentOrNull<PathFollow2D>().GetParentOrNull<Path2D>().GetParentOrNull<PokemonStage>();
				StageInterface stageInterface = pokemonStage.StageInterface;
				StageTeamSlot stageTeamSlot = stageInterface.StageTeamSlots.FindStageTeamSlot(teamSlotIndex);
				stageTeamSlot.AddExperience(Mathf.RoundToInt(experienceAmount));
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
		return Mathf.RoundToInt(Pokemon.Experience.Yield * Pokemon.Level / 3);
	}

	private void EnableCaptureMode()
	{
		IsCatchable = true;
		MouseFilter = MouseFilterEnum.Stop;
		
		_healthBar.Value = 1;
		_healthBar.MaxValue = 1;
		_healthBar.TintProgress = new Color(0.5f, 0, 0); // Red Color
	}
}
