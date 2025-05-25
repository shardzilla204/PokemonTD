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
	public int PokemonPerPage = 18;

    public override void _EnterTree()
    {
        Instance = this;
		LoadPokemonFiles();
    }

    public override void _Ready()
    {
		PokemonTD.Signals.GameSaved += SavePokemon;
		PokemonTD.Signals.GameLoaded += LoadPokemonFiles;
        PokemonTD.Signals.PokemonEnemyCaptured += AddCapturedPokemon;
		PokemonTD.Signals.GameReset += () => 
		{
			Pokemon.Clear();
			if (PokemonTD.IsPokeCenterRandomized) AddRandomPokemon();
		};

		if (PokemonTD.IsPokeCenterRandomized && !PokemonTD.HasSelectedStarter) AddRandomPokemon();
    }

	public void SavePokemon()
	{
		int pageCount = GetPageCount();
		SavePokemonFiles(pageCount);
	}

	private void SavePokemonFiles(int pageCount)
	{
		RemovePokemonFiles();
		AddPokemonFiles(pageCount);
	}

	private void AddPokemonFiles(int pageCount)
	{
		using DirAccess pokeCenterDirectory = DirAccess.Open(_pokeCenterPath);
		Dictionary<int, List<Pokemon>> pokemonPages = GetPokemonPages(pageCount);
		for (int i = 0; i < pageCount; i++)
		{
			string filePath = $"{_pokeCenterPath}pokeCenterPage{i}.pokemon";
			using FileAccess pageFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);

			List<Pokemon> pokemonPage = pokemonPages[i];
			GC.Dictionary<string, Variant> pokeCenterData = GetData(i, pokemonPage);
			string jsonString = Json.Stringify(pokeCenterData, "\t");

			if (jsonString == "") continue;

			pageFile.StoreLine(jsonString);
		}
	}

	public Dictionary<int, List<Pokemon>> GetPokemonPages(int pageCount)
	{
		Dictionary<int, List<Pokemon>> pokemonPages = new Dictionary<int, List<Pokemon>>();

		// Count of left to iterate through
		int pokemonLeft = Pokemon.Count; 

		// Pokemon's position in the list
		int pokemonIndex = 0; 

		for (int i = 0; i <= pageCount; i++)
		{
			int pokemonCount = pokemonLeft > PokemonPerPage ? PokemonPerPage : pokemonLeft;
			List<Pokemon> pokemonPage = GetPokemonPage(pokemonCount, pokemonIndex);

			pokemonPages.Add(i, pokemonPage);

			// Update amount of iterations 
			pokemonLeft -= pokemonCount;

			// Update starting index for the next page
			pokemonIndex += pokemonCount;
		}
		return pokemonPages;
	}

	public Dictionary<int, List<Pokemon>> GetPokemonPages(int pageCount, List<Pokemon> filteredPokemon)
	{
		Dictionary<int, List<Pokemon>> pokemonPages = new Dictionary<int, List<Pokemon>>();

		// Count of left to iterate through
		int pokemonLeft = filteredPokemon.Count;

		// Pokemon's position in the list
		int pokemonIndex = 0; 

		for (int i = 0; i <= pageCount; i++)
		{
			int pokemonCount = pokemonLeft > PokemonPerPage ? PokemonPerPage : pokemonLeft;
			List<Pokemon> pokemonPage = GetPokemonPage(pokemonCount, pokemonIndex, filteredPokemon);

			pokemonPages.Add(i, pokemonPage);

			// Update amount of iterations 
			pokemonLeft -= pokemonCount;

			// Update starting index for the next page
			pokemonIndex += pokemonCount;
		}
		return pokemonPages;
	}

	// A list that can hold up to 30 pokemon
	private List<Pokemon> GetPokemonPage(int pokemonCount, int pokemonIndex)
	{
		List<Pokemon> pokemonPage = new List<Pokemon>();
		for (int i = 0; i < pokemonCount; i++)
		{
			Pokemon pokemon = Pokemon[pokemonIndex];
			pokemonPage.Add(pokemon);
			pokemonIndex++;
		}
		return pokemonPage;
	}
	
	// A list that can hold up to 30 pokemon
	private List<Pokemon> GetPokemonPage(int pokemonCount, int pokemonIndex, List<Pokemon> filteredPokemon)
	{
		List<Pokemon> pokemonPage = new List<Pokemon>();
		for (int i = 0; i < pokemonCount; i++)
		{
			Pokemon pokemon = filteredPokemon[pokemonIndex];
			pokemonPage.Add(pokemon);
			pokemonIndex++;
		}
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

	public int GetPageCount()
	{
		int pageCount = 0;
		int pokemonCount = Pokemon.Count;
		while (pokemonCount >= PokemonPerPage)
		{
			pageCount++;
			pokemonCount -= PokemonPerPage;
		}

		// Add another page to accomodate for pages that are not full
		if (pokemonCount > 0) pageCount++;
		return pageCount;
	}

	public int GetPageCount(int pokemonCount)
	{
		int pageCount = 0;
		while (pokemonCount >= PokemonPerPage)
		{
			pageCount++;
			pokemonCount -= PokemonPerPage;
		}

		// Add another page to accomodate for pages that are not full
		if (pokemonCount > 0) pageCount++;
		return pageCount;
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

		// Print message to console
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

	private GC.Dictionary<string, Variant> GetData(int pageIndex, List<Pokemon> pokemonPage)
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

	private void SetData(GC.Dictionary<string, Variant> pokeCenterData)
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
