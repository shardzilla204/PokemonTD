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
			if (PokemonTeam.Instance.Pokemon.Count == 0) return;

			StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
			AddSibling(stageSelectInterface);
			QueueFree();

		};

		if (!PokemonTD.AudioManager.IsPlayingSong(11)) PokemonTD.AudioManager.PlaySong(11); // 11. Pokémon Center
	}
}
