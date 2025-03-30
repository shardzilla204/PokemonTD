using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;

namespace PokémonTD;

public partial class StageSlot : NinePatchRect
{
	[Signal]
	public delegate void DraggingEventHandler(bool isDragging);
	
	[Export]
	private Area2D _area;

	[Export]
	private TextureRect _sprite;

	[Export]
	private InteractComponent _interactComponent;

	[Export]
	private Timer _attackTimer;

	public Pokémon Pokémon;

	public List<PokémonEnemy> PokémonEnemyQueue = new List<PokémonEnemy>();

	private bool _isDragging;
	private bool _isFilled;
	private PokémonEnemy _focusedPokémonEnemy;

	private int _teamSlotID = -1;

    public override void _EnterTree()
    {
		PokémonTD.Signals.PokémonEnemyPassed += (pokémonEnemy) => PokémonEnemyQueue.Remove(pokémonEnemy);
    }

    public override void _ExitTree()
    {
		PokémonTD.Signals.PokémonEnemyPassed -= (pokémonEnemy) => PokémonEnemyQueue.Remove(pokémonEnemy);
    }

	public override void _Ready()
	{
		PokémonStage pokémonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokémonStage>();
		foreach (StageSlot stageSlot in pokémonStage.StageSlots)
		{
			stageSlot.Dragging += SetStageSlotAlpha;
		}

		List<StageTeamSlot> stageTeamSlots = pokémonStage.StageInterface.GetStageTeamSlots();
		foreach (StageTeamSlot stageTeamSlot in stageTeamSlots) 
		{
			stageTeamSlot.Dragging += SetStageSlotAlpha;
		}
		
		PokémonTD.Signals.PokémonTeamUpdated += OnTeamUpdated;

		_interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) => 
		{
			if (!isDoubleClick || !isLeftClick || Pokémon is null) return;

			string offStageMessage = $"{Pokémon.Name} Is Off Stage";
			PrintRich.PrintLine(TextColor.Purple, offStageMessage);

			PokémonTD.Signals.EmitSignal(Signals.SignalName.PokémonOffStage, _teamSlotID);
			Update(null);
		};
		_attackTimer.Timeout += AttackEnemy;

