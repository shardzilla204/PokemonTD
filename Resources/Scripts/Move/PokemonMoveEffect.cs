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
    public TrapMoves TrapMoves = new TrapMoves(); // Done
    public FlinchMoves FlinchMoves = new FlinchMoves(); // Done
    public OneHitMoves OneHitMoves = new OneHitMoves(); // Done
    public ChargeMoves ChargeMoves = new ChargeMoves(); // ? Done
    public MissMoves MissMoves = new MissMoves(); // Done
    public RecoverMoves RecoverMoves = new RecoverMoves(); // Done
    public InflictingMoves InflictingMoves = new InflictingMoves(); // Done

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
        Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemon.Name, pokemon.Level);
        float criticalHitRatio = pokemonData.Speed / 2;

        bool isHighCriticalRatioMove = HighCriticalRatioMoves.IsHighCriticalRatioMove(pokemonMove);
        if (isHighCriticalRatioMove)
        {
            float maximumThreshold = 255;
            criticalHitRatio = Math.Min(4 * criticalHitRatio, maximumThreshold);
        }
        return criticalHitRatio;
    }

    public void DecreaseStat(Pokemon pokemon, StatMove statMove)
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

    public void ApplyStatChange(Pokemon pokemon, StatMove statMove)
    {
        int statValue = GetStatValue(pokemon, statMove);
        float changeValue = statMove.IsSharp ? 1 : 0.5f;
        switch (statMove.PokemonStat)
        {
            case PokemonStat.Attack:
                pokemon.Attack = Mathf.Clamp(statValue, 0, 255);
                break;
            case PokemonStat.SpecialAttack:
                pokemon.SpecialAttack = Mathf.Clamp(statValue, 0, 255);
                break;
            case PokemonStat.Defense:
                pokemon.Defense = Mathf.Clamp(statValue, 0, 255);
                break;
            case PokemonStat.SpecialDefense:
                pokemon.SpecialDefense = Mathf.Clamp(statValue, 0, 255);
                break;
            case PokemonStat.Speed:
                pokemon.Speed = Mathf.Clamp(statValue, 0, 255);
                break;
            case PokemonStat.Accuracy:
                pokemon.Accuracy += statMove.IsIncreasing ? changeValue : -changeValue;
                pokemon.Accuracy = Mathf.Clamp(pokemon.Accuracy, 0.5f, 2);
                break;
            case PokemonStat.Evasion:
                pokemon.Evasion += statMove.IsIncreasing ? changeValue : -changeValue;
                pokemon.Evasion = Mathf.Clamp(pokemon.Accuracy, 0.5f, 2);
                break;
        }
    }

    private float GetStatChangePercentage(StatMove statMove)
    {
        float statChangePercentage = statMove.IsIncreasing ? 1.25f : 0.75f;
        if (statMove.IsSharp)
        {
            float sharpMultiplier = 1.25f;
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

    public void ApplyMoveEffect<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        if (UniqueMoves.IsUniqueMove(pokemonMove))
        {
            UniqueMoves.ApplyUniqueMove(attackingPokemon, pokemonMove, defendingPokemon);
        }
        else if (TrapMoves.IsTrapMove(pokemonMove))
        {
            TrapMoves.ApplyTrapMove(defendingPokemon);
        }
        else if (ChargeMoves.IsChargeMove(pokemonMove).IsChargeMove)
        {
            ChargeMoves.ApplyChargeMove(attackingPokemon, pokemonMove, defendingPokemon);
            ChargeMoves.HasUsedDig(attackingPokemon, pokemonMove);
        }
        else if (FlinchMoves.IsFlinchMove(pokemonMove))
        {
            FlinchMoves.ApplyFlinchMove(defendingPokemon);
        }
        else if (OneHitMoves.IsOneHitKOMove(pokemonMove))
        {
            OneHitMoves.ApplyOneHitKO(defendingPokemon);
        }
        else if (RecoverMoves.IsHealthRecoveryMove(pokemonMove))
        {
            if (pokemonMove.Name == "Leech Seed")
            {
                RecoverMoves.LeechSeed(attackingPokemon, defendingPokemon);
            }
            else
            {
                RecoverMoves.ApplyHealthRecoveryMove(attackingPokemon, pokemonMove, defendingPokemon);
            }
        }
        else if (RecoverMoves.IsRareCandyRecoveryMove(pokemonMove))
        {
            if (attackingPokemon is not PokemonStageSlot pokemonStageSlot) return;

            RecoverMoves.ApplyRareCandyRecoveryMove(pokemonStageSlot, pokemonMove);
        }
        else if (InflictingMoves.IsRecoilDamageMove(pokemonMove))
        {
            InflictingMoves.ApplyRecoilDamage(attackingPokemon);
        }
        else if (InflictingMoves.IsFaintMove(pokemonMove))
        {
            InflictingMoves.ApplyFaint(attackingPokemon);
        }
    }
}