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

	public Pokemon Pokemon;

	public List<PokemonEnemy> PokemonEnemyQueue = new List<PokemonEnemy>();

	private bool _isDragging;
	private bool _isFilled;
	private PokemonEnemy _focusedPokemonEnemy;

	private int _teamSlotID = -1;

    public override void _EnterTree()
    {
		PokemonTD.Signals.PokemonEnemyFainted += OnPokemonEnemyFainted;
        PokemonTD.Signals.PokemonEnemyPassed += UpdatePokemonQueue;
        PokemonTD.Signals.PokemonEnemyCaptured += UpdatePokemonQueue;
		PokemonTD.Signals.DraggingTeamStageSlot += SetStageSlotAlpha;
    }

	public override void _ExitTree()
    {
        PokemonTD.Signals.PokemonEnemyFainted -= OnPokemonEnemyFainted;
        PokemonTD.Signals.PokemonEnemyPassed -= UpdatePokemonQueue;
        PokemonTD.Signals.PokemonEnemyCaptured -= UpdatePokemonQueue;
		PokemonTD.Signals.DraggingTeamStageSlot -= SetStageSlotAlpha;
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

	private void OnPokemonEnemyFainted(PokemonEnemy pokemonEnemy)
	{
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
		SetStageSlotAlpha(false);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingStageSlot, false);
		
		if (IsDragSuccessful()) UpdateSlot(null);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		_isDragging = true;
		SetStageSlotAlpha(true);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingStageSlot, true);

		SetDragPreview(GetDragPreview());
		return GetDragData();
	}

	private GC.Dictionary<string, Variant> GetDragData()
	{
		GC.Dictionary<string, Variant> data = new GC.Dictionary<string, Variant>()
		{
			{ "TeamSlotID", _teamSlotID },
			{ "FromTeamSlot", false },
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
			Position = -minSize / 2
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

		UpdateSlot(Pokemon);
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

		if (pokemonMove.Power != 0) DealDamage(pokemonMove);
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

		string damageMessage = $"For {damage} Damage";

		PrintRich.Print(TextColor.Orange, damageMessage);
		PrintRich.PrintEffectiveness(TextColor.Orange, effectiveType);

		_focusedPokemonEnemy.DamagePokemon(damage);
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
}
