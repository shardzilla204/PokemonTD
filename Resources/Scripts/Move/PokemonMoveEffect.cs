using Godot;
using System;

namespace PokemonTD;

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
        Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemon.Name);
        float criticalHitRatio = pokemonData.Speed / 2;

        bool isHighCriticalRatioMove = HighCriticalRatioMoves.IsHighCriticalRatioMove(pokemonMove);
        if (isHighCriticalRatioMove)
        {
            float maxThreshold = 255;
            criticalHitRatio = Math.Min(4 * criticalHitRatio, maxThreshold);
        }
        return criticalHitRatio;
    }

    public void ChangeStat(Pokemon pokemon, StatMove statMove)
    {
        ApplyStatChange(pokemon, statMove);

        Timer timer = GetTimer(1.5f);
        timer.Timeout += () => PokemonManager.Instance.SetPokemonStats(pokemon); // Resets Stats
        AddChild(timer);
    }

    private int GetStatValue(Pokemon pokemon, StatMove statMove)
    {
        float statChangePercentage = GetStatChangePercentage(statMove);
        int statValue = PokemonManager.Instance.GetOtherPokemonStat(pokemon, statMove.PokemonStat);
        return Mathf.RoundToInt(statValue * statChangePercentage);
    }

    private void ApplyStatChange(Pokemon pokemon, StatMove statMove)
    {
        int statValue = GetStatValue(pokemon, statMove);
        switch (statMove.PokemonStat)
        {
            case PokemonStat.Attack:
                pokemon.Attack = statValue;
                break;
            case PokemonStat.SpecialAttack:
                pokemon.SpecialAttack = statValue;
                break;
            case PokemonStat.Defense:
                pokemon.Defense = statValue;
                break;
            case PokemonStat.SpecialDefense:
                pokemon.SpecialDefense = statValue;
                break;
            case PokemonStat.Speed:
                pokemon.Speed = statValue;
                break;
            case PokemonStat.Accuracy:
                if (statMove.IsIncreasing)
                {
                    pokemon.Accuracy += statMove.IsSharp ? 1 : 0.5f;
                }
                else
                {
                    pokemon.Accuracy -= statMove.IsSharp ? 1 : 0.5f;
                    pokemon.Accuracy = Mathf.Clamp(pokemon.Accuracy, 0.5f, 2);
                }
                break;
            case PokemonStat.Evasion:
                if (statMove.IsIncreasing)
                {
                    pokemon.Evasion += statMove.IsSharp ? 1 : 0.5f;
                }
                else
                {
                    pokemon.Evasion -= statMove.IsSharp ? 1 : 0.5f;
                    pokemon.Evasion = Mathf.Clamp(pokemon.Accuracy, 0.5f, 2);
                }
                break;
        }
    }

    private float GetStatChangePercentage(StatMove statMove)
    {
        float statChangePercentage = statMove.IsIncreasing ? 1.5f : 0.5f;
        if (statMove.IsSharp)
        {
            float sharpMultiplier = 1.5f;
            if (statMove.IsIncreasing)
            {
                statChangePercentage *= sharpMultiplier;
            }
            else
            {
                statChangePercentage /= sharpMultiplier;
            }
        }

        return statChangePercentage;
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
}