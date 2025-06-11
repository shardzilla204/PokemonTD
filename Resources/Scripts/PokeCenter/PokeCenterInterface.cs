using Godot;

namespace PokemonTD;

public partial class PokeCenterInterface : CanvasLayer
{
	[Export]
	private CustomButton _exitButton;

	public override void _Ready()
	{
		_exitButton.Pressed += () => 
		{
			// Grab the first Pokemon in the Poke Center and add it to the team
			if (PokemonTeam.Instance.Pokemon.Count == 0)
			{
				if (PokeCenter.Instance.Pokemon.Count == 0) return;
				
				Pokemon pokemon = PokeCenter.Instance.Pokemon[0];
				PokeCenter.Instance.RemovePokemon(pokemon);
			};

			StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
			AddSibling(stageSelectInterface);
			QueueFree();
		};

		if (!PokemonTD.AudioManager.IsPlayingSong(11)) PokemonTD.AudioManager.PlaySong(11); // 11. Pokémon Center
	}
}
