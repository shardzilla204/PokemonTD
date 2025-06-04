using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class StarterSelectionInterface : CanvasLayer
{
	[Export]
	private CustomButton _exitButton;

	[Export]
	private Container _starterOptions;

	private List<string> _starterOptionNames = new List<string>()
	{ 
		"Bulbasaur",
		"Charmander",
		"Squirtle" 
	};

    public override void _ExitTree()
    {
		PokemonTD.Signals.PokemonStarterSelected -= PokemonStarterSelected;
    }

	public override void _Ready()
	{
		PokemonTD.Signals.PokemonStarterSelected += PokemonStarterSelected;

		_exitButton.Pressed += ExitPressed;

		foreach (string starterOptionName in _starterOptionNames)
		{
			AddStarterOption(starterOptionName);
		}
	}

	private void PokemonStarterSelected(Pokemon pokemon)
	{
		PokemonTD.HasSelectedStarter = true;
		StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
		AddSibling(stageSelectInterface);
		QueueFree();
	}

	private void ExitPressed()
	{
		MenuInterface menuInterface = PokemonTD.PackedScenes.GetMenuInterface();
		AddSibling(menuInterface);
		QueueFree();
	}

	private void AddStarterOption(string pokemonName)
	{
		StarterOption starterOption = PokemonTD.PackedScenes.GetStarterOption();
		int starterPokemonLevel = PokemonTD.StarterPokemonLevel;
		starterOption.Pokemon = PokemonManager.Instance.GetPokemon(pokemonName, starterPokemonLevel);
		_starterOptions.AddChild(starterOption);
	}
}
