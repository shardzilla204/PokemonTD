using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class InflictingMoves : Node
{
    private List<string> _recoilDamageMoveNames = new List<string>()
    {
        "Double-Edge",
        "Submission",
        "Take Down",
    };

    private List<string> _faintMoves = new List<string>()
    {
        "Self-Destruct",
        "Explosion"
    };

    public bool IsRecoilDamageMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _recoilDamageMoveNames.Find(moveName => moveName == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public bool IsFaintMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _faintMoves.Find(moveName => moveName == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public void ApplyRecoilDamage(GodotObject attacking)
    {
        float recoilPercentage = 0.05f;
        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);
        Pokemon attackingPokemonData = PokemonManager.Instance.GetPokemon(attackingPokemon.Name, attackingPokemon.Level);

        int damage = Mathf.RoundToInt(attackingPokemonData.Stats.HP * recoilPercentage);
        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.DamagePokemon(damage);
        }
        else if (attacking is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.DamagePokemon(damage);
        }

        string recoilMessage = $"{attackingPokemonData.Name} Took {damage} Recoil Damage";
        PrintRich.PrintLine(TextColor.Yellow, recoilMessage);
    }

    public void ApplyFaint(GodotObject attacking)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);
        int damage = Mathf.RoundToInt(attackingPokemon.Stats.HP);

        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.DamagePokemon(damage);
            pokemonStageSlot.Pokemon.Effects.UsedFaintMove = true;
        }
        else if (attacking is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.DamagePokemon(damage);
        }
    }
}
