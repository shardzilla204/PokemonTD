using Godot;
using System;
using System.Collections.Generic;

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
        float statChangePercentage = GetStatChangePercentage(statMove);
        int statValue = PokemonManager.Instance.GetOtherPokemonStat(pokemon, statMove.PokemonStat);
        statValue = Mathf.RoundToInt(statValue * statChangePercentage);

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
                    if (statMove.IsSharp)
                    {
                        pokemon.Accuracy++;
                    }
                    else
                    {
                        pokemon.Accuracy += 0.5f;
                    }
                } 
                else 
                {
                    if (statMove.IsSharp)
                    {
                        pokemon.Accuracy--;
                        pokemon.Accuracy = Mathf.Clamp(pokemon.Accuracy, 0.5f, 2);
                    }
                    else
                    {
                        pokemon.Accuracy -= 0.5f;
                        pokemon.Accuracy = Mathf.Clamp(pokemon.Accuracy, 0.5f, 2);
                    }
                }
            break;
            case PokemonStat.Evasion:
                if (statMove.IsIncreasing)
                {
                    if (statMove.IsSharp)
                    {
                        pokemon.Evasion++;
                    }
                    else
                    {
                        pokemon.Evasion += 0.5f;
                    }
                } 
                else 
                {
                    if (statMove.IsSharp)
                    {
                        pokemon.Evasion--;
                        pokemon.Evasion = Mathf.Clamp(pokemon.Evasion, 0, 2);
                    }
                    else
                    {
                        pokemon.Evasion -= 0.5f;
                        pokemon.Evasion = Mathf.Clamp(pokemon.Evasion, 0, 2);
                    }
                }
            break;
        }

        Timer timer = GetTimer(1.5f);
        timer.Timeout += () => PokemonManager.Instance.SetPokemonStats(pokemon); // Resets Stats
        AddChild(timer);
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

// Swift && Metronome already handled
public partial class UniqueMoves : Node
{
    private List<string> _uniqueMoveNames = new List<string>()
    {
        "Dragon Rage",
        "Low Kick",
        "Seismic Toss",
        "Mirror Move",
        "Night Shade",
        "Mimic",
        "Pay Day",
        "Skull Bash",
        "Sonic Boom",
        "Super Fang",
        "Psywave",
        "Surf"
    };

