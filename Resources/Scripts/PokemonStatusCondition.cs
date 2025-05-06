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
    
    public void ApplyBurnCondition(PokemonEnemy pokemonEnemy)
    {
        ApplyStatusColor(pokemonEnemy, StatusCondition.Burn);

        float percentage = .0625f; // 1/16
        int healthPercent = GetHealthAmount(pokemonEnemy.Pokemon, percentage);
        DamagePokemon(pokemonEnemy, healthPercent);
    }

    public void ApplyFreezeCondition(PokemonEnemy pokemonEnemy)
    {
        ApplyStatusColor(pokemonEnemy, StatusCondition.Freeze);
        FreezePokemon(pokemonEnemy, 5f);
    }

    public void ApplyParalysisCondition(PokemonEnemy pokemonEnemy)
    {
        ApplyStatusColor(pokemonEnemy, StatusCondition.Paralysis);

        if (IsFullyParalyzed()) 
        {
            FreezePokemon(pokemonEnemy, 3f);
            return;
        }

        ApplyParalysis(pokemonEnemy);
    }

    private bool IsFullyParalyzed()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float fullParalysisThreshold = 0.25f;
        float randomValue = RNG.RandfRange(0, 1);
        return fullParalysisThreshold >= randomValue;
    }

    private async void FreezePokemon(PokemonEnemy pokemonEnemy, float timeSeconds)
    {
        pokemonEnemy.CanMove = false;

        await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);

        pokemonEnemy.CanMove = true;
        ApplyStatusColor(pokemonEnemy, StatusCondition.None);
    }

    private async void ApplyParalysis(PokemonEnemy pokemonEnemy)
    {
        float reductionPercent = 0.25f;
        int pokemonSpeed = pokemonEnemy.Speed;
        pokemonEnemy.Speed = Mathf.RoundToInt(pokemonSpeed * reductionPercent);

        float timeSeconds = 3f;
        await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);

        pokemonEnemy.Speed = pokemonSpeed;
        ApplyStatusColor(pokemonEnemy, StatusCondition.None);
    }

    public void ApplyPoisonCondition(PokemonEnemy pokemonEnemy)
    {
        ApplyStatusColor(pokemonEnemy, StatusCondition.Poison);

        float percentage = .0625f; // 1/16
        int healthPercent = GetHealthAmount(pokemonEnemy.Pokemon, percentage);
        DamagePokemon(pokemonEnemy, healthPercent);
    }

    private async void DamagePokemon(PokemonEnemy pokemonEnemy, int healthAmount)
    {
        float timeSeconds = 1f;
        int iterations = 3;
        for (int i = 0; i < iterations; i++)
        {
            await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);
            pokemonEnemy.DamagePokemon(healthAmount);
        }
        ApplyStatusColor(pokemonEnemy, StatusCondition.None);
    }

    public async void ApplyBadlyPoisonedCondition(PokemonEnemy pokemonEnemy)
    {
        ApplyStatusColor(pokemonEnemy, StatusCondition.BadlyPoisoned);

        float timeSeconds = 1f;
        int iterations = 3;
        float percentage = .0625f; // 1/16
        for (int i = 0; i < iterations; i++)
        {
            await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);
            int healthAmount = GetHealthAmount(pokemonEnemy.Pokemon, percentage);
            pokemonEnemy.DamagePokemon(healthAmount);
            percentage *= 2;
        }

        ApplyStatusColor(pokemonEnemy, StatusCondition.None);
    }

    public void ApplySleepCondition(PokemonEnemy pokemonEnemy)
    {
        ApplyStatusColor(pokemonEnemy, StatusCondition.Sleep);
        FreezePokemon(pokemonEnemy, 3f);
    }

    public async void ApplyConfuseCondition(PokemonEnemy pokemonEnemy)
    {
        pokemonEnemy.Speed = -pokemonEnemy.Speed;
        ApplyStatusColor(pokemonEnemy, StatusCondition.Confuse);

        float timeSeconds = 3f;
        await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);

        pokemonEnemy.Speed = -pokemonEnemy.Speed;
        ApplyStatusColor(pokemonEnemy, StatusCondition.None);
    }

    private int GetHealthAmount(Pokemon pokemon, float percentage)
    {
        int healthAmount = Mathf.RoundToInt(pokemon.HP * percentage);
        return healthAmount;
    }

    private void ApplyStatusColor(PokemonEnemy pokemonEnemy, StatusCondition statusCondtion)
    {
        string statusHexColor = GetStatusHexColor(statusCondtion);
        Color statusColor = Color.FromHtml(statusHexColor);
        pokemonEnemy.SelfModulate = statusColor;
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