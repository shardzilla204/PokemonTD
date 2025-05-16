using Godot;
using System.Collections.Generic;

namespace PokemonTD;

// Skip Oppenents Next Move
public partial class FlinchMoves : Node
{
    private List<string> _flinchMoveNames = new List<string>()
    {
        "Bite",
        "Rolling Kick",
        "Sky Attack",
        "Bone Club",
        "Stomp",
        "Rock Slide",
        "Waterfall"
    };

    public bool IsFlinchMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _flinchMoveNames.Find(move => move == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public void ApplyFlinchMove<T>(T defendingPokemon)
    {
        PokemonStatusCondition.Instance.FreezePokemon(defendingPokemon, 1f);
    }
}
