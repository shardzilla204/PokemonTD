using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokeMartTeamSlots : Container
{
    private List<PokeMartTeamSlot> _pokeMartTeamSlots = new List<PokeMartTeamSlot>();

    public override void _ExitTree()
    {
        PokemonTD.Signals.PokemonEvolved -= PokemonEvolved;
        PokemonTD.Signals.PokemonLeveledUp -= PokemonLeveledUp;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.PokemonEvolved += PokemonEvolved;
        PokemonTD.Signals.PokemonLeveledUp += PokemonLeveledUp;

        foreach (PokeMartTeamSlot pokeMartTeamSlot in GetChildren())
        {
            _pokeMartTeamSlots.Add(pokeMartTeamSlot);
        }

        ClearSlots();
        FillSlots();
    }

    private void ClearSlots()
    {
        foreach (PokeMartTeamSlot pokeMartTeamSlot in _pokeMartTeamSlots)
        {
            pokeMartTeamSlot.SetPokemon(null);
        }
    }

    private void FillSlots()
    {
        for (int i = 0; i < PokemonTeam.Instance.Pokemon.Count; i++)
        {
            Pokemon pokemon = PokemonTeam.Instance.Pokemon[i];
            PokeMartTeamSlot pokeMartTeamSlot = _pokeMartTeamSlots[i];
            pokeMartTeamSlot.SetPokemon(pokemon);
        }
    }

    private void PokemonLeveledUp(int levels, int pokemonTeamIndex)
    {
        ClearSlots();
        FillSlots();
    }

    private void PokemonEvolved(Pokemon pokemonEvolution, int pokemonTeamIndex)
    {
        ClearSlots();
        FillSlots();
    }
}
