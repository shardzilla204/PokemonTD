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

        _experienceBar.Value = pokemon != null ? pokemon.Experience.Min : 0;
        _experienceBar.MaxValue = pokemon != null ? pokemon.Experience.Max : 100;
    }

    public void AddExperience(Pokemon pokemon, int experience)
    {
        _experienceBar.Value += experience;
        pokemon.Experience.Min += experience;

        CheckExperience(pokemon);
    }

    private void CheckExperience(Pokemon pokemon)
    {
        if (pokemon.Experience.Min < pokemon.Experience.Max) return;

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
        while (pokemon.Experience.Min >= pokemon.Experience.Max)
        {
            levels++;

            SetExperience(pokemon);
            Update(pokemon);
        }
        return levels;
    }

    private void SetExperience(Pokemon pokemon)
    {
        pokemon.Experience.Min -= pokemon.Experience.Max;

        // ? Comment Out For Faster Level Ups
        int experienceRequired = PokemonManager.Instance.GetExperienceRequired(pokemon);
        pokemon.Experience.Max = experienceRequired;
    }
}
