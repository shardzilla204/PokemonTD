using Godot; 

namespace PokemonTD;

public partial class PokemonEffects : Node
{
    public bool IsCharging = false;
    public bool UsedDig = false;
    public bool UsedFaintMove = false;
    public bool HasLightScreen = false;
    public bool HasReflect = false;
    public bool HasMist = false;
    public bool HasCounter = false;
    public bool HasQuickAttack = false;
    public bool HasSubstitute = false;
    public bool HasRage = false;
    public bool HasMoveSkipped = false;
    public bool HasHyperBeam = false;
    public bool HasConversion = false;
    public Pokemon PokemonTransform = null; // For Pokemon that used Transform

    public void Reset()
    {
        IsCharging = false;
        UsedDig = false;
        UsedFaintMove = false;
        HasLightScreen = false;
        HasReflect = false;
        HasMist = false;
        HasCounter = false;
        HasQuickAttack = false;
        HasSubstitute = false;
        HasRage = false;
        HasMoveSkipped = false;
        HasHyperBeam = false;
        HasConversion = false;
        PokemonTransform = null;
    }

    public void ResetPokemon(Pokemon pokemon, int pokemonTeamIndex)
    {
        if (PokemonTransform != null)
        {
            RevertTransformation(pokemon);
            PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonTransformed, pokemon, pokemonTeamIndex);
        }
        RevertTypes(pokemon);
    }

    private void RevertTypes(Pokemon pokemon)
    {
        Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemon.Name, pokemon.Level);
        pokemon.Types.Clear();
        pokemon.Types.AddRange(pokemonData.Types);
    }

    private void RevertTransformation(Pokemon pokemon)
    {
        string tranformationRevertedMessage = $"{pokemon.Name} Has Reverted Back To Normal";
        PrintRich.PrintLine(TextColor.Orange, tranformationRevertedMessage);

        pokemon.Sprite = PokemonTransform.Sprite;
        pokemon.Species = PokemonTransform.Species;

        pokemon.Moves.Clear();
        pokemon.Moves.AddRange(PokemonTransform.Moves);
        pokemon.Move = PokemonTransform.Move;

        pokemon.Stats.Attack = PokemonTransform.Stats.Attack;
        pokemon.Stats.SpecialAttack = PokemonTransform.Stats.SpecialAttack;
        pokemon.Stats.Defense = PokemonTransform.Stats.Defense;
        pokemon.Stats.SpecialDefense = PokemonTransform.Stats.SpecialDefense;
        pokemon.Stats.Speed = PokemonTransform.Stats.Speed;

        pokemon.Height = PokemonTransform.Height;
        pokemon.Weight = PokemonTransform.Weight;

        PokemonTransform = null;
    }

    public void UseRage(Pokemon pokemon)
    {
        StatMove statIncreaseMove = PokemonStatMoves.Instance.FindIncreasingStatMove("Rage");
        PokemonStatMoves.Instance.ChangeStat(pokemon, statIncreaseMove);

        string activatedRageMessage = $"{pokemon.Name} Has Been Enraged";
        PrintRich.PrintLine(TextColor.Purple, activatedRageMessage);
    }

    public void ApplyEffects(Pokemon pokemon)
	{
		foreach (PokemonMove pokemonMove in pokemon.Moves)
		{
			switch (pokemonMove.Name)
			{
				case "Light Screen":
					pokemon.Effects.HasLightScreen = true;
					break;
				case "Reflect":
					pokemon.Effects.HasReflect = true;
					break;
				case "Mist":
					pokemon.Effects.HasMist = true;
					break;
				case "Conversion":
					pokemon.Effects.HasConversion = true;
					break;
			}
		}
	}
}