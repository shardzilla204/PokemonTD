using Godot;
using System;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonStats : Node
{
    private static PokemonStats _instance;

    public static PokemonStats Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

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

    public override void _EnterTree()
    {
        Instance = this;
    }

    public List<StatMove> FindIncreasingStatMoves(PokemonMove pokemonMove)
    {
        return _statIncreaseMoves.FindAll(statIncreaseMove => statIncreaseMove.PokemonMoveName == pokemonMove.Name);
    }

    public StatMove FindIncreasingStatMove(string pokemonMoveName)
    {
         return _statIncreaseMoves.Find(statIncreaseMove => statIncreaseMove.PokemonMoveName == pokemonMoveName);
    }

    public List<StatMove> FindDecreasingStatMoves(PokemonMove pokemonMove)
    {
        return _statDecreaseMoves.FindAll(statDecreaseMove => statDecreaseMove.PokemonMoveName == pokemonMove.Name);
    }

    public bool HasIncreasingStatChanges(PokemonMove pokemonMove)
    {
        List<StatMove> statIncreasingMoves = FindIncreasingStatMoves(pokemonMove);
        return statIncreasingMoves.Count > 0;
    }

    public bool HasAnyIncreasingStatChanges(List<PokemonMove> pokemonMoves)
    {
        foreach (PokemonMove pokemonMove in pokemonMoves)
        {
            if (HasIncreasingStatChanges(pokemonMove)) return true;
        }
        return false;
    }

    public bool HasDecreasingStatChanges(PokemonMove pokemonMove)
    {
        List<StatMove> statDecreasingMoves = FindDecreasingStatMoves(pokemonMove);
        return statDecreasingMoves.Count > 0;
    }

    public void CheckStatChanges<Defending>(Defending defendingPokemon, PokemonMove pokemonMove)
    {
        Pokemon pokemon = null;
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemon = pokemonStageSlot.Pokemon;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemon = pokemonEnemy.Pokemon;
        }
        DecreaseStats(pokemon, pokemonMove);
    }

    public void IncreaseStats(Pokemon pokemon, List<StatMove> statMoves)
    {
        foreach (StatMove statMove in statMoves)
        {
            if (statMove.Name == "Rage") continue;

            PokemonMoveEffect.Instance.ApplyStatChange(pokemon, statMove);
        }
    }

    // Only apply if it doesn't have the change
    public void DecreaseStats(Pokemon defendingPokemon, PokemonMove pokemonMove)
    {
        PokemonMove hasMist = defendingPokemon.Moves.Find(move => move.Name == "Mist");
        if (hasMist != null) return;

        if (!HasDecreasingStatChanges(pokemonMove)) return;

        List<StatMove> statDecreasingMoves = FindDecreasingStatMoves(pokemonMove);
        foreach (StatMove statDecreasingMove in statDecreasingMoves)
        {
            if (!CanApplyStatChange(statDecreasingMove)) continue;
            PokemonMoveEffect.Instance.ChangeStat(defendingPokemon, statDecreasingMove);
        }
    }
    
    private bool CanApplyStatChange(StatMove statMove)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomValue = Mathf.RoundToInt(RNG.RandfRange(0, 255));
        randomValue = randomValue - statMove.Probability;

        return randomValue <= 0;
    }
}