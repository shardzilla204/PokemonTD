using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class OneHitMoves : Node
{
    private List<string> _oneHitMoveNames = new List<string>()
    {
        "Fissure",
        "Guillotine",
        "Horn Drill"
    };

    public bool IsOneHitKOMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _oneHitMoveNames.Find(pokemonMoveName => pokemonMoveName == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public void ApplyOneHitKO(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetDefendingPokemon(defending);
       
        int damage = defendingPokemon.HP;
        PokemonCombat.Instance.DealDamage(defending, damage);
    }
}