using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PokemonTD;

public partial class PokeCenter : Node
{
	private static PokeCenter _instance;

    public static PokeCenter Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }
	
	private string _pokeCenterPath = "user://PokeCenter/";

	public List<Pokemon> Pokemon = new List<Pokemon>();
	public int PokemonPerPage = 30;

    public override void _EnterTree()
    {
        Instance = this;
		PokemonTD.Signals.GameSaved += SavePokemon;
		PokemonTD.Signals.GameLoaded += LoadPokemonFiles;

		LoadPokemonFiles();
    }

    public override void _Ready()
    {
		PokemonTD.Signals.GameReset += Pokemon.Clear;
		
        PokemonTD.Signals.PokemonEnemyCaptured += AddCapturedPokemon;
		PokemonTD.Signals.GameReset += () => 
		{
			if (PokemonTD.IsPokeCenterRandomized) AddRandomPokemon();
		};

		if (PokemonTD.IsPokeCenterRandomized && !PokemonTD.HasSelectedStarter) AddRandomPokemon();
    }

	public void SavePokemon()
	{
		int totalPageCount = GetTotalPageCount();
		SavePokemonFiles(totalPageCount);
	}

	private void SavePokemonFiles(int totalPageCount)
	{
		RemovePokemonFiles();
		AddPokemonFiles(totalPageCount);
	}

	private void AddPokemonFiles(int totalPageCount)
	{
		using DirAccess pokeCenterDirectory = DirAccess.Open(_pokeCenterPath);
		for (int i = 0; i < totalPageCount; i++)
		{
			string filePath = $"{_pokeCenterPath}pokeCenterPage{i}.pokemon";
			using FileAccess pageFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
			List<Pokemon> pokemonPage = GetPokemonPage(i);
			string jsonString = Json.Stringify(GetData(i, pokemonPage), "\t");

			if (jsonString == "") continue;

			pageFile.StoreLine(jsonString);
		}
	}

	private List<Pokemon> GetPokemonPage(int pageIndex)
	{
		int pokemonIndex = pageIndex * PokemonPerPage;
		List<Pokemon> pokemonPage = new List<Pokemon>();
		for (int i = pokemonIndex; i < PokemonPerPage + pokemonIndex; i++)
		{
			Pokemon pokemon = Pokemon[i];
			pokemonPage.Add(pokemon);
		}
		GD.Print($"Page Index: {pageIndex}");
		GD.Print($"Pokemon Count: {pokemonPage.Count}");
		return pokemonPage;
	}

	private void RemovePokemonFiles()
	{
		try 
		{
			using DirAccess pokeCenterDirectory = DirAccess.Open(_pokeCenterPath);
			string[] files = pokeCenterDirectory.GetFiles();
			foreach (string file in files)
			{
				pokeCenterDirectory.Remove(file);
			}
		}
		catch (NullReferenceException)
		{
			DirAccess.MakeDirAbsolute(_pokeCenterPath);
		}
	}

	private int GetTotalPageCount()
	{
		int totalPageCount = 0;
		int pokemonCount = Pokemon.Count;
		while (pokemonCount > 0)
		{
			totalPageCount++;
			pokemonCount -= PokemonPerPage;
		}
		return totalPageCount;
	}

	private void LoadPokemonFiles()
	{
		using DirAccess pokeCenterDirectory = DirAccess.Open(_pokeCenterPath);

		if (pokeCenterDirectory == null) return;

		pokeCenterDirectory.ListDirBegin();
		string fileName = pokeCenterDirectory.GetNext();
		while (fileName != "")
		{
			if (!pokeCenterDirectory.CurrentIsDir())
			{
				string filePath = $"{_pokeCenterPath}{fileName}";
				LoadPokemon(filePath);
			}

			fileName = pokeCenterDirectory.GetNext();
		}
	}

	public void LoadPokemon(string filePath)
    {
        using FileAccess pageFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = pageFile.GetAsText();

		if (pageFile.GetLength() == 0)
		{
			GD.PrintErr("Page File Is Empty");
			return;
		}

		Json json = new Json();
		Error result = json.Parse(jsonString);

		if (result != Error.Ok) return;

		GC.Dictionary<string, Variant> pokemonData = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
		SetData(pokemonData);
    }

	private void AddCapturedPokemon(PokemonEnemy pokemonEnemy)
	{
		if (PokemonTeam.Instance.Pokemon.Count < PokemonTD.MaxTeamSize) return;

		string pokemonName = pokemonEnemy.Pokemon.Name;
		int pokemonLevel = pokemonEnemy.Pokemon.Level;
		
		Pokemon capturedPokemon = PokemonManager.Instance.GetPokemon(pokemonName, pokemonLevel);
		Pokemon.Add(capturedPokemon);

		string transferredMessage = $"{capturedPokemon.Name} Was Transferred To The Pokemon Center";
		PrintRich.PrintLine(TextColor.Yellow, transferredMessage);
	}

    public void OrderByLevel(bool isDescending)
	{
		Pokemon = isDescending ? Pokemon.OrderByDescending(pokemon => pokemon.Level).ToList() : Pokemon.OrderBy(pokemon => pokemon.Level).ToList();
	}

	public void OrderByNationalNumber(bool isDescending)
	{
		Pokemon = isDescending ? Pokemon.OrderByDescending(pokemon => pokemon.NationalNumber).ToList() : Pokemon.OrderBy(pokemon => pokemon.NationalNumber).ToList();
	}

	public void OrderByName(bool isDescending)
	{
		Pokemon = isDescending ? Pokemon.OrderByDescending(pokemon => pokemon.Name).ToList() : Pokemon.OrderBy(pokemon => pokemon.Name).ToList();
	}

	public void OrderByType(bool isDescending)
	{
		Pokemon = isDescending ? Pokemon.OrderByDescending(pokemon => pokemon.Types[0]).ToList() : Pokemon.OrderBy(pokemon => pokemon.Types[0]).ToList();
	}

	private void AddRandomPokemon()
	{
		for (int i = 0; i < PokemonTD.PokeCenterCount; i++)
		{
			Pokemon randomPokemon = PokemonManager.Instance.GetRandomPokemon();
			Pokemon.Add(randomPokemon);
			PrintRich.Print(TextColor.Purple, $"{i}. {randomPokemon.Name} {randomPokemon.Level} ");
		}
	}

	public void AddPokemon(Pokemon pokemon)
	{
		Pokemon.Insert(0, pokemon);
		PokemonTeam.Instance.RemovePokemon(pokemon);
	}

	public void RemovePokemon(Pokemon pokemon)
	{
		Pokemon.Remove(pokemon);
		PokemonTeam.Instance.AddPokemon(pokemon);
	}

	public GC.Dictionary<string, Variant> GetData(int pageIndex, List<Pokemon> pokemonPage)
	{
		GC.Dictionary<string, Variant> pokeCenterData = new GC.Dictionary<string, Variant>();
		for (int i = 0; i < pokemonPage.Count; i++)
		{
			Pokemon pokemon = pokemonPage[i];
			GC.Dictionary<string, Variant> pokemonData = PokemonTD.GetPokemonData(pokemon);
			if (pokemonData.Count == 0)
			{
				GD.PrintErr($"Page {pageIndex} - ({i}) {pokemon.Name}");
			}
			pokeCenterData.Add($"{i}", pokemonData);
		}
		return pokeCenterData;
	}

	public void SetData(GC.Dictionary<string, Variant> pokeCenterData)
	{
		List<string> pokemonKeys = pokeCenterData.Keys.ToList();
		for (int i = 0; i < pokeCenterData.Keys.Count; i++)
		{
			string pokemonKey = pokemonKeys[i];
			try 
			{
				GC.Dictionary<string, Variant> pokemonData = pokeCenterData[pokemonKey].As<GC.Dictionary<string, Variant>>();
				string pokemonName = pokemonData["Name"].As<string>();
				Pokemon pokemon = PokemonTD.SetPokemonData(pokemonName, pokemonData);
				Pokemon.Add(pokemon);
			}
			catch (KeyNotFoundException)
			{
				GD.PrintErr($"{pokemonKey} Is Empty");
			}
		}
	}
}
