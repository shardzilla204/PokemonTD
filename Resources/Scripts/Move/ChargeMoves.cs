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
        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);

        attackingPokemon.Effects.IsCharging = !attackingPokemon.Effects.IsCharging;
        if (attackingPokemon.Effects.HasHyperBeam) attackingPokemon.Effects.IsCharging = !attackingPokemon.Effects.IsCharging;

        if (attackingPokemon.Effects.IsCharging)
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
        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);

        bool usedDig = attackingPokemon.Effects.IsCharging && pokemonMove.Name == "Dig";
        attackingPokemon.Effects.UsedDig = usedDig;
    }
}