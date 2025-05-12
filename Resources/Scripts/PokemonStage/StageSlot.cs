using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;

namespace PokemonTD;

public partial class StageSlot : NinePatchRect
{
	[Signal]
	public delegate void ActivityChangedEventHandler(StageSlot stageSlot, bool isActive);

	[Export]
	private Area2D _area;

	[Export]
	private TextureRect _sprite;

	[Export]
	private InteractComponent _interactComponent;

	[Export]
	private Timer _attackTimer;

	[Export]
	private AudioStreamPlayer _pokemonMovePlayer;

	public TextureRect Sprite => _sprite;
	public Pokemon Pokemon;

	public List<PokemonEnemy> PokemonEnemyQueue = new List<PokemonEnemy>();

	public bool IsActive = true;

	private bool _isDragging;
	private bool _isFilled;
	private bool _isMuted;

	public int TeamSlotIndex = -1;

	private Control _dragPreview;

    public override void _EnterTree()
    {
		PokemonTD.Signals.PokemonEnemyFainted += UpdatePokemonQueue;
        PokemonTD.Signals.PokemonEnemyPassed += UpdatePokemonQueue;
        PokemonTD.Signals.PokemonEnemyCaptured += UpdatePokemonQueue;
		PokemonTD.Signals.DraggingStageTeamSlot += SetOpacity;
		PokemonTD.Signals.DraggingStageSlot += SetOpacity;
		PokemonTD.Signals.EvolutionFinished += OnEvolutionFinished;
		PokemonTD.Signals.SpeedToggled += OnSpeedToggled;
		PokemonTD.Signals.StageTeamSlotMuted += OnStageTeamSlotMuted;
    }

	public override void _ExitTree()
    {
        PokemonTD.Signals.PokemonEnemyFainted -= UpdatePokemonQueue;
        PokemonTD.Signals.PokemonEnemyPassed -= UpdatePokemonQueue;
        PokemonTD.Signals.PokemonEnemyCaptured -= UpdatePokemonQueue;
		PokemonTD.Signals.DraggingStageTeamSlot -= SetOpacity;
		PokemonTD.Signals.DraggingStageSlot -= SetOpacity;
		PokemonTD.Signals.EvolutionFinished -= OnEvolutionFinished;
		PokemonTD.Signals.SpeedToggled -= OnSpeedToggled;
		PokemonTD.Signals.StageTeamSlotMuted -= OnStageTeamSlotMuted;
    }

