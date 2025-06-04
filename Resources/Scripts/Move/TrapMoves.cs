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
    
    public void ApplyTrapMove(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);
        float percentage = .125f; // 1/8

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomIterationCount = RNG.RandiRange(4, 5);
        int damage = PokemonCombat.Instance.GetDamage(defendingPokemon, percentage);
        PokemonCombat.Instance.DamagePokemonOverTime(defending, damage, randomIterationCount, StatusCondition.None);

        string trappedMessage = $"{defendingPokemon.Name} Is Trapped";
        PrintRich.PrintLine(TextColor.Yellow, trappedMessage);
    }
}