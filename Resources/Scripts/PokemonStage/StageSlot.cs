using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;

namespace PokemonTD;

public partial class StageSlot : NinePatchRect
{
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

	public Pokemon Pokemon;

	public List<PokemonEnemy> PokemonEnemyQueue = new List<PokemonEnemy>();

	private bool _isDragging;
	private bool _isFilled;
	private bool _isMuted;
	private PokemonEnemy _focusedPokemonEnemy;

	private int _teamSlotID = -1;

	private Control _dragPreview;

    public override void _EnterTree()
    {
		PokemonTD.Signals.PokemonEnemyFainted += OnPokemonEnemyFainted;
        PokemonTD.Signals.PokemonEnemyPassed += UpdatePokemonQueue;
        PokemonTD.Signals.PokemonEnemyCaptured += UpdatePokemonQueue;
		PokemonTD.Signals.DraggingStageTeamSlot += SetStageSlotAlpha;
		PokemonTD.Signals.DraggingStageSlot += SetStageSlotAlpha;
		PokemonTD.Signals.EvolutionFinished += OnEvolutionFinished;
		PokemonTD.Signals.SpeedToggled += OnSpeedToggled;
		PokemonTD.Signals.StageTeamSlotMuted += OnStageTeamSlotMuted;
    }

	public override void _ExitTree()
    {
        PokemonTD.Signals.PokemonEnemyFainted -= OnPokemonEnemyFainted;
        PokemonTD.Signals.PokemonEnemyPassed -= UpdatePokemonQueue;
        PokemonTD.Signals.PokemonEnemyCaptured -= UpdatePokemonQueue;
		PokemonTD.Signals.DraggingStageTeamSlot -= SetStageSlotAlpha;
		PokemonTD.Signals.DraggingStageSlot -= SetStageSlotAlpha;
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

			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonOffStage, _teamSlotID);
			UpdateSlot(null);
		};
		_attackTimer.Timeout += AttackEnemy;

		UpdateSlot(null);
	}

	public override void _Process(double delta)
	{
		if (_dragPreview is not null) TweenRotate();
	}

	private void OnStageTeamSlotMuted(int teamSlotID, bool isToggled)
	{
		if (_teamSlotID != teamSlotID) return;

		_isMuted = isToggled;
	}

	private void OnEvolutionFinished(Pokemon pokemonEvolution, int teamSlotID)
	{
		if (_teamSlotID != teamSlotID) return;

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

	private void OnPokemonEnemyFainted(PokemonEnemy pokemonEnemy, int teamSlotID)
	{
		if (_teamSlotID != teamSlotID) return;
		
		AddExperience(pokemonEnemy);
		UpdatePokemonQueue(pokemonEnemy);
	}

	private void UpdatePokemonQueue(PokemonEnemy pokemonEnemy)
	{
		PokemonEnemyQueue.Remove(pokemonEnemy);
	}

	private void AddExperience(PokemonEnemy pokemonEnemy)
	{
		if (Pokemon is null) return;

		if (Pokemon.Level >= PokemonTD.MaxPokemonLevel) return;
		
		PokemonStage pokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
		StageTeamSlot stageTeamSlot = pokemonStage.StageInterface.FindStageTeamSlot(_teamSlotID);
		int experience = stageTeamSlot.GetExperience(pokemonEnemy);
		stageTeamSlot.AddExperience(experience);
	}

   	public override void _Notification(int what)
	{		
		if (what != NotificationDragEnd) return;
		
		if (!_isDragging) return;

		_isDragging = false; 
		_dragPreview = null;

		SetStageSlotAlpha(false);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingStageSlot, false);
		
		if (IsDragSuccessful()) UpdateSlot(null);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		_isDragging = true;
		SetStageSlotAlpha(true);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingStageSlot, true);

		_dragPreview = GetDragPreview();
		SetDragPreview(_dragPreview);
		return GetDragData();
	}

	private GC.Dictionary<string, Variant> GetDragData()
	{
		GC.Dictionary<string, Variant> data = new GC.Dictionary<string, Variant>()
		{
			{ "TeamSlotID", _teamSlotID },
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
		
		int teamSlotID = dataDictionary["TeamSlotID"].As<int>();
		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();

		if (!fromTeamSlot) return _isFilled ? false : true;

		if (IsPokemonOnStage(teamSlotID)) return false;
		
		return _isFilled ? false : true;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();

		if (dataDictionary is null) return;

		Pokemon = dataDictionary["Pokemon"].As<Pokemon>();
		_teamSlotID = dataDictionary["TeamSlotID"].As<int>();
		_isMuted = dataDictionary["IsMuted"].As<bool>();

		UpdateSlot(Pokemon);

		PokemonTD.AudioManager.PlayPokemonCry(Pokemon, true);
	}

	private bool IsPokemonOnStage(int teamSlotID)
	{
		PokemonStage pokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
		return pokemonStage.StageInterface.IsStageTeamSlotInUse(teamSlotID);
	}

	private void SetStageSlotAlpha(bool isDragging)
	{
		Color color = Colors.White;
		color.A = isDragging ? 0.25f : 0; 
		SelfModulate = color;
	}

	private void UpdateSlot(Pokemon pokemon)
	{
		Pokemon = pokemon;

		_sprite.Texture = pokemon is null ? null : pokemon.Sprite;
		_teamSlotID = pokemon is null ? -1 : _teamSlotID;
		_isFilled = pokemon is null ? false : true;

		if (pokemon is null) _isMuted = false;

		SetWaitTime();
	}

	private void AttackEnemy()
	{
		if (PokemonEnemyQueue.Count == 0 || PokemonTD.IsGamePaused) return;

		_focusedPokemonEnemy = PokemonEnemyQueue[0];

		PokemonMove pokemonMove = GetPokemonMoveFromTeam();

		if (!PokemonTD.PokemonManager.IsPokemonMoveLanding(pokemonMove))
		{
			MissedPokemonMove(pokemonMove);
			return;
		}

		string usedMessage = $"{Pokemon.Name} Used {pokemonMove.Name} On {_focusedPokemonEnemy.Pokemon.Name}";
		PrintRich.PrintLine(TextColor.Orange, usedMessage);
		PokemonTD.AddStageConsoleMessage(TextColor.Orange, usedMessage);

		if (!_isMuted) PokemonTD.AudioManager.PlayPokemonMove(_pokemonMovePlayer, pokemonMove.Name, Pokemon);

		TweenAttack(_focusedPokemonEnemy);

		if (pokemonMove.Power != 0) DealDamage(pokemonMove);
	}

	private void TweenAttack(PokemonEnemy pokemonEnemy)
	{
		Vector2 originalPosition = _sprite.Position;
		Vector2 originalGlobalPosition = _sprite.GlobalPosition;

		Vector2 direction = originalGlobalPosition.DirectionTo(pokemonEnemy.GlobalPosition);
		_sprite.FlipH = direction.X > 0;

		Vector2 positionOne = originalPosition - (direction * 5);
		Vector2 positionTwo = originalPosition + (direction * 15);

		// TODO: Create a ratio between the position of the enemy and the slot
		// TODO: Use the ratio and use a set offset to display the attacking animation
		Tween tween = CreateTween().SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(_sprite, "position", positionOne, _attackTimer.WaitTime / 4);
		tween.TweenProperty(_sprite, "position", positionTwo, _attackTimer.WaitTime / 4);
		tween.TweenProperty(_sprite, "position", originalPosition, _attackTimer.WaitTime / 4);
	}

	private void DealDamage(PokemonMove pokemonMove)
	{
		int damage = PokemonTD.PokemonManager.GetDamage(Pokemon, pokemonMove, _focusedPokemonEnemy);
		
		float firstTypeMultiplier = PokemonTD.PokemonTypes.GetTypeMultiplier(pokemonMove.Type, _focusedPokemonEnemy.Pokemon.Types[0]);
		if (_focusedPokemonEnemy.Pokemon.Types.Count > 1)
		{
			float secondTypeMultiplier = PokemonTD.PokemonTypes.GetTypeMultiplier(pokemonMove.Type, _focusedPokemonEnemy.Pokemon.Types[1]);

			if (firstTypeMultiplier < secondTypeMultiplier) firstTypeMultiplier = secondTypeMultiplier;
		}

		EffectiveType effectiveType = PokemonTD.PokemonTypes.GetEffectiveType(firstTypeMultiplier);

		string damageMessage = $"For {damage} Damage ";

		string effectiveMessage = PrintRich.GetEffectiveMessage(effectiveType);
		damageMessage += effectiveMessage;

		PrintRich.Print(TextColor.Orange, damageMessage);
		PokemonTD.AddStageConsoleMessage(TextColor.Orange, damageMessage);

		_focusedPokemonEnemy.DamagePokemon(damage, _teamSlotID);

	}

	private void MissedPokemonMove(PokemonMove pokemonMove)
	{
		if (pokemonMove.Accuracy == 0) return;

		string missedMessage = $"{Pokemon.Name}'s {pokemonMove.Name} Missed";
		PrintRich.PrintLine(TextColor.Purple, missedMessage);
	}

	private PokemonMove GetPokemonMoveFromTeam()
	{
		PokemonStage PokemonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
		StageTeamSlot stageTeamSlot = PokemonStage.StageInterface.FindStageTeamSlot(_teamSlotID);
		
		return stageTeamSlot.Pokemon.Move;
	}

	private async void TweenRotate()
	{
		Vector2 initialPosition = _dragPreview.Position;

		await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);

		if (!_isDragging) return;

		Vector2 finalPosition = _dragPreview.Position;
		Vector2 direction = finalPosition.DirectionTo(initialPosition);

		float rotationValue = 25;
		float degrees = direction.X > 0 ? -rotationValue : rotationValue;
		degrees = direction.X == 0 ? 0 : degrees;

		float rotation = Mathf.DegToRad(degrees);

		float duration = 0.5f;
		Tween tween = CreateTween();
		tween.TweenProperty(_dragPreview, "rotation", rotation, duration);
	}
}
