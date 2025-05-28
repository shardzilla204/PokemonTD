using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonTeamSlots : Container
{
    private List<PokemonTeamSlot> _pokemonTeamSlots = new List<PokemonTeamSlot>();

    public override void _ExitTree()
    {
        PokemonTD.Signals.EvolutionFinished -= UpdatePokemonTeamSlot;
		PokemonTD.Signals.PokemonTransformed -= UpdatePokemonTeamSlot;
        PokemonTD.Signals.PokemonTeamUpdated -= PokemonTeamUpdated;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.EvolutionFinished += UpdatePokemonTeamSlot;
		PokemonTD.Signals.PokemonTransformed += UpdatePokemonTeamSlot;
        PokemonTD.Signals.PokemonTeamUpdated += PokemonTeamUpdated;

        ClearPokemonTeamSlots();
		AddPokemonTeamSlots();
		AddEmptyPokemonTeamSlots();
    }

    private void ClearPokemonTeamSlots()
	{
		_pokemonTeamSlots.Clear();
		foreach (Node pokemonTeamSlot in GetChildren())
		{
			pokemonTeamSlot.QueueFree();
		}
	}

	private void RemoveEmptyPokemonTeamSlot(int index)
	{
		Control emptyPokemonTeamSlot = GetChildOrNull<Control>(index);
		emptyPokemonTeamSlot.QueueFree();
	}

	private void AddPokemonTeamSlots()
	{
		for (int i = 0; i < PokemonTeam.Instance.Pokemon.Count; i++)
		{
			PokemonTeamSlot pokemonTeamSlot = GetPokemonTeamSlot(i);
			AddPokemonTeamSlot(pokemonTeamSlot, i);
		}
	}

	private void AddPokemonTeamSlot(PokemonTeamSlot pokemonTeamSlot, int pokemonTeamIndex)
	{
		AddChild(pokemonTeamSlot);
		MoveChild(pokemonTeamSlot, pokemonTeamIndex);
		_pokemonTeamSlots.Insert(pokemonTeamIndex, pokemonTeamSlot);
	}

	public void UpdatePokemonTeamSlot(Pokemon pokemon, int pokemonTeamIndex)
	{
		PokemonTeamSlot newPokemonTeamSlot = FindPokemonTeamSlot(pokemonTeamIndex);
		newPokemonTeamSlot.SetControls(pokemon);
	}

	private PokemonTeamSlot GetPokemonTeamSlot(int pokemonTeamIndex)
	{
		PokemonTeamSlot pokemonTeamSlot = PokemonTD.PackedScenes.GetPokemonTeamSlot();
		pokemonTeamSlot.PokemonTeamIndex = pokemonTeamIndex;
		
		Pokemon pokemon = PokemonTeam.Instance.Pokemon[pokemonTeamIndex];
		pokemonTeamSlot.SetControls(pokemon);

		return pokemonTeamSlot;
	}

	// Fill the rest of the slots with an empty slot state
	private void AddEmptyPokemonTeamSlots()
	{
		int emptyTeamSlots = PokemonTD.MaxTeamSize - PokemonTeam.Instance.Pokemon.Count;
		for (int i = 0; i < emptyTeamSlots; i++)
		{
			Control emptyPokemonTeamSlot = PokemonTD.PackedScenes.GetEmptyPokemonTeamSlot();		
			AddChild(emptyPokemonTeamSlot);
		}
	}

    public PokemonTeamSlot FindPokemonTeamSlot(int pokemonTeamIndex)
	{
		return _pokemonTeamSlots.Find(PokemonTeamSlot => PokemonTeamSlot.PokemonTeamIndex == pokemonTeamIndex);
	}

    private void PokemonTeamUpdated()
	{
		int iteration = PokemonTeam.Instance.Pokemon.Count - 1;
		RemoveEmptyPokemonTeamSlot(iteration);

		PokemonTeamSlot pokemonTeamSlot = GetPokemonTeamSlot(iteration);
		AddPokemonTeamSlot(pokemonTeamSlot, iteration);
	}
}
