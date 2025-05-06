namespace PokemonTD;

public partial class StageSelectButton : CustomButton
{
	public PokemonStage PokemonStage;

    public override void _Ready()
    {
		base._Ready();
		
		Text = $"{PokemonStage.ID}";

        Pressed += () => 
		{
			SetStage();
			PokemonTD.IsGamePaused = true;
			PokemonTD.AudioManager.PlayButtonPressed();
		};
		MouseEntered += () => 
		{
			PokemonTD.Signals.EmitSignal(Signals.SignalName.StageSelectButtonHovered, PokemonStage);
			PokemonTD.AudioManager.PlayButtonHovered();
		};
    }

	private void SetStage()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		PokemonTD.Signals.EmitSignal(Signals.SignalName.StageSelectButtonPressed, pokemonStage);
	}

	// Create a scene and copy and paste the variables
	private PokemonStage GetPokemonStage()
	{
		PokemonStage pokemonStage = PokemonTD.PackedScenes.GetPokemonStage(PokemonStage.ID - 1);
		pokemonStage.WaveCount = PokemonStage.WaveCount;
		pokemonStage.PokemonNames = PokemonStage.PokemonNames;
		pokemonStage.PokemonPerWave = PokemonStage.PokemonPerWave;
		pokemonStage.PokemonLevels = PokemonStage.PokemonLevels;

		return pokemonStage;
	}
}
