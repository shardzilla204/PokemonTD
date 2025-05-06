using Godot;

namespace PokemonTD;

public partial class PokeCenterInterface : CanvasLayer
{
	[Export]
	private CustomButton _exitButton;

	public override void _Ready()
	{
		_exitButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
		_exitButton.Pressed += () => 
		{
			if (PokemonTD.PokemonTeam.Pokemon.Count == 0) return;

			StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
			AddSibling(stageSelectInterface);
			QueueFree();

			PokemonTD.AudioManager.PlayButtonPressed();
		};

		if (!PokemonTD.AudioManager.IsPlayingSong(11)) PokemonTD.AudioManager.PlaySong(11); // 11. Pokémon Center
	}
}
