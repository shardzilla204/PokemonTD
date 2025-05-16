using Godot;
using System.Collections.Generic;

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
    Confuse // #A6A6A6
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
        ApplyStatusColor(defendingPokemon, StatusCondition.Burn);

        float percentage = .0625f; // 1/16

        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            int healthPercent = GetHealthAmount(pokemonStageSlot.Pokemon, percentage);
            DamagePokemonOverTime(pokemonStageSlot, healthPercent, 2);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int healthPercent = GetHealthAmount(pokemonEnemy.Pokemon, percentage);
            DamagePokemonOverTime(pokemonEnemy, healthPercent, 2);
        }
    }

    public async void ApplyFreezeCondition<Defending>(Defending defendingPokemon)
    {
        ApplyStatusColor(defendingPokemon, StatusCondition.Freeze);

        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Fainted += (pokemonStageSlot) => { return; };
        }

        FreezePokemon(defendingPokemon, 5);

        await ToSignal(this, SignalName.Finished);

        ApplyStatusColor(defendingPokemon, StatusCondition.None);
    }

    public async void ApplyParalysisCondition<Defending>(Defending defendingPokemon)
    {
        ApplyStatusColor(defendingPokemon, StatusCondition.Paralysis);
        
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Fainted += (pokemonStageSlot) => { return; };
        }

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
        if (defendingPokemon is StageSlot pokemonStageSlot)
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
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name);
            pokemonEnemy.Pokemon.Speed = pokemonData.Speed;
        }
        EmitSignal(SignalName.Finished);
    }

    public void TrapPokemon<Defending>(Defending defendingPokemon)
    {
        float percentage = .125f; // 1/8

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomIterationCount = RNG.RandiRange(4, 5);

        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            int healthPercent = GetHealthAmount(pokemonStageSlot.Pokemon, percentage);
            DamagePokemonOverTime(pokemonStageSlot, healthPercent, randomIterationCount);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int healthPercent = GetHealthAmount(pokemonEnemy.Pokemon, percentage);
            DamagePokemonOverTime(pokemonEnemy, healthPercent, randomIterationCount);
        }
    }

    private async void ApplyParalysis<Defending>(Defending defendingPokemon)
    {
        float reductionPercent = 0.25f;

        CustomTimer timer = GetTimer(3);
        AddChild(timer);
        _timers.Add(timer);

        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };

            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonStageSlot.Pokemon.Name);
            pokemonStageSlot.Pokemon.Speed = Mathf.RoundToInt(pokemonData.Speed * reductionPercent);
            await ToSignal(timer, Timer.SignalName.Timeout);
            pokemonStageSlot.Pokemon.Speed = pokemonData.Speed;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Fainted += (pokemonEnemy) => { return; };

            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name);
            pokemonEnemy.Pokemon.Speed = Mathf.RoundToInt(pokemonData.Speed * reductionPercent);
            await ToSignal(timer, Timer.SignalName.Timeout);
            pokemonEnemy.Pokemon.Speed = pokemonData.Speed;
        }

        timer.QueueFree();
        ApplyStatusColor(defendingPokemon, StatusCondition.None);
    }

    public void ApplyPoisonCondition<Defending>(Defending defendingPokemon)
    {
        ApplyStatusColor(defendingPokemon, StatusCondition.Poison);

        float percentage = .0625f; // 1/16
        int healthPercent = 0;
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            healthPercent = GetHealthAmount(pokemonStageSlot.Pokemon, percentage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            healthPercent = GetHealthAmount(pokemonEnemy.Pokemon, percentage);
        }
        DamagePokemonOverTime(defendingPokemon, healthPercent, 2);
    }

    private async void DamagePokemonOverTime<Defending>(Defending defendingPokemon, int healthAmount, int iterations)
    {
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Fainted += (pokemonStageSlot) => { return; };
        }

        for (int i = 0; i < iterations; i++)
        {
            CustomTimer timer = GetTimer(1);
            AddChild(timer);

            await ToSignal(timer, Timer.SignalName.Timeout);

            DamagePokemon(defendingPokemon, healthAmount);
            timer.QueueFree();
        }

        ApplyStatusColor(defendingPokemon, StatusCondition.None);
    }

    public async void ApplyBadlyPoisonedCondition<Defending>(Defending defendingPokemon)
    {
        ApplyStatusColor(defendingPokemon, StatusCondition.BadlyPoisoned);

        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Fainted += (pokemonStageSlot) => { return; };
        }

        int iterations = 2;
        float percentage = .0625f; // 1/16
        for (int i = 0; i < iterations; i++)
        {
            CustomTimer timer = GetTimer(1);
            AddChild(timer);

            await ToSignal(timer, Timer.SignalName.Timeout);

            DamagePokemon(defendingPokemon, percentage);
            percentage *= 2;
            timer.QueueFree();
        }

        ApplyStatusColor(defendingPokemon, StatusCondition.None);
    }

    private void DamagePokemon<Defending>(Defending defendingPokemon, int healthAmount)
    {
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.IsActive) pokemonStageSlot.DamagePokemon(healthAmount);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.IsActive) pokemonEnemy.DamagePokemon(healthAmount);
        }
    }

    private void DamagePokemon<Defending>(Defending defendingPokemon, float percentage)
    {
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            int healthAmount = GetHealthAmount(pokemonStageSlot.Pokemon, percentage);
            if (pokemonStageSlot.IsActive) pokemonStageSlot.DamagePokemon(healthAmount);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int healthAmount = GetHealthAmount(pokemonEnemy.Pokemon, percentage);
            if (pokemonEnemy.IsActive) pokemonEnemy.DamagePokemon(healthAmount);
        }
    }

    public async void ApplySleepCondition<Defending>(Defending defendingPokemon)
    {
        ApplyStatusColor(defendingPokemon, StatusCondition.Sleep);
        FreezePokemon(defendingPokemon, 3);
        await ToSignal(this, SignalName.Finished);
        ApplyStatusColor(defendingPokemon, StatusCondition.None);
    }

    public async void ApplyConfuseCondition<Defending>(Defending defendingPokemon)
    {
        ApplyStatusColor(defendingPokemon, StatusCondition.Confuse);
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };

            int pokemonSpeed = pokemonStageSlot.Pokemon.Speed;
            pokemonStageSlot.Pokemon.Speed /= 2;

            CustomTimer timer = GetTimer(3);
            AddChild(timer);

            await ToSignal(timer, Timer.SignalName.Timeout);

            pokemonStageSlot.Pokemon.Speed = pokemonSpeed;

            timer.QueueFree();
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemonEnemy.Pokemon.Name);

            pokemonEnemy.IsMovingForward = false;
            pokemonEnemy.Pokemon.Speed = -pokemonData.Speed;

            CustomTimer timer = GetTimer(1);
            AddChild(timer);

            await ToSignal(timer, Timer.SignalName.Timeout);

            pokemonEnemy.IsMovingForward = true;
            pokemonEnemy.Pokemon.Speed = pokemonData.Speed;
            timer.QueueFree();
        }
        ApplyStatusColor(defendingPokemon, StatusCondition.None);
    }

    private int GetHealthAmount(Pokemon pokemon, float percentage)
    {
        int healthAmount = Mathf.RoundToInt(pokemon.HP * percentage);
        return healthAmount;
    }

    private void ApplyStatusColor<Defending>(Defending defendingPokemon, StatusCondition statusCondtion)
    {
        string statusHexColor = GetStatusHexColor(statusCondtion);
        Color statusColor = Color.FromHtml(statusHexColor);

        if (defendingPokemon is StageSlot pokemonStageSlot)
        {   
            if (pokemonStageSlot.IsActive) pokemonStageSlot.Sprite.SelfModulate = statusColor;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.IsActive) pokemonEnemy.SelfModulate = statusColor;
        }
    }

    private CustomTimer GetTimer(float timeSeconds)
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
        _ => "#FFFFFF"
    };
}