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
    public bool IsChargeMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _chargeMoveNames.Find(pokemonMoveName => pokemonMoveName == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public void ApplyChargeMove(GodotObject attacking)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetAttackingPokemon(attacking);
        PokemonEffects attackingPokemonEffects = PokemonCombat.Instance.GetAttackingPokemonEffects(attacking);

        attackingPokemonEffects.IsCharging = !attackingPokemonEffects.IsCharging;
        if (attackingPokemonEffects.HasHyperBeam) attackingPokemonEffects.IsCharging = !attackingPokemonEffects.IsCharging;

        if (attackingPokemonEffects.IsCharging)
        {
            string chargingMessage = $"{attackingPokemon.Name} Is Charging";
            PrintRich.PrintLine(TextColor.Purple, chargingMessage);
        }
        else
        {
            string dischargingMessage = $"{attackingPokemon.Name} Is Discharging";
            PrintRich.PrintLine(TextColor.Purple, dischargingMessage);
        }
    }

    public void HasUsedDig(GodotObject attacking, PokemonMove pokemonMove)
    {
        PokemonEffects attackingPokemonEffects = PokemonCombat.Instance.GetAttackingPokemonEffects(attacking);
        
        bool usedDig = attackingPokemonEffects.IsCharging && pokemonMove.Name == "Dig";
        attackingPokemonEffects.UsedDig = usedDig;
    }
}