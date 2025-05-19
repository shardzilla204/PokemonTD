using Godot;
using System.Collections.Generic;

namespace PokemonTD;

// Skip Oppenents Next Move
public partial class FlinchMoves : Node
{
    private List<string> _flinchMoveNames = new List<string>()
    {
        "Bite",
        "Rolling Kick",
        "Sky Attack",
        "Bone Club",
        "Stomp",
        "Headbutt",
        "Rock Slide",
        "Hyper Fang",
        "Waterfall"
    };

    public bool IsFlinchMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _flinchMoveNames.Find(pokemonMoveName => pokemonMoveName == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public void ApplyFlinchMove<Defending>(Defending defendingPokemon)
    {
        if (!CanApplyFlinchMove()) return;
        
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.HasMoveSkipped = true;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.HasMoveSkipped = true;
        }
    }

    private bool CanApplyFlinchMove()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float changeValue = 0.25f;
        float randomThreshold = RNG.RandfRange(0, 1);
        randomThreshold -= changeValue;
        return randomThreshold <= 0;
    }
}
