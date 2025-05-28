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
	private StatusConditionContainer _statusConditionContainer;

	[Export]
	private Timer _attackTimer;

	[Export]
	private AudioStreamPlayer _pokemonMovePlayer;

	public Pokemon Pokemon;
	private List<PokemonEnemy> _targetQueue = new List<PokemonEnemy>();

	public bool IsActive = true;

	public int PokemonTeamIndex = -1;
	public PokemonEffects Effects = new PokemonEffects();

	private Control _dragPreview;
	private bool _isDragging;

	public override void _ExitTree()
	{
		PokemonTD.Signals.PokemonEnemyPassed -= UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEnemyCaptured -= UpdatePokemonQueue;
		PokemonTD.Signals.Dragging -= SetOpacity;
		PokemonTD.Signals.PokemonEvolved -= PokemonEvolved;
		PokemonTD.Signals.SpeedToggled -= SpeedToggled;
		PokemonTD.Signals.PokemonTransformed -= PokemonUpdated;

		if (Pokemon != null && Effects.PokemonTransform != null)
		{
			if (Effects.PokemonTransform != null)
			{
				Effects.RevertTransformation(Pokemon);
				PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTransformed, Pokemon, PokemonTeamIndex);
			}
			Effects.RevertTypes(Pokemon);
		}

		Effects.Reset();
	}

	public override void _Ready()
	{
		PokemonTD.Signals.PokemonEnemyPassed += UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEnemyCaptured += UpdatePokemonQueue;
		PokemonTD.Signals.Dragging += SetOpacity;
		PokemonTD.Signals.PokemonEvolved += PokemonEvolved;
		PokemonTD.Signals.SpeedToggled += SpeedToggled;
		PokemonTD.Signals.PokemonTransformed += PokemonUpdated;

		Color color = Colors.White;
		_interactComponent.MouseEntered += () =>
		{
			if (Pokemon == null || _isDragging) return;
			color.A = 0.4f;
			_areaSprite.SelfModulate = color;
		};
		_interactComponent.MouseExited += () =>
		{
			if (Pokemon == null || _isDragging) return;
			color.A = 0;
			_areaSprite.SelfModulate = color;
		};
		color.A = 0;
		_areaSprite.SelfModulate = color;
		_interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) =>
		{
			if (!isDoubleClick || !isLeftClick || Pokemon is null || !IsActive) return;

			if (Effects.PokemonTransform != null)
			{
				Effects.RevertTransformation(Pokemon);
				PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTransformed, Pokemon, PokemonTeamIndex);
			}

			Effects.RevertTypes(Pokemon);
			Effects.Reset();
			ClearStatusConditions();

			// Print message to console
			string offStageMessage = $"{Pokemon.Name} Is Off Stage";
			PrintRich.PrintLine(TextColor.Purple, offStageMessage);

			EmitSignal(SignalName.Retrieved);
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonUsed, false, PokemonTeamIndex);
			UpdateSlot(null);

			Color color = Colors.White;
			color.A = 0;
			_areaSprite.SelfModulate = color; 	
		};
		_attackTimer.Timeout += AttackPokemonEnemy;

		UpdateSlot(null);
	}

	public override void _Process(double delta)
	{
		if (_dragPreview == null) return;

		PokemonTD.Tween.TweenSlotDragRotation(_dragPreview, _isDragging);
	}

	private void PokemonEvolved(Pokemon pokemonEvolution, int pokemonTeamIndex)
	{
		if (PokemonTeamIndex != pokemonTeamIndex) return;

		UpdateSlot(pokemonEvolution);
	}

	private void SpeedToggled(float speed)
	{
		SetWaitTime();
	}

	private void PokemonUpdated(Pokemon pokemon, int pokemonTeamIndex)
	{
		if (PokemonTeamIndex != pokemonTeamIndex) return;

		UpdateSlot(pokemon);
	}

	private void ChangeAreaSize()
	{
		int baseRadius = 100;

		CircleShape2D circleShape = GetCircleShape();
		circleShape.Radius = baseRadius + Pokemon.Level;
		
		float increase = 0.02f;
		float scaleSize = baseRadius * increase;
		_areaSprite.Scale = new Vector2(scaleSize, scaleSize);
	}

	private CircleShape2D GetCircleShape()
	{
		CollisionShape2D collisionShape = _area.GetChildOrNull<CollisionShape2D>(0);
		CircleShape2D circleShape = (CircleShape2D) collisionShape.Shape;
		return circleShape;
	}

	public void SetWaitTime()
	{
		if (Pokemon == null)
		{
			_attackTimer.Stop();
			return;
		}

		_attackTimer.WaitTime = 100 / (Pokemon.Stats.Speed * PokemonTD.GameSpeed * 1.25f);
		_attackTimer.WaitTime *= Effects.HasQuickAttack ? 0.75f : 1;
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
		PokemonTeamSlot pokemonTeamSlot = stageInterface.PokemonTeamSlots.FindPokemonTeamSlot(PokemonTeamIndex);
		pokemonTeamSlot.AddExperience(experience);
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
		PokemonTD.Signals.EmitSignal(Signals.SignalName.Dragging, false);

		Color color = Colors.White;
		color.A = 0;
		_areaSprite.SelfModulate = color; 	

		ClearStatusConditions();
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (!IsActive || Pokemon == null) return new Control();

		_isDragging = true;
		SetOpacity(true);
		EmitSignal(SignalName.Dragging);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.Dragging, true);

		_dragPreview = GetStageDragPreview(Pokemon);
		SetDragPreview(_dragPreview);

		GC.Dictionary<string, Variant> dataDictionary = new GC.Dictionary<string, Variant>()
		{
			{ "Origin", this },
			{ "Pokemon", Pokemon },
			{ "PokemonTeamIndex", PokemonTeamIndex },
			{ "PokemonEffects", Effects }
		};
		return dataDictionary;
	}

	private Control GetStageDragPreview(Pokemon pokemon)
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

		// Swap Pokemon if both slots are filled
		PokemonStageSlot givingSlot = GetGivingSlot(pokemonTeamIndex);
		if (Pokemon != null)
		{
			if (origin is PokemonTeamSlot)
			{
				PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonUsed, true, PokemonTeamIndex);
			}
			else
			{
				SwapPokemon(givingSlot);
				PokemonTD.AudioManager.PlayPokemonCry(givingSlot.Pokemon, true);
			}
		}
		else if (givingSlot != null)
		{
			givingSlot.UpdateSlot(null); // Empties previous slot
		}

		if (origin is PokemonStageSlot)
		{
			PokemonEffects pokemonEffects = dataDictionary["PokemonEffects"].As<PokemonEffects>();
			Effects = pokemonEffects;
		}

		PokemonTeamIndex = pokemonTeamIndex;
		UpdateSlot(pokemon);
		PokemonTD.AudioManager.PlayPokemonCry(pokemon, true);
		ChangeAreaSize();

		// Reapply status conditions
		ClearStatusConditions();
		foreach (StatusCondition statusCondition in pokemon.GetStatusConditions())
		{
			AddStatusCondition(statusCondition);
			PokemonStatusCondition.Instance.ApplyStatusColor(this, statusCondition);
		}

		if (origin is PokemonStageSlot) return;

		// Automatically applies any stat increasing moves & mist if there are any
		bool hasAnyIncreasingStatChanges = PokemonStatMoves.Instance.HasAnyIncreasingStatChanges(pokemon.Moves);
		if (!hasAnyIncreasingStatChanges) return;

		PrintRich.Print(TextColor.Blue, "Before");
		PrintRich.PrintStats(TextColor.Blue, pokemon);
		foreach (PokemonMove pokemonMove in pokemon.Moves)
		{
			List<StatMove> statIncreasingMoves = PokemonStatMoves.Instance.FindIncreasingStatMoves(pokemonMove);
			PokemonStatMoves.Instance.IncreaseStats(pokemon, statIncreasingMoves);
		}
		PrintRich.Print(TextColor.Blue, "After");
		PrintRich.PrintStats(TextColor.Blue, pokemon);
	}

	private PokemonStageSlot GetGivingSlot(int pokemonTeamIndex)
	{
		PokemonStage pokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
		return pokemonTeamIndex == -1 ? null : pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
	}

	public void HealPokemon(int health)
	{
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonHealed, health, PokemonTeamIndex);
	}

	public void DamagePokemon(int damage)
	{
		if (Effects.HasRage)
		{
			StatMove statIncreaseMove = PokemonStatMoves.Instance.FindIncreasingStatMove("Rage");
			PokemonStatMoves.Instance.ChangeStat(Pokemon, statIncreaseMove);

			string activatedRageMessage = $"{Pokemon.Name} Has Been Enraged";
			PrintRich.PrintLine(TextColor.Purple, activatedRageMessage);
		}
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonDamaged, damage, PokemonTeamIndex);
	}

	public void SwapPokemon(PokemonStageSlot givingSlot)
	{
		Pokemon givingSlotPokemon = givingSlot.Pokemon;
		int givingSlotID = givingSlot.PokemonTeamIndex;

		Pokemon recievingSlotPokemon = Pokemon;
		int recievingSlotID = PokemonTeamIndex;

		givingSlot.UpdateSlot(recievingSlotPokemon);
		givingSlot.PokemonTeamIndex = recievingSlotID;

		UpdateSlot(givingSlotPokemon);
		PokemonTeamIndex = givingSlotID;
	}

	private bool IsPokemonOnStage(int pokemonTeamIndex)
	{
		StageInterface stageInterface = GetStageInterface();
		return stageInterface.IsPokemonTeamSlotInUse(pokemonTeamIndex);
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

		if (pokemon == null) IsActive = true;
		if (pokemon == null)
		{
			Effects.Reset();
		}

		SetWaitTime();
	}

	private void AttackPokemonEnemy()
	{
		if (_targetQueue.Count <= 0 || PokemonTD.IsGamePaused || !IsActive || Pokemon == null) return;

		if (Effects.HasMoveSkipped)
		{
			Effects.HasMoveSkipped = false;

			string skippedMessage = $"{Pokemon.Name} Had It's Turn Skipped";
			PrintRich.PrintLine(TextColor.Yellow, skippedMessage);
			return;
		}

		PokemonMove pokemonMove = Pokemon.Move;
		if (pokemonMove.Name == "Mirror Move" || pokemonMove.Name == "Mimic")
		{
			pokemonMove = PokemonMoveEffect.Instance.CopyMoves.GetCopiedPokemonMove(Pokemon, _targetQueue[0].Pokemon);

			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonCopiedMove, pokemonMove, PokemonTeamIndex);
		}
		
		Effects.HasHyperBeam = pokemonMove.Name == "Hyper Beam";
		
		pokemonMove = pokemonMove.Name == "Metronome" ? PokemonMoves.Instance.GetRandomPokemonMove() : pokemonMove;

		PokemonEnemy pokemonEnemy = PokemonCombat.Instance.GetNextPokemonEnemy(_targetQueue, pokemonMove); ;
		pokemonEnemy.Fainted += UpdatePokemonQueue;

		bool hasPokemonMoveHit = PokemonCombat.Instance.HasPokemonMoveHit(Pokemon, pokemonMove, pokemonEnemy.Pokemon);
		if (!hasPokemonMoveHit) return;

		PokemonTD.Tween.TweenAttack(_pokemonSprite, pokemonEnemy, _attackTimer);
		AddContribution(pokemonEnemy);

		if (PokemonMoveEffect.Instance.ChargeMoves.IsChargeMove(pokemonMove))
		{
			PokemonMoveEffect.Instance.ChargeMoves.ApplyChargeMove(this);
			PokemonMoveEffect.Instance.ChargeMoves.HasUsedDig(this, pokemonMove);

			if (Effects.IsCharging) return;
		}

		PokemonCombat.Instance.DealDamage(this, pokemonEnemy, pokemonMove);
		PokemonMoveEffect.Instance.ApplyMoveEffect(this, pokemonEnemy, pokemonMove);

		PokemonStatMoves.Instance.DecreaseStats(pokemonEnemy, pokemonMove);

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonEnemy, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(this, pokemonEnemy, statusCondition);
	}

	// For Unique Moves
	public void AttackPokemonEnemy(PokemonMove pokemonMove)
	{
		PokemonEnemy pokemonEnemy = PokemonCombat.Instance.GetNextPokemonEnemy(_targetQueue, pokemonMove);
		PokemonCombat.Instance.DealDamage(this, pokemonEnemy, pokemonMove);

		StatusCondition statusCondition = PokemonStatusCondition.Instance.GetStatusCondition(pokemonEnemy, pokemonMove);
		PokemonStatusCondition.Instance.ApplyStatusCondition(this, pokemonEnemy, statusCondition);

		PokemonStatMoves.Instance.DecreaseStats(pokemonEnemy, pokemonMove);
	}

	public void AddContribution(PokemonEnemy pokemonEnemy)
	{
		bool hasTeamSlotIndex = pokemonEnemy.SlotContributionCount.ContainsKey(PokemonTeamIndex);
		if (!hasTeamSlotIndex)
		{
			pokemonEnemy.SlotContributionCount.Add(PokemonTeamIndex, 1);
		}
		else
		{
			int contributionValue = pokemonEnemy.SlotContributionCount[PokemonTeamIndex];
			pokemonEnemy.SlotContributionCount[PokemonTeamIndex] = ++contributionValue;
		}
	}

	private void TweenAttack(PokemonEnemy pokemonEnemy)
	{
		Vector2 direction = _pokemonSprite.GlobalPosition.DirectionTo(pokemonEnemy.GlobalPosition);
		_pokemonSprite.FlipH = direction.X > 0;

		Vector2 positionOne = _pokemonSprite.Position - (direction * 5); // Moves the sprite backwards
		Vector2 positionTwo = _pokemonSprite.Position + (direction * 15); // Moves the sprite forwards

		float speedMultiplier = 0.25f;
		Vector2 originalPosition = _pokemonSprite.Position;
		Tween tween = CreateTween().SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(_pokemonSprite, "position", positionOne, _attackTimer.WaitTime * speedMultiplier);
		tween.TweenProperty(_pokemonSprite, "position", positionTwo, _attackTimer.WaitTime * speedMultiplier);
		tween.TweenProperty(_pokemonSprite, "position", originalPosition, _attackTimer.WaitTime * speedMultiplier);
	}

	public void SetAlpha(bool isDragging)
	{
		Color color = Colors.White;
		color.A = isDragging ? 0.75f : 1;
		SelfModulate = color;
	}

	public void AddToQueue(PokemonEnemy pokemonEnemy)
	{
		_targetQueue.Insert(_targetQueue.Count, pokemonEnemy);
	}

	public void RemoveFromQueue(PokemonEnemy pokemonEnemy)
	{
		_targetQueue.Remove(pokemonEnemy);
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

    public void ClearStatusConditions()
    {
		_statusConditionContainer.ClearStatusConditions(); 
    }
}
