using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public void ApplyDamage<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
	{
        int randomHitCount = PokemonMoveEffect.Instance.GetRandomHitCount(pokemonMove);
        if (attackingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = attackingPokemon as StageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            ApplyEnemyDamage(pokemonStageSlot, pokemonMove, pokemonEnemy, randomHitCount);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;
            ApplyStageSlotDamage(pokemonEnemy, pokemonMove, pokemonStageSlot, randomHitCount);
        }
	}

    public void ApplyDamage<Defending>(Defending defendingPokemon, int damage)
	{
        if (defendingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;
            ApplyStageSlotDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            ApplyEnemyDamage(pokemonEnemy, damage);
        }
	}    
    
    // ? Comment 
    public void ApplyEnemyDamage(StageSlot pokemonStageSlot, PokemonMove pokemonMove, PokemonEnemy pokemonEnemy, int hitCount)
    {
        for (int i = 0; i < hitCount; i++)
        {
            int damage = PokemonManager.Instance.GetDamage(pokemonStageSlot, pokemonMove, pokemonEnemy) /* * 100 */;
            pokemonEnemy.DamagePokemon(damage);

            string damageMessage = PrintRich.GetDamageMessage(damage, pokemonEnemy.Pokemon, pokemonMove);
            PrintRich.Print(TextColor.Orange, damageMessage);
        }
    }

    public void ApplyEnemyDamage(PokemonEnemy pokemonEnemy, int damage)
    {
        pokemonEnemy.DamagePokemon(damage);
    }

    public void ApplyStageSlotDamage(PokemonEnemy pokemonEnemy, PokemonMove pokemonMove, StageSlot pokemonStageSlot, int hitCount)
    {
        float damageReduction = 0.1f;
        for (int i = 0; i < hitCount; i++)
        {
            int damage = PokemonManager.Instance.GetDamage(pokemonEnemy, pokemonMove, pokemonStageSlot);
            damage = Mathf.RoundToInt(damage * damageReduction);
            pokemonStageSlot.DamagePokemon(damage);

            string damageMessage = PrintRich.GetDamageMessage(damage, pokemonStageSlot.Pokemon, pokemonMove);
            PrintRich.Print(TextColor.Red, damageMessage);
        }
    }

    public void ApplyStageSlotDamage(StageSlot pokemonStageSlot, int damage)
    {
        pokemonStageSlot.DamagePokemon(damage);
    }

    public void AttackAllPokemon<Defending>(Defending defendingPokemon, int damage)
    {
        if (defendingPokemon is List<PokemonEnemy> pokemonEnemies)
        {
            foreach (PokemonEnemy pokemonEnemy in pokemonEnemies)
            {
                pokemonEnemy.DamagePokemon(damage);
            }
        }
        else if (defendingPokemon is List<StageSlot> pokemonStageSlots)
        {
            foreach (StageSlot pokemonStageSlot in pokemonStageSlots)
            {
                pokemonStageSlot.DamagePokemon(damage);
            }
        }
    }
        
    public void ApplyStatChanges(Pokemon pokemon, List<StatMove> statMoves)
	{
		if (statMoves.Count <= 0) return;
		
        PrintRich.Print(TextColor.Blue, "Before");
        PrintRich.PrintStats(TextColor.Blue, pokemon);
        foreach (StatMove statMove in statMoves)
        {
            if (!CanApplyStatChange(statMove)) continue;
            PokemonMoveEffect.Instance.ChangeStat(pokemon, statMove);
        }
        PrintRich.Print(TextColor.Blue, "After");
        PrintRich.PrintStats(TextColor.Blue, pokemon);
	}

    private bool CanApplyStatChange(StatMove statMove)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomValue = Mathf.RoundToInt(RNG.RandfRange(0, 255));
        randomValue = randomValue - statMove.Probability;

        return randomValue <= 0;
    }

    public void ApplyStatusConditions<T>(int teamSlotIndex, T parameter, PokemonMove pokemonMove)
	{
		if (pokemonMove.StatusCondition.Count == 0) return;
		
		List<string> statusNames = pokemonMove.StatusCondition.Keys.ToList();
		foreach (string statusName in statusNames)
		{
			if (!CanApplyStatusCondition(pokemonMove, statusName)) continue;
			
			StatusCondition statusCondition = Enum.Parse<StatusCondition>(statusName);
            if (parameter is StageSlot stageSlot)
            {
                AddStatusCondition(stageSlot, statusCondition);
            }
            else if (parameter is PokemonEnemy pokemonEnemy)
            {
                pokemonEnemy.TeamSlotIndex = teamSlotIndex;
                AddStatusCondition(pokemonEnemy, statusCondition);
            }
		}
	}

    private bool CanApplyStatusCondition(PokemonMove pokemonMove, string statusName)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float hitThreshold = pokemonMove.StatusCondition[statusName].As<float>();
        float randomValue = RNG.RandfRange(0, 1);
        randomValue -= hitThreshold;

        return randomValue <= 0;
    }

    public void AddStatusCondition<T>(T parameter, StatusCondition statusCondition)
	{
		switch (statusCondition)
		{
			case StatusCondition.Burn: 
				PokemonStatusCondition.Instance.ApplyBurnCondition(parameter); 
			break;
			case StatusCondition.Freeze: 
				PokemonStatusCondition.Instance.ApplyFreezeCondition(parameter); 
			break;
			case StatusCondition.Paralysis: 
				PokemonStatusCondition.Instance.ApplyParalysisCondition(parameter); 
			break;
			case StatusCondition.Poison: 
				PokemonStatusCondition.Instance.ApplyPoisonCondition(parameter); 
			break;
			case StatusCondition.BadlyPoisoned: 
				PokemonStatusCondition.Instance.ApplyBadlyPoisonedCondition(parameter); 
			break;
			case StatusCondition.Sleep: 
				PokemonStatusCondition.Instance.ApplySleepCondition(parameter); 
			break;
			case StatusCondition.Confuse: 
				PokemonStatusCondition.Instance.ApplyConfuseCondition(parameter); 
			break;
		}
        
        if (parameter is StageSlot pokemonStageSlot)
        {
            string conditionMessage = $"{pokemonStageSlot.Pokemon.Name} Is Now {PrintRich.GetStatusConditionMessage(statusCondition)}";
            PrintRich.PrintLine(TextColor.Yellow, conditionMessage);
        }
        else if (parameter is PokemonEnemy pokemonEnemy)
        {
            string conditionMessage = $"{pokemonEnemy.Pokemon.Name} Is Now {PrintRich.GetStatusConditionMessage(statusCondition)}";
            PrintRich.PrintLine(TextColor.Red, conditionMessage);
        }
	}
}