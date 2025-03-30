using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokémonTD;

public partial class PersonalComputerTeam : Container
{
	[Export]
	private GC.Array<InventoryTeamSlot> _inventoryTeamSlots;

    public override void _EnterTree()
    {
		for (int i = 0; i < _inventoryTeamSlots.Count; i++)
		{
			_inventoryTeamSlots[i].ID = i;
		}
    }

    public override void _Ready()
    {
		for (int i = 0; i < PokémonTD.PokémonTeam.Pokémon.Count; i++)
		{
			Pokémon pokémon = PokémonTD.PokémonTeam.Pokémon[i];
			_inventoryTeamSlots[i].UpdatePokémon(pokémon);
		}

		PokémonTD.Signals.InventoryTeamSlotRemoved += (pokémon, teamSlotID) => RemovePokémon(pokémon);
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
		if (IsFilled()) return false;

		GC.Dictionary<string, Variant> dragDictionary = data.As<GC.Dictionary<string, Variant>>();
		bool fromTeamSlot = dragDictionary["FromTeamSlot"].As<bool>();

		return !fromTeamSlot;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
		GC.Dictionary<string, Variant> dragDictionary = data.As<GC.Dictionary<string, Variant>>();

		InventorySlot inventorySlot = dragDictionary["Slot"].As<InventorySlot>();
      	Pokémon pokémon = inventorySlot.Pokémon;
		AddPokémon(pokémon);

		PokémonTD.Signals.EmitSignal(Signals.SignalName.InventorySlotRemoved, inventorySlot.Pokémon, inventorySlot.ID);
		inventorySlot.QueueFree();
    }

	private InventoryTeamSlot GetEmptySlot()
	{
		InventoryTeamSlot inventoryTeamSlot = _inventoryTeamSlots[0];
		for (int i = 0; i < _inventoryTeamSlots.Count; i++)
		{
			if (!_inventoryTeamSlots[i].IsFilled) return _inventoryTeamSlots[i];
		}
		return inventoryTeamSlot;
	}

	private bool IsFilled()
	{
		List<InventoryTeamSlot> filledSlots = _inventoryTeamSlots.ToList().FindAll(slot => slot.IsFilled);
		bool isFilled = filledSlots.Count >= PokémonTD.MaxTeamSize;

		if (isFilled)
		{
			string filledMessage = $"Team Is Currently Full";
			PrintRich.PrintLine(TextColor.Yellow, filledMessage);
		}

		return isFilled ? true : false;
	}

	private void AddPokémon(Pokémon pokémon)
	{
		string addToTeamMessage = $"Adding {pokémon.Name} To Team";
		PrintRich.PrintLine(TextColor.Purple, addToTeamMessage);

		InventoryTeamSlot inventoryTeamSlot = GetEmptySlot();
		inventoryTeamSlot.UpdatePokémon(pokémon);
      	PokémonTD.PokémonTeam.Pokémon.Insert(inventoryTeamSlot.ID, pokémon);

		PrintRich.PrintTeam(TextColor.Orange);
	}

	private void RemovePokémon(Pokémon pokémon)
	{
		string addToTeamMessage = $"Removing {pokémon.Name} From Team";
		PrintRich.PrintLine(TextColor.Purple, addToTeamMessage);

		PokémonTD.PokémonTeam.Pokémon.Remove(pokémon);
		
		PrintRich.PrintTeam(TextColor.Orange);
	}
}
