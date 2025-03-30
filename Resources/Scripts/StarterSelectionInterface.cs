using Godot;

using System.Collections.Generic;

namespace PokémonTD;

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
			Pokémon pokémon = PokémonTD.PokémonManager.GetPokémon(starterOptionName);

			StarterOption starterOption = PokémonTD.PackedScenes.GetStarterOption();
			starterOption.Pokémon = pokémon;

			_starterOptions.AddChild(starterOption);
		}

		PokémonTD.Signals.PokémonStarterSelected += (pokémon) => 
		{
			StageSelectInterface stageSelectInterface = PokémonTD.PackedScenes.GetStageSelectInterface();
			AddSibling(stageSelectInterface);
			QueueFree();
		};
	}
}
