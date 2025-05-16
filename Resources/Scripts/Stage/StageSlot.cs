using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;

namespace PokemonTD;

public partial class StageSlot : NinePatchRect
{
	[Signal]
	public delegate void FaintedEventHandler(StageSlot pokemonStageSlot);

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
	public int LightScreenCount;
	public int ReflectCount;
	public bool IsCharging = false;
	public bool UsedDig = false;

	private Control _dragPreview;

	public override void _ExitTree()
    {
        PokemonTD.Signals.PokemonEnemyPassed -= UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEnemyCaptured -= UpdatePokemonQueue;
		PokemonTD.Signals.DraggingStageTeamSlot -= SetOpacity;
		PokemonTD.Signals.DraggingStageSlot -= SetOpacity;
		PokemonTD.Signals.PokemonEvolved -= PokemonEvolved;
		PokemonTD.Signals.SpeedToggled -= OnSpeedToggled;
		PokemonTD.Signals.StageTeamSlotMuted -= OnStageTeamSlotMuted;
    }

	public override void _Ready()
	{
        PokemonTD.Signals.PokemonEnemyPassed += UpdatePokemonQueue;
		PokemonTD.Signals.PokemonEnemyCaptured += UpdatePokemonQueue;
		PokemonTD.Signals.DraggingStageTeamSlot += SetOpacity;
		PokemonTD.Signals.DraggingStageSlot += SetOpacity;
		PokemonTD.Signals.PokemonEvolved += PokemonEvolved;
		PokemonTD.Signals.SpeedToggled += OnSpeedToggled;
		PokemonTD.Signals.StageTeamSlotMuted += OnStageTeamSlotMuted;

		_interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) => 
		{
			if (!isDoubleClick || !isLeftClick || Pokemon is null) return;

			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonUsed, false, TeamSlotIndex);
			UpdateSlot(null);

