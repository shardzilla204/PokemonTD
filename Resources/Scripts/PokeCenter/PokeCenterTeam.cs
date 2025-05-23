using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;

namespace PokemonTD;

public partial class PokeCenterTeam : Container
{
	[Export]
	private Container _teamSlotContainer;

	[Export]
	private CustomButton _clearButton;

	private List<PokeCenterTeamSlot> _teamSlots = new List<PokeCenterTeamSlot>();

    public override void _ExitTree()
    {
		PokemonTD.Signals.PokemonTeamUpdated -= OnPokemonTeamUpdated;
    }

    public override void _Ready()
    {
		PokemonTD.Signals.PokemonTeamUpdated += OnPokemonTeamUpdated;
		_clearButton.Pressed += ClearTeam;

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
			PokeCenterAnalysis pokeCenterAnalysis = dataDictionary["PokeCenterAnalysis"].As<PokeCenterAnalysis>();
			PokemonTeam.Instance.AddPokemon(pokeCenterAnalysis.Pokemon);
			pokeCenterAnalysis.SetPokemon(null);
			return;
		}

		PokeCenterSlot pokeCenterSlot = dataDictionary["Slot"].As<PokeCenterSlot>();
		PokeCenter.Instance.RemovePokemon(pokeCenterSlot.Pokemon);

		pokeCenterSlot.QueueFree();
    }

	private void ClearTeam()
	{
		List<Pokemon> pokemonToRemove = [.. PokemonTeam.Instance.Pokemon];
		
		foreach (Pokemon pokemon in pokemonToRemove)
		{
			PokeCenter.Instance.AddPokemon(pokemon);
		}
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
		PrintRich.PrintTeam(TextColor.Yellow);
	}
}
