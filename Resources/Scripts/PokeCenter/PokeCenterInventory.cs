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

		_sortByLevel.Pressed += () => SortBy(SortCategory.Level, _sortByLevel.IsDescending);
		_sortByName.Pressed += () => SortBy(SortCategory.Name, _sortByName.IsDescending);
		_sortByNumber.Pressed += () => SortBy(SortCategory.NationalNumber, _sortByNumber.IsDescending);
		_sortByType.Pressed  += () => SortBy(SortCategory.Type, _sortByType.IsDescending);

		SetPokemonPages();
		SetButtonOpacity();
		
		// Default to sorting levels
		SortBy(SortCategory.Level, false);
		_sortByLevel.UpdateArrows(false);
    }
	
	private void SortBy(SortCategory sortCategory, bool isDescending)
	{
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
		SetPokemonPages();
	}

	private void OnPokemonTeamUpdated()
	{
		SetPokemonPages();
		SetButtonOpacity();
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

		// Add pokemon if its from PokeCenterAnalysis
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

	private void SetButtonOpacity()
	{
		_cycleLeftButton.Visible = _pageIndex > 0;
		_cycleRightButton.Visible = _pageIndex < _maxPageIndex - 1;
	}

	private void CycleInventory(bool isCyclingRight)
	{
		_pageIndex += isCyclingRight ? 1 : -1;

		SetPokemonPages();
		SetButtonOpacity();
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
		pokeCenterSlot.Pokemon = pokemon;
		pokeCenterSlot.ID = _pokemon.Count;
		
		_pokeCenterSlots.AddChild(pokeCenterSlot);
		Slots.Add(pokeCenterSlot);
	}
}

