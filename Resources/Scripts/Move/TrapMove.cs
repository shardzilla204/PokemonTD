using Godot;
using System.Collections.Generic;

namespace PokemonTD;

// Trapping = Remove 1/8 Health For 4-5 turns
public partial class TrapMoves : Node
{
    private List<string> _trapMoveNames = new List<string>()
    {
        "Fire Spin",
        "Bind",
        "Wrap",
        "Clamp"
    };
    
    public bool IsTrapMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _trapMoveNames.Find(move => move == pokemonMove.Name);
        return pokemonMoveName != null;
    }
}