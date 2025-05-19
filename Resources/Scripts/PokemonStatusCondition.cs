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
    [Signal]
    public delegate void FinishedEventHandler();

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
    }

    public void ApplyBurnCondition<Defending>(Defending defendingPokemon)
    {
        bool hasStatusCondition = HasStatusCondition(defendingPokemon, StatusCondition.Burn);
        if (hasStatusCondition) return;

        ApplyStatusColor(defendingPokemon, StatusCondition.Burn);

        float percentage = .0625f; // 1/16

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            int healthPercent = PokemonCombat.Instance.GetDamageAmount(pokemonStageSlot.Pokemon, percentage);
            PokemonCombat.Instance.DamagePokemonOverTime(pokemonStageSlot, healthPercent, 2, StatusCondition.Burn);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int healthPercent = PokemonCombat.Instance.GetDamageAmount(pokemonEnemy.Pokemon, percentage);
            PokemonCombat.Instance.DamagePokemonOverTime(pokemonEnemy, healthPercent, 2, StatusCondition.Burn);
        }
    }

    public async void ApplyFreezeCondition<Defending>(Defending defendingPokemon)
    {
        bool hasStatusCondition = HasStatusCondition(defendingPokemon, StatusCondition.Freeze);
        if (hasStatusCondition) return;

        ApplyStatusColor(defendingPokemon, StatusCondition.Freeze);
        AddStatusCondition(defendingPokemon, StatusCondition.Freeze);

        FreezePokemon(defendingPokemon, 5);

        await ToSignal(this, SignalName.Finished);

        ApplyStatusColor(defendingPokemon, StatusCondition.None);
        RemoveStatusCondition(defendingPokemon, StatusCondition.Freeze);
    }

    public async void ApplyParalysisCondition<Defending>(Defending defendingPokemon)
    {
        bool hasStatusCondition = HasStatusCondition(defendingPokemon, StatusCondition.Paralysis);
        if (hasStatusCondition) return;

        ApplyStatusColor(defendingPokemon, StatusCondition.Paralysis);
        AddStatusCondition(defendingPokemon, StatusCondition.Paralysis);

        if (IsFullyParalyzed())
        {
            FreezePokemon(defendingPokemon, 3);

            await ToSignal(this, SignalName.Finished);
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

    public async void FreezePokemon<Defending>(Defending defendingPokemon, float timeSeconds)
    {
        CustomTimer timer = GetTimer(timeSeconds);
        AddChild(timer);
        _timers.Add(timer);
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };

            pokemonStageSlot.IsActive = false;
            await ToSignal(timer, Timer.SignalName.Timeout);
            pokemonStageSlot.IsActive = true;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Fainted += (pokemonEnemy) => { return; };

            pokemonEnemy.Pokemon.Speed = 0;
            await ToSignal(timer, Timer.SignalName.Timeout);
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name, pokemonEnemy.Pokemon.Level);
            pokemonEnemy.Pokemon.Speed = pokemonData.Speed;
        }
        EmitSignal(SignalName.Finished);
    }

    private async void ApplyParalysis<Defending>(Defending defendingPokemon)
    {
        float reductionPercent = 0.25f;

        CustomTimer timer = GetTimer(3);
        AddChild(timer);
        _timers.Add(timer);

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };

            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonStageSlot.Pokemon.Name, pokemonStageSlot.Pokemon.Level);
            pokemonStageSlot.Pokemon.Speed = Mathf.RoundToInt(pokemonData.Speed * reductionPercent);
            await ToSignal(timer, Timer.SignalName.Timeout);
            pokemonStageSlot.Pokemon.Speed = pokemonData.Speed;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Fainted += (pokemonEnemy) => { return; };

            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name, pokemonEnemy.Pokemon.Level);
            pokemonEnemy.Pokemon.Speed = Mathf.RoundToInt(pokemonData.Speed * reductionPercent);
            await ToSignal(timer, Timer.SignalName.Timeout);
            pokemonEnemy.Pokemon.Speed = pokemonData.Speed;
        }

        timer.QueueFree();
        ApplyStatusColor(defendingPokemon, StatusCondition.None);
        RemoveStatusCondition(defendingPokemon, StatusCondition.Paralysis);
    }

    public void ApplyPoisonCondition<Defending>(Defending defendingPokemon)
    {
        bool hasStatusCondition = HasStatusCondition(defendingPokemon, StatusCondition.Poison);
        if (hasStatusCondition) return;

        ApplyStatusColor(defendingPokemon, StatusCondition.Poison);

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
        PokemonCombat.Instance.DamagePokemonOverTime(defendingPokemon, damageAmount, 2, StatusCondition.Poison);
    }

    public async void ApplyBadlyPoisonedCondition<Defending>(Defending defendingPokemon)
    {
        bool hasStatusCondition = HasStatusCondition(defendingPokemon, StatusCondition.BadlyPoisoned);
        if (hasStatusCondition) return;

        ApplyStatusColor(defendingPokemon, StatusCondition.BadlyPoisoned);
        AddStatusCondition(defendingPokemon, StatusCondition.BadlyPoisoned);

        int iterations = 2;
        float percentage = .0625f; // 1/16
        for (int i = 0; i < iterations; i++)
        {
            CustomTimer timer = GetTimer(1);
            AddChild(timer);

            await ToSignal(timer, Timer.SignalName.Timeout);

            DamagePokemon(defendingPokemon, percentage);
            percentage *= 2;
        }

        ApplyStatusColor(defendingPokemon, StatusCondition.None);
        RemoveStatusCondition(defendingPokemon, StatusCondition.BadlyPoisoned);
    }

    private void DamagePokemon<Defending>(Defending defendingPokemon, float percentage)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            int damageAmount = PokemonCombat.Instance.GetDamageAmount(pokemonStageSlot.Pokemon, percentage);
            if (pokemonStageSlot.IsActive) pokemonStageSlot.DamagePokemon(damageAmount);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damageAmount = PokemonCombat.Instance.GetDamageAmount(pokemonEnemy.Pokemon, percentage);
            if (pokemonEnemy.IsActive) pokemonEnemy.DamagePokemon(damageAmount);
        }
    }

    public async void ApplySleepCondition<Defending>(Defending defendingPokemon)
    {
        bool hasStatusCondition = HasStatusCondition(defendingPokemon, StatusCondition.Sleep);
        if (hasStatusCondition) return;

        ApplyStatusColor(defendingPokemon, StatusCondition.Sleep);
        AddStatusCondition(defendingPokemon, StatusCondition.Sleep);

        FreezePokemon(defendingPokemon, 3);
        await ToSignal(this, SignalName.Finished);

        ApplyStatusColor(defendingPokemon, StatusCondition.None);
        RemoveStatusCondition(defendingPokemon, StatusCondition.Sleep);
    }

    // ? Have a chance to do recoil damage if the pokemon is confused
    public async void ApplyConfuseCondition<Defending>(Defending defendingPokemon)
    {
        bool hasStatusCondition = HasStatusCondition(defendingPokemon, StatusCondition.Confuse);
        if (hasStatusCondition) return;

        ApplyStatusColor(defendingPokemon, StatusCondition.Confuse);
        AddStatusCondition(defendingPokemon, StatusCondition.Confuse);

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            int pokemonSpeed = pokemonStageSlot.Pokemon.Speed;
            pokemonStageSlot.Pokemon.Speed /= 2;

            CustomTimer timer = GetTimer(3);
            AddChild(timer);

            await ToSignal(timer, Timer.SignalName.Timeout);

            pokemonStageSlot.Pokemon.Speed = pokemonSpeed;

        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name, pokemonEnemy.Pokemon.Level);

            pokemonEnemy.IsMovingForward = false;
            pokemonEnemy.Pokemon.Speed = -pokemonData.Speed;

            CustomTimer timer = GetTimer(1);
            AddChild(timer);

            await ToSignal(timer, Timer.SignalName.Timeout);

            pokemonEnemy.IsMovingForward = true;
            pokemonEnemy.Pokemon.Speed = pokemonData.Speed;
        }

        ApplyStatusColor(defendingPokemon, StatusCondition.None);
        RemoveStatusCondition(defendingPokemon, StatusCondition.Confuse);
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
            if (pokemonEnemy.IsActive) pokemonEnemy.SelfModulate = statusColor;
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

        timer.TreeEntered += () =>
        {
            _timers.Add(timer);

            PokemonTD.Signals.PressedPlay += StartTimers;
            PokemonTD.Signals.PressedPause += StopTimers;
        };

        timer.TreeExiting += () =>
        {
            _timers.Remove(timer);

            PokemonTD.Signals.PressedPlay -= StartTimers;
            PokemonTD.Signals.PressedPause -= StopTimers;
        };

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

    public void ApplyStatusCondition<Defending>(Defending defendingPokemon, StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None) return;

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => pokemonStageSlot.RemoveStatusConditionIcon(statusCondition);
            pokemonStageSlot.AddStatusConditionIcon(statusCondition);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Fainted += (pokemonEnemy) => pokemonEnemy.RemoveStatusConditionIcon(statusCondition);
            pokemonEnemy.AddStatusConditionIcon(statusCondition);
        }
    }

    public void RemoveStatusCondition<Defending>(Defending defendingPokemon, StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None) return;

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted -= (pokemonStageSlot) => pokemonStageSlot.RemoveStatusConditionIcon(statusCondition);
            pokemonStageSlot.RemoveStatusConditionIcon(statusCondition);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.IsActive) pokemonEnemy.RemoveStatusConditionIcon(statusCondition);
        }
    }

    public bool HasStatusCondition<Defending>(Defending defendingPokemon, StatusCondition statusCondition)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            return pokemonStageSlot.HasStatusCondition(statusCondition);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            return pokemonEnemy.HasStatusCondition(statusCondition);
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
                if (!PokemonHasStatusCondition(pokemonStageSlot, statusCondition)) return statusCondition;
            }
            else if (defendingPokemon is PokemonEnemy pokemonEnemy)
            {
                if (!PokemonHasStatusCondition(pokemonEnemy, statusCondition)) return statusCondition;
            }
        }
        return statusCondition;
    }

    // Checks if pokemon already has status condition
    public bool PokemonHasStatusCondition<Defending>(Defending defendingPokemon, StatusCondition statusCondition)
    {
        bool hasStatusCondition = false;
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            hasStatusCondition = !pokemonStageSlot.HasStatusCondition(statusCondition);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            hasStatusCondition = !pokemonEnemy.HasStatusCondition(statusCondition);
        }
        return hasStatusCondition;
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
    
    public void AddStatusCondition<Defending>(Defending defendingPokemon, StatusCondition statusCondition)
    {
        switch (statusCondition)
        {
            case StatusCondition.Burn:
                ApplyBurnCondition(defendingPokemon);
                break;
            case StatusCondition.Freeze:
                ApplyFreezeCondition(defendingPokemon);
                break;
            case StatusCondition.Paralysis:
                ApplyParalysisCondition(defendingPokemon);
                break;
            case StatusCondition.Poison:
                ApplyPoisonCondition(defendingPokemon);
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
            // Print Message To Console
            string statusConditionText = PrintRich.GetStatusConditionText(statusCondition);
            string statusConditionMessage = $"{pokemonStageSlot.Pokemon.Name} Is Now {statusConditionText}";
            PrintRich.PrintLine(TextColor.Yellow, statusConditionMessage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            // Print Message To Console
            string statusConditionText = PrintRich.GetStatusConditionText(statusCondition);
            string statusConditionMessage = $"{pokemonEnemy.Pokemon.Name} Is Now {statusConditionText}";
            PrintRich.PrintLine(TextColor.Red, statusConditionMessage);
        }
    }
}