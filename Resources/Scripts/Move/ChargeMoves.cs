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
        "Skull Bash"
    };

    // Hyper Beam attacks first then charges afterward
    public (bool IsChargeMove, bool IsHyperBeam) IsChargeMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _chargeMoveNames.Find(move => move == pokemonMove.Name);
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

    public bool ApplyChargeMove<Defending>(bool isCharging, PokemonMove pokemonMove, Defending defendingPokemon)
    {
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
            PokemonCombat.Instance.ApplyDamage(this, pokemonMove, defendingPokemon);
        }
        return isCharging;
    }

    public bool HasUsedDig(bool isCharging, PokemonMove pokemonMove)
    {
        return isCharging && pokemonMove.Name == "Dig";
    }
}