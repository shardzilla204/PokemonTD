using Godot;
using System;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonStatMoves : Node
{
    private static PokemonStatMoves _instance;

    public static PokemonStatMoves Instance
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

    private List<CustomTimer> _timers = new List<CustomTimer>();

    public override void _EnterTree()
    {
        Instance = this;

        PokemonTD.Signals.HasLeftStage += () =>
        {
            foreach (CustomTimer timer in _timers)
            {
                timer.QueueFree();
            }
        };
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

    public void IncreaseStats(Pokemon pokemon, List<StatMove> statMoves)
    {
        foreach (StatMove statMove in statMoves)
        {
            if (statMove.Name == "Rage") continue;

            ApplyStatChange(pokemon, statMove);
        }
    }

    // Only apply if it doesn't have the change
    public void DecreaseStats(GodotObject defending, PokemonMove pokemonMove)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);

        PokemonMove hasMist = defendingPokemon.Moves.Find(move => move.Name == "Mist");
        if (!HasDecreasingStatChanges(pokemonMove) || hasMist != null) return;

        List<StatMove> statDecreasingMoves = FindDecreasingStatMoves(pokemonMove);
        foreach (StatMove statDecreasingMove in statDecreasingMoves)
        {
            if (!CanApplyStatChange(statDecreasingMove)) continue;
            ChangeStat(defending, statDecreasingMove);
        }
    }

    private bool CanApplyStatChange(StatMove statMove)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomValue = Mathf.RoundToInt(RNG.RandfRange(0, 255));
        randomValue = randomValue - statMove.Probability;

        return randomValue <= 0;
    }

    public void ChangeStat(GodotObject defending, StatMove statMove)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);
        ApplyStatChange(defendingPokemon, statMove);

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.AddStat(statMove.PokemonStat, false);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.AddStat(statMove.PokemonStat, false);
        }

        defendingPokemon.Debuffs.Add(statMove.PokemonStat);

        CustomTimer timer = GetTimer(5);
        timer.Timeout += () =>
        {
            PokemonManager.Instance.SetPokemonStats(defendingPokemon); // Reset Stats
            if (defending is PokemonStageSlot pokemonStageSlot)
            {
                pokemonStageSlot.RemoveStat(statMove.PokemonStat);
            }
            else if (defending is PokemonEnemy pokemonEnemy)
            {
                pokemonEnemy.RemoveStat(statMove.PokemonStat);
            }

            defendingPokemon.Debuffs.Remove(statMove.PokemonStat);
        };

        AddChild(timer);
    }

    private int GetStatValue(Pokemon pokemon, StatMove statMove)
    {
        float statChangePercentage = GetStatChangePercentage(statMove);
        int statValue = PokemonManager.Instance.GetOtherPokemonStat(pokemon, statMove.PokemonStat) / 2;
        return Mathf.RoundToInt(statValue * statChangePercentage);
    }

    public void ApplyStatChange(Pokemon pokemon, StatMove statMove)
    {
        int baseStatValue = PokemonManager.Instance.GetOtherPokemonStat(pokemon, statMove.PokemonStat);
        int statValue = GetStatValue(pokemon, statMove);

        float changeValue = statMove.IsSharp ? 0.6f : 0.25f; // For accuracy and evasion
        switch (statMove.PokemonStat)
        {
            case PokemonStat.Attack:
                pokemon.Stats.Attack = Math.Max(0, baseStatValue + statValue);
                break;
            case PokemonStat.SpecialAttack:
                pokemon.Stats.SpecialAttack = Math.Max(0, baseStatValue + statValue);
                break;
            case PokemonStat.Defense:
                pokemon.Stats.Defense = Math.Max(0, baseStatValue + statValue);
                break;
            case PokemonStat.SpecialDefense:
                pokemon.Stats.SpecialDefense = Math.Max(0, baseStatValue + statValue);
                break;
            case PokemonStat.Speed:
                pokemon.Stats.Speed = Math.Max(0, baseStatValue + statValue);
                break;
            case PokemonStat.Accuracy:
                pokemon.Stats.Accuracy += statMove.IsIncreasing ? changeValue : -changeValue;
                pokemon.Stats.Accuracy = Mathf.Clamp(pokemon.Stats.Accuracy, 0, 2);
                break;
            case PokemonStat.Evasion:
                pokemon.Stats.Evasion += statMove.IsIncreasing ? changeValue : -changeValue;
                pokemon.Stats.Evasion = Mathf.Clamp(pokemon.Stats.Evasion, 0, 2);
                break;
        }

        string statChangeMessage = $"{pokemon.Name}'s {statMove.PokemonStat} Has";
        statChangeMessage += statMove.IsIncreasing ? " Increased" : " Decreased";
        PrintRich.PrintLine(TextColor.Yellow, statChangeMessage);
    }

    private float GetStatChangePercentage(StatMove statMove)
    {
        float statChangePercentage = statMove.IsIncreasing ? 1.1f : -1.1f;
        if (statMove.IsSharp)
        {
            float sharpMultiplier = 1.35f;
            statChangePercentage *= sharpMultiplier;
        }

        return statChangePercentage;
    }

    private CustomTimer GetTimer(float waitTime)
    {
        CustomTimer timer = new CustomTimer(waitTime);

        timer.TreeEntered += () => _timers.Add(timer);
        timer.TreeExiting += () => _timers.Remove(timer);
        
        return timer;
    }

    private void StartTimer(CustomTimer timer)
    {
        try
        {
            timer.Start(timer.WaitTimeLeft);
        }
        catch (ObjectDisposedException) {}
    }

    private void StartTimers()
    {
        foreach (CustomTimer timer in _timers)
        {
            StartTimer(timer);
        }
    }

    private void StopTimers()
    {
        foreach (CustomTimer timer in _timers)
        {
            timer.Stop();
        }
    }

    private async void Dragging(bool isDragging)
    {
        if (isDragging)
        {
            StopTimers();
        }
        else
        {
            if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, PokemonSignals.SignalName.PressedPlay);
            StartTimers();
        }
    }

}