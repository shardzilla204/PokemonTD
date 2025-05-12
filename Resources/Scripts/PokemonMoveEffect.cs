using Godot;
using System;
using System.Collections.Generic;

namespace PokemonTD;

// TODO: Add move effect that forces a charge
// TODO: 

public partial class PokemonMoveEffect : Node
{
    private static PokemonMoveEffect _instance;

    public static PokemonMoveEffect Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

    public UniqueMoves UniqueMoves = new UniqueMoves();
    public HighCriticalRatioMoves HighCriticalRatioMoves = new HighCriticalRatioMoves(); // Done
    public TrapMoves TrapMoves = new TrapMoves();
    public FlinchMoves FlinchMoves = new FlinchMoves();
    public OneHitKOMoves OneHitKOMoves = new OneHitKOMoves();
    public ChargeMoves ChargeMoves = new ChargeMoves();
    public StatMoves StatMoves = new StatMoves(); // Done

    public override void _EnterTree()
    {
        Instance = this;
    }

    public int GetRandomHitCount(PokemonMove pokemonMove)
    {
        int minimumHitCount = pokemonMove.HitCount[0];
        int maximumHitCount = pokemonMove.HitCount[1];
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        
        return RNG.RandiRange(minimumHitCount, maximumHitCount);
    }

    public float GetCriticalHitRatio(Pokemon pokemon, PokemonMove pokemonMove)
    {
        float criticalHitRatio = pokemon.Speed / 2;
        bool isHighCriticalRatioMove = HighCriticalRatioMoves.IsHighCriticalRatioMove(pokemonMove);
        if (isHighCriticalRatioMove)
        {
            float maxThreshold = 255;
            criticalHitRatio = Math.Min(8 * criticalHitRatio, maxThreshold);
        }
        return criticalHitRatio;
    }

    public void ChangeStat(Pokemon pokemon, StatMove statMove)
    {
        float changePercentage = statMove.IsSharp ? 1.5f : 1.2f;
        changePercentage = !statMove.IsIncreasing ? changePercentage - 1 : changePercentage;
        
        int statValue = GetStatValue(pokemon, statMove.PokemonStat);
        statValue = Mathf.RoundToInt(statValue * changePercentage);

        if (statMove.PokemonStat == PokemonStat.Attack)
        {
            pokemon.Attack = statValue;
        }
        else if (statMove.PokemonStat == PokemonStat.SpecialAttack)
        {
            pokemon.SpecialAttack = statValue;
        }
        else if (statMove.PokemonStat == PokemonStat.Defense)
        {
            pokemon.Defense = statValue;
        }
        else if (statMove.PokemonStat == PokemonStat.SpecialDefense)
        {
            pokemon.SpecialDefense = statValue;
        }
        else if (statMove.PokemonStat == PokemonStat.Speed)
        {
            pokemon.Speed = statValue;
        }
        else if (statMove.PokemonStat == PokemonStat.Accuracy)
        {
            pokemon.Accuracy = statMove.IsIncreasing ? 1.5f : 0.5f;
            pokemon.Accuracy = statMove.IsSharp ? 2f : pokemon.Accuracy;
        }
        else if (statMove.PokemonStat == PokemonStat.Evasion)
        {
            pokemon.Evasion = statMove.IsIncreasing ? 1.5f : 0.5f;
            pokemon.Evasion = statMove.IsSharp ? 2f : pokemon.Evasion;
        }

        Timer timer = GetTimer(2f);
        timer.Timeout += () => PokemonManager.Instance.SetPokemonStats(pokemon); // Resets Stats
        AddChild(timer);
    }

    private Timer GetTimer(float waitTime)
    {
        Timer timer = new Timer()
        {
            WaitTime = waitTime,
            Autostart = true
        };
        timer.Timeout += timer.QueueFree;
        return timer;
    }

