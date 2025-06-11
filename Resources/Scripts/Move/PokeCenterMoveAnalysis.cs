using System.Collections.Generic;
using System.Linq;
using Godot;

namespace PokemonTD;

public partial class PokeCenterMoveAnalysis : NinePatchRect
{
    [Export]
    private Container _moveOptionContainer;

    [Export]
    private Label _moveInfo;

    private List<MoveOption> _moveOptions = new List<MoveOption>();
    private Pokemon _pokemon;

    public override void _ExitTree()
    {
        PokemonTD.Signals.PokemonAnalyzed -= OnPokemonAnalyzed;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.PokemonAnalyzed += OnPokemonAnalyzed;

        ClearMoveOptions();
        _moveInfo.Text = "";
        Visible = false;
    }

    private void OnPokemonAnalyzed(Pokemon pokemon)
    {
        _moveOptions.Clear();
        ClearMoveOptions();
        _pokemon = pokemon;
        Visible = pokemon != null;

        if (pokemon == null) 
        {
            _moveInfo.Text = "";
            return;
        }

        AddMoveOptions(pokemon);
    }

    private void AddMoveOptions(Pokemon pokemon)
    {
        Color gray = Color.FromHtml("#999999");
        foreach (PokemonMove pokemonMove in pokemon.Moves)
        {
            MoveOption moveOption = PokemonTD.PackedScenes.GetMoveOption();
            moveOption.SetOption(pokemonMove, false);
            moveOption.SetBackgroundColor(gray);
            moveOption.MouseEntered += () => OnMoveOptionHovered(pokemonMove);
            moveOption.Pressed += () => OnMoveOptionPressed(moveOption);

            _moveOptions.Add(moveOption);
            _moveOptionContainer.AddChild(moveOption);
        }

        MoveOption selectedMoveOption = _moveOptions.Find(moveOption => moveOption.PokemonMove == pokemon.Move);
        DarkenMoveOption(selectedMoveOption, true);
    }

    private void ClearMoveOptions()
    {
        foreach (MoveOption moveOption in _moveOptionContainer.GetChildren())
        {
            moveOption.QueueFree();
        }
    }

    private void OnMoveOptionHovered(PokemonMove pokemonMove)
    {
        string power = pokemonMove.Power == 0 ? "" : $"Power: {pokemonMove.Power}\n";
        string accuracy = pokemonMove.Accuracy == 0 ? "" : $"Accuracy: {pokemonMove.Accuracy}%";
        string effect = pokemonMove.Effect == "" ? "" : $"{pokemonMove.Effect}";
        
        if (effect != "" && accuracy != "") accuracy += "\n\n";

        _moveInfo.Text = $"{power}{accuracy}{effect}";
    }

    private void OnMoveOptionPressed(MoveOption moveOption)
	{
		List<MoveOption> otherMoveOptions = _moveOptions.FindAll(otherMoveOption => otherMoveOption != moveOption);
		foreach (MoveOption otherMoveOption in otherMoveOptions)
		{
			DarkenMoveOption(otherMoveOption, false);
		}

		DarkenMoveOption(moveOption, true);
        _pokemon.Move = moveOption.PokemonMove;
	}

    private void DarkenMoveOption(MoveOption moveOption, bool isPressed)
    {
		float darknessPercentage = 0.15f;
        Color pressedColor = Colors.White.Darkened(darknessPercentage);
        moveOption.Modulate = isPressed ? pressedColor : Colors.White;
    }
}
