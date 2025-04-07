using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public partial class PokemonCenterTeam : Container
{
	[Export]
	private Container _pokemonCenterTeamSlotContainer;

	public List<PokemonCenterTeamSlot> Slots = new List<PokemonCenterTeamSlot>();

    public override void _EnterTree()
    {
		// Fill pokemon center team slot list
		foreach (Node child in _pokemonCenterTeamSlotContainer.GetChildren())
		{
			if (child is PokemonCenterTeamSlot pokemonCenterTeamSlot) Slots.Add(pokemonCenterTeamSlot);
		}

		// Assign ID to each team slot
		for (int i = 0; i < Slots.Count; i++)
		{
			Slots[i].ID = i;
		}
    }

    public override void _Ready()
    {
		// Show pokemon already in your team
		for (int i = 0; i < PokemonTD.PokemonTeam.Pokemon.Count; i++)
		{
			Pokemon pokemon = PokemonTD.PokemonTeam.Pokemon[i];
			Slots[i].UpdateSlot(pokemon);
		}

		// Add signals to team slots
		foreach (PokemonCenterTeamSlot pokemonCenterTeamSlot in Slots)
		{
			pokemonCenterTeamSlot.Removed += RemovePokemon;
		}

		AddSlotSignals();
    }

	public void AddSlotSignals()
	{
		PokemonCenterInterface pokemonCenterInterface = GetParentOrNull<Node>().GetOwnerOrNull<PokemonCenterInterface>();
		foreach (PokemonCenterSlot pokemonCenterSlot in pokemonCenterInterface.PokemonCenterInventory.Slots)
		{
			pokemonCenterSlot.Removed += AddPokemon;
		}
	}

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
		if (IsTeamFull()) return false;

		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();

		return !fromTeamSlot;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();

		PokemonCenterSlot pokemonCenterSlot = dataDictionary["Slot"].As<PokemonCenterSlot>();
		pokemonCenterSlot.EmitSignal(PokemonCenterSlot.SignalName.Removed, pokemonCenterSlot.Pokemon);
		pokemonCenterSlot.QueueFree();
    }

	private PokemonCenterTeamSlot GetEmptySlot()
	{
		PokemonCenterTeamSlot pokemonCenterTeamSlot = Slots[0];
		for (int i = 0; i < Slots.Count; i++)
		{
			if (!Slots[i].IsFilled) return Slots[i];
		}

		string errorMessage = "Couldn't Find An Empty Slot";
		PrintRich.PrintLine(TextColor.Red, errorMessage);
		return pokemonCenterTeamSlot;
	}

	private bool IsTeamFull()
	{
		List<PokemonCenterTeamSlot> filledSlots = Slots.ToList().FindAll(slot => slot.IsFilled);
		bool isTeamFull = filledSlots.Count >= PokemonTD.MaxTeamSize;

		return isTeamFull;
	}

	private void AddPokemon(Pokemon pokemon)
	{
		string addToTeamMessage = $"Adding {pokemon.Name} To Team";
		PrintRich.PrintLine(TextColor.Purple, addToTeamMessage);

		PokemonCenterTeamSlot pokemonCenterTeamSlot = GetEmptySlot();
		pokemonCenterTeamSlot.UpdateSlot(pokemon);
      	PokemonTD.PokemonTeam.Pokemon.Insert(pokemonCenterTeamSlot.ID, pokemon);

		PrintRich.PrintTeam(TextColor.Orange);
	}

	private void RemovePokemon(Pokemon pokemon)
	{
		string removeFromTeamMessage = $"Removing {pokemon.Name} From Team";
		PrintRich.PrintLine(TextColor.Purple, removeFromTeamMessage);

		PokemonTD.PokemonTeam.Pokemon.Remove(pokemon);
		
		PrintRich.PrintTeam(TextColor.Orange);
	}
}
