using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;

namespace PokemonTD;

public partial class MasterModeInterface : CanvasLayer
{
    [Export]
    private CustomButton _exitButton;

    [Export]
    private CheckButton _masterModeOption;

    [Export]
    private Label _masterModeLabel;

    [Export]
    private ScrollOption _minPokemonLevelOption;

    [Export]
    private ScrollOption _maxPokemonLevelOption;

    public override void _Ready()
    {
        _exitButton.Pressed += QueueFree;
        
        bool hasCompletedAllStages = HasCompletedAllStages();
        _masterModeOption.Disabled = !hasCompletedAllStages;
        _minPokemonLevelOption.MouseFilter = hasCompletedAllStages ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
        _maxPokemonLevelOption.MouseFilter = hasCompletedAllStages ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;

        if (!hasCompletedAllStages) return;

        _masterModeOption.Toggled += MasterModeToggled;
        _minPokemonLevelOption.ValueChanged += MinPokemonLevelChanged;
        _maxPokemonLevelOption.ValueChanged += MaxPokemonLevelChanged;

        MasterModeToggled(PokemonTD.MasterMode.IsEnabled);
        MinPokemonLevelChanged(PokemonTD.MasterMode.MinPokemonLevel);
        MaxPokemonLevelChanged(PokemonTD.MasterMode.MaxPokemonLevel);

        SetHighestPokemonLevel();
    }

    private bool HasCompletedAllStages()
    {
        GC.Dictionary<string, Variant> stageData = PokemonStages.Instance.GetData();
        foreach (bool hasCompleted in stageData.Values)
        {
            if (!hasCompleted) return false;
        }

        return true;
    }

    private void MasterModeToggled(bool isToggled)
    {
        _masterModeOption.ButtonPressed = isToggled;

        string IsEnabled = isToggled ? "On" : "Off";
        string optionText = $"Enabled: {IsEnabled}";
        _masterModeLabel.Text = optionText;

        PokemonTD.MasterMode.IsEnabled = isToggled;
    }

    private void MinPokemonLevelChanged(int value)
    {
        _minPokemonLevelOption.SetProgress(value);

        string optionText = $"Minimum Pokemon Level: {value}";
        _minPokemonLevelOption.SetText(optionText);

        PokemonTD.MasterMode.MinPokemonLevel = value;

        int maxLevelOptionValue = _maxPokemonLevelOption.Value;

        if (maxLevelOptionValue < value)
        {
            MaxPokemonLevelChanged(value);
        }
    }

    private void MaxPokemonLevelChanged(int value)
    {
        _maxPokemonLevelOption.SetProgress(value);

        string optionText = $"Maximum Pokemon Level: {value}";
        _maxPokemonLevelOption.SetText(optionText);

        PokemonTD.MasterMode.MaxPokemonLevel = value;

        int minLevelOptionValue = _minPokemonLevelOption.Value;

        if (minLevelOptionValue > value)
        {
            MinPokemonLevelChanged(value);
        }
    }

    private void SetHighestPokemonLevel()
    {
        int highestPokemonLevel = GetHighestPokemonLevel();

        _minPokemonLevelOption.SetMaxValue(highestPokemonLevel);
        _minPokemonLevelOption.SetProgress(highestPokemonLevel);

        _maxPokemonLevelOption.SetMaxValue(highestPokemonLevel);
        _maxPokemonLevelOption.SetProgress(highestPokemonLevel);

        MinPokemonLevelChanged(highestPokemonLevel);
        MaxPokemonLevelChanged(highestPokemonLevel);
    }

    private int GetHighestPokemonLevel()
    {
        List<Pokemon> pokeCenterPokemon = PokeCenter.Instance.Pokemon;
        int highestPokemonLevel = 1;

        foreach (Pokemon pokemon in pokeCenterPokemon)
        {
            if (pokemon.Level <= highestPokemonLevel) continue;

            highestPokemonLevel = pokemon.Level;
        };

        List<Pokemon> pokemonTeam = PokemonTeam.Instance.Pokemon;
        foreach (Pokemon pokemon in pokemonTeam)
        {
            if (pokemon.Level <= highestPokemonLevel) continue;

            highestPokemonLevel = pokemon.Level;
        }
        return highestPokemonLevel;
    }
}
