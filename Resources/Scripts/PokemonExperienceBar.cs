using Godot;

namespace PokemonTD;

public partial class PokemonExperienceBar : Container
{
    [Signal]
    public delegate void LeveledUpEventHandler(int levels);

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

    public void AddExperience(Pokemon pokemon, int experience)
    {
        _experienceBar.Value += experience;
        pokemon.Experience.Minimum += experience;

        CheckExperience(pokemon);
    }

    private void CheckExperience(Pokemon pokemon)
    {
        if (pokemon.Experience.Minimum < pokemon.Experience.Maximum) return;

        LevelUp(pokemon);
        Update(pokemon);
    }

    private void LevelUp(Pokemon pokemon)
    {
        PokemonTD.AudioManager.PlayPokemonLeveledUp();

        int levels = GetLevels(pokemon);
        EmitSignal(SignalName.LeveledUp, levels);

        // Print Message To Console
        string pokemonLeveledUpMessage = $"{pokemon.Name} Has Leveled Up To Level {pokemon.Level}";
        PrintRich.PrintLine(TextColor.Purple, pokemonLeveledUpMessage);
    }

    private int GetLevels(Pokemon pokemon)
    {
        int levels = 0;
        while (pokemon.Experience.Minimum >= pokemon.Experience.Maximum)
        {
            levels++;

            SetExperience(pokemon);
            Update(pokemon);
        }
        return levels;
    }

    private void SetExperience(Pokemon pokemon)
    {
        pokemon.Experience.Minimum -= pokemon.Experience.Maximum;

        // ? Comment Out For Faster Level Ups
        int experienceRequired = PokemonManager.Instance.GetExperienceRequired(pokemon);
        pokemon.Experience.Maximum = experienceRequired;
    }
}
