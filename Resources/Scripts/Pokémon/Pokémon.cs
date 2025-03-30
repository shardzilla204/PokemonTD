using Godot;
using GC = Godot.Collections;
using System;
using System.Collections.Generic;

namespace PokémonTD;

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

public enum PokémonStat
{
	HP,
	Attack,
	Defense,
	SpecialAttack,
	SpecialDefense,
	Speed
}

public partial class Pokémon : Node
{
	public new string Name;
	public string NationalNumber;
	public string Species;
   	public string Description;
	public float Height;
	public float Weight;
   	public Texture2D Sprite;
   	public List<PokémonType> Types = new List<PokémonType>();
	public List<PokémonMove> Moves = new List<PokémonMove>();
	public int ExperienceYield;

	public int HP;
	public int Attack;
	public int Defense;
	public int SpecialAttack;
	public int SpecialDefense;
	public int Speed;

   	public int Level = PokémonTD.MinPokémonLevel;
	public int MinExperience;
	public int MaxExperience = 100;

	public Gender Gender;

	public PokémonMove CurrentMove;

	public Pokémon(string pokémonName, GC.Dictionary<string, Variant> pokémonDictionary, GC.Array<string> pokémonTypes, GC.Dictionary<string, Variant> pokémonStats)
	{
		Name = pokémonName;
		Species = pokémonDictionary["Species"].As<string>();
		Height = pokémonDictionary["Height"].As<float>();
		Weight = pokémonDictionary["Weight"].As<float>();
		Description = pokémonDictionary["Description"].As<string>();
		Sprite = PokémonTD.GetPokémonSprite(pokémonName);
		ExperienceYield = pokémonDictionary["Base Experience Yield"].As<int>();
		MaxExperience = GetExperienceRequired();

		foreach (string pokémonType in pokémonTypes) Types.Add(Enum.Parse<PokémonType>(pokémonType));

		HP = pokémonStats["HP"].As<int>();
		Attack = pokémonStats["Attack"].As<int>();
		Defense = pokémonStats["Defense"].As<int>();
		SpecialAttack = pokémonStats["Special Attack"].As<int>();
		SpecialDefense = pokémonStats["Special Defense"].As<int>();
		Speed = pokémonStats["Speed"].As<int>();

		Gender = GetRandomGender();
	}

	public void SetMoveset()
	{
		List<PokémonMove> pokémonMoves = PokémonTD.PokémonManager.GetPokémonMoves(this);

		Moves.Clear();
		Moves.AddRange(pokémonMoves);

		CurrentMove = Moves[0];
	}

	public void SetStats()
	{
		HP = PokémonTD.PokémonManager.GetPokémonHP(this);
		Attack = PokémonTD.PokémonManager.GetOtherPokémonStat(this, PokémonStat.Attack);
		Defense = PokémonTD.PokémonManager.GetOtherPokémonStat(this, PokémonStat.Defense);
		SpecialAttack = PokémonTD.PokémonManager.GetOtherPokémonStat(this, PokémonStat.SpecialAttack);
		SpecialDefense = PokémonTD.PokémonManager.GetOtherPokémonStat(this, PokémonStat.SpecialDefense);
		Speed = PokémonTD.PokémonManager.GetOtherPokémonStat(this, PokémonStat.Speed);

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
	// n = Next Pokémon Level
	public int GetExperienceRequired()
	{
		int nextLevel = Level + 1;
		int experience = Mathf.RoundToInt(6 / 5 * Mathf.Pow(nextLevel, 3) - 15 * Mathf.Pow(nextLevel, 2) + (100 * nextLevel) - 140);
		return experience;
	}
}
