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

    public void ApplyChargeMove(GodotObject attacking, PokemonMove pokemonMove, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetAttackingPokemon(attacking);
        PokemonEffects attackingPokemonEffects = PokemonCombat.Instance.GetAttackingPokemonEffects(attacking);

        if (attackingPokemonEffects.IsCharging) return;
        
        bool isCharging = false;
        bool IsHyperBeam = PokemonMoveEffect.Instance.ChargeMoves.IsChargeMove(pokemonMove).IsHyperBeam;
        if (IsHyperBeam && !isCharging)
        {
            isCharging = true;
            PokemonCombat.Instance.DealDamage(attacking, pokemonMove, defending);
        }
        else if (!IsHyperBeam)
        {
            isCharging = true;
        }

        attackingPokemonEffects.IsCharging = isCharging;

        if (!isCharging) return;

        string chargingMessage = $"{attackingPokemon.Name} Is Charging";
        PrintRich.PrintLine(TextColor.Purple, chargingMessage);
    }

    public void HasUsedDig(GodotObject attacking, PokemonMove pokemonMove)
    {
        PokemonEffects attackingPokemonEffects = PokemonCombat.Instance.GetAttackingPokemonEffects(attacking);
        bool usedDig = attackingPokemonEffects.IsCharging && pokemonMove.Name == "Dig";
        attackingPokemonEffects.UsedDig = usedDig;
    }
}