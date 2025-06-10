using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public enum StatusCondition
{
    None, // #FFFFFF
    Burn, // #FF8080
    Freeze, // #80FFFF
    Paralysis, // #FFFF80
    Poison, // #FF8CFF
    BadlyPoisoned, // #FF59FF
    Sleep, // #8080FF
    Confuse // #FE509B
}

public partial class PokemonStatusCondition : Node
{
    private static PokemonStatusCondition _instance;

    public static PokemonStatusCondition Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

    private PokemonTimer _pokemonTimer = new PokemonTimer();

    public override void _EnterTree()
    {
        Instance = this;
    }

    private void ApplyBurnCondition(GodotObject defending)
    {
        StatusCondition statusCondition = StatusCondition.Burn;
        ApplyStatusColor(defending, statusCondition);

        int iterations = 3;
        PokemonCombat.Instance.DamagePokemonOverTime(defending, iterations, statusCondition);
    }

    private void ApplyFreezeCondition(GodotObject defending)
    {
        StatusCondition statusCondition = StatusCondition.Freeze;
        ApplyStatusColor(defending, statusCondition);
        FreezePokemon(defending, statusCondition, 5);
    }

    private void ApplyParalysisCondition(GodotObject defending)
    {
        StatusCondition statusCondition = StatusCondition.Paralysis;
        ApplyStatusColor(defending, statusCondition);

        if (IsFullyParalyzed())
        {
            FreezePokemon(defending, statusCondition, 3);
            return;
        }

        ApplyParalysis(defending);
    }

    private bool IsFullyParalyzed()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float randomValue = RNG.RandfRange(0, 1);

