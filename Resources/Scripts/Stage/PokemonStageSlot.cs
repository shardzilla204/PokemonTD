using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonStageSlot : NinePatchRect
{
	[Signal]
	public delegate void AttackedEventHandler();
	
	[Signal]
	public delegate void FaintedEventHandler(PokemonStageSlot pokemonStageSlot);

	[Export]
	private Area2D _area;

	[Export]
	private TextureRect _sprite;

	[Export]
	private InteractComponent _interactComponent;

	[Export]
	private Container _statusConditions;

	[Export]
	private Timer _attackTimer;

	[Export]
	private AudioStreamPlayer _pokemonMovePlayer;

	public TextureRect Sprite => _sprite;
	public Pokemon Pokemon;

	public List<PokemonEnemy> PokemonEnemyQueue = new List<PokemonEnemy>();

	public bool IsActive = true;
	public bool HasMoveSkipped = false;

	private bool _isDragging;
	private bool _isMuted;

	public int TeamSlotIndex = -1;
	public int LightScreenCount;
	public int ReflectCount;
	public bool IsCharging = false;
	public bool UsedDig = false;
	public bool HasCounter = false;
	public bool UsedQuickAttack = false;

	private List<StatusConditionIcon> _statusConditionIcons = new List<StatusConditionIcon>();

	private Control _dragPreview;

	public override void _ExitTree()
	{
		PokemonTD.Signals.PokemonEnemyPassed -= UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEnemyCaptured -= UpdatePokemonQueue;
		PokemonTD.Signals.DraggingPokemonTeamSlot -= SetOpacity;
		PokemonTD.Signals.DraggingPokemonStageSlot -= SetOpacity;
		PokemonTD.Signals.PokemonEvolved -= PokemonEvolved;
		PokemonTD.Signals.SpeedToggled -= OnSpeedToggled;
		PokemonTD.Signals.PokemonTeamSlotMuted -= OnPokemonTeamSlotMuted;
	}

	public override void _Ready()
	{
		PokemonTD.Signals.PokemonEnemyPassed += UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEnemyCaptured += UpdatePokemonQueue;
		PokemonTD.Signals.DraggingPokemonTeamSlot += SetOpacity;
		PokemonTD.Signals.DraggingPokemonStageSlot += SetOpacity;
		PokemonTD.Signals.PokemonEvolved += PokemonEvolved;
		PokemonTD.Signals.SpeedToggled += OnSpeedToggled;
		PokemonTD.Signals.PokemonTeamSlotMuted += OnPokemonTeamSlotMuted;

		_interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) =>
		{
			if (!isDoubleClick || !isLeftClick || Pokemon is null) return;

			// Restore Pokemon Moves
			Pokemon pokemonData = PokemonManager.Instance.GetPokemon(Pokemon.Name, Pokemon.Level);
			Pokemon.Types.Clear();
			Pokemon.Types.AddRange(pokemonData.Types);

			// Print Message To Console
			string offStageMessage = $"{Pokemon.Name} Is Off Stage";
			PrintRich.PrintLine(TextColor.Purple, offStageMessage);

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

	private void OnPokemonTeamSlotMuted(int teamSlotIndex, bool isToggled)
	{
		if (TeamSlotIndex != teamSlotIndex) return;

		_isMuted = isToggled;
	}

	private void PokemonEvolved(Pokemon pokemonEvolution, int teamSlotIndex)
	{
		if (TeamSlotIndex != teamSlotIndex) return;

		UpdateSlot(pokemonEvolution);
	}

	private void OnSpeedToggled(float speed)
	{
		SetWaitTime();
	}

	public void SetWaitTime()
	{
		if (Pokemon is null)
		{
			_attackTimer.Stop();
			return;
		}

		_attackTimer.WaitTime = 100 / (Pokemon.Speed * PokemonTD.GameSpeed);
		_attackTimer.WaitTime *= UsedQuickAttack ? 1.65f : 1;
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
		if (what != NotificationDragEnd || !_isDragging) return;

		_dragPreview = null;
		_isDragging = false;
		HasCounter = false;

		SetOpacity(false);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingPokemonStageSlot, false);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (!IsActive || Pokemon == null) return new Control();

		_isDragging = true;
		SetOpacity(true);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingPokemonStageSlot, true);

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

		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();
		if (!fromTeamSlot) return;

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

		if (HasMoveSkipped)
		{
			HasMoveSkipped = false;
			return;
		}

		PokemonMove pokemonMove = GetPokemonMoveFromTeam();
		pokemonMove = pokemonMove.Name == "Metronome" ? PokemonMoves.Instance.GetRandomPokemonMove() : pokemonMove;
		if (!_isMuted) PokemonTD.AudioManager.PlayPokemonMove(_pokemonMovePlayer, pokemonMove.Name, Pokemon);

		PokemonEnemy pokemonEnemy = PokemonCombat.Instance.GetNextPokemonEnemy(PokemonEnemyQueue, pokemonMove);;
		pokemonEnemy.Fainted += UpdatePokemonQueue;	

		bool hasPokemonMoveHit = PokemonCombat.Instance.HasPokemonMoveHit(this, pokemonMove, pokemonEnemy);
		if (!hasPokemonMoveHit) return;

		// Print Message To Console
		string usedMessage = $"{Pokemon.Name} Used {pokemonMove.Name} On {pokemonEnemy.Pokemon.Name}";
		PrintRich.PrintLine(TextColor.Orange, usedMessage);

		TweenAttack(pokemonEnemy);
		AddContribution(pokemonEnemy);

		PokemonCombat.Instance.DealDamage(this, pokemonMove, pokemonEnemy);

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonEnemy, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(pokemonEnemy, statusCondition);

		PokemonMoveEffect.Instance.ApplyMoveEffect(this, pokemonMove, pokemonEnemy);
		PokemonStats.Instance.CheckStatChanges(pokemonEnemy, pokemonMove);
	}

	// For Unique Moves
	public void AttackPokemonEnemy(PokemonMove pokemonMove)
	{
		PokemonEnemy pokemonEnemy = PokemonCombat.Instance.GetNextPokemonEnemy(PokemonEnemyQueue, pokemonMove);
		PokemonCombat.Instance.DealDamage(this, pokemonMove, pokemonEnemy);

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonEnemy, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(pokemonEnemy, statusCondition);

		PokemonStats.Instance.CheckStatChanges(pokemonEnemy, pokemonMove);
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

	public void AddStatusConditionIcon(StatusCondition statusCondition)
	{
		bool hasStatusCondition = HasStatusCondition(statusCondition);
		if (hasStatusCondition) return;

		StatusConditionIcon statusConditionIcon = PokemonTD.PackedScenes.GetStatusConditionIcon(statusCondition);
		_statusConditions.AddChild(statusConditionIcon);
		_statusConditionIcons.Add(statusConditionIcon);
	}

	public void RemoveStatusConditionIcon(StatusCondition statusCondition)
	{
		StatusConditionIcon statusConditionIcon = _statusConditionIcons.Find(statusConditionIcon => statusConditionIcon.StatusCondition == statusCondition);
		_statusConditionIcons.Remove(statusConditionIcon);
		if (statusConditionIcon != null) statusConditionIcon.QueueFree();
	}

	public bool HasStatusCondition(StatusCondition statusCondition)
	{
		StatusConditionIcon statusConditionIcon = _statusConditionIcons.Find(statusConditionIcon => statusConditionIcon.StatusCondition == statusCondition);
		return statusConditionIcon != null;
	}
}