	public override void _Ready()
	{
		_interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) => 
		{
			if (!isDoubleClick || !isLeftClick || Pokemon is null) return;

			string offStageMessage = $"{Pokemon.Name} Is Off Stage";
			PrintRich.PrintLine(TextColor.Purple, offStageMessage);

			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonOffStage, TeamSlotIndex);
			UpdateSlot(null);
		};
		_attackTimer.Timeout += AttackPokemonEnemy;

		UpdateSlot(null);
	}

	public override void _Process(double delta)
	{
		if (_dragPreview is null) return;

		Vector2 initialPosition = _dragPreview.GlobalPosition;
		Vector2 finalPosition = GetGlobalMousePosition();
		PokemonTD.Tween.TweenSlotDragRotation(_dragPreview, initialPosition, finalPosition, _isDragging);
	}

	private void OnStageTeamSlotMuted(int teamSlotIndex, bool isToggled)
	{
		if (TeamSlotIndex != teamSlotIndex) return;

		_isMuted = isToggled;
	}

	private void OnEvolutionFinished(Pokemon pokemonEvolution, int teamSlotIndex)
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
		_attackTimer.Start();
	}

	private void UpdatePokemonQueue(PokemonEnemy pokemonEnemy)
	{
		if (!PokemonEnemyQueue.Contains(pokemonEnemy)) return;

		PokemonEnemyQueue.Remove(pokemonEnemy);
	}

	private void AddExperience(PokemonEnemy pokemonEnemy)
	{
		if (Pokemon is null) return;

		if (Pokemon.Level >= PokemonTD.MaxPokemonLevel) return;
		
		PokemonStage pokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
		StageTeamSlot stageTeamSlot = pokemonStage.StageInterface.FindStageTeamSlot(TeamSlotIndex);
		int experience = pokemonEnemy.GetExperience();
		stageTeamSlot.AddExperience(experience);
	}

   	public override void _Notification(int what)
	{		
		if (what != NotificationDragEnd || !_isDragging) return;

		_dragPreview = null;
		_isDragging = false;

		SetOpacity(false);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingStageSlot, false);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (!IsActive) return new Control();

		_isDragging = true;
		SetOpacity(true);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingStageSlot, true);

		_dragPreview = GetDragPreview();
		SetDragPreview(_dragPreview);
		return GetDragData();
	}

	private GC.Dictionary<string, Variant> GetDragData()
	{
		GC.Dictionary<string, Variant> data = new GC.Dictionary<string, Variant>()
		{
			{ "TeamSlotIndex", TeamSlotIndex },
			{ "FromTeamSlot", false },
			{ "IsMuted", _isMuted },
			{ "Pokemon", Pokemon }
		};
		return data;
	}

	private Control GetDragPreview()
	{
		int minValue = 85;
		Vector2 minSize = new Vector2(minValue, minValue);
		TextureRect textureRect = new TextureRect()
		{
			CustomMinimumSize = minSize,
			Texture = Pokemon.Sprite,
			TextureFilter = TextureFilterEnum.Nearest,
			Position = -new Vector2(minSize.X / 2, minSize.Y / 4),
			PivotOffset = new Vector2(minSize.X / 2, 0)
		};

		Control control = new Control();
		control.AddChild(textureRect);

		return control;
	}

   	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		if (dataDictionary.Count == 0) return false;
		
		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();
		if (fromTeamSlot) return !_isFilled;
		
		return true;
	}

	private StageSlot GetGivingSlot(int teamSlotIndex)
	{
		PokemonStage pokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
		return teamSlotIndex == -1 ? null : pokemonStage.FindStageSlot(teamSlotIndex);
	}

	public void DamagePokemon(int damage)
	{
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonDamaged, damage, TeamSlotIndex);
	}

	private void SwapPokemon(StageSlot givingSlot)
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

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();

		if (dataDictionary is null) return;

		int teamSlotIndex = dataDictionary["TeamSlotIndex"].As<int>();
		Pokemon pokemon = dataDictionary["Pokemon"].As<Pokemon>();
		_isMuted = dataDictionary["IsMuted"].As<bool>();

		StageSlot givingSlot = GetGivingSlot(teamSlotIndex);
		if (_isFilled)
		{
			SwapPokemon(givingSlot);
			PokemonTD.AudioManager.PlayPokemonCry(pokemon, true);
			PokemonTD.AudioManager.PlayPokemonCry(givingSlot.Pokemon, true);
			return;
		}
		else if (givingSlot != null)
		{
			givingSlot.UpdateSlot(null); // Empties slot
		}

		UpdateSlot(pokemon);
		TeamSlotIndex = teamSlotIndex;
		PokemonTD.AudioManager.PlayPokemonCry(pokemon, true);
	}

	private bool IsPokemonOnStage(int teamSlotIndex)
	{
		PokemonStage pokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
		return pokemonStage.StageInterface.IsStageTeamSlotInUse(teamSlotIndex);
	}

	public void SetOpacity(bool isDragging)
	{
		Color color = Colors.White;
		color.A = isDragging ? 0.4f : 0;
		SelfModulate = color;
	}

	private void UpdateSlot(Pokemon pokemon)
	{
		TeamSlotIndex = pokemon != null ? TeamSlotIndex : -1;
		Pokemon = pokemon != null ? pokemon : null;
		Sprite.Texture = pokemon != null ? pokemon.Sprite : null;
		_isFilled = pokemon != null;

		if (pokemon == null) IsActive = true;
		if (pokemon == null) _isMuted = false;

		SetWaitTime();
	}

	private void AttackPokemonEnemy()
	{
		if (PokemonEnemyQueue.Count <= 0 || PokemonTD.IsGamePaused || !IsActive) return;

		PokemonEnemy pokemonEnemy = PokemonEnemyQueue[0];
		AddContribution(pokemonEnemy);
		
		PokemonMove pokemonMove = GetPokemonMoveFromTeam();

		bool hasPokemonMoveHit = PokemonManager.Instance.HasPokemonMoveHit(Pokemon, pokemonMove, pokemonEnemy.Pokemon);
		if (!hasPokemonMoveHit && pokemonMove.Accuracy != 0)
		{
			PokemonManager.Instance.PokemonMoveMissed(Pokemon, pokemonMove);
			return;
		}

		string usedMessage = $"{Pokemon.Name} Used {pokemonMove.Name} On {pokemonEnemy.Pokemon.Name}";
		PrintRich.PrintLine(TextColor.Orange, usedMessage);
		PokemonTD.AddStageConsoleMessage(TextColor.Orange, usedMessage);

		if (!_isMuted) PokemonTD.AudioManager.PlayPokemonMove(_pokemonMovePlayer, pokemonMove.Name, Pokemon);

		TweenAttack(pokemonEnemy);
		
		// TODO: Check if it's a unique move

		PokemonCombat.Instance.ApplyStatusConditions(TeamSlotIndex, pokemonEnemy, pokemonMove);
		PokemonCombat.Instance.ApplyStatChanges(pokemonEnemy.Pokemon, pokemonMove);
		
		if (pokemonMove.Power != 0) PokemonCombat.Instance.ApplyDamage(this, pokemonMove, pokemonEnemy);
	}

	private void AddContribution(PokemonEnemy pokemonEnemy)
	{
		bool hasTeamSlotIndex = pokemonEnemy.SlotContributionCount.ContainsKey(TeamSlotIndex);
		if (!hasTeamSlotIndex)
		{
			pokemonEnemy.SlotContributionCount.Add(TeamSlotIndex, 0);
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
		PokemonStage pokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
		StageTeamSlot stageTeamSlot = pokemonStage.StageInterface.FindStageTeamSlot(TeamSlotIndex);
		
		return stageTeamSlot.Pokemon.Move;
	}

	public void SetAlpha(bool isDragging)
	{
		Color color = Colors.White;
		color.A = isDragging ? 0.75f : 1; 
		SelfModulate = color;
	}

	public void ChangeActivity(bool isActive)
	{
		IsActive = isActive;
		EmitSignal(SignalName.ActivityChanged, this, IsActive);
	}
}
