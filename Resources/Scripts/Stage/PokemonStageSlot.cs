using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonStageSlot : NinePatchRect
{
	[Signal]
	public delegate void AttackedEventHandler();

	[Signal]
	public delegate void DraggingEventHandler();

	[Signal]
	public delegate void RetrievedEventHandler();

	[Signal]
	public delegate void FaintedEventHandler(PokemonStageSlot pokemonStageSlot);

	[Export]
	private Area2D _area;

	[Export]
	private TextureRect _sprite;

	[Export]
	private InteractComponent _interactComponent;

	[Export]
	private StatusConditionContainer _statusConditionContainer;

	[Export]
	private Timer _attackTimer;

	[Export]
	private AudioStreamPlayer _pokemonMovePlayer;

	public TextureRect Sprite => _sprite;
	public StatusConditionContainer StatusConditions => _statusConditionContainer;

	public Pokemon Pokemon;
	public List<PokemonEnemy> PokemonEnemyQueue = new List<PokemonEnemy>();

	public bool IsActive = true;

	private bool _isDragging;
	private bool _isMuted;

	public int TeamSlotIndex = -1;
	public PokemonEffects Effects = new PokemonEffects();

	private Control _dragPreview;

	public override void _ExitTree()
	{
		PokemonTD.Signals.PokemonEnemyPassed -= UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEnemyCaptured -= UpdatePokemonQueue;
		PokemonTD.Signals.DraggingPokemonTeamSlot -= DraggingTeamSlot;
		PokemonTD.Signals.DraggingPokemonStageSlot -= DraggingStageSlot;
		PokemonTD.Signals.PokemonEvolved -= PokemonEvolved;
		PokemonTD.Signals.SpeedToggled -= SpeedToggled;
		PokemonTD.Signals.PokemonTeamSlotMuted -= PokemonTeamSlotMuted;
		PokemonTD.Signals.PokemonUpdated -= PokemonUpdated;

		if (Pokemon != null && Effects.PokemonTransform != null)
		{
			Effects.RevertTransformation(Pokemon);
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonUpdated, Pokemon, TeamSlotIndex);
		}

		if (Pokemon != null) Effects.RevertTypes(Pokemon);

		Effects.Reset();
	}

	public override void _Ready()
	{
		PokemonTD.Signals.PokemonEnemyPassed += UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEnemyCaptured += UpdatePokemonQueue;
		PokemonTD.Signals.DraggingPokemonTeamSlot += DraggingTeamSlot;
		PokemonTD.Signals.DraggingPokemonStageSlot += DraggingStageSlot;
		PokemonTD.Signals.PokemonEvolved += PokemonEvolved;
		PokemonTD.Signals.SpeedToggled += SpeedToggled;
		PokemonTD.Signals.PokemonTeamSlotMuted += PokemonTeamSlotMuted;
		PokemonTD.Signals.PokemonUpdated += PokemonUpdated;

		_interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) =>
		{
			if (!isDoubleClick || !isLeftClick || Pokemon is null || !IsActive) return;

			if (Effects.PokemonTransform != null)
			{
				Effects.RevertTransformation(Pokemon);
				PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonUpdated, Pokemon, TeamSlotIndex);
			}

			Effects.RevertTypes(Pokemon);
			_statusConditionContainer.RemoveAllStatusConditions();

			// Print Message To Console
			string offStageMessage = $"{Pokemon.Name} Is Off Stage";
			PrintRich.PrintLine(TextColor.Purple, offStageMessage);

			EmitSignal(SignalName.Retrieved);
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonUsed, false, TeamSlotIndex);
			UpdateSlot(null);
		};
		_attackTimer.Timeout += AttackPokemonEnemy;

		UpdateSlot(null);
	}

	public override void _Process(double delta)
	{
		if (_dragPreview == null) return;

		PokemonTD.Tween.TweenSlotDragRotation(_dragPreview, _isDragging);
	}

	private void PokemonTeamSlotMuted(int teamSlotIndex, bool isToggled)
	{
		if (TeamSlotIndex != teamSlotIndex) return;

		_isMuted = isToggled;
	}

	private void PokemonEvolved(Pokemon pokemonEvolution, int teamSlotIndex)
	{
		if (TeamSlotIndex != teamSlotIndex) return;

		UpdateSlot(pokemonEvolution);
	}

	private void SpeedToggled(float speed)
	{
		SetWaitTime();
	}

	private void PokemonUpdated(Pokemon pokemon, int teamSlotIndex)
	{
		if (TeamSlotIndex != teamSlotIndex) return;

		UpdateSlot(pokemon);
	}

	public void SetWaitTime()
	{
		if (Pokemon is null)
		{
			_attackTimer.Stop();
			return;
		}

		_attackTimer.WaitTime = 100 / (Pokemon.Speed * PokemonTD.GameSpeed * 1.25f);
		_attackTimer.WaitTime *= Effects.UsedQuickAttack ? 0.75f : 1;
		_attackTimer.Start();
	}

	private void UpdatePokemonQueue(PokemonEnemy pokemonEnemy)
	{
		pokemonEnemy.Fainted -= UpdatePokemonQueue;

		if (!PokemonEnemyQueue.Contains(pokemonEnemy)) return;

		PokemonEnemyQueue.Remove(pokemonEnemy);
	}

	private void AddExperience(PokemonEnemy pokemonEnemy)
	{
		if (Pokemon is null || Pokemon.Level >= PokemonTD.MaxPokemonLevel) return;

		StageInterface stageInterface = GetStageInterface();
		PokemonTeamSlot PokemonTeamSlot = stageInterface.PokemonTeamSlots.FindPokemonTeamSlot(TeamSlotIndex);
		int experience = pokemonEnemy.GetExperience();
		PokemonTeamSlot.AddExperience(experience);
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest && Pokemon != null)
		{
			if (Effects.PokemonTransform != null) Effects.RevertTransformation(Pokemon);
			Effects.RevertTypes(Pokemon);
		}

		if (what != NotificationDragEnd || !_isDragging) return;

		_dragPreview = null;

		_isDragging = false;
		SetOpacity(false);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingPokemonStageSlot, this, false);

		StatusConditions.RemoveAllStatusConditions();
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (!IsActive || Pokemon == null) return new Control();

		_isDragging = true;
		SetOpacity(true);
		EmitSignal(SignalName.Dragging);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingPokemonStageSlot, this, true);

		_dragPreview = PokemonTD.GetStageDragPreview(Pokemon);
		SetDragPreview(_dragPreview);
		return PokemonTD.GetStageDragData(Pokemon, TeamSlotIndex, false, _isMuted);
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		if (dataDictionary.Count == 0) return false;

		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();
		if (fromTeamSlot) return Pokemon == null;

		return true;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();

		if (dataDictionary is null) return;

		_isMuted = dataDictionary["IsMuted"].As<bool>();

		Pokemon pokemon = dataDictionary["Pokemon"].As<Pokemon>();
		int teamSlotIndex = dataDictionary["TeamSlotIndex"].As<int>();

		// Swap Pokemon if both slots are filled
		PokemonStageSlot givingSlot = GetGivingSlot(teamSlotIndex);
		if (Pokemon != null)
		{
			SwapPokemon(givingSlot);
			PokemonTD.AudioManager.PlayPokemonCry(pokemon, true);
			PokemonTD.AudioManager.PlayPokemonCry(givingSlot.Pokemon, true);
		}
		else if (givingSlot != null)
		{
			givingSlot.UpdateSlot(null); // Empties previous slot
		}

		TeamSlotIndex = teamSlotIndex;
		UpdateSlot(pokemon);
		PokemonTD.AudioManager.PlayPokemonCry(pokemon, true);

		StatusConditions.RemoveAllStatusConditions();
		foreach (StatusCondition statusCondition in pokemon.GetStatusConditions())
		{
			StatusConditions.AddStatusCondition(statusCondition);
			PokemonStatusCondition.Instance.ApplyStatusColor(this, statusCondition);
		}

		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();
		if (!fromTeamSlot) return;

		bool hasAnyIncreasingStatChanges = PokemonStats.Instance.HasAnyIncreasingStatChanges(pokemon.Moves);
		if (!hasAnyIncreasingStatChanges) return;

		// Automatically applies any stat increasing moves & mist
		PrintRich.Print(TextColor.Blue, "Before");
		PrintRich.PrintStats(TextColor.Blue, pokemon);
		foreach (PokemonMove pokemonMove in pokemon.Moves)
		{
			List<StatMove> statIncreasingMoves = PokemonStats.Instance.FindIncreasingStatMoves(pokemonMove);
			PokemonStats.Instance.IncreaseStats(pokemon, statIncreasingMoves);
		}
		PrintRich.Print(TextColor.Blue, "After");
		PrintRich.PrintStats(TextColor.Blue, pokemon);
	}

	private PokemonStageSlot GetGivingSlot(int teamSlotIndex)
	{
		PokemonStage pokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
		return teamSlotIndex == -1 ? null : pokemonStage.FindPokemonStageSlot(teamSlotIndex);
	}

	public void HealPokemon(int health)
	{
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonHealed, health, TeamSlotIndex);
	}

	public void DamagePokemon(int damage)
	{
		if (Effects.HasRage)
		{
			StatMove statIncreaseMove = PokemonStats.Instance.FindIncreasingStatMove("Rage");
			PokemonStats.Instance.ChangeStat(Pokemon, statIncreaseMove);

			string activatedRageMessage = $"{Pokemon.Name} Has Been Enraged";
			PrintRich.PrintLine(TextColor.Purple, activatedRageMessage);
		}
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonDamaged, damage, TeamSlotIndex);
	}

	public void SwapPokemon(PokemonStageSlot givingSlot)
	{
		Pokemon givingSlotPokemon = givingSlot.Pokemon;
		int givingSlotID = givingSlot.TeamSlotIndex;

		Pokemon recievingSlotPokemon = Pokemon;
		int recievingSlotID = TeamSlotIndex;

		givingSlot.UpdateSlot(recievingSlotPokemon);
		givingSlot.TeamSlotIndex = recievingSlotID;

		UpdateSlot(givingSlotPokemon);
		TeamSlotIndex = givingSlotID;
	}

	private bool IsPokemonOnStage(int teamSlotIndex)
	{
		StageInterface stageInterface = GetStageInterface();
		return stageInterface.IsPokemonTeamSlotInUse(teamSlotIndex);
	}

	private StageInterface GetStageInterface()
	{
		PokemonStage pokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
		return pokemonStage.StageInterface;
	}

	private void DraggingTeamSlot(PokemonTeamSlot pokemonTeamSlot, bool isDragging)
	{
		SetOpacity(isDragging);
	}

	private void DraggingStageSlot(PokemonStageSlot pokemonStageSlot, bool isDragging)
	{
		SetOpacity(isDragging);
	}

	public void SetOpacity(bool isDragging)
	{
		Color color = Colors.White;
		color.A = isDragging ? 0.4f : 0;
		SelfModulate = color;
	}

	public void UpdateSlot(Pokemon pokemon)
	{
		TeamSlotIndex = pokemon != null ? TeamSlotIndex : -1;
		Pokemon = pokemon != null ? pokemon : null;
		Sprite.Texture = pokemon != null ? pokemon.Sprite : null;

		if (pokemon == null) IsActive = true;
		if (pokemon == null) _isMuted = false;

		SetWaitTime();
	}

	private void AttackPokemonEnemy()
	{
		if (PokemonEnemyQueue.Count <= 0 || PokemonTD.IsGamePaused || !IsActive) return;

		if (Effects.HasMoveSkipped)
		{
			Effects.HasMoveSkipped = false;

			string skippedMessage = $"{Pokemon.Name} Has It's Turn Skipped";
            PrintRich.PrintLine(TextColor.Yellow, skippedMessage);
			return;
		}

		PokemonMove pokemonMove = GetPokemonMoveFromTeam();
		pokemonMove = pokemonMove.Name == "Metronome" ? PokemonMoves.Instance.GetRandomPokemonMove() : pokemonMove;
		if (!_isMuted) PokemonTD.AudioManager.PlayPokemonMove(_pokemonMovePlayer, pokemonMove.Name, Pokemon);

		PokemonEnemy pokemonEnemy = PokemonCombat.Instance.GetNextPokemonEnemy(PokemonEnemyQueue, pokemonMove); ;
		pokemonEnemy.Fainted += UpdatePokemonQueue;

		if (Effects.IsCharging)
		{
			Effects.IsCharging = false;
			if (pokemonMove.Name != "Hyper Beam") PokemonCombat.Instance.DealDamage(this, pokemonMove, pokemonEnemy);

			string dischargeMessage = $"{Pokemon.Name} Has Discharged";
			PrintRich.PrintLine(TextColor.Purple, dischargeMessage);
		}

		bool hasPokemonMoveHit = PokemonCombat.Instance.HasPokemonMoveHit(this, pokemonMove, pokemonEnemy);
		if (!hasPokemonMoveHit) return;

		TweenAttack(pokemonEnemy);
		AddContribution(pokemonEnemy);

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonEnemy, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(this, pokemonEnemy, statusCondition);

		if (!Effects.IsCharging) PokemonMoveEffect.Instance.ApplyMoveEffect(this, pokemonMove, pokemonEnemy);
		PokemonStats.Instance.DecreaseStats(pokemonEnemy, pokemonMove);

		if (!Effects.IsCharging) PokemonCombat.Instance.DealDamage(this, pokemonMove, pokemonEnemy);
	}

	// For Unique Moves
	public void AttackPokemonEnemy(PokemonMove pokemonMove)
	{
		PokemonEnemy pokemonEnemy = PokemonCombat.Instance.GetNextPokemonEnemy(PokemonEnemyQueue, pokemonMove);
		PokemonCombat.Instance.DealDamage(this, pokemonMove, pokemonEnemy);

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonEnemy, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(this, pokemonEnemy, statusCondition);

		PokemonStats.Instance.DecreaseStats(pokemonEnemy, pokemonMove);
	}

	public void AddContribution(PokemonEnemy pokemonEnemy)
	{
		bool hasTeamSlotIndex = pokemonEnemy.SlotContributionCount.ContainsKey(TeamSlotIndex);
		if (!hasTeamSlotIndex)
		{
			pokemonEnemy.SlotContributionCount.Add(TeamSlotIndex, 1);
		}
		else
		{
			int contributionValue = pokemonEnemy.SlotContributionCount[TeamSlotIndex];
			pokemonEnemy.SlotContributionCount[TeamSlotIndex] = ++contributionValue;
		}
	}

	private void TweenAttack(PokemonEnemy pokemonEnemy)
	{
		Vector2 direction = Sprite.GlobalPosition.DirectionTo(pokemonEnemy.GlobalPosition);
		Sprite.FlipH = direction.X > 0;

		Vector2 positionOne = Sprite.Position - (direction * 5); // Moves the sprite backwards
		Vector2 positionTwo = Sprite.Position + (direction * 15); // Moves the sprite forwards

		float speedMultiplier = 0.25f;
		Vector2 originalPosition = Sprite.Position;
		Tween tween = CreateTween().SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(Sprite, "position", positionOne, _attackTimer.WaitTime * speedMultiplier);
		tween.TweenProperty(Sprite, "position", positionTwo, _attackTimer.WaitTime * speedMultiplier);
		tween.TweenProperty(Sprite, "position", originalPosition, _attackTimer.WaitTime * speedMultiplier);
	}

	private PokemonMove GetPokemonMoveFromTeam()
	{
		StageInterface stageInterface = GetStageInterface();
		PokemonTeamSlot PokemonTeamSlot = stageInterface.PokemonTeamSlots.FindPokemonTeamSlot(TeamSlotIndex);
		return PokemonTeamSlot.Pokemon.Move;
	}

	public void SetAlpha(bool isDragging)
	{
		Color color = Colors.White;
		color.A = isDragging ? 0.75f : 1;
		SelfModulate = color;
	}
}
