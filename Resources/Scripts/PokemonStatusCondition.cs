using Godot;

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

    public override void _EnterTree()
    {
        Instance = this;
    }

    public void ApplyBurnCondition<T>(T parameter)
    {
        ApplyStatusColor(parameter, StatusCondition.Burn);
        float percentage = .0625f; // 1/16
        if (parameter is StageSlot pokemonStageSlot)
        {
            int healthPercent = GetHealthAmount(pokemonStageSlot.Pokemon, percentage);
            DamagePokemonOverTime(pokemonStageSlot, healthPercent);
        }
        else if (parameter is PokemonEnemy pokemonEnemy)
        {
            int healthPercent = GetHealthAmount(pokemonEnemy.Pokemon, percentage);
            DamagePokemonOverTime(pokemonEnemy, healthPercent);
        }
    }

    public async void ApplyFreezeCondition<T>(T parameter)
    {
        ApplyStatusColor(parameter, StatusCondition.Freeze);
        FreezePokemon(parameter, 5f);
        await ToSignal(this, SignalName.Finished);
        ApplyStatusColor(parameter, StatusCondition.None);
    }

    public async void ApplyParalysisCondition<T>(T parameter)
    {
        ApplyStatusColor(parameter, StatusCondition.Paralysis);

        if (IsFullyParalyzed()) 
        {
            FreezePokemon(parameter, 3f);
            await ToSignal(this, SignalName.Finished);
            return;
        }

        ApplyParalysis(parameter);
    }

    private bool IsFullyParalyzed()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float fullParalysisThreshold = 0.25f;
        float randomValue = RNG.RandfRange(0, 1);
        return fullParalysisThreshold >= randomValue;
    }

    public async void FreezePokemon<T>(T parameter, float timeSeconds)
    {
        if (parameter is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.IsActive = false;
            await ToSignal(GetTree().CreateTimer(timeSeconds / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout);
            pokemonStageSlot.IsActive = true;
        }
        else if (parameter is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.CanMove = false;
            await ToSignal(GetTree().CreateTimer(timeSeconds / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout);
            pokemonEnemy.CanMove = true;
        }
    }

    private async void ApplyParalysis<T>(T parameter)
    {
        float reductionPercent = 0.25f;
        float timeSeconds = 3f;

        if (parameter is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };

            int pokemonSpeed = pokemonStageSlot.Pokemon.Speed;
            pokemonStageSlot.Pokemon.Speed = Mathf.RoundToInt(pokemonSpeed * reductionPercent);
            await ToSignal(GetTree().CreateTimer(timeSeconds / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout);
            pokemonStageSlot.Pokemon.Speed = pokemonSpeed;
        }
        else if (parameter is PokemonEnemy pokemonEnemy)
        {
            int pokemonSpeed = pokemonEnemy.Pokemon.Speed;
            pokemonEnemy.Pokemon.Speed = Mathf.RoundToInt(pokemonSpeed * reductionPercent);
            await ToSignal(GetTree().CreateTimer(timeSeconds / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout);
            pokemonEnemy.Pokemon.Speed = pokemonSpeed;
        }

        ApplyStatusColor(parameter, StatusCondition.None);
    }

    public void ApplyPoisonCondition<T>(T parameter)
    {
        ApplyStatusColor(parameter, StatusCondition.Poison);

        float percentage = .0625f; // 1/16
        int healthPercent = 0;
        if (parameter is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };
            healthPercent = GetHealthAmount(pokemonStageSlot.Pokemon, percentage);
        }
        else if (parameter is PokemonEnemy pokemonEnemy)
        {
            healthPercent = GetHealthAmount(pokemonEnemy.Pokemon, percentage);
        }
        DamagePokemonOverTime(parameter, healthPercent);
    }

    private async void DamagePokemonOverTime<T>(T parameter, int healthAmount)
    {
        float timeSeconds = 1f;
        int iterations = 2;
        for (int i = 0; i < iterations; i++)
        {
            await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);
            if (parameter is StageSlot pokemonStageSlot)
            {
                pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };
                pokemonStageSlot.DamagePokemon(healthAmount);
            }
            else if (parameter is PokemonEnemy pokemonEnemy)
            {
                pokemonEnemy.DamagePokemon(healthAmount);
            }
        }
        ApplyStatusColor(parameter, StatusCondition.None);
    }

    public async void ApplyBadlyPoisonedCondition<T>(T parameter)
    {
        ApplyStatusColor(parameter, StatusCondition.BadlyPoisoned);

        float timeSeconds = 1f;
        int iterations = 2;
        float percentage = .0625f; // 1/16
        for (int i = 0; i < iterations; i++)
        {
            await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);
            if (parameter is StageSlot pokemonStageSlot)
            {
                pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };

                int healthAmount = GetHealthAmount(pokemonStageSlot.Pokemon, percentage);
                pokemonStageSlot.DamagePokemon(healthAmount);
            }
            else if (parameter is PokemonEnemy pokemonEnemy)
            {
                int healthAmount = GetHealthAmount(pokemonEnemy.Pokemon, percentage);
                pokemonEnemy.DamagePokemon(healthAmount);
            }
            percentage *= 2;
        }

        ApplyStatusColor(parameter, StatusCondition.None);
    }

    public async void ApplySleepCondition<T>(T parameter)
    {
        ApplyStatusColor(parameter, StatusCondition.Sleep);
        FreezePokemon(parameter, 3f);
        await ToSignal(this, SignalName.Finished);
        ApplyStatusColor(parameter, StatusCondition.None);
    }

    public async void ApplyConfuseCondition<T>(T parameter)
    {
        ApplyStatusColor(parameter, StatusCondition.Confuse);
        if (parameter is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };

            FreezePokemon(pokemonStageSlot, 2f);
            await ToSignal(this, SignalName.Finished);
        }
        else if (parameter is PokemonEnemy pokemonEnemy)
        {
            float timeSeconds = 1f;
            pokemonEnemy.Pokemon.Speed = -pokemonEnemy.Pokemon.Speed;
            await ToSignal(GetTree().CreateTimer(timeSeconds / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout);
            pokemonEnemy.Pokemon.Speed = -pokemonEnemy.Pokemon.Speed;
        }
        ApplyStatusColor(parameter, StatusCondition.None);
    }

    public async void ApplyConfuseCondition(StageSlot pokemonStageSlot)
    {
        int pokemonSpeed = pokemonStageSlot.Pokemon.Speed;
        pokemonStageSlot.Pokemon.Speed /= 2;
        ApplyStatusColor(pokemonStageSlot, StatusCondition.Confuse);

        float timeSeconds = 3f;
        await ToSignal(GetTree().CreateTimer(timeSeconds / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout);

        pokemonStageSlot.Pokemon.Speed = pokemonSpeed;
        ApplyStatusColor(pokemonStageSlot, StatusCondition.None);
    }

    private int GetHealthAmount(Pokemon pokemon, float percentage)
    {
        int healthAmount = Mathf.RoundToInt(pokemon.HP * percentage);
        return healthAmount;
    }

    private void ApplyStatusColor<T>(T parameter, StatusCondition statusCondtion)
    {
        string statusHexColor = GetStatusHexColor(statusCondtion);
        Color statusColor = Color.FromHtml(statusHexColor);

        if (parameter is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Sprite.SelfModulate = statusColor;
        }
        else if (parameter is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.SelfModulate = statusColor;
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
        _ => "#FFFFFF"
    };
}