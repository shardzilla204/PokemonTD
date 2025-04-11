using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;

namespace PokemonTD;

public partial class PokeCenterTeam : Container
{
	[Export]
	private Container _teamSlotContainer;

	public List<PokeCenterTeamSlot> TeamSlots = new List<PokeCenterTeamSlot>();

    public override void _EnterTree()
    {
		PokemonTD.Signals.PokeCenterSlotRemoved += AddPokemon;
		PokemonTD.Signals.PokeCenterTeamSlotRemoved += RemovePokemon;

		foreach (Node child in _teamSlotContainer.GetChildren())
		{
			if (child is PokeCenterTeamSlot teamSlot) TeamSlots.Add(teamSlot);
		}
    }

    public override void _ExitTree()
    {
        PokemonTD.Signals.PokeCenterSlotRemoved -= AddPokemon;
		PokemonTD.Signals.PokeCenterTeamSlotRemoved -= RemovePokemon;
    }

    public override void _Ready()
    {
		// Show pokemon already in your team
		for (int i = 0; i < PokemonTD.PokemonTeam.Pokemon.Count; i++)
		{
			Pokemon pokemon = PokemonTD.PokemonTeam.Pokemon[i];
			TeamSlots[i].UpdateSlot(pokemon);
		}
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
		if (PokemonTD.PokemonTeam.IsFull()) return false;

		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();

		return !fromTeamSlot;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();

		PokeCenterSlot pokeCenterSlot = dataDictionary["Slot"].As<PokeCenterSlot>();
		pokeCenterSlot.QueueFree();
		
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokeCenterSlotRemoved, pokeCenterSlot.Pokemon);
    }

	private void AddPokemon(Pokemon pokemon)
	{
		string addToTeamMessage = $"Adding {pokemon.Name} To Team";
		PrintRich.PrintLine(TextColor.Purple, addToTeamMessage);

      	PokemonTD.PokemonTeam.Pokemon.Add(pokemon);

		ResetTeamSlots();

		PrintRich.PrintTeam(TextColor.Orange);
	}

	private void RemovePokemon(Pokemon pokemon)
	{
		string removeFromTeamMessage = $"Removing {pokemon.Name} From Team";
		PrintRich.PrintLine(TextColor.Purple, removeFromTeamMessage);

		PokemonTD.PokemonTeam.Pokemon.Remove(pokemon);

		ResetTeamSlots();

		PrintRich.PrintTeam(TextColor.Orange);
	}

	private void ResetTeamSlots()
	{
		foreach (PokeCenterTeamSlot teamSlot in TeamSlots)
		{
			teamSlot.UpdateSlot(null);
		}

		for (int i = 0; i < PokemonTD.PokemonTeam.Pokemon.Count; i++)
		{
			Pokemon pokemon = PokemonTD.PokemonTeam.Pokemon[i];
			TeamSlots[i].UpdateSlot(pokemon);
		}
	}
}