        float fullParalysisThreshold = 0.25f;
        return fullParalysisThreshold >= randomValue;
    }

    public void FreezePokemon(GodotObject defending, StatusCondition statusCondition, float timeSeconds)
    {
        CustomTimer timer = GetDamageTimer(timeSeconds);

        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);
        Pokemon defendingPokemonData = PokemonManager.Instance.GetPokemon(defendingPokemon.Name, defendingPokemon.Level);
        
        defendingPokemon.Stats.Speed = 0;
        
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            int pokemonTeamIndex = pokemonStageSlot.PokemonTeamIndex;
            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();

            timer.Timeout += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                if (pokemonStageSlot == null) return; // * For the next part thats appended with the same signal (Timeout)
            };
            timer.TreeExiting += () =>
            {
                 pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                if (pokemonStageSlot == null) return; // * For the next part thats appended with the same signal (TreeExiting)
            };

            pokemonStageSlot.Retrieved += (pokemonStageSlot) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
            pokemonStageSlot.Fainted += (pokemonStageSlot) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };

            pokemonStageSlot.AddChild(timer);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Fainted += (pokemonEnemy) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
            pokemonEnemy.AddChild(timer);
        }

        // *
        timer.Timeout += () =>
        {
            defendingPokemon.Stats.Speed = defendingPokemonData.Stats.Speed;
            timer.QueueFree();
        };

        // *
        timer.TreeExiting += () =>
        {
            ApplyStatusColor(defending, StatusCondition.None);
            RemoveStatusCondition(defending, statusCondition);
            defendingPokemon.RemoveStatusCondition(statusCondition);
        };
    }

    private void ApplyParalysis(GodotObject defending)
    {
        float reductionPercent = 0.25f;
        StatusCondition statusCondition = StatusCondition.Paralysis;

        CustomTimer timer = GetDamageTimer(3);

        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);
        Pokemon defendingPokemonData = PokemonManager.Instance.GetPokemon(defendingPokemon.Name, defendingPokemon.Level);

        defendingPokemon.Stats.Speed = Mathf.RoundToInt(defendingPokemonData.Stats.Speed * reductionPercent);

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
            int pokemonTeamIndex = pokemonStageSlot.PokemonTeamIndex;

            timer.Timeout += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                if (pokemonStageSlot == null) return; // * For the next part thats appended with the same signal (Timeout)
            };

            timer.TreeExiting += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                if (pokemonStageSlot == null) return; // * For the next part thats appended with the same signal (TreeExiting)
            };
            pokemonStageSlot.AddChild(timer);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Fainted += (pokemonEnemy) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
            pokemonEnemy.AddChild(timer);
        }

        // *
        timer.Timeout += () =>
        {
            defendingPokemon.Stats.Speed = defendingPokemonData.Stats.Speed;
            timer.QueueFree();
        };

        // *
        timer.TreeExiting += () =>
        {
            ApplyStatusColor(defending, StatusCondition.None);
            RemoveStatusCondition(defending, statusCondition);
            defendingPokemon.RemoveStatusCondition(statusCondition);
        };
    }

    private void ApplyPoisonCondition(GodotObject defending)
    {
        StatusCondition statusCondition = StatusCondition.Poison;
        ApplyStatusColor(defending, statusCondition);

        int iterations = 3;
        PokemonCombat.Instance.DamagePokemonOverTime(defending, iterations, statusCondition);
    }

    private void ApplyBadlyPoisonedCondition(GodotObject defending)
    {
        StatusCondition statusCondition = StatusCondition.BadlyPoisoned;
        ApplyStatusColor(defending, statusCondition);

        int iterations = 2;
        PokemonCombat.Instance.DamagePokemonOverTime(defending, iterations, statusCondition);
    }

    private void ApplySleepCondition(GodotObject defending)
    {
        StatusCondition statusCondition = StatusCondition.Sleep;
        ApplyStatusColor(defending, statusCondition);
        FreezePokemon(defending, statusCondition, 3);
    }

    // ? Have a chance to do recoil damage if the pokemon is confused
    private void ApplyConfuseCondition(GodotObject defending)
    {
        StatusCondition statusCondition = StatusCondition.Confuse;
        ApplyStatusColor(defending, statusCondition);

        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);
        Pokemon defendingPokemonData = PokemonManager.Instance.GetPokemon(defendingPokemon.Name, defendingPokemon.Level);

        CustomTimer timer = GetDamageTimer(3);

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            int pokemonSpeed = defendingPokemon.Stats.Speed;
            defendingPokemon.Stats.Speed = Mathf.RoundToInt(pokemonSpeed * 0.5f);

            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
            int pokemonTeamIndex = pokemonStageSlot.PokemonTeamIndex;

            timer.Timeout += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                if (pokemonStageSlot == null) return;

                defendingPokemon.Stats.Speed = pokemonSpeed;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                if (pokemonStageSlot == null) return;
            };
            pokemonStageSlot.AddChild(timer);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            defendingPokemon.Stats.Speed = -defendingPokemonData.Stats.Speed;

            timer.Timeout += () =>
            {
                defendingPokemon.Stats.Speed = defendingPokemonData.Stats.Speed;
                timer.QueueFree();
            };
            pokemonEnemy.AddChild(timer);
        }
        timer.TreeExiting += () =>
        {
            ApplyStatusColor(defending, StatusCondition.None);
            RemoveStatusCondition(defending, statusCondition);
            defendingPokemon.RemoveStatusCondition(statusCondition);
        };
    }

    public void ApplyStatusColor(GodotObject defending, StatusCondition statusCondtion)
    {
        string statusHexColor = GetStatusHexColor(statusCondtion);
        Color statusConditionColor = Color.FromHtml(statusHexColor);

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.ApplyStatusColor(statusConditionColor);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            if (IsInstanceValid(pokemonEnemy)) pokemonEnemy.SelfModulate = statusConditionColor;
        }
    }

    public CustomTimer GetDamageTimer(float waitTime)
    {
        CustomTimer timer = new CustomTimer(waitTime);

        timer.TreeEntered += () => _pokemonTimer.AddTimer(timer);
        timer.TreeExiting += () => _pokemonTimer.RemoveTimer(timer);
        
        return timer;
    }

    private string GetStatusHexColor(StatusCondition statusCondtion) => statusCondtion switch
    {
        StatusCondition.Burn => "#FF8080",
        StatusCondition.Freeze => "#80FFFF",
        StatusCondition.Paralysis => "#FFFF80",
        StatusCondition.Poison => "#FF8CFF",
        StatusCondition.BadlyPoisoned => "#FF59FF",
        StatusCondition.Sleep => "#8080FF",
        StatusCondition.Confuse => "#FE509B",
        _ => "#FFFFFF"
    };

    public void ApplyStatusCondition(GodotObject defending, StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None) return;

        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);
        if (defendingPokemon.HasStatusCondition(statusCondition)) return;

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.AddStatusCondition(statusCondition);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.AddStatusCondition(statusCondition);
        }
        AddStatusCondition(defending, statusCondition);
    }

    private void AddStatusCondition(GodotObject defending, StatusCondition statusCondition)
    {
        switch (statusCondition)
        {
            case StatusCondition.Burn:
                ApplyBurnCondition(defending);
                break;
            case StatusCondition.Freeze:
                ApplyFreezeCondition(defending);
                break;
            case StatusCondition.Paralysis:
                ApplyParalysisCondition(defending);
                break;
            case StatusCondition.Poison:
                ApplyPoisonCondition(defending);
                break;
            case StatusCondition.BadlyPoisoned:
                ApplyBadlyPoisonedCondition(defending);
                break;
            case StatusCondition.Sleep:
                ApplySleepCondition(defending);
                break;
            case StatusCondition.Confuse:
                ApplyConfuseCondition(defending);
                break;
        }

        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);
        defendingPokemon.AddStatusCondition(statusCondition);

        // Print message to console
        string statusConditionText = PrintRich.GetStatusConditionText(statusCondition);
        string statusConditionMessage = $"{defendingPokemon.Name} Is Now {statusConditionText}";
        PrintRich.PrintLine(TextColor.Yellow, statusConditionMessage);
    }

    public void RemoveStatusCondition(GodotObject defending, StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None) return;

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.RemoveStatusCondition(statusCondition);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.RemoveStatusCondition(statusCondition);
        }
    }
    
    // Get a status condition that the pokemon doesn't already have
    public StatusCondition GetStatusCondition<Defending>(Defending defending, PokemonMove pokemonMove)
    {
        StatusCondition statusCondition = StatusCondition.None;
        List<string> statusNames = pokemonMove.StatusCondition.Keys.ToList();
        foreach (string statusName in statusNames)
        {
            if (!HasHitStatusCondition(pokemonMove, statusName)) continue;

            statusCondition = Enum.Parse<StatusCondition>(statusName);
            if (defending is PokemonStageSlot pokemonStageSlot)
            {
                if (!pokemonStageSlot.Pokemon.HasStatusCondition(statusCondition)) return statusCondition;
            }
            else if (defending is PokemonEnemy pokemonEnemy)
            {
                if (!pokemonEnemy.Pokemon.HasStatusCondition(statusCondition)) return statusCondition;
            }
        }
        return statusCondition;
    }

    // Checks if status condition applies
    private bool HasHitStatusCondition(PokemonMove pokemonMove, string statusName)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float hitThreshold = pokemonMove.StatusCondition[statusName];
        float randomValue = RNG.RandfRange(0, 1);
        randomValue -= hitThreshold;

        return randomValue <= 0;
    }
    
}