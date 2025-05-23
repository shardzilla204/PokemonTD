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
    
    public void ApplyTrapMove<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        float percentage = .125f; // 1/8

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomIterationCount = RNG.RandiRange(4, 5);

        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            int damageAmount = PokemonCombat.Instance.GetDamageAmount(pokemonEnemy.Pokemon, percentage);
            PokemonCombat.Instance.DamagePokemonOverTime(pokemonStageSlot, pokemonEnemy, damageAmount, randomIterationCount, StatusCondition.None);

            string trappedMessage = $"{pokemonEnemy.Pokemon.Name} Is Trapped";
            PrintRich.PrintLine(TextColor.Yellow, trappedMessage);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

            int damageAmount = PokemonCombat.Instance.GetDamageAmount(pokemonStageSlot.Pokemon, percentage);
            PokemonCombat.Instance.DamagePokemonOverTime(pokemonEnemy, pokemonStageSlot, damageAmount, randomIterationCount, StatusCondition.None);

            string trappedMessage = $"{pokemonStageSlot.Pokemon.Name} Is Trapped";
            PrintRich.PrintLine(TextColor.Red, trappedMessage);
        }
    }
}