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
        // Check if it already is charging
        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            if (pokemonStageSlot.Effects.IsCharging) return;
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            if (pokemonEnemy.Effects.IsCharging) return;
        }
        
        bool isCharging = false;
        bool IsHyperBeam = PokemonMoveEffect.Instance.ChargeMoves.IsChargeMove(pokemonMove).IsHyperBeam;
        if (IsHyperBeam && !isCharging)
        {
            isCharging = true;
            PokemonCombat.Instance.DealDamage(attackingPokemon, pokemonMove, defendingPokemon);
        }
        else if (!IsHyperBeam)
        {
            isCharging = true;
        }

        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            pokemonStageSlot.Effects.IsCharging = isCharging;

            if (!isCharging) return;

            string chargingMessage = $"{pokemonStageSlot.Pokemon.Name} Is Charging";
            PrintRich.PrintLine(TextColor.Purple, chargingMessage);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            pokemonEnemy.Effects.IsCharging = isCharging;

            if (!isCharging) return;

            string chargingMessage = $"{pokemonEnemy.Pokemon.Name} Is Charging";
            PrintRich.PrintLine(TextColor.Red, chargingMessage);
        }
    }

    public void HasUsedDig<Attacking>(Attacking attackingPokemon, PokemonMove pokemonMove)
    {
        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            bool usedDig = pokemonStageSlot.Effects.IsCharging && pokemonMove.Name == "Dig";
            pokemonStageSlot.Effects.UsedDig = usedDig;
        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            bool usedDig = pokemonEnemy.Effects.IsCharging && pokemonMove.Name == "Dig";
            pokemonEnemy.Effects.UsedDig = usedDig;
        }
    }
}