    public bool IsUniqueMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _uniqueMoveNames.Find(move => move == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    // Always inflicts 40 HP
    public void DragonRage<Defending>(Defending defendingPokemon)
    {
        int damage = 40;
        PokemonCombat.Instance.ApplyDamage(defendingPokemon, damage);
    }

    // The heavier the opponent, the stronger the attack
    public void LowKick<Defending>(Defending defendingPokemon)
    {
        float multiplier = 1.5f;
        if (defendingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;
            int damage = Mathf.RoundToInt((1 + pokemonStageSlot.Pokemon.Weight) * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            int damage = Mathf.RoundToInt((1 + pokemonEnemy.Pokemon.Weight) * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonEnemy, damage);
        }
    }

    //Inflicts damage equal to user's level
    public void SeismicToss<Defending>(Defending defendingPokemon)
    {
        float multiplier = 1.5f;
        if (defendingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;
            int damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.Level * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            int damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.Level * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonEnemy, damage);
        }
    }

    // User performs the opponent's last move
    public void MirrorMove<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        if (defendingPokemon is StageSlot)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;
            PokemonMove pokemonMove = pokemonStageSlot.Pokemon.Move;
            pokemonEnemy.AttackPokemon(pokemonMove, pokemonStageSlot);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            StageSlot pokemonStageSlot = attackingPokemon as StageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            PokemonMove pokemonMove = pokemonEnemy.Pokemon.Move;
            pokemonStageSlot.AttackPokemonEnemy(pokemonMove, pokemonEnemy);
        }
    }

    //Inflicts damage equal to user's level
    public void NightShade<Defending>(Defending defendingPokemon)
    {
        float multiplier = 1.5f;
        if (defendingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;
            int damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.Level * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            int damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.Level * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonEnemy, damage);
        }
    }

    // User performs the opponent's last move
    public void Mimic<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        if (attackingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = attackingPokemon as StageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            PokemonMove pokemonMove = pokemonStageSlot.Pokemon.Move;
            pokemonEnemy.AttackPokemon(pokemonMove, pokemonStageSlot);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;
            PokemonMove pokemonMove = pokemonEnemy.Pokemon.Move;
            pokemonStageSlot.AttackPokemonEnemy(pokemonMove, pokemonEnemy);
        }
    }

    // Money is earned.
    public void PayDay()
    {
        PokemonTD.PokeDollars += 5;
        PokemonTD.Signals.EmitSignal(Signals.SignalName.PokeDollarsUpdated);
    }

    // Always inflicts 20 HP
    public void SonicBoom<Defending>(Defending defendingPokemon)
    {
        int damage = 20;
        PokemonCombat.Instance.ApplyDamage(defendingPokemon, damage);
    }
    
    // Always takes off half of the opponent's HP
    public void SuperFang<Defending>(Defending defendingPokemon)
    {
        if (defendingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;
            int damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.HP / 2);
            PokemonCombat.Instance.ApplyDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            int damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.HP / 2);
            PokemonCombat.Instance.ApplyDamage(pokemonEnemy, damage);
        }
    }

    // Inflicts damage 50-150% of user's level
    public void Psywave<Defending>(Defending defendingPokemon)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float percentage = RNG.RandfRange(0.5f, 1.5f);
        if (defendingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;
            int damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.Level * percentage);
            PokemonCombat.Instance.ApplyDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            int damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.Level * percentage);
            PokemonCombat.Instance.ApplyDamage(pokemonEnemy, damage);
        }
    }

    // Hits all adjacent Pokemon
    public void Surf<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        if (attackingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = attackingPokemon as StageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            int damage = PokemonManager.Instance.GetDamage(pokemonStageSlot.Pokemon, pokemonMove, pokemonEnemy.Pokemon);
            PokemonCombat.Instance.AttackAllPokemon(pokemonStageSlot.PokemonEnemyQueue, damage);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;
            int damage = PokemonManager.Instance.GetDamage(pokemonEnemy.Pokemon, pokemonMove, pokemonStageSlot.Pokemon);
            PokemonCombat.Instance.AttackAllPokemon(pokemonEnemy.PokemonQueue, damage);
        }
    }

    // Halves damage from Special attacks for 5 turns
    public void LightScreen<T>(T parameter)
    {
        if (parameter is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.LightScreenCount = 5;
        }
        else if (parameter is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.LightScreenCount = 5;
        }
    }

    // Halves damage from Physical attacks for 5 turns
    public void Reflect<T>(T parameter)
    {
        if (parameter is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.ReflectCount = 5;
        }
        else if (parameter is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.ReflectCount = 5;
        }
    }

    // Allows user to move between areas on the map
    public void Teleport()
    {
        
    }
}

// Trapping = remove 1/8 health for 4-5 turns
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
        "Razor Wind",
        "Skull Bash"
    };

    // Hyper Beam attacks first then charges afterward
    public (bool IsChargeMove, bool IsHyperBeam) IsChargeMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _chargeMoveNames.Find(move => move == pokemonMove.Name);
        if (pokemonMoveName == "Hyper Beam")
        {
            return (true, true);
        }
        else if (pokemonMoveName == "Skull Bash")
        {
            
            return (true, false);
        }
        else if (pokemonMoveName != null)
        {
            return (true, false);
        }
        return (false, false);
    }

    public void ApplyChargeMove()
    {

    }
}

// Skip oppenents next move
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

    public void ApplyFlinchMove<T>(T parameter)
    {
        PokemonStatusCondition.Instance.FreezePokemon(parameter, 1f);
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