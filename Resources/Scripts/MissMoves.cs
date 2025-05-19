using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class MissMoves : Node
{
    private List<string> _missMoveNames = new List<string>()
    {
        "High Jump Kick",
        "Jump Kick"
    };

    public bool IsMissMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _missMoveNames.Find(move => move == pokemonMove.Name);
        return pokemonMoveName != null;
    }
}
