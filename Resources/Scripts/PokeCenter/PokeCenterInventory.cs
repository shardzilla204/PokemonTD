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
	private Label _pageCountLabel;

	[Export]
	private SortButton _sortByLevel;

	[Export]
	private SortButton _sortByNumber;

	[Export]
	private SortButton _sortByName;
	
	[Export]
	private SortButton _sortByType;

	public List<PokeCenterSlot> Slots = new List<PokeCenterSlot>();

	private List<Pokemon> _pokemon = new List<Pokemon>();

	private int _pageIndex = 0;
	private int _maxPageIndex = 999;

    public override void _ExitTree()
    {
		PokemonTD.Signals.PokemonTeamUpdated -= OnPokemonTeamUpdated;
    }	

    public override void _Ready()
    {
		PokemonTD.Signals.PokemonTeamUpdated += OnPokemonTeamUpdated;

        _cycleLeftButton.Pressed += () => CycleInventory(false);
        _cycleRightButton.Pressed += () => CycleInventory(true);

		_sortByLevel.Pressed += () => SortByLevel(_sortByLevel.IsDescending);
		_sortByName.Pressed += () => SortByName(_sortByName.IsDescending);
		_sortByNumber.Pressed += () => SortByNationalNumber(_sortByNumber.IsDescending);
		_sortByType.Pressed  += () => SortByType(_sortByType.IsDescending);

		_cycleLeftButton.Visible = _pageIndex > 0 ? true : false;
		_cycleRightButton.Visible = _pageIndex < _maxPageIndex ? true : false;

		SortByLevel(false);
		_sortByLevel.UpdateArrows(false);
    }

	private void SortByLevel(bool isDescending)
	{
		PokeCenter.Instance.OrderByLevel(isDescending);

		SetPokemonPages(_pageIndex);
	}

	private void SortByName(bool isDescending)
	{
		PokeCenter.Instance.OrderByName(isDescending);

		SetPokemonPages(_pageIndex);
	}

	private void SortByNationalNumber(bool isDescending)
	{
		PokeCenter.Instance.OrderByNationalNumber(isDescending);

		SetPokemonPages(_pageIndex);
	}

	private void SortByType(bool isDescending)
	{
		PokeCenter.Instance.OrderByType(isDescending);

		SetPokemonPages(_pageIndex);
	}

	private void OnPokemonTeamUpdated()
	{
		SetPokemonPages(_pageIndex);
	}

	private void SetPokemonPages(int index)
	{
		_maxPageIndex = GetPageCount(PokeCenter.Instance.Pokemon.Count);
		Dictionary<int, List<Pokemon>> pokemonPages = GetPokemonPages(_maxPageIndex);

		_pokemon.Clear();
		_pokemon.AddRange(pokemonPages[index]);

		UpdateInventory();
	}

	private Dictionary<int, List<Pokemon>> GetPokemonPages(int pageCount)
	{
		Dictionary<int, List<Pokemon>> pokemonPages = new Dictionary<int, List<Pokemon>>();

		// Count of left to iterate through
		int pokemonLeft = PokeCenter.Instance.Pokemon.Count; 

		// Pokemons position in the list
		int pokemonIndex = 0; 

		for (int i = 0; i <= pageCount; i++)
		{
			int pokemonCount = pokemonLeft > PokeCenter.Instance.PokemonPerPage ? PokeCenter.Instance.PokemonPerPage : pokemonLeft;
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
		while (pokemonCount > PokeCenter.Instance.PokemonPerPage)
		{
			pageCount++;
			pokemonCount -= PokeCenter.Instance.PokemonPerPage;
		}
		return pageCount;
	}

	// A list that can hold up to 30 pokemon
	private List<Pokemon> GetPokemonPage(int pokemonCount, int pokemonIndex)
	{
		List<Pokemon> pokemonPage = new List<Pokemon>();
		for (int i = 0; i < pokemonCount; i++)
		{
			Pokemon pokemon = PokeCenter.Instance.Pokemon[pokemonIndex];
			pokemonPage.Add(pokemon);
			pokemonIndex++;
		}
		return pokemonPage;
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (PokemonTeam.Instance.Pokemon.Count == 0) return false;

		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		bool fromAnalysisSlot = dataDictionary["FromAnalysisSlot"].As<bool>();

		if (fromAnalysisSlot) return true;

		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();
		return fromTeamSlot;
	}

    public override void _DropData(Vector2 atPosition, Variant data)
    {
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		bool fromAnalysisSlot = dataDictionary["FromAnalysisSlot"].As<bool>();
		if (fromAnalysisSlot)
		{
			PokeCenterAnalysis pokeCenterAnalysis = dataDictionary["PokeCenterAnalysis"].As<PokeCenterAnalysis>();
			PokeCenter.Instance.Pokemon.Insert(0, pokeCenterAnalysis.Pokemon);
			pokeCenterAnalysis.SetPokemon(null);
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);
			return;
		}

		PokeCenterTeamSlot pokeCenterTeamSlot = dataDictionary["Slot"].As<PokeCenterTeamSlot>();
		PokeCenter.Instance.AddPokemon(pokeCenterTeamSlot.Pokemon);
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

		_pageCountLabel.Text = $"Page {_pageIndex + 1}/{_maxPageIndex + 1}";
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

