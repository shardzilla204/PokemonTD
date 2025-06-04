using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System;

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

	[Export]
	private LineEdit _pokeCenterSearch;

	public List<PokeCenterSlot> Slots = new List<PokeCenterSlot>();

	private List<Pokemon> _pokemon = new List<Pokemon>();

	private int _pageIndex = 0;
	private int _maxPageIndex = 999;
	private SortCategory _sortCategory;
	private bool _isDescending;

    public override void _ExitTree()
	{
		PokemonTD.Signals.PokemonTeamUpdated -= PokemonTeamUpdated;
	}	

    public override void _Ready()
    {
		PokemonTD.Signals.PokemonTeamUpdated += PokemonTeamUpdated;

		_pokeCenterSearch.TextChanged += SearchTextChanged;

        _cycleLeftButton.Pressed += () => CycleInventory(false);
        _cycleRightButton.Pressed += () => CycleInventory(true);

		_sortByLevel.Pressed += () =>
		{
			SortBy(SortCategory.Level, _sortByLevel.IsDescending);
			SetPokemonPages();
		};
		_sortByName.Pressed += () =>
		{
			SortBy(SortCategory.Name, _sortByName.IsDescending);
			SetPokemonPages();
		};
		_sortByNumber.Pressed += () =>
		{
			SortBy(SortCategory.NationalNumber, _sortByNumber.IsDescending);
			SetPokemonPages();
		};
		_sortByType.Pressed += () =>
		{
			SortBy(SortCategory.Type, _sortByType.IsDescending);
			SetPokemonPages();
		};

		SetPokemonPages();
		
		// Default to sorting levels
		SortBy(SortCategory.Level, _sortByLevel.IsDescending);
		_sortByLevel.UpdateArrows(_sortByLevel.IsDescending);
    }
	
	private void SortBy(SortCategory sortCategory, bool isDescending)
	{
		_sortCategory = sortCategory;
		_isDescending = isDescending;
		switch (sortCategory)
		{
			case SortCategory.Level:
				PokeCenter.Instance.OrderByLevel(isDescending);
				break;
			case SortCategory.Name:
				PokeCenter.Instance.OrderByName(isDescending);
				break;
			case SortCategory.NationalNumber:
				PokeCenter.Instance.OrderByNationalNumber(isDescending);
				break;
			case SortCategory.Type:
				PokeCenter.Instance.OrderByType(isDescending);
				break;
		}
	}

	private void PokemonTeamUpdated()
	{
		if (_pokeCenterSearch.Text != "")
		{
			SearchTextChanged(_pokeCenterSearch.Text);
			return;
		}
		SortBy(_sortCategory, _isDescending);
		SetPokemonPages();
	}

	private void SearchTextChanged(string text)
	{
		text = text.Trim();
		if (text == "")
		{
			SetPokemonPages();
			return;
		}

		ClearInventory();

		List<Pokemon> filteredPokemon = new List<Pokemon>();
		foreach (Pokemon pokemonToFind in PokeCenter.Instance.Pokemon)
		{
			Pokemon pokemon = FindPokemon(pokemonToFind, text);
			if (pokemon != null) filteredPokemon.Add(pokemon);
		}
		int pageCount = PokeCenter.Instance.GetPageCount(filteredPokemon.Count);

		SortBy(_sortCategory, _isDescending);
		SetPokemonPages(pageCount, filteredPokemon);
	}

	private Pokemon FindPokemon(Pokemon pokemon, string text)
	{
		string pokemonName = "";
		foreach (char character in pokemon.Name)
		{
			string uppercaseCharacter = character.ToString().ToUpper();
			pokemonName += uppercaseCharacter;
			if (pokemonName == text.ToUpper()) return pokemon;
		}
		return null;
	}

	private void SetPokemonPages()
	{
		_maxPageIndex = PokeCenter.Instance.GetPageCount();
		Dictionary<int, List<Pokemon>> pokemonPages = PokeCenter.Instance.GetPokemonPages(_maxPageIndex);

		_pageIndex = _maxPageIndex < _pageIndex ? _maxPageIndex : _pageIndex;

		_pokemon.Clear();
		_pokemon.AddRange(pokemonPages[_pageIndex]);

		UpdateInventory();
	}

	private void SetPokemonPages(int pageCount, List<Pokemon> filteredPokemon)
	{
		_maxPageIndex = pageCount;
		Dictionary<int, List<Pokemon>> pokemonPages = PokeCenter.Instance.GetPokemonPages(_maxPageIndex, filteredPokemon);

		_pageIndex = 0;

		_pokemon.Clear();
		_pokemon.AddRange(pokemonPages[_pageIndex]);

		UpdateInventory();
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();
		bool fromAnalysisSlot = dataDictionary["FromAnalysisSlot"].As<bool>();
		if (fromAnalysisSlot) return true;

		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();
		return fromTeamSlot;
	}

    public override void _DropData(Vector2 atPosition, Variant data)
    {
		GC.Dictionary<string, Variant> dataDictionary = data.As<GC.Dictionary<string, Variant>>();

		// Add pokemon if its from PokeCenterAnalysis
		bool fromAnalysisSlot = dataDictionary["FromAnalysisSlot"].As<bool>();
		if (fromAnalysisSlot)
		{
			PokeCenterAnalysis pokeCenterAnalysis = dataDictionary["PokeCenterAnalysis"].As<PokeCenterAnalysis>();
			PokeCenter.Instance.Pokemon.Insert(0, pokeCenterAnalysis.Pokemon);
			pokeCenterAnalysis.SetPokemon(null);
			PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonTeamUpdated);
			return;
		}

		PokeCenterTeamSlot pokeCenterTeamSlot = dataDictionary["Slot"].As<PokeCenterTeamSlot>();
		PokeCenter.Instance.AddPokemon(pokeCenterTeamSlot.Pokemon);
    }

	private void CycleInventory(bool isCyclingRight)
	{
		_pageIndex += isCyclingRight ? 1 : -1;

		if (_pageIndex > _maxPageIndex - 1)
		{
			_pageIndex = 0;
		}
		else if (_pageIndex < 0)
		{
			_pageIndex = _maxPageIndex - 1;
		}

		SetPokemonPages();
	}

	private void UpdateInventory()
	{
		ClearInventory();

		foreach (Pokemon pokemon in _pokemon)
		{
			AddPokeCenterSlot(pokemon);
		}

		_pageCountLabel.Text = $"Page {_pageIndex + 1}/{_maxPageIndex}";
	}

	private void ClearInventory()
	{
		foreach (Node pokeCenterSlot in _pokeCenterSlots.GetChildren())
		{
			pokeCenterSlot.QueueFree();
		}
	}

	private void AddPokeCenterSlot(Pokemon pokemon)
	{
		PokeCenterSlot pokeCenterSlot = PokemonTD.PackedScenes.GetPokeCenterSlot();
		pokeCenterSlot.ID = _pokemon.Count;
		
		pokeCenterSlot.UpdateSlot(pokemon);
		_pokeCenterSlots.AddChild(pokeCenterSlot);
		Slots.Add(pokeCenterSlot);
	}
}

