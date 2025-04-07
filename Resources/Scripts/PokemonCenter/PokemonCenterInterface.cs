using Godot;

namespace PokemonTD;

public partial class PokemonCenterInterface : CanvasLayer
{
	[Export]
	private CustomButton _exitButton;

	[Export]
	private PokemonCenterTeam _pokemonCenterTeam;

	[Export]
	private PokemonCenterInventory _pokemonCenterInventory;

	public PokemonCenterTeam PokemonCenterTeam => _pokemonCenterTeam;
	public PokemonCenterInventory PokemonCenterInventory => _pokemonCenterInventory;


	public override void _Ready()
	{
		_exitButton.Pressed += () => 
		{
			StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
			AddSibling(stageSelectInterface);
			QueueFree();
		};
	}
}
