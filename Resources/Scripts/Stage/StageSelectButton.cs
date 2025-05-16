namespace PokemonTD;

public partial class StageSelectButton : CustomButton
{
	public PokemonStage PokemonStage;

    public override void _Ready()
    {
		base._Ready();
		
		Text = $"{PokemonStage.ID}";

		Pressed += SetStage;
		MouseEntered += () => PokemonTD.Signals.EmitSignal(Signals.SignalName.StageSelectButtonHovered, PokemonStage);
    }

	private void SetStage()
	{
		PokemonTD.IsGamePaused = true;
		PokemonStage pokemonStage = GetPokemonStage();
		PokemonTD.Signals.EmitSignal(Signals.SignalName.StageSelectButtonPressed, pokemonStage);
	}

	// Create a scene and copy and paste the variables
	private PokemonStage GetPokemonStage()
	{
		PokemonStage pokemonStage = PokemonTD.PackedScenes.GetPokemonStage(PokemonStage.ID - 1);
		pokemonStage.ID = PokemonStage.ID;
		pokemonStage.WaveCount = PokemonStage.WaveCount;
		pokemonStage.PokemonNames = PokemonStage.PokemonNames;
		pokemonStage.PokemonPerWave = PokemonStage.PokemonPerWave;
		pokemonStage.PokemonLevels = PokemonStage.PokemonLevels;

		return pokemonStage;
	}
}
