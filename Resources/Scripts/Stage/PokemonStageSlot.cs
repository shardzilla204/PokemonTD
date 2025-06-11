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
	public delegate void RetrievedEventHandler(PokemonStageSlot pokemonStageSlot);

	[Signal]
	public delegate void FaintedEventHandler(PokemonStageSlot pokemonStageSlot);

	[Export]
	private Area2D _area;

	[Export]
	private TextureRect _areaSprite;

	[Export]
	private TextureRect _pokemonSprite;

	[Export]
	private InteractComponent _interactComponent;

	[Export]
	private StatContainer _statContainer;

	[Export]
	private StatusConditionContainer _statusConditionContainer;

	[Export]
	private Timer _attackTimer;

	[Export]
	private AudioStreamPlayer _pokemonMovePlayer;

	public Pokemon Pokemon;
	private List<PokemonEnemy> _targetQueue = new List<PokemonEnemy>();

	public bool IsRecovering = false;

	public int PokemonTeamIndex = -1;

	private Control _dragPreview;
	private bool _isDragging;

	public override void _ExitTree()
	{
		PokemonTD.Signals.PokemonEnemyPassed -= UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEnemyCaptured -= UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEvolved -= PokemonUpdated;
		PokemonTD.Signals.PokemonTransformed -= PokemonUpdated;
		PokemonTD.Signals.PokemonMoveChanged -= PokemonMoveChanged;
		PokemonTD.Signals.SpeedToggled -=  SpeedToggled;
		PokemonTD.Signals.Dragging -= SetOpacity;

		if (Pokemon != null) Pokemon.ResetEffects(PokemonTeamIndex);
	}

	public override void _Ready()
	{
		PokemonTD.Signals.PokemonEnemyPassed += UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEnemyCaptured += UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEvolved += PokemonUpdated;
		PokemonTD.Signals.PokemonTransformed += PokemonUpdated;
		PokemonTD.Signals.PokemonMoveChanged += PokemonMoveChanged;
		PokemonTD.Signals.SpeedToggled += SpeedToggled;
		PokemonTD.Signals.Dragging += SetOpacity;

		_interactComponent.MouseEntered += () => SetAreaSpriteOpacity(false);
		_interactComponent.MouseExited += () => SetAreaSpriteOpacity(true);
		_interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) => Interacted(isLeftClick, isDoubleClick);

		_attackTimer.Timeout += Attack;

		UpdateSlot(null);
		SetAreaSpriteOpacity(true);
	}

	public override void _Process(double delta)
	{
		if (_dragPreview == null) return;

		PokemonTD.Tween.TweenSlotDragRotation(_dragPreview, _isDragging);
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest && Pokemon != null) Pokemon.Effects.ResetPokemon(Pokemon, PokemonTeamIndex);

		if (what != NotificationDragEnd || !_isDragging) return;

		_dragPreview = null;
		_isDragging = false;

		SetOpacity(false);
		SetAreaSpriteOpacity(true);

		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.Dragging, false);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (IsRecovering || Pokemon == null) return new Control();

		_dragPreview = GetDragPreview(Pokemon);
		SetDragPreview(_dragPreview);

		_isDragging = true;

		SetOpacity(true);
		SetAreaSpriteOpacity(true);

		EmitSignal(SignalName.Dragging);
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.Dragging, true);

		GC.Dictionary<string, Variant> dataDictionary = new GC.Dictionary<string, Variant>()
		{
			{ "Origin", this },
			{ "Pokemon", Pokemon },
			{ "PokemonTeamIndex", PokemonTeamIndex },
			{ "PokemonEffects", Pokemon.Effects }
		};
		return dataDictionary;
	}

	private Control GetDragPreview(Pokemon pokemon)
    {
        int minValue = 125;
        Vector2 minSize = new Vector2(minValue, minValue);
        TextureRect textureRect = new TextureRect()
        {
            CustomMinimumSize = minSize,
            Texture = pokemon.Sprite,
            TextureFilter = TextureFilterEnum.Nearest,
            Position = -new Vector2(minSize.X / 2, minSize.Y / 4),
            PivotOffset = new Vector2(minSize.X / 2, 0)
        };

        Control stageDragPreview = new Control();
        stageDragPreview.AddChild(textureRect);

        return stageDragPreview;
    }

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		return true;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		Node origin = dataDictionary["Origin"].As<Node>();
		Pokemon pokemon = dataDictionary["Pokemon"].As<Pokemon>();
		int pokemonTeamIndex = dataDictionary["PokemonTeamIndex"].As<int>();

		PokemonStageSlot previousSlot = GetPreviousSlot(pokemonTeamIndex);

		if (origin is PokemonTeamSlot)
		{
			// Check if the Pokemon is already on the stage
			PokemonStage pokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
			PokemonStageSlot pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);

			if (pokemonStageSlot != null && pokemonStageSlot.Pokemon != null) pokemonStageSlot.UpdateSlot(null);

			PokemonTeamIndex = pokemonTeamIndex;
		}
		else
		{
			SwapPokemon(previousSlot);
		}

		// Carry over effects from previous slot
		if (origin is PokemonStageSlot)
		{
			PokemonEffects pokemonEffects = dataDictionary["PokemonEffects"].As<PokemonEffects>();
			if (Pokemon == null) return;
			
			Pokemon.Effects = pokemonEffects;
		}
		
		ChangeAreaSize(pokemon);
		UpdateSlot(pokemon);

		PokemonTD.AudioManager.PlayPokemonCry(pokemon, true);
	}

	// Transfers over the status condition icons to the new slot
	public void RefreshStatusConditions(Pokemon pokemon)
	{
		ClearStatusConditions();
		if (pokemon == null) return;

		foreach (StatusCondition statusCondition in pokemon.StatusConditions)
		{
			AddStatusCondition(statusCondition);
			PokemonStatusCondition.Instance.ApplyStatusColor(this, statusCondition);
		}

		if (pokemon.StatusConditions.Count == 0)
		{
			PokemonStatusCondition.Instance.ApplyStatusColor(this, StatusCondition.None);
		}
	}

	public void RefreshStats(Pokemon pokemon)
	{
		ClearStats();
		if (pokemon == null) return;

		foreach (PokemonStat stat in pokemon.Debuffs)
		{
			AddStat(stat, false);
		}
	}

	// Automatically applies any stat increasing moves & mist if there are any
	private void ApplyIncreasingStats(Pokemon pokemon)
	{
		if (pokemon == null) return;

		bool hasAnyIncreasingStatChanges = PokemonStatMoves.Instance.HasAnyIncreasingStatChanges(pokemon.Moves);
		if (!hasAnyIncreasingStatChanges) return;

		PrintRich.Print(TextColor.Blue, "Before");
		PrintRich.PrintStats(TextColor.Blue, pokemon);
		foreach (PokemonMove pokemonMove in pokemon.Moves)
		{
			List<StatMove> statIncreasingMoves = PokemonStatMoves.Instance.FindIncreasingStatMoves(pokemonMove);
			foreach (StatMove statIncreasingMove in statIncreasingMoves)
			{
				AddStat(statIncreasingMove.PokemonStat, true);
			}
			PokemonStatMoves.Instance.IncreaseStats(pokemon, statIncreasingMoves);
		}
		PrintRich.Print(TextColor.Blue, "After");
		PrintRich.PrintStats(TextColor.Blue, pokemon);
	}
	
	private void Interacted(bool isLeftClick, bool isDoubleClick)
	{
		if (!isLeftClick || !isDoubleClick || Pokemon is null || IsRecovering) return;

		// Print message to console
		string offStageMessage = $"{Pokemon.Name} Is Off Stage";
		PrintRich.PrintLine(TextColor.Purple, offStageMessage);

		Pokemon.ResetEffects(PokemonTeamIndex);

		ClearStatusConditions();

		EmitSignal(SignalName.Retrieved, this);

		SetAreaSpriteOpacity(true);
		UpdateSlot(null);
	}

	private void SetAreaSpriteOpacity(bool isTransparent)
	{
		Color color = Colors.White;
		color.A = 0;
		if (Pokemon != null && !_isDragging && !isTransparent)
		{
			color.A = 0.4f;
		}

		_areaSprite.SelfModulate = color;
	}

	private void PokemonUpdated(Pokemon pokemon, int pokemonTeamIndex)
	{
		if (PokemonTeamIndex == pokemonTeamIndex) UpdateSlot(pokemon);
	}

	public void ChangeAreaSize(Pokemon pokemon)
	{
		if (pokemon == null) return;

		int baseRadius = 100;
		int radius = baseRadius + pokemon.Level;

		CircleShape2D circleShape = GetCircleShape();
		circleShape.Radius = radius;
		
		float increase = 0.02f;
		float scaleSize = radius * increase;
		_areaSprite.Scale = new Vector2(scaleSize, scaleSize);
	}

	private CircleShape2D GetCircleShape()
	{
		CollisionShape2D collisionShape = _area.GetChildOrNull<CollisionShape2D>(0);
		CircleShape2D circleShape = (CircleShape2D) collisionShape.Shape;
		return circleShape;
	}

	private void SpeedToggled(float speed)
	{
		SetWaitTime();
	}

	private void PokemonMoveChanged(int pokemonTeamIndex)
	{
		if (PokemonTeamIndex == pokemonTeamIndex) SetWaitTime();
	}

	private void SetWaitTime()
	{
		if (Pokemon == null)
		{
			_attackTimer.Stop();
			return;
		}

		_attackTimer.WaitTime = 100 / (Pokemon.Stats.Speed * PokemonTD.GameSpeed * 1.25f);
		_attackTimer.WaitTime *= Pokemon.Effects.HasQuickAttack ? 0.7f : 1;
		_attackTimer.Start();
	}

	private void UpdatePokemonQueue(PokemonEnemy pokemonEnemy)
	{
		pokemonEnemy.Fainted -= UpdatePokemonQueue;

		if (!_targetQueue.Contains(pokemonEnemy)) return;

		_targetQueue.Remove(pokemonEnemy);
	}

	private void AddExperience(PokemonEnemy pokemonEnemy)
	{
		if (Pokemon is null || Pokemon.Level >= PokemonTD.MaxPokemonLevel) return;

		int experience = pokemonEnemy.GetExperience();
		StageInterface stageInterface = GetStageInterface();
		PokemonTeamSlot pokemonTeamSlot = stageInterface.FindPokemonTeamSlot(PokemonTeamIndex);
		pokemonTeamSlot.AddExperience(experience);
	}

	private PokemonStageSlot GetPreviousSlot(int pokemonTeamIndex)
	{
		PokemonStage pokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
		PokemonStageSlot previousSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
		return previousSlot;
	}

	public void HealPokemon(int health)
	{
		if (Pokemon == null) return;
		
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonHealed, health, PokemonTeamIndex);
	}

	public void DamagePokemon(int damage)
	{
		if (Pokemon == null) return;

		Pokemon.IsEnraged();
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonDamaged, damage, PokemonTeamIndex);
	}

	public void SwapPokemon(PokemonStageSlot previousSlot)
	{
		Pokemon pokemon = previousSlot.Pokemon;
		int pokemonTeamIndex = previousSlot.PokemonTeamIndex;

		previousSlot.UpdateSlot(Pokemon);
		previousSlot.ChangeAreaSize(Pokemon);
		previousSlot.PokemonTeamIndex = PokemonTeamIndex;

		UpdateSlot(pokemon);
		ChangeAreaSize(pokemon);
		PokemonTeamIndex = pokemonTeamIndex;
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
		PokemonTeamIndex = pokemon != null ? PokemonTeamIndex : -1;
		Pokemon = pokemon != null ? pokemon : null;
		_pokemonSprite.Texture = pokemon != null ? pokemon.Sprite : null;

		if (pokemon == null)
		{
			IsRecovering = false;
		}
		else 
		{
			pokemon.ApplyEffects();
		}

		SetWaitTime();

		RefreshStats(pokemon);
		RefreshStatusConditions(pokemon);

		ApplyIncreasingStats(pokemon);
	}

	private void Attack()
	{
		if (_targetQueue.Count <= 0 || PokemonTD.IsGamePaused || IsRecovering || Pokemon == null || Pokemon.IsMoveSkipped()) return;

		PokemonMove pokemonMove = PokemonCombat.Instance.GetCombatMove(Pokemon, _targetQueue[0].Pokemon, Pokemon.Move, PokemonTeamIndex);
		
		PokemonEnemy pokemonEnemy = (PokemonEnemy) PokemonCombat.Instance.GetNextTarget(_targetQueue, pokemonMove);
		if (pokemonEnemy == null) return;

		pokemonEnemy.Fainted += UpdatePokemonQueue;

		AddContribution(pokemonEnemy);

		if (pokemonMove.Name == "Roar" || pokemonMove.Name == "Whirlwind")
		{
			PokemonMoveEffect.Instance.UniqueMoves.ApplyUniqueMove(this, pokemonEnemy, pokemonMove);
		}

		if (Pokemon.Effects.HasConversion) PokemonMoveEffect.Instance.UniqueMoves.Conversion(this, pokemonEnemy);

		bool hasPokemonMoveHit = PokemonCombat.Instance.HasPokemonMoveHit(Pokemon, pokemonMove, pokemonEnemy.Pokemon);
		if (!hasPokemonMoveHit) return;
		
		PokemonTD.AudioManager.PlayPokemonMove(_pokemonMovePlayer, pokemonMove.Name, Pokemon);

		PokemonTD.Tween.TweenAttack(_pokemonSprite, pokemonEnemy, _attackTimer);

		if (PokemonMoveEffect.Instance.ChargeMoves.IsChargeMove(pokemonMove))
		{
			PokemonMoveEffect.Instance.ChargeMoves.ApplyChargeMove(this);
			PokemonMoveEffect.Instance.ChargeMoves.HasUsedDig(this, pokemonMove);

			if (Pokemon.Effects.IsCharging) return;
		}

		if (Pokemon.Effects.HasHyperBeam) Pokemon.Effects.IsCharging = true;

		AttackPokemonEnemy(pokemonEnemy, pokemonMove);
		PokemonMoveEffect.Instance.ApplyMoveEffect(this, pokemonEnemy, pokemonMove);
	}

	public void AttackPokemonEnemy(PokemonEnemy pokemonEnemy, PokemonMove pokemonMove)
	{
		PokemonCombat.Instance.DealDamage(this, pokemonEnemy, pokemonMove);

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonEnemy, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(pokemonEnemy, statusCondition);

		PokemonStatMoves.Instance.DecreaseStats(pokemonEnemy, pokemonMove);
	}

	private void AddContribution(PokemonEnemy pokemonEnemy)
	{
		bool hasTeamSlotIndex = pokemonEnemy.SlotContributionCount.ContainsKey(PokemonTeamIndex);
		if (!hasTeamSlotIndex)
		{
			// If it hasn't contributed yet, add it to the dictionary
			pokemonEnemy.SlotContributionCount.Add(PokemonTeamIndex, 1);
		}
		else
		{
			// Add another contribution to the existing index
			int contributionValue = pokemonEnemy.SlotContributionCount[PokemonTeamIndex];
			pokemonEnemy.SlotContributionCount[PokemonTeamIndex] = ++contributionValue;
		}
	}

	public void AddToQueue(PokemonEnemy pokemonEnemy)
	{
		_targetQueue.Insert(_targetQueue.Count, pokemonEnemy);
	}

	public void RemoveFromQueue(PokemonEnemy pokemonEnemy)
	{
		_targetQueue.Remove(pokemonEnemy);
	}

	public void AddStat(PokemonStat stat, bool isIncreasing)
	{
		_statContainer.AddStat(stat, isIncreasing);
	}

	public void RemoveStat(PokemonStat stat)
	{
		_statContainer.RemoveStat(stat);
	}

	public void ClearStats()
	{
		_statContainer.ClearStats();
	}
	
	public void ApplyStatusColor(Color statusConditionColor)
	{
		_pokemonSprite.SelfModulate = statusConditionColor;
	}

	public void AddStatusCondition(StatusCondition statusCondition)
	{
		_statusConditionContainer.AddStatusCondition(statusCondition);
	}

    public void RemoveStatusCondition(StatusCondition statusCondition)
    {
        _statusConditionContainer.RemoveStatusCondition(statusCondition);
    }

	private void ClearStatusConditions()
	{
		_statusConditionContainer.ClearStatusConditions();
    }
	
}
