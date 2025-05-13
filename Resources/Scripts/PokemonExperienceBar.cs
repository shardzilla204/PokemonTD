using Godot;

namespace PokemonTD;

public partial class PokemonExperienceBar : Container
{
    [Signal]
    public delegate void LeveledUpEventHandler();

    [Export]
    private Label _pokemonLevel;

    [Export]
    private TextureProgressBar _experienceBar;

    public void Update(Pokemon pokemon)
    {
        _pokemonLevel.Text = pokemon != null ? $"LVL. {pokemon.Level}" : null;

        _experienceBar.Value = pokemon != null ? pokemon.Experience.Minimum : 0;
		_experienceBar.MaxValue = pokemon != null ? pokemon.Experience.Maximum : 100;
    }

    public void AddExperience(Pokemon pokemon, float experience)
    {
        _experienceBar.Value += experience;
		pokemon.Experience.Minimum = (int) _experienceBar.Value;

        CheckExperience(pokemon);
    }

    private void CheckExperience(Pokemon pokemon)
    {
        if (_experienceBar.Value < _experienceBar.MaxValue) return;

        LevelUp(pokemon);
    }

    private void LevelUp(Pokemon pokemon)
    {
        while (pokemon.Experience.Minimum >= pokemon.Experience.Maximum)
		{
			pokemon.Level++;

			_experienceBar.Value -= _experienceBar.MaxValue;
			_experienceBar.MaxValue = PokemonManager.Instance.GetExperienceRequired(pokemon);

			pokemon.Experience.Minimum = (int) _experienceBar.Value;
			pokemon.Experience.Maximum = (int) _experienceBar.MaxValue;

			EmitSignal(SignalName.LeveledUp);

			Update(pokemon);

			string pokemonLeveledUpMessage = $"{pokemon.Name} Has Leveled Up To Level {pokemon.Level}";
			PrintRich.PrintLine(TextColor.Purple, pokemonLeveledUpMessage);
			PokemonTD.AddStageConsoleMessage(TextColor.Purple, pokemonLeveledUpMessage);
		}

		PokemonTD.AudioManager.PlayPokemonLeveledUp();
    }
}