			// Print Message To Console
			string offStageMessage = $"{Pokemon.Name} Is Off Stage";
			PrintRich.PrintLine(TextColor.Purple, offStageMessage);
		};
		_attackTimer.Timeout += AttackPokemonEnemy;

		UpdateSlot(null);
	}

	public override void _Process(double delta)
	{
		if (_dragPreview == null) return;

		PokemonTD.Tween.TweenSlotDragRotation(_dragPreview, _isDragging);
	}

	private void OnStageTeamSlotMuted(int teamSlotIndex, bool isToggled)
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
		StageTeamSlot stageTeamSlot = stageInterface.StageTeamSlots.FindStageTeamSlot(TeamSlotIndex);
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

		_dragPreview = PokemonTD.GetStageDragPreview(Pokemon);
		SetDragPreview(_dragPreview);
		return PokemonTD.GetStageDragData(Pokemon, TeamSlotIndex, false, _isMuted);
	}

   	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		if (dataDictionary.Count == 0) return false;
		
		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();
		if (fromTeamSlot) return !_isFilled;
		
		return true;
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

		StageInterface stageInterface = GetStageInterface();
		StageTeamSlot stageTeamSlot = stageInterface.StageTeamSlots.FindStageTeamSlot(teamSlotIndex);
		UpdateSlot(stageTeamSlot.Pokemon);

		TeamSlotIndex = teamSlotIndex;
		PokemonTD.AudioManager.PlayPokemonCry(pokemon, true);
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

	private bool IsPokemonOnStage(int teamSlotIndex)
	{
		StageInterface stageInterface = GetStageInterface();
		return stageInterface.IsStageTeamSlotInUse(teamSlotIndex);
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
		pokemonEnemy.Fainted += UpdatePokemonQueue;
		AddContribution(pokemonEnemy);
		
		PokemonMove pokemonMove = GetPokemonMoveFromTeam();
		pokemonMove = pokemonMove.Name == "Metronome" ? PokemonMoves.Instance.GetRandomPokemonMove() : pokemonMove;

		bool hasPokemonMoveHit = PokemonManager.Instance.HasPokemonMoveHit(Pokemon, pokemonMove, pokemonEnemy.Pokemon);
		if (!hasPokemonMoveHit && pokemonMove.Name != "Swift")
		{
			if (pokemonMove.Accuracy != 0)
			{
				string missedMessage = $"{Pokemon.Name}'s {pokemonMove.Name} Missed";
				PrintRich.PrintLine(TextColor.Purple, missedMessage);
			}
			return;
		}

		string usedMessage = $"{Pokemon.Name} Used {pokemonMove.Name} On {pokemonEnemy.Pokemon.Name}";
		PrintRich.PrintLine(TextColor.Orange, usedMessage);

		if (!_isMuted) PokemonTD.AudioManager.PlayPokemonMove(_pokemonMovePlayer, pokemonMove.Name, Pokemon);

		TweenAttack(pokemonEnemy);

		if (PokemonMoveEffect.Instance.UniqueMoves.IsUniqueMove(pokemonMove))
		{
			UniqueMoves uniqueMoves = PokemonMoveEffect.Instance.UniqueMoves;
			switch (pokemonMove.Name)
			{
				case "Dragon Rage":
					uniqueMoves.DragonRage(pokemonEnemy);
					break;
				case "Low Kick":
					uniqueMoves.LowKick(pokemonEnemy);
					break;
				case "Seismic Toss":
					uniqueMoves.SeismicToss(pokemonEnemy);
					break;
				case "Mirror Move":
					uniqueMoves.MirrorMove(this, pokemonEnemy);
					break;
				case "Night Shade":
					uniqueMoves.MirrorMove(this, pokemonEnemy);
					break;
				case "Mimic":
					uniqueMoves.Mimic(this, pokemonEnemy);
					break;
				case "Pay Day":
					uniqueMoves.PayDay();
					break;
				case "Sonic Boom":
					uniqueMoves.SonicBoom(pokemonEnemy);
					break;
				case "Super Fang":
					uniqueMoves.SuperFang(pokemonEnemy);
					break;
				case "Psywave":
					uniqueMoves.Psywave(pokemonEnemy);
					break;
				case "Surf":
					uniqueMoves.Surf(this, pokemonMove, pokemonEnemy);
					break;
				case "Teleport":
					uniqueMoves.Teleport();
					break;
			}
			return;
		}
		else if (PokemonMoveEffect.Instance.TrapMoves.IsTrapMove(pokemonMove))
		{

		}
		else if (PokemonMoveEffect.Instance.ChargeMoves.IsChargeMove(pokemonMove).IsChargeMove)
		{
			IsCharging = PokemonMoveEffect.Instance.ChargeMoves.ApplyChargeMove(IsCharging, pokemonMove, pokemonEnemy);
			UsedDig = PokemonMoveEffect.Instance.ChargeMoves.HasUsedDig(IsCharging, pokemonMove);
		}
		else if (PokemonMoveEffect.Instance.FlinchMoves.IsFlinchMove(pokemonMove))
		{
			PokemonMoveEffect.Instance.FlinchMoves.ApplyFlinchMove(pokemonEnemy);
		}
		else if (PokemonMoveEffect.Instance.OneHitKOMoves.IsOneHitKOMove(pokemonMove))
		{
			PokemonMoveEffect.Instance.OneHitKOMoves.ApplyOneHitKO(pokemonEnemy);
		}

		PokemonCombat.Instance.ApplyStatusConditions(TeamSlotIndex, pokemonEnemy, pokemonMove);
		PokemonMoveEffect.Instance.StatMoves.CheckStatChanges(pokemonEnemy, pokemonMove);
		
		if (pokemonMove.Power != 0) PokemonCombat.Instance.ApplyDamage(this, pokemonMove, pokemonEnemy);
	}

	public void AttackPokemonEnemy(PokemonMove pokemonMove, PokemonEnemy pokemonEnemy)
	{
		PokemonCombat.Instance.ApplyStatusConditions(TeamSlotIndex, pokemonEnemy, pokemonMove);
		PokemonMoveEffect.Instance.StatMoves.CheckStatChanges(pokemonEnemy, pokemonMove);
		
		if (pokemonMove.Power != 0) PokemonCombat.Instance.ApplyDamage(this, pokemonMove, pokemonEnemy);
	}

	private void AddContribution(PokemonEnemy pokemonEnemy)
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
		StageTeamSlot stageTeamSlot = stageInterface.StageTeamSlots.FindStageTeamSlot(TeamSlotIndex);
		return stageTeamSlot.Pokemon.Move;
	}

	public void SetAlpha(bool isDragging)
	{
		Color color = Colors.White;
		color.A = isDragging ? 0.75f : 1; 
		SelfModulate = color;
	}
}
