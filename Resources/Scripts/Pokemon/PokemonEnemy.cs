using Godot;
using GC = Godot.Collections;
using System;
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
	public bool CanMove = true;
	public int Speed;
	public List<StageSlot> PokemonQueue = new List<StageSlot>();
	public GC.Dictionary<int, int> SlotContributionCount = new GC.Dictionary<int, int>();

	public int TeamSlotIndex = -1;

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
			_healthBar.Value = Pokemon.HP * healthIncrease;
			_healthBar.MaxValue = Pokemon.HP * healthIncrease;
		}

		ScreenNotifier.ScreenExited += () =>
		{
			string passedMessage = $"{Pokemon.Name} Has Breached The Defenses";
			PrintRich.PrintLine(TextColor.Yellow, passedMessage);

			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEnemyPassed, this);
			QueueFree();

			PokemonTD.AudioManager.PlayPokemonCry(Pokemon, true);
		};

		_area.AreaEntered += AddToQueue;
		_area.AreaExited += RemoveFromQueue;
		_attackTimer.Timeout += AttackPokemon;

		Speed = Pokemon.Speed;
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

		stageSlot.ActivityChanged += OnStageSlotActivityChanged;
		PokemonQueue.Insert(PokemonQueue.Count, stageSlot);
	}

	private void OnStageSlotActivityChanged(StageSlot stageSlot, bool isActive)
	{
		// Remove when pokemon becomes downed
		if (stageSlot.Pokemon.HP <= 0) 
		{
			PokemonQueue.Remove(stageSlot);
			stageSlot.ActivityChanged -= OnStageSlotActivityChanged;
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

		if (!PokemonManager.Instance.HasPokemonMoveHit(Pokemon, pokemonMove, pokemonStageSlot.Pokemon))
		{
			PokemonManager.Instance.PokemonMoveMissed(Pokemon, pokemonMove);
			return;
		}

		string usedMessage = $"{Pokemon.Name} Used {pokemonMove.Name} On {pokemonStageSlot.Pokemon.Name}";
		PrintRich.PrintLine(TextColor.Red, usedMessage);
		PokemonTD.AddStageConsoleMessage(TextColor.Red, usedMessage);

		PokemonCombat.Instance.ApplyStatusConditions(TeamSlotIndex, pokemonStageSlot, pokemonMove);
		PokemonCombat.Instance.ApplyStatChanges(pokemonStageSlot.Pokemon, pokemonMove);

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

	public void DamagePokemon(int damage, int teamSlotIndex)
	{
		TeamSlotIndex = teamSlotIndex;
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
		CalculateExperienceDistribution();
		QueueFree();

		PokemonTD.AudioManager.PlayPokemonFaint();
	}

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
				StageInterface stageInterface = GetParentOrNull<Node>().GetOwnerOrNull<StageInterface>();
				StageTeamSlot stageTeamSlot = stageInterface.FindStageTeamSlot(teamSlotIndex);
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
		return Mathf.RoundToInt(Pokemon.ExperienceYield * Pokemon.Level / 3);
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
