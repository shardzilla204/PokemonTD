using Godot;
using System.Collections.Generic;

namespace PokémonTD;

public partial class PokémonTeam : Node
{
	public List<Pokémon> Pokémon = new List<Pokémon>();

	public override void _EnterTree()
	{
		PokémonTD.PokémonTeam = this;
	}

	public override void _Ready()
	{
		PokémonTD.Signals.PokémonEnemyCaptured += AddCapturedPokémon;
		PokémonTD.Signals.PokémonStarterSelected += AddStarterPokémon;
	}

	public void GetRandomTeam(int teamCount)
	{
		for (int i = 0; i < teamCount; i++)
		{
			Pokémon pokémon = PokémonTD.GetRandomPokémon();
			pokémon.Level = 5;
			Pokémon.Add(pokémon);
		}
	}

	private void AddStarterPokémon(Pokémon pokémon)
	{
		Pokémon.Add(pokémon);

		string addedMessage = $"{pokémon.Name} Was Selected As Your Starter";
		PrintRich.PrintLine(TextColor.Yellow, addedMessage);

		PokémonTD.Signals.EmitSignal(Signals.SignalName.PokémonTeamUpdated);
	}

	private void AddCapturedPokémon(PokémonEnemy pokémonEnemy)
	{
		if (Pokémon.Count >= PokémonTD.MaxTeamSize) return;

		Pokémon.Add(pokémonEnemy.Pokémon);

		string addedMessage = $"{pokémonEnemy.Pokémon.Name} Was Added To The Team";
		PrintRich.PrintLine(TextColor.Yellow, addedMessage);

		PokémonTD.Signals.EmitSignal(Signals.SignalName.PokémonTeamUpdated);
	}
}