		Update(Pokémon);
	}

	private void OnTeamUpdated()
	{
		PokémonStage pokémonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokémonStage>();
		List<StageTeamSlot> stageTeamSlots = pokémonStage.StageInterface.GetStageTeamSlots();
		foreach (StageTeamSlot stageTeamSlot in stageTeamSlots) 
		{
			stageTeamSlot.Dragging += SetStageSlotAlpha;
		}
	}

	private void AddExperience(PokémonEnemy pokémonEnemy)
	{
		PokémonStage pokémonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokémonStage>();
		StageTeamSlot stageTeamSlot = pokémonStage.StageInterface.FindStageTeamSlot(_teamSlotID);

		if (stageTeamSlot.Pokémon.Level >= PokémonTD.MaxPokémonLevel) return;
		
		int experience = stageTeamSlot.GetExperience(pokémonEnemy);
		stageTeamSlot.AddExperience(experience);
	}

   public override void _Notification(int what)
	{		
		if (what != NotificationDragEnd) return;
		
		if (!_isDragging) return;

		_isDragging = false; 
		SetStageSlotAlpha(false);
		EmitSignal(SignalName.Dragging, false);
		
		if (IsDragSuccessful()) Update(null);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		_isDragging = true;
		SetStageSlotAlpha(true);
		EmitSignal(SignalName.Dragging, true);

		SetDragPreview(GetDragPreview());
		return GetDragData();
	}

	private GC.Dictionary<string, Variant> GetDragData()
	{
		GC.Dictionary<string, Variant> data = new GC.Dictionary<string, Variant>()
		{
			{ "TeamSlotID", _teamSlotID },
			{ "FromTeamSlot", false },
			{ "Pokémon", Pokémon }
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
			Texture = Pokémon.Sprite,
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

		if (IsPokémonOnStage(teamSlotID)) return false;
		
		return _isFilled ? false : true;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();

		if (dataDictionary is null) return;

		Pokémon = dataDictionary["Pokémon"].As<Pokémon>();
		_teamSlotID = dataDictionary["TeamSlotID"].As<int>();

		Update(Pokémon);
	}

	private bool IsPokémonOnStage(int teamSlotID)
	{
		PokémonStage pokémonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokémonStage>();
		return pokémonStage.StageInterface.IsStageTeamSlotInUse(teamSlotID);
	}

	private void SetStageSlotAlpha(bool isDragging)
	{
		Color color = Colors.White;
		color.A = isDragging ? 0.25f : 0; 
		SelfModulate = color;
	}

	private void Update(Pokémon pokémon)
	{
		Pokémon = pokémon;

		_sprite.Texture = Pokémon is null ? null : Pokémon.Sprite;
		_teamSlotID = Pokémon is null ? -1 : _teamSlotID;
		_isFilled = Pokémon is null ? false : true;

		if (Pokémon is not null) 
		{
			_attackTimer.WaitTime = 100 / (Pokémon.Speed * PokémonTD.GameSpeed);
			_attackTimer.Start();
		}
		else
		{
			_attackTimer.Stop();
		}
	}

	private void AttackEnemy()
	{
		if (PokémonEnemyQueue.Count == 0 || PokémonTD.IsGamePaused) return;

		_focusedPokémonEnemy = PokémonEnemyQueue[0];
		_focusedPokémonEnemy.Fainted += (pokémonEnemy) => 
		{
			AddExperience(pokémonEnemy);
			PokémonEnemyQueue.Remove(pokémonEnemy);
		};
		_focusedPokémonEnemy.Captured += (pokémonEnemy) => PokémonEnemyQueue.Remove(pokémonEnemy);

		PokémonMove pokémonMove = GetPokémonMoveFromTeam();

		if (!PokémonTD.PokémonManager.IsMoveLanding(pokémonMove))
		{
			MissedPokémonMove(pokémonMove);
			return;
		}

		string usedMessage = $"{Pokémon.Name} Used {pokémonMove.Name} On {_focusedPokémonEnemy.Pokémon.Name}";
		PrintRich.Print(TextColor.Orange, usedMessage);

		if (pokémonMove.Power != 0) DealDamage(pokémonMove);
	}

	private void DealDamage(PokémonMove pokémonMove)
	{
		int damage = PokémonTD.PokémonManager.GetDamage(Pokémon, pokémonMove, _focusedPokémonEnemy);
		
		float firstTypeMultiplier = PokémonTD.PokémonTypes.GetTypeMultiplier(pokémonMove.Type, _focusedPokémonEnemy.Pokémon.Types[0]);
		if (_focusedPokémonEnemy.Pokémon.Types.Count > 1)
		{
			float secondTypeMultiplier = PokémonTD.PokémonTypes.GetTypeMultiplier(pokémonMove.Type, _focusedPokémonEnemy.Pokémon.Types[1]);

			if (firstTypeMultiplier < secondTypeMultiplier) firstTypeMultiplier = secondTypeMultiplier;
		}

		EffectiveType effectiveType = PokémonTD.PokémonTypes.GetEffectiveType(firstTypeMultiplier);

		string damageMessage = $"For {damage} Damage";

		PrintRich.Print(TextColor.Orange, damageMessage);
		PrintRich.PrintEffectiveness(TextColor.Orange, effectiveType);

		_focusedPokémonEnemy.DamagePokémon(damage);
	}

	private void MissedPokémonMove(PokémonMove pokémonMove)
	{
		if (pokémonMove.Accuracy == 0) return;

		string missedMessage = $"{Pokémon.Name}'s {pokémonMove.Name} Missed";
		PrintRich.PrintLine(TextColor.Purple, missedMessage);
	}

	private PokémonMove GetPokémonMoveFromTeam()
	{
		PokémonStage pokémonStage = GetParentOrNull<Node>().GetOwnerOrNull<PokémonStage>();
		StageTeamSlot stageTeamSlot = pokémonStage.StageInterface.FindStageTeamSlot(_teamSlotID);
		
		return stageTeamSlot.Pokémon.CurrentMove;
	}
}
