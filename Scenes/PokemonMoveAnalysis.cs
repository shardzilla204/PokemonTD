using Godot;

namespace PokemonTD;

public partial class PokemonMoveAnalysis : NinePatchRect
{
    [Export]
    private Container _moveOptionContainer;

    [Export]
    private Label _moveInfo;

    public override void _ExitTree()
    {
        PokemonTD.Signals.PokemonAnalyzed -= OnPokemonAnalyzed;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.PokemonAnalyzed += OnPokemonAnalyzed;

        ClearContainer();
        _moveInfo.Text = "";
        Visible = false;
    }

    private void OnPokemonAnalyzed(Pokemon pokemon)
    {
        ClearContainer();
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
            moveOption.PokemonMove = pokemonMove;
            moveOption.SetBackgroundColor(gray);
            moveOption.MouseEntered += () => OnMoveOptionHovered(pokemonMove);
            _moveOptionContainer.AddChild(moveOption);
        }
    }

    private void ClearContainer()
    {
        foreach (Node child in _moveOptionContainer.GetChildren())
        {
            child.QueueFree();
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
}
