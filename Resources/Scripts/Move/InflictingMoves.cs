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

    public void ApplyRecoilDamage<Attacking>(Attacking attackingPokemon)
    {
        float recoilPercentage = 0.05f;
        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            Pokemon pokemon = pokemonStageSlot.Pokemon;
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemon.Name, pokemon.Level);
            int damage = Mathf.RoundToInt(pokemonData.HP * recoilPercentage);
            pokemonStageSlot.DamagePokemon(damage);

            string recoilMessage = $"{pokemon.Name} Took {damage} Recoil Damage";
            PrintRich.PrintLine(TextColor.Yellow, recoilMessage);
        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            Pokemon pokemon = pokemonEnemy.Pokemon;
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemon.Name, pokemon.Level);
            int damage = Mathf.RoundToInt(pokemonData.HP * recoilPercentage);
            pokemonEnemy.DamagePokemon(damage);

            string recoilMessage = $"{pokemon.Name} Took {damage} Recoil Damage";
            PrintRich.PrintLine(TextColor.Red, recoilMessage);
        }
    }

    public void ApplyFaint<Attacking>(Attacking attackingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            int damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.HP);
            pokemonStageSlot.DamagePokemon(damage);

            pokemonStageSlot.Effects.UsedFaintMove = true;
        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.HP);
            pokemonEnemy.DamagePokemon(damage);
        }
    }
}
