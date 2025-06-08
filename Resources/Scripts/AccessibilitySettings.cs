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

    [Export]
    private CheckButton _autoHealOption;

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
        _windowOption.Selected = (int) index;
        PokemonSettings.Instance.WindowModeIndex = (int) index;

        PokemonSettings.Instance.ApplySettings();
    }

    private void ButtonSFXOptionToggled(bool isToggled)
    {
        _buttonSFXOption.ButtonPressed = isToggled;

        string optionText = "Button SFX: ";
        _buttonSFXOption.Text = AppendOptionText(optionText, isToggled);

        PokemonSettings.Instance.ButtonSFXEnabled = isToggled;
    }

    private void PokemonSFXOptionToggled(bool isToggled)
    {
        _pokemonSFXOption.ButtonPressed = isToggled;

        string optionText = "Pokemon SFX: ";
        _pokemonSFXOption.Text = AppendOptionText(optionText, isToggled);

        PokemonSettings.Instance.PokemonSFXEnabled = isToggled;
    }

    private void PokemonMoveSFXOptionToggled(bool isToggled)
    {
        _pokemonMoveSFXOption.ButtonPressed = isToggled;

        string optionText = "Pokemon Move SFX: ";
        _pokemonMoveSFXOption.Text = AppendOptionText(optionText, isToggled);

        PokemonSettings.Instance.PokemonMoveSFXEnabled = isToggled;
    }

    private void AutoHealOptionToggled(bool isToggled)
    {
        _autoHealOption.ButtonPressed = isToggled;

        string optionText = "Auto Heal: ";
        _autoHealOption.Text = AppendOptionText(optionText, isToggled);

        PokemonSettings.Instance.AutoHealEnabled = isToggled;
    }

    private string AppendOptionText(string optionText, bool isToggled)
    {
        string toggledText = isToggled ? "On" : "Off";
        optionText += toggledText;
        return optionText;
    }
}
