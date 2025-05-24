using Godot;

namespace PokemonTD;

public partial class PokeMartInterface : CanvasLayer
{
    [Export]
    private CustomButton _exitButton;

    [Export]
    private Label _pokeDollarsLabel;

    [Export]
    private Container _pokeMartItems;

    [Export]
    private Container _pokeMartTeamSlots;

    [Export]
    private PokeMartInventory _pokeMartInventory;

    public override void _ExitTree()
    {
        PokemonTD.Signals.PokeDollarsUpdated -= PokeDollarsUpdated;
        PokemonTD.Signals.ForgetMove -= PokemonForgettingMove;
        PokemonTD.Signals.PokemonEvolving -= PokemonEvolving;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.PokeDollarsUpdated += PokeDollarsUpdated;
        PokemonTD.Signals.ForgetMove += PokemonForgettingMove;
        PokemonTD.Signals.PokemonEvolving += PokemonEvolving;

        _exitButton.Pressed += () =>
        {
            StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
            AddSibling(stageSelectInterface);
            QueueFree();
        };
        _pokeDollarsLabel.Text = $"₽ {PokemonTD.PokeDollars}";

        ClearPokeMartItems();
        AddPokeMartItems();
    }
    
    private async void PokemonForgettingMove(Pokemon pokemon, PokemonMove pokemonMove)
	{
		if (!PokemonEvolution.Instance.IsQueueEmpty()) await ToSignal(PokemonEvolution.Instance, PokemonEvolution.SignalName.QueueCleared);
		
		ForgetMoveInterface forgetMoveInterface = PokemonTD.PackedScenes.GetForgetMoveInterface(pokemon, pokemonMove);
		forgetMoveInterface.Finished += () =>
		{
			if (!PokemonMoves.Instance.IsQueueEmpty()) PokemonMoves.Instance.ShowNext(this);
		};

		PokemonMoves.Instance.AddToQueue(forgetMoveInterface, this);
	}

    private async void PokemonEvolving(Pokemon pokemon, EvolutionStone evolutionStone, int teamSlotIndex)
    {
        if (!PokemonMoves.Instance.IsQueueEmpty()) await ToSignal(PokemonMoves.Instance, PokemonMoves.SignalName.QueueCleared);

        string pokemonEvolutionName = PokemonEvolution.Instance.GetPokemonEvolutionName(pokemon, evolutionStone);
        Pokemon pokemonEvolution = PokemonEvolution.Instance.GetPokemonEvolution(pokemon, pokemonEvolutionName);
        EvolutionInterface evolutionInterface = PokemonTD.PackedScenes.GetEvolutionInterface(pokemon, pokemonEvolution, teamSlotIndex);
        evolutionInterface.Finished += (pokemonEvolution) =>
        {
            if (!PokemonEvolution.Instance.IsQueueEmpty()) PokemonEvolution.Instance.ShowNext(this);
        };

        PokemonEvolution.Instance.AddToQueue(evolutionInterface, this);
    }

    private void ClearPokeMartItems()
    {
        foreach (Node child in _pokeMartItems.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void AddPokeMartItems()
    {
        foreach (PokeMartItem pokeMartItemData in PokeMart.Instance.Items)
        {
            PokeMartItem pokeMartItem = PokemonTD.PackedScenes.GetPokeMartItem(pokeMartItemData);
            pokeMartItem.Bought += _pokeMartInventory.UpdateSlots;
            _pokeMartItems.AddChild(pokeMartItem);
        }
    }

    private void PokeDollarsUpdated()
    {
        _pokeDollarsLabel.Text = $"₽ {PokemonTD.PokeDollars}";
    }
}
