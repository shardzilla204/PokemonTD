using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class ChargeMoves : Node
{
    private List<string> _chargeMoveNames = new List<string>()
    {
        "Sky Attack",
        "Solar Beam",
        "Dig",
        "Hyper Beam",
        "Razor Wind",
        "Skull Bash",
        "Fly"
    };

    // Hyper Beam attacks first then charges afterward
    public (bool IsChargeMove, bool IsHyperBeam) IsChargeMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _chargeMoveNames.Find(pokemonMoveName => pokemonMoveName == pokemonMove.Name);
        if (pokemonMoveName == "Hyper Beam")
        {
            return (true, true);
        }
        else if (pokemonMoveName != null)
        {
            return (true, false);
        }
        return (false, false);
    }

    public void ApplyChargeMove<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        bool isCharging = false;
        
        bool IsHyperBeam = PokemonMoveEffect.Instance.ChargeMoves.IsChargeMove(pokemonMove).IsHyperBeam;
        if (IsHyperBeam && !isCharging)
        {
            isCharging = true;
        }
        else if (!IsHyperBeam)
        {
            isCharging = true;
        }
        else if (isCharging)
        {
            isCharging = false;
            PokemonCombat.Instance.DealDamage(this, pokemonMove, defendingPokemon);
        }

        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.IsCharging = isCharging;
        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.IsCharging = isCharging;
        }
    }

    public void HasUsedDig<Attacking>(Attacking attackingPokemon, PokemonMove pokemonMove)
    {
        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            bool usedDig = pokemonStageSlot.IsCharging && pokemonMove.Name == "Dig";
            pokemonStageSlot.UsedDig = usedDig;
        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            bool usedDig = pokemonEnemy.IsCharging && pokemonMove.Name == "Dig";
            pokemonEnemy.UsedDig = usedDig;
        }
    }
}