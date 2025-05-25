using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class HighCriticalRatioMoves : Node
{
    private List<string> _highCriticalRatioMoveNames = new List<string>()
    {
        "Razor Wind",
        "Sky Attack",
        "Karate Chop",
        "Razor Leaf",
        "Slash",
        "Crabhammer"
    };

    public bool IsHighCriticalRatioMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _highCriticalRatioMoveNames.Find(pokemonMoveName => pokemonMoveName == pokemonMove.Name);
        return pokemonMoveName != null;
    }
}