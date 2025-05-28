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
    public CopyMoves CopyMoves = new CopyMoves(); // Done

    public override void _EnterTree()
    {
        Instance = this;
    }

    public int GetRandomHitCount(PokemonMove pokemonMove)
    {
        int minimumHitCount = pokemonMove.HitCount[0];
        int MaxHitCount = pokemonMove.HitCount[1];
        RandomNumberGenerator RNG = new RandomNumberGenerator();

        return RNG.RandiRange(minimumHitCount, MaxHitCount);
    }

    public float GetCriticalHitRatio(Pokemon attackingPokemon, PokemonMove pokemonMove)
    {
        Pokemon pokemonData = PokemonManager.Instance.GetPokemon(attackingPokemon.Name, attackingPokemon.Level);
        float criticalHitRatio = pokemonData.Stats.Speed / 2;

        bool isHighCriticalRatioMove = HighCriticalRatioMoves.IsHighCriticalRatioMove(pokemonMove);
        if (isHighCriticalRatioMove)
        {
            float MaxThreshold = 255;
            criticalHitRatio = Math.Min(4 * criticalHitRatio, MaxThreshold);
        }
        return criticalHitRatio;
    }

    public void ApplyMoveEffect(GodotObject attacking, GodotObject defending, PokemonMove pokemonMove)
    {
        if (UniqueMoves.IsUniqueMove(pokemonMove))
        {
            UniqueMoves.ApplyUniqueMove(attacking, defending, pokemonMove);
        }
        else if (TrapMoves.IsTrapMove(pokemonMove))
        {
            TrapMoves.ApplyTrapMove(attacking, defending);
        }
        else if (FlinchMoves.IsFlinchMove(pokemonMove))
        {
            FlinchMoves.ApplyFlinchMove(defending);
        }
        else if (OneHitMoves.IsOneHitKOMove(pokemonMove))
        {
            OneHitMoves.ApplyOneHitKO(defending);
        }
        else if (RecoverMoves.IsHealthRecoveryMove(pokemonMove))
        {
            if (pokemonMove.Name == "Leech Seed")
            {
                RecoverMoves.LeechSeed(attacking, defending);
            }
            else
            {
                RecoverMoves.ApplyHealthRecoveryMove(attacking, defending, pokemonMove);
            }
        }
        else if (RecoverMoves.IsRareCandyRecoveryMove(pokemonMove))
        {
            if (attacking is not PokemonStageSlot pokemonStageSlot) return;

            RecoverMoves.ApplyRareCandyRecoveryMove(pokemonStageSlot, pokemonMove);
        }
        else if (InflictingMoves.IsRecoilDamageMove(pokemonMove))
        {
            InflictingMoves.ApplyRecoilDamage(attacking);
        }
        else if (InflictingMoves.IsFaintMove(pokemonMove))
        {
            InflictingMoves.ApplyFaint(attacking);
        }
    }
}