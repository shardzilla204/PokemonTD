using Godot;
using GC = Godot.Collections;

using System;
using System.Collections.Generic;

namespace PokemonTD;

public enum EvolutionStone
{
	None,
	Fire,
	Water,
	Thunder,
	Leaf,
	Moon
}

public enum Gender
{
	Male,
	Female
}

public enum PokemonStat
{
	HP,
	Attack,
	Defense,
	SpecialAttack,
	SpecialDefense,
	Speed
}

public partial class Pokemon : Node
{
	public new string Name;
	public string NationalNumber;
	public string Species;
   	public string Description;
	public float Height;
	public float Weight;
   	public Texture2D Sprite;
   	public List<PokemonType> Types = new List<PokemonType>();
	public List<PokemonMove> Moves = new List<PokemonMove>();
	public int ExperienceYield;

	public int HP;
	public int Attack;
	public int Defense;
	public int SpecialAttack;
	public int SpecialDefense;
	public int Speed;

   	public int Level = PokemonTD.MinPokemonLevel;
	public int MinExperience;
	public int MaxExperience = 100;

	public Gender Gender;

	public PokemonMove Move;

	private List<PokemonMove> _oldMoves = new List<PokemonMove>();

	public Pokemon(string pokemonName, GC.Dictionary<string, Variant> pokemonDictionary, GC.Array<string> pokemonTypes, GC.Dictionary<string, Variant> pokemonStats)
	{
		Name = pokemonName;
		Species = pokemonDictionary["Species"].As<string>();
		Height = pokemonDictionary["Height"].As<float>();
		Weight = pokemonDictionary["Weight"].As<float>();
		Description = pokemonDictionary["Description"].As<string>();
		Sprite = PokemonTD.GetPokemonSprite(pokemonName);
		ExperienceYield = pokemonDictionary["Base Experience Yield"].As<int>();
		MaxExperience = GetExperienceRequired();

		foreach (string pokemonType in pokemonTypes) 
		{
			Types.Add(Enum.Parse<PokemonType>(pokemonType));
		}

		HP = pokemonStats["HP"].As<int>();
		Attack = pokemonStats["Attack"].As<int>();
		Defense = pokemonStats["Defense"].As<int>();
		SpecialAttack = pokemonStats["Special Attack"].As<int>();
		SpecialDefense = pokemonStats["Special Defense"].As<int>();
		Speed = pokemonStats["Speed"].As<int>();
	}

	public Pokemon(string pokemonName, int pokemonLevel)
	{
		Name = pokemonName;

		Gender = GetRandomGender();

		Level = pokemonLevel;
		Moves = GetMoveset();

		Move = Moves[0];
	}

	public void SetLevel(int level)
	{
		// Default to the minimum level if below the threshold
		level = level < PokemonTD.MinPokemonLevel ? PokemonTD.MinPokemonLevel : level;
		Level = PokemonTD.AreLevelsRandomized ? PokemonTD.GetRandomLevel() : level;
	}

	// Call when leveled up
	public void FindAndAddPokemonMove()
	{
		PokemonMove pokemonMove = PokemonTD.PokemonManager.GetPokemonMoveFromLevelUp(this, _oldMoves);

		if (pokemonMove is null) return;

		if (Moves.Contains(pokemonMove)) return;

		AddPokemonMove(pokemonMove);
		_oldMoves.Add(pokemonMove);
	}

	public List<PokemonMove> GetMoveset()
	{
		List<PokemonMove> pokemonMoves = PokemonTD.PokemonManager.GetPokemonMoves(this);
		_oldMoves.AddRange(pokemonMoves);

		if (pokemonMoves.Count > PokemonTD.MaxMoveCount)
		{
			pokemonMoves = GetRandomMoveset(pokemonMoves);
		}
		return pokemonMoves;
	}

	public List<PokemonMove> GetRandomMoveset(List<PokemonMove> pokemonMoves)
	{
		List<PokemonMove> randomPokemonMoves = new List<PokemonMove>();
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		for (int i = 0; i < PokemonTD.MaxMoveCount; i++)
		{
			while (true)
			{
				int randomMoveIndex = RNG.RandiRange(0, pokemonMoves.Count - 1);
				PokemonMove randomPokemonMove = pokemonMoves[randomMoveIndex];

				if (!randomPokemonMoves.Contains(randomPokemonMove)) 
				{
					randomPokemonMoves.Add(randomPokemonMove);
					break;
				}
			}
		}
		return randomPokemonMoves;
	}

	private void AddPokemonMove(PokemonMove pokemonMove)
	{
		if (Moves.Count < PokemonTD.MaxMoveCount) 
		{
			Moves.Add(pokemonMove);
		}
		else
		{
			PokemonTD.Signals.EmitSignal(Signals.SignalName.ForgetMove, this, pokemonMove);
		}
	}

	public void SetStats()
	{
		Pokemon pokemonData = PokemonTD.PokemonManager.GetPokemonData(Name);

		HP = PokemonTD.PokemonManager.GetPokemonHP(pokemonData);
		Attack = PokemonTD.PokemonManager.GetOtherPokemonStat(pokemonData, PokemonStat.Attack);
		Defense = PokemonTD.PokemonManager.GetOtherPokemonStat(pokemonData, PokemonStat.Defense);
		SpecialAttack = PokemonTD.PokemonManager.GetOtherPokemonStat(pokemonData, PokemonStat.SpecialAttack);
		SpecialDefense = PokemonTD.PokemonManager.GetOtherPokemonStat(pokemonData, PokemonStat.SpecialDefense);
		Speed = PokemonTD.PokemonManager.GetOtherPokemonStat(pokemonData, PokemonStat.Speed);

		PrintRich.PrintStats(TextColor.Purple, this);
	}

	private Gender GetRandomGender()
	{
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		int randomValue = RNG.RandiRange((int) Gender.Male, (int) Gender.Female);

		return (Gender) randomValue;
	}

	// ? EXP Formula
	// EXP = 6/5n^3 - 15n^2 + 100n - 140
	// n = Next Pokemon Level
	public int GetExperienceRequired()
	{
		int nextLevel = Level + 1;
		int experience = Mathf.RoundToInt(6 / 5 * Mathf.Pow(nextLevel, 3) - 15 * Mathf.Pow(nextLevel, 2) + (100 * nextLevel) - 140);
		return experience;
	}
}
