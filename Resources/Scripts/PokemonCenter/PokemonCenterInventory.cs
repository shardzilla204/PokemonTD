using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonCenterInventory : Container
{
	[Export]
	private CustomButton _cycleLeftButton;

	[Export]
	private CustomButton _cycleRightButton;

	[Export]
	private Container _pokemonCenterSlots;

	public List<PokemonCenterSlot> Slots = new List<PokemonCenterSlot>();

	private List<Pokemon> _pokemon = new List<Pokemon>();

	private int _pageIndex = 0;
	private int _maxPageIndex = 999;

    public override void _Ready()
    {
        _cycleLeftButton.Pressed += () => CycleInventory(false);
        _cycleRightButton.Pressed += () => CycleInventory(true);

		_cycleLeftButton.Visible = _pageIndex > 0 ? true : false;
		_cycleRightButton.Visible = _pageIndex < _maxPageIndex ? true : false;

		SetPokemonPages(0);
		
		// Add pokemon from team slot to the front the list in the inventory when removed and update
		PokemonCenterInterface pokemonCenterInterface = GetParentOrNull<Node>().GetOwnerOrNull<PokemonCenterInterface>();
		foreach (PokemonCenterTeamSlot pokemonCenterTeamSlot in pokemonCenterInterface.PokemonCenterTeam.Slots)
		{
			pokemonCenterTeamSlot.Removed += (pokemon) => 
			{
				PokemonTD.PokemonCenter.Pokemon.Insert(0, pokemon);
				ResetInventory();
				SetPokemonPages(0);
			};
		}
    }

	private void SetPokemonPages(int index)
	{
		_maxPageIndex = GetPageCount(PokemonTD.PokemonCenter.Pokemon.Count);
		Dictionary<int, List<Pokemon>> pokemonPages = GetPokemonPages(_maxPageIndex);

		_pokemon.Clear();
		_pokemon.AddRange(pokemonPages[index]);

		UpdateInventory();
	}

	private Dictionary<int, List<Pokemon>> GetPokemonPages(int pageCount)
	{
		Dictionary<int, List<Pokemon>> pokemonPages = new Dictionary<int, List<Pokemon>>();

		// Count of left to iterate through
		int pokemonLeft = PokemonTD.PokemonCenter.Pokemon.Count; 

		// Pokemons position in the list
		int pokemonIndex = 0; 

		for (int i = 0; i <= pageCount; i++)
		{
			int pokemonCount = pokemonLeft > PokemonTD.PokemonCenter.PokemonPerPage ? PokemonTD.PokemonCenter.PokemonPerPage : pokemonLeft;
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
		while (pokemonCount >= PokemonTD.PokemonCenter.PokemonPerPage)
		{
			pageCount++;
			pokemonCount -= PokemonTD.PokemonCenter.PokemonPerPage;
		}
		return pageCount;
	}

	// A list that can hold up to 30 pokemon
	private List<Pokemon> GetPokemonPage(int pokemonCount, int pokemonIndex)
	{
		List<Pokemon> pokemonPage = new List<Pokemon>();
		for (int i = 0; i < pokemonCount; i++)
		{
			Pokemon pokemon = PokemonTD.PokemonCenter.Pokemon[pokemonIndex];
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

		PokemonCenterTeamSlot pokemonCenterTeamSlot = dataDictionary["Slot"].As<PokemonCenterTeamSlot>();
		pokemonCenterTeamSlot.EmitSignal(PokemonCenterTeamSlot.SignalName.Removed, pokemonCenterTeamSlot.Pokemon);
		pokemonCenterTeamSlot.UpdateSlot(null);
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
			AddPokemonCenterSlot(pokemon);
		}

		// Add slot signals after updating inventory
		PokemonCenterInterface pokemonCenterInterface = GetParentOrNull<Node>().GetOwnerOrNull<PokemonCenterInterface>();
		pokemonCenterInterface.PokemonCenterTeam.AddSlotSignals();
	}

	private void ClearInventory()
	{
		foreach (Node pokemonCenterSlot in _pokemonCenterSlots.GetChildren())
		{
			pokemonCenterSlot.QueueFree();
		}
	}

	private void SetPokemonPage()
	{
		_pokemon.Clear();

		Dictionary<int, List<Pokemon>> pokemonPages = GetPokemonPages(_maxPageIndex);
		List<Pokemon> pokemonPage = pokemonPages[_pageIndex];
		_pokemon.AddRange(pokemonPage);
	}

	private void AddPokemonCenterSlot(Pokemon pokemon)
	{
		PokemonCenterSlot pokemonCenterSlot = PokemonTD.PackedScenes.GetPokemonCenterSlot();
		pokemonCenterSlot.Pokemon = pokemon;
		pokemonCenterSlot.ID = _pokemon.Count;
		pokemonCenterSlot.Removed += (pokemon) => SetPokemonPages(_pageIndex);
		
		_pokemonCenterSlots.AddChild(pokemonCenterSlot);
		Slots.Add(pokemonCenterSlot);
	}
}

