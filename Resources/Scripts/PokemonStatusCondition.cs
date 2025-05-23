using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        PokemonTD.Signals.DraggingPokemonStageSlot += Dragging;
        PokemonTD.Signals.DraggingPokemonTeamSlot += Dragging;
        PokemonTD.Signals.DraggingPokeBall += Dragging;
        PokemonTD.Signals.HasLeftStage += HasLeftStage;
    }

    private void ApplyBurnCondition<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        StatusCondition statusCondition = StatusCondition.Burn;
        ApplyStatusColor(defendingPokemon, statusCondition);

        float percentage = .0625f; // 1/16
        int iterations = 3;

        int healthPercent = 0;
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            healthPercent = PokemonCombat.Instance.GetDamageAmount(pokemonStageSlot.Pokemon, percentage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            healthPercent = PokemonCombat.Instance.GetDamageAmount(pokemonEnemy.Pokemon, percentage);
        }
        PokemonCombat.Instance.DamagePokemonOverTime(attackingPokemon, defendingPokemon, healthPercent, iterations, statusCondition);
    }

    private void ApplyFreezeCondition<Defending>(Defending defendingPokemon)
    {
        StatusCondition statusCondition = StatusCondition.Freeze;
        ApplyStatusColor(defendingPokemon, statusCondition);
        FreezePokemon(defendingPokemon, statusCondition, 5);
    }

    private void ApplyParalysisCondition<Defending>(Defending defendingPokemon)
    {
        StatusCondition statusCondition = StatusCondition.Paralysis;
        ApplyStatusColor(defendingPokemon, statusCondition);

        if (IsFullyParalyzed())
        {
            FreezePokemon(defendingPokemon, statusCondition, 3);
            return;
        }

        ApplyParalysis(defendingPokemon);
    }

    private bool IsFullyParalyzed()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float randomValue = RNG.RandfRange(0, 1);

        float fullParalysisThreshold = 0.25f;
        return fullParalysisThreshold >= randomValue;
    }

    public void FreezePokemon<Defending>(Defending defendingPokemon, StatusCondition statusCondition, float timeSeconds)
    {
        CustomTimer timer = GetTimer(timeSeconds);
        AddChild(timer);
        _timers.Add(timer);
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            Pokemon pokemon = pokemonStageSlot.Pokemon;
            int teamSlotIndex = pokemonStageSlot.TeamSlotIndex;
            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();

            pokemonStageSlot.IsActive = false;
            timer.Timeout += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(teamSlotIndex);
                if (pokemonStageSlot == null) return;

                pokemonStageSlot.IsActive = true;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(teamSlotIndex);
                if (pokemonStageSlot == null) return;

                ApplyStatusColor(pokemonStageSlot, StatusCondition.None);
                RemoveStageSlotStatusCondition(pokemonStageSlot, statusCondition);
            };

            pokemonStageSlot.Fainted += (pokemonStageSlot) => timer.QueueFree();
            pokemonStageSlot.Retrieved += timer.QueueFree;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name, pokemonEnemy.Pokemon.Level);
            pokemonEnemy.Pokemon.Speed = 0;
            timer.Timeout += () =>
            {
                if (!IsInstanceValid(pokemonEnemy)) return;

                pokemonEnemy.Pokemon.Speed = pokemonData.Speed;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                if (!IsInstanceValid(pokemonEnemy)) return;
                
                ApplyStatusColor(pokemonEnemy, StatusCondition.None);
                RemoveEnemyStatusCondition(pokemonEnemy, statusCondition);
            };

            pokemonEnemy.TreeExiting += timer.QueueFree;
            pokemonEnemy.Fainted += (pokemonEnemy) => timer.QueueFree();
        }
    }

    private void ApplyParalysis<Defending>(Defending defendingPokemon)
    {
        float reductionPercent = 0.25f;
        StatusCondition statusCondition = StatusCondition.Paralysis;

        CustomTimer timer = GetTimer(3);
        AddChild(timer);
        _timers.Add(timer);

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonStageSlot.Pokemon.Name, pokemonStageSlot.Pokemon.Level);
            pokemonStageSlot.Pokemon.Speed = Mathf.RoundToInt(pokemonData.Speed * reductionPercent);

            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
            int teamSlotIndex = pokemonStageSlot.TeamSlotIndex;

            timer.Timeout += () =>
            {
                pokemonStageSlot.Pokemon.Speed = pokemonData.Speed;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(teamSlotIndex);
                if (pokemonStageSlot == null) return;

                ApplyStatusColor(pokemonStageSlot, StatusCondition.None);
                RemoveStageSlotStatusCondition(pokemonStageSlot, statusCondition);
            };

            pokemonStageSlot.Retrieved += timer.QueueFree;
            pokemonStageSlot.Fainted += (pokemonStageSlot) => timer.QueueFree();
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name, pokemonEnemy.Pokemon.Level);
            pokemonEnemy.Pokemon.Speed = Mathf.RoundToInt(pokemonData.Speed * reductionPercent);
            timer.Timeout += () =>
            {
                if (!IsInstanceValid(pokemonEnemy)) return;

                pokemonEnemy.Pokemon.Speed = pokemonData.Speed;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                if (!IsInstanceValid(pokemonEnemy)) return;

                ApplyStatusColor(pokemonEnemy, StatusCondition.None);
                RemoveEnemyStatusCondition(pokemonEnemy, statusCondition);
            };

            pokemonEnemy.TreeExiting += timer.QueueFree;
            pokemonEnemy.Fainted += (pokemonEnemy) => timer.QueueFree();
        }
    }

    private void ApplyPoisonCondition<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        StatusCondition statusCondition = StatusCondition.Poison;
        ApplyStatusColor(defendingPokemon, statusCondition);

        float percentage = .0625f; // 1/16
        int damageAmount = 0;
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            damageAmount = PokemonCombat.Instance.GetDamageAmount(pokemonStageSlot.Pokemon, percentage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            damageAmount = PokemonCombat.Instance.GetDamageAmount(pokemonEnemy.Pokemon, percentage);
        }
        PokemonCombat.Instance.DamagePokemonOverTime(attackingPokemon, defendingPokemon, damageAmount, 2, statusCondition);
    }

    private void ApplyBadlyPoisonedCondition<Defending>(Defending defendingPokemon)
    {
        StatusCondition statusCondition = StatusCondition.BadlyPoisoned;
        ApplyStatusColor(defendingPokemon, statusCondition);

        int iterations = 2;
        float percentage = .0625f; // 1/16
        for (int i = 0; i < iterations; i++)
        {
            CustomTimer timer = GetTimer(1);
            timer.Timeout += () =>
            {
                PokemonCombat.Instance.DamagePokemon(defendingPokemon, percentage);
                percentage *= 2;
            };

            if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
            {
                PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
                int teamSlotIndex = pokemonStageSlot.TeamSlotIndex;

                timer.TreeExiting += () =>
                {
                    pokemonStageSlot = pokemonStage.FindPokemonStageSlot(teamSlotIndex);
                    if (pokemonStageSlot == null) return;

                    ApplyStatusColor(pokemonStageSlot, StatusCondition.None);
                    RemoveStageSlotStatusCondition(pokemonStageSlot, statusCondition);
                };

                pokemonStageSlot.Retrieved += timer.QueueFree;
                pokemonStageSlot.Fainted += (pokemonStageSlot) => timer.QueueFree();
            }
            else if (defendingPokemon is PokemonEnemy pokemonEnemy)
            {
                timer.TreeExiting += () =>
                {
                    if (!IsInstanceValid(pokemonEnemy)) return;

                    ApplyStatusColor(pokemonEnemy, StatusCondition.None);
                    RemoveEnemyStatusCondition(pokemonEnemy, statusCondition);
                };

                pokemonEnemy.TreeExiting += timer.QueueFree;
                pokemonEnemy.Fainted += (pokemonEnemy) => timer.QueueFree();
            }
            AddChild(timer);
        }
    }

    private void ApplySleepCondition<Defending>(Defending defendingPokemon)
    {
        StatusCondition statusCondition = StatusCondition.Sleep;
        ApplyStatusColor(defendingPokemon, statusCondition);
        FreezePokemon(defendingPokemon, statusCondition, 3);
    }

    // ? Have a chance to do recoil damage if the pokemon is confused
    private void ApplyConfuseCondition<Defending>(Defending defendingPokemon)
    {
        StatusCondition statusCondition = StatusCondition.Confuse;
        ApplyStatusColor(defendingPokemon, statusCondition);
        float timeSeconds = 3;

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            int pokemonSpeed = pokemonStageSlot.Pokemon.Speed;
            pokemonStageSlot.Pokemon.Speed = Mathf.RoundToInt(pokemonSpeed / 1.4f);

            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
            int teamSlotIndex = pokemonStageSlot.TeamSlotIndex;

            CustomTimer timer = GetTimer(timeSeconds);
            timer.Timeout += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(teamSlotIndex);
                if (pokemonStageSlot == null) return;
                
                pokemonStageSlot.Pokemon.Speed = pokemonSpeed;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                pokemonStageSlot = pokemonStage.FindPokemonStageSlot(teamSlotIndex);
                if (pokemonStageSlot == null) return;

                ApplyStatusColor(pokemonStageSlot, StatusCondition.None);
                RemoveStageSlotStatusCondition(pokemonStageSlot, statusCondition);
            };
            AddChild(timer);

            pokemonStageSlot.Retrieved += timer.QueueFree;
            pokemonStageSlot.Fainted += (pokemonStageSlot) => timer.QueueFree();
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.IsMovingForward = false;

            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name, pokemonEnemy.Pokemon.Level);
            pokemonEnemy.Pokemon.Speed = -pokemonData.Speed;

            CustomTimer timer = GetTimer(timeSeconds);
            timer.Timeout += () =>
            {
                if (!IsInstanceValid(pokemonEnemy)) return;

                pokemonEnemy.IsMovingForward = true;
                pokemonEnemy.Pokemon.Speed = pokemonData.Speed;
                timer.QueueFree();
            };
            timer.TreeExiting += () =>
            {
                if (!IsInstanceValid(pokemonEnemy)) return;

                ApplyStatusColor(pokemonEnemy, StatusCondition.None);
                RemoveEnemyStatusCondition(pokemonEnemy, statusCondition);
            };
            AddChild(timer);

            pokemonEnemy.TreeExiting += timer.QueueFree;
            pokemonEnemy.Fainted += (pokemonEnemy) => timer.QueueFree();
        }
    }

    public void ApplyStatusColor<Defending>(Defending defendingPokemon, StatusCondition statusCondtion)
    {
        string statusHexColor = GetStatusHexColor(statusCondtion);
        Color statusColor = Color.FromHtml(statusHexColor);

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.IsActive) pokemonStageSlot.Sprite.SelfModulate = statusColor;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (IsInstanceValid(pokemonEnemy)) pokemonEnemy.SelfModulate = statusColor;
        }
    }

    public CustomTimer GetTimer(float timeSeconds)
    {
        CustomTimer timer = new CustomTimer()
        {
            Autostart = true,
            OneShot = true,
            WaitTime = timeSeconds / PokemonTD.GameSpeed
        };

        timer.TreeEntered += () => _timers.Add(timer);
        timer.TreeExiting += () => _timers.Remove(timer);
        
        return timer;
    }

    private void StartTimers()
    {
        foreach (CustomTimer timer in _timers)
        {
            StartTimer(timer);
        }
    }

    private void StartTimer(CustomTimer timer)
    {
        timer.Start(timer.WaitTimeLeft);
    }

    private void StopTimers()
    {
        foreach (CustomTimer timer in _timers)
        {
            StopTimer(timer);
        }
    }

    private void StopTimer(CustomTimer timer)
    {
        timer.WaitTimeLeft = timer.TimeLeft;
        timer.Stop();
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

    public void ApplyStatusCondition<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon, StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None) return;

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.Pokemon.HasStatusCondition(statusCondition)) return;

            pokemonStageSlot.StatusConditions.AddStatusCondition(statusCondition);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.Pokemon.HasStatusCondition(statusCondition)) return;

            pokemonEnemy.StatusConditions.AddStatusCondition(statusCondition);
        }
        AddStatusCondition(attackingPokemon, defendingPokemon, statusCondition);
    }

    private void AddStatusCondition<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon, StatusCondition statusCondition)
    {
        switch (statusCondition)
        {
            case StatusCondition.Burn:
                ApplyBurnCondition(attackingPokemon, defendingPokemon);
                break;
            case StatusCondition.Freeze:
                ApplyFreezeCondition(defendingPokemon);
                break;
            case StatusCondition.Paralysis:
                ApplyParalysisCondition(defendingPokemon);
                break;
            case StatusCondition.Poison:
                ApplyPoisonCondition(attackingPokemon, defendingPokemon);
                break;
            case StatusCondition.BadlyPoisoned:
                ApplyBadlyPoisonedCondition(defendingPokemon);
                break;
            case StatusCondition.Sleep:
                ApplySleepCondition(defendingPokemon);
                break;
            case StatusCondition.Confuse:
                ApplyConfuseCondition(defendingPokemon);
                break;
        }

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Pokemon.AddStatusCondition(statusCondition);

            // Print Message To Console
            string statusConditionText = PrintRich.GetStatusConditionText(statusCondition);
            string statusConditionMessage = $"{pokemonStageSlot.Pokemon.Name} Is Now {statusConditionText}";
            PrintRich.PrintLine(TextColor.Yellow, statusConditionMessage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Pokemon.AddStatusCondition(statusCondition);

            // Print Message To Console
            string statusConditionText = PrintRich.GetStatusConditionText(statusCondition);
            string statusConditionMessage = $"{pokemonEnemy.Pokemon.Name} Is Now {statusConditionText}";
            PrintRich.PrintLine(TextColor.Red, statusConditionMessage);
        }
    }

    public void RemoveStageSlotStatusCondition(PokemonStageSlot pokemonStageSlot, StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None || pokemonStageSlot.Pokemon == null) return;

        pokemonStageSlot.StatusConditions.RemoveStatusCondition(statusCondition);
        pokemonStageSlot.Pokemon.RemoveStatusCondition(statusCondition);
    }

    public void RemoveEnemyStatusCondition(PokemonEnemy pokemonEnemy, StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None || !IsInstanceValid(pokemonEnemy)) return;

        pokemonEnemy.StatusConditions.RemoveStatusCondition(statusCondition);
        pokemonEnemy.Pokemon.RemoveStatusCondition(statusCondition);
    }

    public bool HasStatusCondition<Defending>(Defending defendingPokemon, StatusCondition statusCondition)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            return pokemonStageSlot.Pokemon.HasStatusCondition(statusCondition);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            return pokemonEnemy.Pokemon.HasStatusCondition(statusCondition);
        }
        return false;
    }

    // Get a status condition that the pokemon doesn't already have
    public StatusCondition GetStatusCondition<Defending>(Defending defendingPokemon, PokemonMove pokemonMove)
    {
        StatusCondition statusCondition = StatusCondition.None;
        List<string> statusNames = pokemonMove.StatusCondition.Keys.ToList();
        foreach (string statusName in statusNames)
        {
            if (!HasHitStatusCondition(pokemonMove, statusName)) continue;

            statusCondition = Enum.Parse<StatusCondition>(statusName);
            if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
            {
                if (!pokemonStageSlot.Pokemon.HasStatusCondition(statusCondition)) return statusCondition;
            }
            else if (defendingPokemon is PokemonEnemy pokemonEnemy)
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