using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;

namespace PokemonTD;

public partial class PokeCenterTeam : Container
{
	[Export]
	private Container _teamSlotContainer;

	private List<PokeCenterTeamSlot> _teamSlots = new List<PokeCenterTeamSlot>();

    public override void _ExitTree()
    {
		PokemonTD.Signals.PokemonTeamUpdated -= OnPokemonTeamUpdated;
    }

    public override void _Ready()
    {
		PokemonTD.Signals.PokemonTeamUpdated += OnPokemonTeamUpdated;

		FillTeamSlotList();
		DisplayPokemonTeam();
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
		if (PokemonTeam.Instance.IsFull()) return false;

		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		bool fromAnalysisSlot = dataDictionary["FromAnalysisSlot"].As<bool>();

		if (fromAnalysisSlot) return true;

		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();
		return !fromTeamSlot;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		bool fromAnalysisSlot = dataDictionary["FromAnalysisSlot"].As<bool>();
		if (fromAnalysisSlot)
		{
			PokemonAnalysis pokemonAnalysis = dataDictionary["PokemonAnalysis"].As<PokemonAnalysis>();
			PokemonTeam.Instance.AddPokemon(pokemonAnalysis.Pokemon);
			pokemonAnalysis.SetPokemon(null);
			return;
		}

		PokeCenterSlot pokeCenterSlot = dataDictionary["Slot"].As<PokeCenterSlot>();
		PokeCenter.Instance.RemovePokemon(pokeCenterSlot.Pokemon);

		pokeCenterSlot.QueueFree();
    }

	private void FillTeamSlotList()
	{
		foreach (Node child in _teamSlotContainer.GetChildren())
		{
			if (child is PokeCenterTeamSlot teamSlot) _teamSlots.Add(teamSlot);
		}
	}

	// Show pokemon already in your team
	private void DisplayPokemonTeam()
	{
		for (int i = 0; i < PokemonTeam.Instance.Pokemon.Count; i++)
		{
			Pokemon pokemon = PokemonTeam.Instance.Pokemon[i];
			_teamSlots[i].UpdateSlot(pokemon);
		}
	}

	// Update display of each slot
	private void OnPokemonTeamUpdated()
	{
		foreach (PokeCenterTeamSlot teamSlot in _teamSlots)
		{
			teamSlot.UpdateSlot(null);
		}

		for (int i = 0; i < PokemonTeam.Instance.Pokemon.Count; i++)
		{
			Pokemon pokemon = PokemonTeam.Instance.Pokemon[i];
			_teamSlots[i].UpdateSlot(pokemon);
		}
		PrintRich.PrintTeam(TextColor.Orange);
	}
}
