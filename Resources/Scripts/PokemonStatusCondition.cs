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

    private List<CustomTimer> _timers = new List<CustomTimer>();

    public override void _EnterTree()
    {
        Instance = this;

        PokemonTD.Signals.PressedPlay += StartTimers;
        PokemonTD.Signals.PressedPause += StopTimers;
        PokemonTD.Signals.Dragging += Dragging;
        PokemonTD.Signals.HasLeftStage += HasLeftStage;
    }

    private void ApplyBurnCondition(GodotObject attacking, GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetDefendingPokemon(defending);

        StatusCondition statusCondition = StatusCondition.Burn;
        ApplyStatusColor(defending, statusCondition);

        float percentage = .0625f; // 1/16
        int iterations = 3;

        int damage = PokemonCombat.Instance.GetDamage(defendingPokemon, percentage);
        PokemonCombat.Instance.DamagePokemonOverTime(attacking, defending, damage, iterations, statusCondition);
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
        CustomTimer timer = GetDamageTimer(defending, timeSeconds);
        AddChild(timer);
        _timers.Add(timer);
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            Pokemon pokemon = pokemonStageSlot.Pokemon;
            int pokemonTeamIndex = pokemonStageSlot.PokemonTeamIndex;
            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();

            pokemonStageSlot.IsActive = false;
            timer.Timeout += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                if (pokemonStageSlot == null) return;

                pokemonStageSlot.IsActive = true;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                if (pokemonStageSlot == null) return;

                ApplyStatusColor(pokemonStageSlot, StatusCondition.None);
                RemoveStageSlotStatusCondition(pokemonStageSlot, statusCondition);
            };

            pokemonStageSlot.Retrieved += (pokemonStageSlot) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
            pokemonStageSlot.Fainted += (pokemonStageSlot) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name, pokemonEnemy.Pokemon.Level);
            pokemonEnemy.Pokemon.Stats.Speed = 0;
            timer.Timeout += () =>
            {
                if (!IsInstanceValid(pokemonEnemy)) return;

                pokemonEnemy.Pokemon.Stats.Speed = pokemonData.Stats.Speed;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                if (!IsInstanceValid(pokemonEnemy)) return;
                
                ApplyStatusColor(pokemonEnemy, StatusCondition.None);
                RemoveEnemyStatusCondition(pokemonEnemy, statusCondition);
            };

            pokemonEnemy.TreeExiting += () =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
            pokemonEnemy.Fainted += (pokemonEnemy) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
        }
    }

    private void ApplyParalysis(GodotObject defending)
    {
        float reductionPercent = 0.25f;
        StatusCondition statusCondition = StatusCondition.Paralysis;

        CustomTimer timer = GetDamageTimer(defending, 3);
        AddChild(timer);
        _timers.Add(timer);

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonStageSlot.Pokemon.Name, pokemonStageSlot.Pokemon.Level);
            pokemonStageSlot.Pokemon.Stats.Speed = Mathf.RoundToInt(pokemonData.Stats.Speed * reductionPercent);

            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
            int pokemonTeamIndex = pokemonStageSlot.PokemonTeamIndex;

            timer.Timeout += () =>
            {
                pokemonStageSlot.Pokemon.Stats.Speed = pokemonData.Stats.Speed;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                if (pokemonStageSlot == null) return;

                ApplyStatusColor(pokemonStageSlot, StatusCondition.None);
                RemoveStageSlotStatusCondition(pokemonStageSlot, statusCondition);
            };

            pokemonStageSlot.Retrieved += (pokemonStageSlot) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
            pokemonStageSlot.Fainted += (pokemonStageSlot) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name, pokemonEnemy.Pokemon.Level);
            pokemonEnemy.Pokemon.Stats.Speed = Mathf.RoundToInt(pokemonData.Stats.Speed * reductionPercent);
            timer.Timeout += () =>
            {
                if (!IsInstanceValid(pokemonEnemy)) return;

                pokemonEnemy.Pokemon.Stats.Speed = pokemonData.Stats.Speed;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                if (!IsInstanceValid(pokemonEnemy)) return;

                ApplyStatusColor(pokemonEnemy, StatusCondition.None);
                RemoveEnemyStatusCondition(pokemonEnemy, statusCondition);
            };

            pokemonEnemy.TreeExiting += () =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
            pokemonEnemy.Fainted += (pokemonEnemy) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
        }
    }

    private void ApplyPoisonCondition(GodotObject attacking, GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetDefendingPokemon(defending);

        StatusCondition statusCondition = StatusCondition.Poison;
        ApplyStatusColor(defending, statusCondition);

        float percentage = .0625f; // 1/16
        int iterations = 3;

        int damage = PokemonCombat.Instance.GetDamage(defendingPokemon, percentage);
        PokemonCombat.Instance.DamagePokemonOverTime(attacking, defending, damage, iterations, statusCondition);
    }

    private void ApplyBadlyPoisonedCondition(GodotObject defending)
    {
        StatusCondition statusCondition = StatusCondition.BadlyPoisoned;
        ApplyStatusColor(defending, statusCondition);

        int iterations = 2;
        float percentage = .0625f; // 1/16
        for (int i = 0; i < iterations; i++)
        {
            CustomTimer timer = GetDamageTimer(defending, 1);
            timer.Timeout += () =>
            {
                PokemonCombat.Instance.DamagePokemon(defending, percentage);
                percentage *= 2;
            };

            if (defending is PokemonStageSlot pokemonStageSlot)
            {
                PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
                int pokemonTeamIndex = pokemonStageSlot.PokemonTeamIndex;

                timer.TreeExiting += () =>
                {

                    pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                    if (pokemonStageSlot == null) return;

                    ApplyStatusColor(pokemonStageSlot, StatusCondition.None);
                    RemoveStageSlotStatusCondition(pokemonStageSlot, statusCondition);
                };

                pokemonStageSlot.Retrieved += (pokemonStageSlot) =>
                {
                    if (IsInstanceValid(timer)) timer.QueueFree();
                };
                pokemonStageSlot.Fainted += (pokemonStageSlot) =>
                {
                    if (IsInstanceValid(timer)) timer.QueueFree();
                };
            }
            else if (defending is PokemonEnemy pokemonEnemy)
            {
                timer.TreeExiting += () =>
                {

                    if (!IsInstanceValid(pokemonEnemy)) return;

                    ApplyStatusColor(pokemonEnemy, StatusCondition.None);
                    RemoveEnemyStatusCondition(pokemonEnemy, statusCondition);
                };

                pokemonEnemy.TreeExiting += () =>
                {
                    if (IsInstanceValid(timer)) timer.QueueFree();
                };
                pokemonEnemy.Fainted += (pokemonEnemy) =>
                {
                    if (IsInstanceValid(timer)) timer.QueueFree();
                };
            }
            AddChild(timer);
        }
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
        float timeSeconds = 3;

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            int pokemonSpeed = pokemonStageSlot.Pokemon.Stats.Speed;
            pokemonStageSlot.Pokemon.Stats.Speed = Mathf.RoundToInt(pokemonSpeed / 1.4f);

            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
            int pokemonTeamIndex = pokemonStageSlot.PokemonTeamIndex;

            CustomTimer timer = GetDamageTimer(defending, timeSeconds);
            timer.Timeout += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                if (pokemonStageSlot == null) return;
                
                pokemonStageSlot.Pokemon.Stats.Speed = pokemonSpeed;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                if (pokemonStageSlot == null) return;

                ApplyStatusColor(pokemonStageSlot, StatusCondition.None);
                RemoveStageSlotStatusCondition(pokemonStageSlot, statusCondition);
            };
            AddChild(timer);

            pokemonStageSlot.Retrieved += (pokemonStageSlot) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
            pokemonStageSlot.Fainted += (pokemonStageSlot) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.IsMovingForward = false;

            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name, pokemonEnemy.Pokemon.Level);
            pokemonEnemy.Pokemon.Stats.Speed = -pokemonData.Stats.Speed;

            CustomTimer timer = GetDamageTimer(defending, timeSeconds);
            timer.Timeout += () =>
            {
                if (!IsInstanceValid(pokemonEnemy)) return;

                pokemonEnemy.IsMovingForward = true;
                pokemonEnemy.Pokemon.Stats.Speed = pokemonData.Stats.Speed;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                timer.Stop();

                if (!IsInstanceValid(pokemonEnemy)) return;

                ApplyStatusColor(pokemonEnemy, StatusCondition.None);
                RemoveEnemyStatusCondition(pokemonEnemy, statusCondition);
            };
            AddChild(timer);

            pokemonEnemy.TreeExiting += () =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
            pokemonEnemy.Fainted += (pokemonEnemy) =>
            {
                if (IsInstanceValid(timer)) timer.QueueFree();
            };
        }
    }

    public void ApplyStatusColor(GodotObject defending, StatusCondition statusCondtion)
    {
        string statusHexColor = GetStatusHexColor(statusCondtion);
        Color statusConditionColor = Color.FromHtml(statusHexColor);

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            if (IsInstanceValid(pokemonStageSlot) && pokemonStageSlot.IsActive) pokemonStageSlot.ApplyStatusColor(statusConditionColor);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            if (IsInstanceValid(pokemonEnemy)) pokemonEnemy.SelfModulate = statusConditionColor;
        }
    }

    public CustomTimer GetDamageTimer(GodotObject defending, float timeSeconds)
    {
        CustomTimer timer = new CustomTimer()
        {
            Autostart = true,
            OneShot = true,
            WaitTime = timeSeconds / PokemonTD.GameSpeed
        };

        timer.TreeEntered += () => _timers.Add(timer);
        timer.Timeout += async () =>
        {
            if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, Signals.SignalName.PressedPlay);
            timer.QueueFree();
        };
        timer.TreeExiting += () =>
        {
            _timers.Remove(timer);
            if (!IsInstanceValid(defending)) return;
        }; 
        
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

    private void StopTimer(CustomTimer timer)
    {
        try
        {
            timer.WaitTimeLeft = timer.TimeLeft;
            timer.Stop();
        }
        catch (ObjectDisposedException) {}
    }

    private void StopTimers()
    {
        foreach (CustomTimer timer in _timers)
        {
            StopTimer(timer);
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
            if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, Signals.SignalName.PressedPlay);
            StartTimers();
        }
    }

    private void HasLeftStage()
    {
        foreach (CustomTimer timer in _timers)
        {
            timer.QueueFree();
        }
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

    public void ApplyStatusCondition(GodotObject attacking, GodotObject defending, StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None) return;

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.Pokemon.HasStatusCondition(statusCondition)) return;

            pokemonStageSlot.AddStatusCondition(statusCondition);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.Pokemon.HasStatusCondition(statusCondition)) return;

            pokemonEnemy.AddStatusCondition(statusCondition);
        }
        AddStatusCondition(attacking, defending, statusCondition);
    }

    private void AddStatusCondition(GodotObject attacking, GodotObject defending, StatusCondition statusCondition)
    {
        switch (statusCondition)
        {
            case StatusCondition.Burn:
                ApplyBurnCondition(attacking, defending);
                break;
            case StatusCondition.Freeze:
                ApplyFreezeCondition(defending);
                break;
            case StatusCondition.Paralysis:
                ApplyParalysisCondition(defending);
                break;
            case StatusCondition.Poison:
                ApplyPoisonCondition(attacking, defending);
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

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Pokemon.AddStatusCondition(statusCondition);

            // Print message to console
            string statusConditionText = PrintRich.GetStatusConditionText(statusCondition);
            string statusConditionMessage = $"{pokemonStageSlot.Pokemon.Name} Is Now {statusConditionText}";
            PrintRich.PrintLine(TextColor.Yellow, statusConditionMessage);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Pokemon.AddStatusCondition(statusCondition);

            // Print message to console
            string statusConditionText = PrintRich.GetStatusConditionText(statusCondition);
            string statusConditionMessage = $"{pokemonEnemy.Pokemon.Name} Is Now {statusConditionText}";
            PrintRich.PrintLine(TextColor.Red, statusConditionMessage);
        }
    }

    public void RemoveStageSlotStatusCondition(PokemonStageSlot pokemonStageSlot, StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None || pokemonStageSlot.Pokemon == null) return;

        pokemonStageSlot.RemoveStatusCondition(statusCondition);
        pokemonStageSlot.Pokemon.RemoveStatusCondition(statusCondition);
    }

    public void RemoveEnemyStatusCondition(PokemonEnemy pokemonEnemy, StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None || !IsInstanceValid(pokemonEnemy)) return;

        pokemonEnemy.RemoveStatusCondition(statusCondition);
        pokemonEnemy.Pokemon.RemoveStatusCondition(statusCondition);
    }

    public bool HasStatusCondition<Defending>(Defending defending, StatusCondition statusCondition)
    {
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            return pokemonStageSlot.Pokemon.HasStatusCondition(statusCondition);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            return pokemonEnemy.Pokemon.HasStatusCondition(statusCondition);
        }
        return false;
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