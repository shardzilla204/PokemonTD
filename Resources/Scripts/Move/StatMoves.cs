using Godot;
using System.Collections.Generic;

namespace PokemonTD;


public partial class StatMoves : Node
{
    private List<StatMove> _statIncreaseMoves = new List<StatMove>()
    {
        new StatMove("Defense Curl", PokemonStat.Defense, true),
        new StatMove("Double Team", PokemonStat.Evasion, true),
        new StatMove("Growth", PokemonStat.Attack, true),
        new StatMove("Growth", PokemonStat.SpecialAttack, true),
        new StatMove("Harden", PokemonStat.Defense, true),
        new StatMove("Minimize", PokemonStat.Evasion, true, true),
        new StatMove("Rage", PokemonStat.Attack, true),
        new StatMove("Sharpen", PokemonStat.Attack, true),
        new StatMove("Skull Bash", PokemonStat.Defense, true),
        new StatMove("Swords Dance", PokemonStat.Attack, true, true),
        new StatMove("Acid Armor", PokemonStat.Defense, true, true),
        new StatMove("Agility", PokemonStat.Speed, true, true),
        new StatMove("Amnesia", PokemonStat.SpecialDefense, true, true),
        new StatMove("Barrier", PokemonStat.Defense, true, true),
        new StatMove("Withdraw", PokemonStat.Defense, true)
    };

    private List<StatMove> _statDecreaseMoves = new List<StatMove>()
    {
        new StatMove("String Shot", PokemonStat.Speed, false, true),
        new StatMove("Sand Attack", PokemonStat.Accuracy, false),
        new StatMove("Aurora Beam", PokemonStat.Attack, false, 85),
        new StatMove("Constrict", PokemonStat.Speed, false, 85),
        new StatMove("Flash", PokemonStat.Accuracy, false),
        new StatMove("Growl", PokemonStat.Attack, false),
        new StatMove("Leer", PokemonStat.Defense, false),
        new StatMove("Screech", PokemonStat.Defense, false, true),
        new StatMove("Smokescreen", PokemonStat.Accuracy, false),
        new StatMove("Tail Whip", PokemonStat.Defense, false),
        new StatMove("Acid", PokemonStat.Defense, false, 85),
        new StatMove("Kinesis", PokemonStat.Defense, false),
        new StatMove("Psychic", PokemonStat.SpecialDefense, false),
        new StatMove("Bubble", PokemonStat.Speed, false, 85),
        new StatMove("Bubble Beam", PokemonStat.Speed, false, 85)
    };

    public List<StatMove> FindIncreaseStatMoves(PokemonMove pokemonMove)
    {
        return _statIncreaseMoves.FindAll(statIncreaseMove => statIncreaseMove.PokemonMoveName == pokemonMove.Name);
    }

    public List<StatMove> FindDecreaseStatMoves(PokemonMove pokemonMove)
    {
        return _statDecreaseMoves.FindAll(statDecreaseMove => statDecreaseMove.PokemonMoveName == pokemonMove.Name);
    }

    private bool HasIncreasingStatChanges(PokemonMove pokemonMove)
    {
        List<StatMove> statIncreaseMoves = FindIncreaseStatMoves(pokemonMove);
        return statIncreaseMoves.Count > 0;
    }

    private bool HasDecreasingStatChanges(PokemonMove pokemonMove)
    {
        List<StatMove> statDecreaseMoves = FindDecreaseStatMoves(pokemonMove);
        return statDecreaseMoves.Count > 0;
    }

    public void CheckStatChanges<Defending>(Defending defendingPokemon, PokemonMove pokemonMove)
    {
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            Pokemon pokemon = pokemonStageSlot.Pokemon;
            ApplyStatChanges(pokemon, pokemonMove);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            Pokemon pokemon = pokemonEnemy.Pokemon;
            ApplyStatChanges(pokemon, pokemonMove);
        }
    }

    private void ApplyStatChanges(Pokemon pokemon, PokemonMove pokemonMove)
    {
        if (HasIncreasingStatChanges(pokemonMove))
        {
            List<StatMove> statIncreaseMoves = PokemonMoveEffect.Instance.StatMoves.FindIncreaseStatMoves(pokemonMove);
            PokemonCombat.Instance.ApplyStatChanges(pokemon, statIncreaseMoves);
        }
        else if (HasDecreasingStatChanges(pokemonMove))
        {
            List<StatMove> statDecreaseMoves = PokemonMoveEffect.Instance.StatMoves.FindDecreaseStatMoves(pokemonMove);
            PokemonCombat.Instance.ApplyStatChanges(pokemon, statDecreaseMoves);
        }
    }
}