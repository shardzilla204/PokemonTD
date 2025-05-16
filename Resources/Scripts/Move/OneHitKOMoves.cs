using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class OneHitKOMoves : Node
{
    private List<string> _oneHitKOMoveNames = new List<string>()
    {
        "Fissure",
        "Guillotine",
        "Horn Drill"
    };

    public bool IsOneHitKOMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _oneHitKOMoveNames.Find(move => move == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public void ApplyOneHitKO<Defending>(Defending defendingPokemon)
    {
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            int damage = pokemonStageSlot.Pokemon.HP;
            PokemonCombat.Instance.ApplyDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damage = pokemonEnemy.Pokemon.HP;
            PokemonCombat.Instance.ApplyDamage(pokemonEnemy, damage);
        }
    }
}