using Godot;
using System;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonCombat : Node
{
    private static PokemonCombat _instance;

    public static PokemonCombat Instance
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

    public void DealDamage<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        if (pokemonMove.Power == 0) return;

        int randomHitCount = PokemonMoveEffect.Instance.GetRandomHitCount(pokemonMove);
        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            DamagePokemonEnemy(pokemonStageSlot, pokemonMove, pokemonEnemy, randomHitCount);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;
            DamagePokemonStageSlot(pokemonEnemy, pokemonMove, pokemonStageSlot, randomHitCount);
        }
    }

    // For moves like Dragon Rage and Sonic Boom
    public void DealDamage<Defending>(Defending defendingPokemon, int damage)
    {
        if (defendingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;
            pokemonStageSlot.DamagePokemon(damage);
        }
        else if (defendingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            pokemonEnemy.DamagePokemon(damage);
        }
    }

    // ? Comment In Value For Higher Damage
    private void DamagePokemonEnemy(PokemonStageSlot pokemonStageSlot, PokemonMove pokemonMove, PokemonEnemy pokemonEnemy, int hitCount)
    {
        for (int i = 0; i < hitCount; i++)
        {
            int pokemonMoveDamage = GetPokemonMoveDamage(pokemonStageSlot, pokemonMove, pokemonEnemy) /* * 100 */;
            pokemonEnemy.DamagePokemon(pokemonMoveDamage);

            string damageMessage = PrintRich.GetDamageMessage(pokemonMoveDamage, pokemonEnemy.Pokemon, pokemonMove);
            PrintRich.Print(TextColor.Orange, damageMessage);
        }
    }

    private void DamagePokemonStageSlot(PokemonEnemy pokemonEnemy, PokemonMove pokemonMove, PokemonStageSlot pokemonStageSlot, int hitCount)
    {
        float damageReduction = 0.1f;
        for (int i = 0; i < hitCount; i++)
        {
            int pokemonMoveDamage = GetPokemonMoveDamage(pokemonEnemy, pokemonMove, pokemonStageSlot);
            pokemonMoveDamage = Mathf.RoundToInt(pokemonMoveDamage * damageReduction);
            pokemonStageSlot.DamagePokemon(pokemonMoveDamage);

            string damageMessage = PrintRich.GetDamageMessage(pokemonMoveDamage, pokemonStageSlot.Pokemon, pokemonMove);
            PrintRich.Print(TextColor.Red, damageMessage);
        }
    }

    private void DamagePokemon<Defending>(Defending defendingPokemon, int damage)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.IsActive) pokemonStageSlot.DamagePokemon(damage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.IsActive) pokemonEnemy.DamagePokemon(damage);
        }
    }

    public async void DamagePokemonOverTime<Defending>(Defending defendingPokemon, int healthAmount, int iterations, StatusCondition statusCondition)
    {
        PokemonStatusCondition.Instance.AddStatusCondition(defendingPokemon, statusCondition);
        for (int i = 0; i < iterations; i++)
        {
            CustomTimer timer = PokemonStatusCondition.Instance.GetTimer(1);
            AddChild(timer);

            await ToSignal(timer, Timer.SignalName.Timeout);

            DamagePokemon(defendingPokemon, healthAmount);
            timer.QueueFree();
        }

        PokemonStatusCondition.Instance.ApplyStatusColor(defendingPokemon, StatusCondition.None);
        PokemonStatusCondition.Instance.RemoveStatusCondition(defendingPokemon, statusCondition);
    }

    // If pokemon move has a status condition that the pokemon already has, get the next pokemon in the queue
    public PokemonStageSlot GetNextPokemonStageSlot(List<PokemonStageSlot> pokemonStageSlots, PokemonMove pokemonMove)
    {
        PokemonStageSlot nextPokemonStageSlot = pokemonStageSlots[0];
        foreach (PokemonStageSlot pokemonStageSlot in pokemonStageSlots)
        {
            foreach (string statusConditionName in pokemonMove.StatusCondition.Keys)
            {
                StatusCondition statusCondition = Enum.Parse<StatusCondition>(statusConditionName);
                bool pokemonHasStatusCondition = PokemonStatusCondition.Instance.PokemonHasStatusCondition(pokemonStageSlot, statusCondition);
                if (!pokemonHasStatusCondition)
                {
                    nextPokemonStageSlot = pokemonStageSlot;
                    break;
                }
            }
        }
        return nextPokemonStageSlot;
    }

    public PokemonEnemy GetNextPokemonEnemy(List<PokemonEnemy> pokemonEnemies, PokemonMove pokemonMove)
    {
        PokemonEnemy nextPokemonEnemy = pokemonEnemies[0];
        foreach (PokemonEnemy pokemonEnemy in pokemonEnemies)
        {
            foreach (string statusConditionName in pokemonMove.StatusCondition.Keys)
            {
                StatusCondition statusCondition = Enum.Parse<StatusCondition>(statusConditionName);
                bool pokemonHasStatusCondition = PokemonStatusCondition.Instance.PokemonHasStatusCondition(pokemonEnemy, statusCondition);
                if (!pokemonHasStatusCondition)
                {
                    nextPokemonEnemy = pokemonEnemy;
                    break;
                }
            }
        }
        return nextPokemonEnemy;
    }

    public int GetPokemonMoveDamage<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        int pokemonMoveDamage = 0;
        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            if (pokemonEnemy.UsedDig && pokemonMove.Name != "Earthquake") return 0;

            pokemonMoveDamage = GetPokemonStageSlotDamage(pokemonStageSlot, pokemonMove, pokemonEnemy);

            bool canCounter = CanCounter(pokemonEnemy.HasCounter, pokemonMove);
            if (!canCounter) return pokemonMoveDamage;
            
            int counterDamage = GetCounterDamage(pokemonMoveDamage);
            pokemonStageSlot.DamagePokemon(counterDamage);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

            if (pokemonStageSlot.UsedDig && pokemonMove.Name != "Earthquake") return 0;

            pokemonMoveDamage = GetPokemonEnemyDamage(pokemonEnemy, pokemonMove, pokemonStageSlot);

            bool canCounter = CanCounter(pokemonEnemy.HasCounter, pokemonMove);
            if (!canCounter) return pokemonMoveDamage;

            int counterDamage = GetCounterDamage(pokemonMoveDamage);
            pokemonEnemy.DamagePokemon(counterDamage);
        }
        return pokemonMoveDamage;
    }

    // Get amount based off the Pokemon's HP
    public int GetDamageAmount(Pokemon pokemon, float percentage)
    {
        int damageAmount = Mathf.RoundToInt(pokemon.HP * percentage);
        return damageAmount;
    }

    private bool CanCounter(bool hasCounter, PokemonMove pokemonMove)
    {
        return hasCounter && pokemonMove.Category == MoveCategory.Physical;
    }

    private int GetCounterDamage(int damage)
    {
        float counterMultiplier = 1.5f;
        return Mathf.RoundToInt(damage * counterMultiplier);
    }

    // Stage Slot = Attacking
    // Pokemon Enemy = Defending
    private int GetPokemonStageSlotDamage(PokemonStageSlot pokemonStageSlot, PokemonMove pokemonMove, PokemonEnemy pokemonEnemy)
    {
        float criticalDamageMultiplier = GetCriticalDamageMultiplier(pokemonStageSlot.Pokemon, pokemonMove);
        int power = GetPower(pokemonEnemy, pokemonMove);
        float attackDefenseRatio = GetAttackDefenseRatio(pokemonStageSlot.Pokemon, pokemonMove, pokemonEnemy.Pokemon);

        float damage = GetDamage(pokemonEnemy.Pokemon, criticalDamageMultiplier, power, attackDefenseRatio);
        damage = AppendDamage(pokemonStageSlot, pokemonMove, damage);

        return Mathf.RoundToInt(damage);
    }

    // Pokemon Enemy = Attacking
    // Stage Slot = Defending
    private int GetPokemonEnemyDamage(PokemonEnemy pokemonEnemy, PokemonMove pokemonMove, PokemonStageSlot pokemonStageSlot)
    {
        float criticalDamageMultiplier = GetCriticalDamageMultiplier(pokemonEnemy.Pokemon, pokemonMove) / 5;
        int power = GetPower(pokemonStageSlot, pokemonMove);
        float attackDefenseRatio = GetAttackDefenseRatio(pokemonEnemy.Pokemon, pokemonMove, pokemonStageSlot.Pokemon);

        float damage = GetDamage(pokemonEnemy.Pokemon, criticalDamageMultiplier, power, attackDefenseRatio);
        damage = AppendDamage(pokemonStageSlot, pokemonMove, damage);

        return Mathf.RoundToInt(damage);
    }

    private float GetDamage(Pokemon attackingPokemon, float criticalDamageMultiplier, int power, float attackDefenseRatio)
    {
        return (((5 * attackingPokemon.Level * criticalDamageMultiplier) + 4) * power * attackDefenseRatio / 25) + 4;
    }

    private float AppendDamage<Defending>(Defending defendingPokemon, PokemonMove pokemonMove, float damage)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            damage = ApplyTypeMultiplers(pokemonStageSlot.Pokemon, pokemonMove, damage);
            damage = HalveDamage(pokemonStageSlot, pokemonMove, damage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            damage = ApplyTypeMultiplers(pokemonEnemy.Pokemon, pokemonMove, damage);
            damage = HalveDamage(pokemonEnemy, pokemonMove, damage);
        }
        return damage;
    }

    public bool HasPokemonMoveHit<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        try
        {
            if (pokemonMove.Name == "Swift") return true;

            if (attackingPokemon is PokemonStageSlot)
            {
                PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
                PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

                if (pokemonMove.Accuracy != 0)
                {
                    // Print Message To Console
                    string missedMessage = $"{pokemonStageSlot.Pokemon.Name}'s {pokemonMove.Name} Missed";
                    PrintRich.PrintLine(TextColor.Purple, missedMessage);
                }

                bool hasPassedAccuracy = HasPassedAccuracy(pokemonStageSlot.Pokemon, pokemonMove, pokemonEnemy.Pokemon);
                if (!hasPassedAccuracy) ApplyMissDamage(pokemonStageSlot, pokemonMove);

                return hasPassedAccuracy;
            }
            else if (attackingPokemon is PokemonEnemy)
            {
                PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
                PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

                bool hasPassedAccuracy = HasPassedAccuracy(pokemonEnemy.Pokemon, pokemonMove, pokemonStageSlot.Pokemon);
                if (!hasPassedAccuracy) ApplyMissDamage(pokemonEnemy, pokemonMove);

                return hasPassedAccuracy;
            }

            return false;
        }
        catch (NullReferenceException error)
        {
            GD.PrintErr(error);
            return false;
        }
    }

    private bool HasPassedAccuracy(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        int accuracy = GetAccuracy(attackingPokemon, pokemonMove, defendingPokemon);

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomThreshold = Mathf.RoundToInt(RNG.RandfRange(0, 100));
        randomThreshold -= accuracy;

        return randomThreshold <= 0;
    }

    private void ApplyMissDamage<Attacking>(Attacking attackingPokemon, PokemonMove pokemonMove)
    {
        bool isMissMove = PokemonMoveEffect.Instance.MissMoves.IsMissMove(pokemonMove);
        if (!isMissMove) return;

        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            int damage = pokemonStageSlot.Pokemon.HP / 2;
            pokemonStageSlot.DamagePokemon(damage);

        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damage = pokemonEnemy.Pokemon.HP / 2;
            pokemonEnemy.DamagePokemon(damage);
        }
    }

    private int GetAccuracy(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        return Mathf.RoundToInt((attackingPokemon.Accuracy - defendingPokemon.Evasion) * pokemonMove.Accuracy);
    }
    
    // Earthquake Doubles It's Power If Opposing Pokemon Used Dig
    private int GetPower<Defending>(Defending defendingPokemon, PokemonMove pokemonMove)
    {
        int power = pokemonMove.Power;
        if (pokemonMove.Name != "Earthquake") return power;

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.UsedDig) power *= 2;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.UsedDig) power *= 2;
        }
        return power;
    }

    private float ApplyTypeMultiplers(Pokemon defendingPokemon, PokemonMove pokemonMove, float damage)
    {
        List<float> typeMultipliers = PokemonTypes.Instance.GetTypeMultipliers(pokemonMove.Type, defendingPokemon.Types);
        foreach (float typeMultiplier in typeMultipliers)
        {
            damage *= typeMultiplier;
        }
        return damage;
    }

    // Light Screen & Reflect Pokemon Moves Halve Damage 
    private float HalveDamage<Defending>(Defending defendingPokemon, PokemonMove pokemonMove, float damage)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.LightScreenCount > 0 && pokemonMove.Category == MoveCategory.Special)
            {
                damage /= 2;
            }
            else if (pokemonStageSlot.ReflectCount > 0 && pokemonMove.Category == MoveCategory.Physical)
            {
                damage /= 2;
            }
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.LightScreenCount > 0 && pokemonMove.Category == MoveCategory.Special)
            {
                damage /= 2;
            }
            else if (pokemonEnemy.ReflectCount > 0 && pokemonMove.Category == MoveCategory.Physical)
            {
                damage /= 2;
            }
        }
        return damage;
    }

    private float GetAttackDefenseRatio(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        float specialRatio = (float)attackingPokemon.SpecialAttack / defendingPokemon.SpecialDefense;
        float normalRatio = (float)attackingPokemon.Attack / defendingPokemon.Defense;

        float attackDefenseRatio = pokemonMove.Category == MoveCategory.Special ? specialRatio : normalRatio;

        return (float) Math.Round(attackDefenseRatio, 2) / 50;
    }
    
    private float GetCriticalDamageMultiplier(Pokemon pokemon, PokemonMove pokemonMove)
    {
        return IsCriticalHit(pokemon, pokemonMove) ? 2 : 1;
    }

    private bool IsCriticalHit(Pokemon pokemon, PokemonMove pokemonMove)
    {
        float criticalHitRatio = PokemonMoveEffect.Instance.GetCriticalHitRatio(pokemon, pokemonMove);
        int maxThreshold = 255;

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int criticalValue = Mathf.RoundToInt(RNG.RandfRange(criticalHitRatio, maxThreshold));
        int randomThreshold = Mathf.RoundToInt(RNG.RandfRange(0, maxThreshold));
        randomThreshold -= criticalValue;

        bool isCriticalHit = randomThreshold <= 0;

		if (isCriticalHit) 
        {
            // Print Message To Console
            string criticalHitMessage = $"{pokemon.Name} Has Landed A Critical Hit";
            PrintRich.PrintLine(TextColor.Purple, criticalHitMessage);
        }
        return isCriticalHit;
    }
}