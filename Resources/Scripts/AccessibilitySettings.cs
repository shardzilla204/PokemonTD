using Godot;

namespace PokemonTD;

public partial class AccessibilitySettings : Container
{
    [Export]
    private OptionButton _windowOption;

    [Export]
    private CheckButton _buttonSFXOption;

    [Export]
    private CheckButton _pokemonSFXOption;

    [Export]
    private CheckButton _pokemonMoveSFXOption;

    public override void _Ready()
    {
        _windowOption.ItemSelected += WindowOptionSelected;
        _buttonSFXOption.Toggled += ButtonSFXOptionToggled;
        _pokemonSFXOption.Toggled += PokemonSFXOptionToggled;
        _pokemonMoveSFXOption.Toggled += PokemonMoveSFXOptionToggled;

        WindowOptionSelected(PokemonSettings.Instance.WindowModeIndex);
        ButtonSFXOptionToggled(PokemonSettings.Instance.ButtonSFXEnabled);
        PokemonSFXOptionToggled(PokemonSettings.Instance.PokemonSFXEnabled);
    }

    private void WindowOptionSelected(long index)
    {
        _windowOption.Selected = (int)index;
        PokemonSettings.Instance.WindowModeIndex = (int)index;

        PokemonSettings.Instance.ApplySettings();
    }

    private void ButtonSFXOptionToggled(bool isToggled)
    {
        _buttonSFXOption.ButtonPressed = isToggled;

        string toggledText = "Button SFX: ";
        toggledText += isToggled ? "On" : "Off";
        _buttonSFXOption.Text = toggledText;

        PokemonSettings.Instance.ButtonSFXEnabled = isToggled;
    }

    private void PokemonSFXOptionToggled(bool isToggled)
    {
        _pokemonSFXOption.ButtonPressed = isToggled;

        string toggledText = "Pokemon SFX: ";
        toggledText += isToggled ? "On" : "Off";
        _pokemonSFXOption.Text = toggledText;

        PokemonSettings.Instance.PokemonSFXEnabled = isToggled;
    }

    private void PokemonMoveSFXOptionToggled(bool isToggled)
    {
        _pokemonMoveSFXOption.ButtonPressed = isToggled;

        string toggledText = "Pokemon Move SFX: ";
        toggledText += isToggled ? "On" : "Off";
        _pokemonMoveSFXOption.Text = toggledText;

        PokemonSettings.Instance.PokemonMoveSFXEnabled = isToggled;
    }
}
