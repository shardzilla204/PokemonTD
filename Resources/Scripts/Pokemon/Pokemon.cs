using Godot;
using GC = Godot.Collections;

using System;
using System.Collections.Generic;

namespace PokemonTD;

public enum EvolutionStone
{
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
	public string BaseName;
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
	public int MaxExperience = 10;

	public Gender Gender;

	public PokemonMove Move;
	public List<PokemonMove> OldMoves = new List<PokemonMove>();

	public Pokemon(string pokemonName, GC.Dictionary<string, Variant> pokemonDictionary, GC.Array<string> pokemonTypes, GC.Dictionary<string, Variant> pokemonStats)
	{
		BaseName = pokemonName;
		Name = pokemonName;
		NationalNumber = pokemonDictionary["National Number"].As<string>();
		Species = pokemonDictionary["Species"].As<string>();
		Height = pokemonDictionary["Height"].As<float>();
		Weight = pokemonDictionary["Weight"].As<float>();
		Description = pokemonDictionary["Description"].As<string>();
		Sprite = GetPokemonSprite(pokemonName);
		ExperienceYield = pokemonDictionary["Base Experience Yield"].As<int>();

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

		if (pokemonName.Contains("Nido"))
		{
			AssignGender(pokemonName);
			return;
		}
		Gender = GetRandomGender();
	}

	// For Nidoran Female & Nidoran Male
	private void AssignGender(string pokemonName)
	{
		foreach (string nidoranFemaleString in PokemonManager.Instance.NidoranFemaleStrings)
		{
			if (!pokemonName.Contains(nidoranFemaleString)) continue;
			
			Gender = Gender.Female;
			return;
		}

		foreach (string nidoranMaleString in PokemonManager.Instance.NidoranMaleStrings)
		{
			if (!pokemonName.Contains(nidoranMaleString)) continue;
			
			Gender = Gender.Male;
			return;
		}
	}

	public void SetMoves(List<PokemonMove> pokemonMoves)
	{
		Moves.AddRange(pokemonMoves);
		Move = Moves[0];
	}

	public void SetLevel(int level)
	{
		// Default to the minimum level if below the threshold
		level = level < PokemonTD.MinPokemonLevel ? PokemonTD.MinPokemonLevel : level;
		Level = PokemonTD.AreLevelsRandomized ? PokemonTD.GetRandomLevel() : level;
	}

	private Texture2D GetPokemonSprite(string pokemonName)
    {
		string filePath = $"res://Assets/Images/Pokemon/{pokemonName}.png";
        return ResourceLoader.Load<Texture2D>(filePath);
    }

	private Gender GetRandomGender()
	{
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		int randomValue = RNG.RandiRange((int) Gender.Male, (int) Gender.Female);

		return (Gender) randomValue;
	}
}
