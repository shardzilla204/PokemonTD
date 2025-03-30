using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;

namespace PokémonTD;

public partial class PersonalComputerInventory : Container
{
	[Export]
	private CustomButton _cycleLeftButton;

	[Export]
	private CustomButton _cycleRightButton;

	[Export]
	private Container _inventorySlotContainer;

	public List<Pokémon> Pokémon = new List<Pokémon>();

	private int _index = 0;
	private int _maxIndex = 3;

    public override void _Ready()
    {
        _cycleLeftButton.Pressed += () => CycleInventory(false);
        _cycleRightButton.Pressed += () => CycleInventory(true);

		_cycleLeftButton.Visible = _index > 0 ? true : false;
		_cycleRightButton.Visible = _index < _maxIndex ? true : false;

		foreach (Pokémon pokémon in PokémonTD.PersonalComputer.Pokémon)
		{
			AddPokémon(pokémon);
		}

		if (PokémonTD.IsPersonalComputerRandomized) AddRandomPokémon();
    }

	private void CycleInventory(bool isCyclingRight)
	{
		_index += isCyclingRight ? 1 : -1;
		_cycleLeftButton.Visible = _index > 0 ? true : false;
		_cycleRightButton.Visible = _index < _maxIndex ? true : false;
	}

	// Each interation can hold up to 30 pokémon
	private void UpdateContainer()
	{
		// Remove the current children
		foreach (Node inventorySlot in _inventorySlotContainer.GetChildren())
		{
			_inventorySlotContainer.RemoveChild(inventorySlot);
			inventorySlot.QueueFree();
		}
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (PokémonTD.PokémonTeam.Pokémon.Count == 0) return false;

		GC.Dictionary<string, Variant> dragDictionary = data.As<GC.Dictionary<string, Variant>>();
		bool fromTeamSlot = dragDictionary["FromTeamSlot"].As<bool>();

		return fromTeamSlot;
	}

    public override void _DropData(Vector2 atPosition, Variant data)
    {
		GC.Dictionary<string, Variant> dragDictionary = data.As<GC.Dictionary<string, Variant>>();

		InventoryTeamSlot inventoryTeamSlot = dragDictionary["Slot"].As<InventoryTeamSlot>();
		Pokémon pokémon = inventoryTeamSlot.Pokémon;
        AddPokémon(pokémon);

		PokémonTD.Signals.EmitSignal(Signals.SignalName.InventoryTeamSlotRemoved, inventoryTeamSlot.Pokémon, inventoryTeamSlot.ID);

		inventoryTeamSlot.UpdatePokémon(null);
    }

	private void AddPokémon(Pokémon pokémon)
	{
		InventorySlot inventorySlot = PokémonTD.PackedScenes.GetInventorySlot();
		inventorySlot.Pokémon = pokémon;
		inventorySlot.ID = Pokémon.Count;
		
		_inventorySlotContainer.AddChild(inventorySlot);

 		Pokémon.Add(pokémon);
	}

	private void AddRandomPokémon()
	{
		for (int i = 0; i < PokémonTD.PersonalComputerCount; i++)
		{
			Pokémon pokémon = PokémonTD.GetRandomPokémon();
			AddPokémon(pokémon);
		}
	}
}

