using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;

namespace PokemonTD;

public partial class PokeCenterInventory : Container
{
	[Export]
	private CustomButton _cycleLeftButton;

	[Export]
	private CustomButton _cycleRightButton;

	[Export]
	private Container _pokeCenterSlots;

	[Export]
	private CustomButton _sortByLevel;

	[Export]
	private CustomButton _sortByNumber;

	[Export]
	private CustomButton _sortByName;
	
	[Export]
	private CustomButton _sortByType;

	public List<PokeCenterSlot> Slots = new List<PokeCenterSlot>();

	private List<Pokemon> _pokemon = new List<Pokemon>();

	private int _pageIndex = 0;
	private int _maxPageIndex = 999;

    public override void _EnterTree()
    {
        PokemonTD.Signals.PokeCenterTeamSlotRemoved += OnPokeCenterTeamSlotRemoved;
    }

    public override void _ExitTree()
    {
        PokemonTD.Signals.PokeCenterTeamSlotRemoved -= OnPokeCenterTeamSlotRemoved;
    }

    public override void _Ready()
    {
        _cycleLeftButton.Pressed += () => CycleInventory(false);
        _cycleRightButton.Pressed += () => CycleInventory(true);

		_sortByLevel.Pressed += SortByLevel;
		_sortByName.Pressed += SortByName;
		_sortByNumber.Pressed += SortByNationalNumber;
		_sortByType.Pressed  += SortByType;

		_cycleLeftButton.Visible = _pageIndex > 0 ? true : false;
		_cycleRightButton.Visible = _pageIndex < _maxPageIndex ? true : false;

		SetPokemonPages(0);
    }

	private void SortByLevel()
	{
		PokemonTD.PokeCenter.OrderByLevel();

		SetPokemonPages(0);
	}

	private void SortByName()
	{
		PokemonTD.PokeCenter.OrderByName();

		SetPokemonPages(0);
	}

	private void SortByNationalNumber()
	{
		PokemonTD.PokeCenter.OrderByNationalNumber();

		SetPokemonPages(0);
	}

	private void SortByType()
	{
		PokemonTD.PokeCenter.OrderByType();

		SetPokemonPages(0);
	}

	private void OnPokeCenterTeamSlotRemoved(Pokemon pokemon)
	{
		PokemonTD.PokeCenter.Pokemon.Insert(0, pokemon);
		ResetInventory();
		SetPokemonPages(0);
	}

	private void SetPokemonPages(int index)
	{
		_maxPageIndex = GetPageCount(PokemonTD.PokeCenter.Pokemon.Count);
		Dictionary<int, List<Pokemon>> pokemonPages = GetPokemonPages(_maxPageIndex);

		_pokemon.Clear();
		_pokemon.AddRange(pokemonPages[index]);

		UpdateInventory();
	}

	private Dictionary<int, List<Pokemon>> GetPokemonPages(int pageCount)
	{
		Dictionary<int, List<Pokemon>> pokemonPages = new Dictionary<int, List<Pokemon>>();

		// Count of left to iterate through
		int pokemonLeft = PokemonTD.PokeCenter.Pokemon.Count; 

		// Pokemons position in the list
		int pokemonIndex = 0; 

		for (int i = 0; i <= pageCount; i++)
		{
			int pokemonCount = pokemonLeft > PokemonTD.PokeCenter.PokemonPerPage ? PokemonTD.PokeCenter.PokemonPerPage : pokemonLeft;
			List<Pokemon> pokemonPage = GetPokemonPage(pokemonCount, pokemonIndex);

			pokemonPages.Add(i, pokemonPage);

			// Update amount of iterations 
			pokemonLeft -= pokemonCount;

			// Update starting index for the next page
			pokemonIndex += pokemonCount;
		}
		return pokemonPages;
	}

	private int GetPageCount(int pokemonCount)
	{
		int pageCount = 0;
		while (pokemonCount > PokemonTD.PokeCenter.PokemonPerPage)
		{
			pageCount++;
			pokemonCount -= PokemonTD.PokeCenter.PokemonPerPage;
		}
		return pageCount;
	}

	// A list that can hold up to 30 pokemon
	private List<Pokemon> GetPokemonPage(int pokemonCount, int pokemonIndex)
	{
		List<Pokemon> pokemonPage = new List<Pokemon>();
		for (int i = 0; i < pokemonCount; i++)
		{
			Pokemon pokemon = PokemonTD.PokeCenter.Pokemon[pokemonIndex];
			pokemonPage.Add(pokemon);
			pokemonIndex++;
		}
		return pokemonPage;
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (PokemonTD.PokemonTeam.Pokemon.Count == 0) return false;

		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();

		return fromTeamSlot;
	}

    public override void _DropData(Vector2 atPosition, Variant data)
    {
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();

		PokeCenterTeamSlot pokeCenterTeamSlot = dataDictionary["Slot"].As<PokeCenterTeamSlot>();
		pokeCenterTeamSlot.UpdateSlot(null);
		
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokeCenterTeamSlotRemoved, pokeCenterTeamSlot.Pokemon);
    }

	// Go back and display the first page 
	private void ResetInventory()
	{
		_pageIndex = 0;

		SetPokemonPage();
		UpdateInventory();
	}

	private void CycleInventory(bool isCyclingRight)
	{
		_pageIndex += isCyclingRight ? 1 : -1;

		SetPokemonPage();
		UpdateInventory();
	}

	private void UpdateInventory()
	{
		_cycleLeftButton.Visible = _pageIndex > 0 ? true : false;
		_cycleRightButton.Visible = _pageIndex < _maxPageIndex ? true : false;

		ClearInventory();

		foreach (Pokemon pokemon in _pokemon)
		{
			AddPokeCenterSlot(pokemon);
		}
	}

	private void ClearInventory()
	{
		foreach (Node pokeCenterSlot in _pokeCenterSlots.GetChildren())
		{
			pokeCenterSlot.QueueFree();
		}
	}

	private void SetPokemonPage()
	{
		_pokemon.Clear();

		Dictionary<int, List<Pokemon>> pokemonPages = GetPokemonPages(_maxPageIndex);
		List<Pokemon> pokemonPage = pokemonPages[_pageIndex];
		_pokemon.AddRange(pokemonPage);
	}

	private void AddPokeCenterSlot(Pokemon pokemon)
	{
		PokeCenterSlot pokeCenterSlot = PokemonTD.PackedScenes.GetPokeCenterSlot();
		pokeCenterSlot.Pokemon = pokemon;
		pokeCenterSlot.ID = _pokemon.Count;
		
		_pokeCenterSlots.AddChild(pokeCenterSlot);
		Slots.Add(pokeCenterSlot);
	}
}

