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

    public void ApplyDamage<Attacking, Defending>(Attacking attacking, PokemonMove pokemonMove, Defending defending)
	{
        if (defending is StageSlot)
        {
            int damage = 0;
            StageSlot pokemonStageSlot = defending as StageSlot;
            Pokemon defendingPokemon = pokemonStageSlot.Pokemon;
            if (attacking is PokemonEnemy pokemonEnemy)
            {
                Pokemon attackingPokemon = pokemonEnemy.Pokemon;
                damage = PokemonManager.Instance.GetDamage(attackingPokemon, pokemonMove, defendingPokemon);
            }
            pokemonStageSlot.DamagePokemon(damage);
            
            string damageMessage = PrintRich.GetDamageMessage(damage, defendingPokemon, pokemonMove);
            PrintRich.Print(TextColor.Red, damageMessage);
        }
        else if (defending is PokemonEnemy)
        {
            int damage = 0;
            PokemonEnemy pokemonEnemy = defending as PokemonEnemy;
            Pokemon defendingPokemon = pokemonEnemy.Pokemon;
            if (attacking is StageSlot pokemonStageSlot)
            {
                Pokemon attackingPokemon = pokemonStageSlot.Pokemon;
                damage = PokemonManager.Instance.GetDamage(attackingPokemon, pokemonMove, defendingPokemon);
            }
            pokemonEnemy.DamagePokemon(damage);
            
            string damageMessage = PrintRich.GetDamageMessage(damage, defendingPokemon, pokemonMove);
            PrintRich.Print(TextColor.Yellow, damageMessage);
        }
	}

    public void ApplyStatChanges(Pokemon pokemon, PokemonMove pokemonMove)
	{
		List<StatMove> statIncreaseMoves = PokemonMoveEffect.Instance.StatMoves.FindIncreaseStatMoves(pokemonMove);
		List<StatMove> statDecreaseMoves = PokemonMoveEffect.Instance.StatMoves.FindDecreaseStatMoves(pokemonMove);

        if (statIncreaseMoves.Count <= 0 && statDecreaseMoves.Count <= 0) return;

        PrintRich.Print(TextColor.Blue, "Before");
        PrintRich.PrintStats(TextColor.Blue, pokemon);
		if (statIncreaseMoves.Count > 0)
		{
			foreach (StatMove statIncreaseMove in statIncreaseMoves)
			{
				PokemonMoveEffect.Instance.ChangeStat(pokemon, statIncreaseMove);
			}
		} 
		else if (statDecreaseMoves.Count > 0)
		{
			foreach (StatMove statDecreaseMove in statDecreaseMoves)
			{
				PokemonMoveEffect.Instance.ChangeStat(pokemon, statDecreaseMove);
			}
		}
        PrintRich.Print(TextColor.Blue, "After");
        PrintRich.PrintStats(TextColor.Blue, pokemon);
	}

    public void ApplyStatusConditions<T>(int teamSlotIndex, T parameter, PokemonMove pokemonMove)
	{
		if (pokemonMove.StatusCondition.Count == 0) return;
		
		List<string> statusNames = pokemonMove.StatusCondition.Keys.ToList();
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		foreach (string statusName in statusNames)
		{
			float hitThreshold = pokemonMove.StatusCondition[statusName].As<float>();
			float randomValue = RNG.RandfRange(0, 1);
			randomValue -= hitThreshold;

			if (randomValue > 0) continue;
			
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