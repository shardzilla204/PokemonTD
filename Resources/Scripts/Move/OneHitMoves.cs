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

    public void ApplyOneHitKO<Defending>(Defending defendingPokemon)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            int damage = pokemonStageSlot.Pokemon.HP;
            PokemonCombat.Instance.DealDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damage = pokemonEnemy.Pokemon.HP;
            PokemonCombat.Instance.DealDamage(pokemonEnemy, damage);
        }
    }
}