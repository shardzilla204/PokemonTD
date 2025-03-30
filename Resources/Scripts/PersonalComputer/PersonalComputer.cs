using Godot;

using System.Collections.Generic;
using System.Linq;

namespace PokémonTD;

public partial class PersonalComputer : Node
{
	public List<Pokémon> Pokémon = new List<Pokémon>();

    public override void _EnterTree()
    {
        PokémonTD.PersonalComputer = this;
    }

    public override void _Ready()
    {
        PokémonTD.Signals.PokémonEnemyCaptured += AddCapturedPokémon;
    }

	private void AddCapturedPokémon(PokémonEnemy pokémonEnemy)
	{
		if (PokémonTD.PokémonTeam.Pokémon.Count < PokémonTD.MaxTeamSize) return;
		
		Pokémon.Add(pokémonEnemy.Pokémon);

		string addedMessage = $"{pokémonEnemy.Pokémon.Name} Was Transferred To The Personal Computer";
		PrintRich.PrintLine(TextColor.Yellow, addedMessage);
	}

    public List<Pokémon> OrderByLevel()
	{
		return Pokémon.OrderBy(pokémon => pokémon.Level).ToList();
	}

	public List<Pokémon> OrderByNationalNumber()
	{
		return Pokémon.OrderBy(pokémon => pokémon.NationalNumber).ToList();
	}

	public List<Pokémon> OrderByName()
	{
		return Pokémon.OrderBy(pokémon => pokémon.Name).ToList();
	}

	public List<Pokémon> OrderByType()
	{
		return Pokémon.OrderBy(pokémon => pokémon.Types[0]).ToList();
	}
}