    private int GetStatValue(Pokemon pokemon, PokemonStat pokemonStat)
    {
        return pokemonStat switch 
        {
            PokemonStat.Attack => pokemon.Attack,
            PokemonStat.Defense => pokemon.Defense,
            PokemonStat.Speed => pokemon.Speed,
            _ => pokemon.Speed
        };
    }
}

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
        new StatMove("Aurora Beam", PokemonStat.Attack, false, 0.25f),
        new StatMove("Constrict", PokemonStat.Speed, false, 0.25f),
        new StatMove("Flash", PokemonStat.Accuracy, false),
        new StatMove("Growl", PokemonStat.Attack, false),
        new StatMove("Leer", PokemonStat.Defense, false),
        new StatMove("Screech", PokemonStat.Defense, false, true),
        new StatMove("Smokescreen", PokemonStat.Accuracy, false),
        new StatMove("Tail Whip", PokemonStat.Defense, false),
        new StatMove("Acid", PokemonStat.Defense, false, 0.25f),
        new StatMove("Kinesis", PokemonStat.Defense, false),
        new StatMove("Psychic", PokemonStat.SpecialDefense, false),
        new StatMove("Bubble", PokemonStat.Speed, false, 0.25f),
        new StatMove("Bubble Beam", PokemonStat.Speed, false, 0.25f),
    };

    public List<StatMove> FindIncreaseStatMoves(PokemonMove pokemonMove)
    {
        return _statIncreaseMoves.FindAll(statIncreaseMove => statIncreaseMove.PokemonMoveName == pokemonMove.Name);
    }

    public List<StatMove> FindDecreaseStatMoves(PokemonMove pokemonMove)
    {
        return _statDecreaseMoves.FindAll(statDecreaseMove => statDecreaseMove.PokemonMoveName == pokemonMove.Name);
    }
}

public partial class StatMove : Node
{
    public StatMove(string pokemonMoveName, PokemonStat pokemonStat, bool isIncreasing)
    {
        PokemonMoveName = pokemonMoveName;
        PokemonStat = pokemonStat;
        IsIncreasing = isIncreasing;
        IsSharp = false;
    }

    public StatMove(string pokemonMoveName, PokemonStat pokemonStat, bool isIncreasing, bool isSharp)
    {
        PokemonMoveName = pokemonMoveName;
        PokemonStat = pokemonStat;
        IsIncreasing = isIncreasing;
        IsSharp = isSharp;
    }

    public StatMove(string pokemonMoveName, PokemonStat pokemonStat, bool isIncreasing, float probability)
    {
        PokemonMoveName = pokemonMoveName;
        PokemonStat = pokemonStat;
        IsIncreasing = isIncreasing;
        Probability = probability;
    }

    public string PokemonMoveName;
    public PokemonStat PokemonStat;
    public bool IsIncreasing;
    public bool IsSharp = false;
    public float Probability = 1;
}

public partial class HighCriticalRatioMoves : Node
{
    private List<string> _highCriticalRatioMoveNames = new List<string>()
    {
        "Razor Wind",
        "Sky Attack",
        "Karate Chop"
    };

    public bool IsHighCriticalRatioMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _highCriticalRatioMoveNames.Find(move => move == pokemonMove.Name);
        return pokemonMoveName != null;
    }
}

public partial class UniqueMoves : Node
{
    private List<string> _uniqueMoveNames = new List<string>()
    {
        "Dragon Rage",
        "Double Kick",
        "Low Kick",
        "Seismic Toss",
        "Mirror Move",
        "Night Shade",
        "Metronome",
        "Mimic",
        "Pay Day",
        "Skull Bash",
        "Sonic Boom",
        "Super Fang",
        "Swift",
        "Psywave",
        "Surf"
    };

    public bool IsUniqueMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _uniqueMoveNames.Find(move => move == pokemonMove.Name);
        return pokemonMoveName != null;
    }
}

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
        string pokemonMoveName = _trapMoveNames.Find(move => move == pokemonMove.Name);
        return pokemonMoveName != null;
    }
}

public partial class ChargeMoves : Node
{
    private List<string> _chargeMoveNames = new List<string>()
    {
        "Sky Attack",
        "Solar Beam",
        "Dig",
        "Hyper Beam",
        "Razor Wind"
    };

    // (isChargeMove, isHyperBeam)
    // Hyper Beam attacks first then charges afterward
    public (bool, bool) IsChargeMove(PokemonMove pokemonMove)
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
}

public partial class FlinchMoves : Node
{
    private List<string> _flinchMoveNames = new List<string>()
    {
        "Bite",
        "Rolling Kick",
        "Sky Attack",
        "Bone Club",
        "Stomp",
        "Rock Slide",
        "Waterfall"
    };

    public bool IsFlinchMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _flinchMoveNames.Find(move => move == pokemonMove.Name);
        return pokemonMoveName != null;
    }
}

public partial class OneHitKOMoves : Node
{
    private List<string> _oneHitKOMoveNames = new List<string>()
    {
        "Fissure",
        "Guillotine",
        "Horn Drill"
    };

    public bool IsOneHitKOMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _oneHitKOMoveNames.Find(move => move == pokemonMove.Name);
        return pokemonMoveName != null;
    }
}