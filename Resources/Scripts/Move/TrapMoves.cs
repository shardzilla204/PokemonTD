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
        string pokemonMoveName = _trapMoveNames.Find(pokemonMoveName => pokemonMoveName == pokemonMove.Name);
        return pokemonMoveName != null;
    }
    
    public void ApplyTrapMove<Defending>(Defending defendingPokemon)
    {
        float percentage = .125f; // 1/8

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomIterationCount = RNG.RandiRange(4, 5);

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            int damageAmount = PokemonCombat.Instance.GetDamageAmount(pokemonStageSlot.Pokemon, percentage);
            PokemonCombat.Instance.DamagePokemonOverTime(pokemonStageSlot, damageAmount, randomIterationCount, StatusCondition.None);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damageAmount = PokemonCombat.Instance.GetDamageAmount(pokemonEnemy.Pokemon, percentage);
            PokemonCombat.Instance.DamagePokemonOverTime(pokemonEnemy, damageAmount, randomIterationCount, StatusCondition.None);
        }
    }
}