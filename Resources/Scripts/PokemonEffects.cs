using Godot; 

namespace PokemonTD;

public partial class PokemonEffects : Node
{
    public int LightScreenCount;
    public int ReflectCount;
    public bool IsCharging = false;
    public bool UsedDig = false;
    public bool HasCounter = false;
    public bool UsedQuickAttack = false;
    public bool HasSubstitute = false;
    public bool UsedFaintMove = false;
    public bool HasRage = false;
    public bool HasMoveSkipped = false;
    public Pokemon PokemonTransform = null; // For Pokemon that used Transform

    public void Reset()
    {
        LightScreenCount = 0;
        ReflectCount = 0;
        IsCharging = false;
        UsedDig = false;
        HasCounter = false;
        UsedQuickAttack = false;
        HasSubstitute = false;
        UsedFaintMove = false;
        HasRage = false;
        HasMoveSkipped = false;
        PokemonTransform = null;
    }
    
    public void RevertTypes(Pokemon pokemon)
	{
		Pokemon pokemonData = PokemonManager.Instance.GetPokemon(pokemon.Name, pokemon.Level);
		pokemon.Types.Clear();
		pokemon.Types.AddRange(pokemonData.Types);
	}
    
    public void RevertTransformation(Pokemon pokemon)
    {
        string tranformationRevertedMessage = $"{PokemonTransform.Name} Has Reverted Back To Normal";
        PrintRich.PrintLine(TextColor.Orange, tranformationRevertedMessage);

        pokemon.Sprite = PokemonTransform.Sprite;
        pokemon.Species = PokemonTransform.Species;

        pokemon.Moves.Clear();
        pokemon.Moves.AddRange(PokemonTransform.Moves);
        pokemon.Move = PokemonTransform.Move;

        pokemon.Attack = PokemonTransform.Attack;
        pokemon.SpecialAttack = PokemonTransform.SpecialAttack;
        pokemon.Defense = PokemonTransform.Defense;
        pokemon.SpecialDefense = PokemonTransform.SpecialDefense;
        pokemon.Speed = PokemonTransform.Speed;

        pokemon.Height = PokemonTransform.Height;
        pokemon.Weight = PokemonTransform.Weight;

        PokemonTransform = null;
    }
}