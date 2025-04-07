using Godot;

using System.Collections.Generic;

namespace PokemonTD;

public partial class StarterSelectionInterface : CanvasLayer
{
	[Export]
	private Container _starterOptions;

	private List<string> _starterOptionNames = new List<string>()
	{ 
		"Bulbasaur",
		"Charmander",
		"Squirtle" 
	};

	public override void _Ready()
	{
		foreach (string starterOptionName in _starterOptionNames)
		{
			StarterOption starterOption = PokemonTD.PackedScenes.GetStarterOption();
			int starterPokemonLevel = PokemonTD.StarterPokemonLevel;
			starterOption.Pokemon = PokemonTD.PokemonManager.GetPokemon(starterOptionName, starterPokemonLevel);

			_starterOptions.AddChild(starterOption);
		}

		PokemonTD.Signals.PokemonStarterSelected += (pokemon) => 
		{
			StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
			AddSibling(stageSelectInterface);
			QueueFree();
		};
	}
}
