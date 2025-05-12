using System.Collections.Generic;
using Godot;

namespace PokemonTD;

public enum TextColor
{
   Red, // In-Game Error
   Orange, // Action
   Yellow, // General Event
   Green, // In-Game Success
   Blue,
   Purple, // Pokemon Event
}

public partial class PrintRich
{
   public static void Print(TextColor textColor, string text)
   {
      if (!PokemonTD.AreConsoleMessagesEnabled) return;

      string textColorString = GetColorHex(textColor);
      GD.PrintRich($"[color={textColorString}]{text}[/color]");
   }

   public static void PrintLine(TextColor textColor, string text)
   {
      if (!PokemonTD.AreConsoleMessagesEnabled) return;

      string textColorString = GetColorHex(textColor);
      GD.PrintRich($"[color={textColorString}]{text}[/color]");
      GD.Print(); // Spacing
   }

   public static void PrintTeam(TextColor textColor)
	{
      if (!PokemonTD.AreConsoleMessagesEnabled) return;

      string textColorString = GetColorHex(textColor);

      List<Pokemon> pokemonTeam = PokemonTeam.Instance.Pokemon;

		GD.PrintRich($"[color={textColorString}]New Team:[/color]");
		for (int i = 0; i < pokemonTeam.Count; i++)
		{
			GD.PrintRich($"[color={textColorString}]\t{i + 1}: {pokemonTeam[i].Name}[/color]");
		}
      GD.Print(); // Spacing
	}

   public static void PrintStats(TextColor textColor, Pokemon pokemon)
   {
      if (!PokemonTD.AreConsoleMessagesEnabled) return;

      string textColorString = GetColorHex(textColor);

      GD.PrintRich($"[color={textColorString}]Level {pokemon.Level} {pokemon.Name} Stats:[/color]");
      GD.PrintRich($"[color={textColorString}]HP: {pokemon.HP}[/color]");
      GD.PrintRich($"[color={textColorString}]Attack: {pokemon.Attack}[/color]");
      GD.PrintRich($"[color={textColorString}]Defense: {pokemon.Defense}[/color]");
      GD.PrintRich($"[color={textColorString}]Special Attack: {pokemon.SpecialAttack}[/color]");
      GD.PrintRich($"[color={textColorString}]Special Defense: {pokemon.SpecialDefense}[/color]");
      GD.PrintRich($"[color={textColorString}]Speed: {pokemon.Speed}[/color]");
      GD.Print(); // Spacing
   }

   public static string GetEffectiveMessage(Pokemon pokemon, PokemonMove pokemonMove)
   {
      float typeMultiplier = GetTypeMultiplier(pokemon, pokemonMove);
      EffectiveType effectiveType = PokemonTypes.Instance.GetEffectiveType(typeMultiplier);
      string effectiveMessage = effectiveType switch
      {
         EffectiveType.SuperEffective => "And Was Super Effective",
         EffectiveType.Effective => "And Was Effective",
         EffectiveType.NotEffective => "And Was Not Effective",
         EffectiveType.NoEffect => "And Had No Effect",
         _ => "Effective" 
      };

      return effectiveMessage;
   }

   public static string GetDamageMessage(int damage, Pokemon defendingPokemon, PokemonMove pokemonMove)
   {
      string damageMessage = $"For {damage} Damage ";
      string effectiveMessage = GetEffectiveMessage(defendingPokemon, pokemonMove);
      damageMessage += effectiveMessage;
      return damageMessage;
   }

   private static float GetTypeMultiplier(Pokemon pokemon, PokemonMove pokemonMove)
   {
      float firstTypeMultiplier = PokemonTypes.Instance.GetTypeMultiplier(pokemonMove.Type, pokemon.Types[0]);
      if (pokemon.Types.Count > 1)
      {
         float secondTypeMultiplier = PokemonTypes.Instance.GetTypeMultiplier(pokemonMove.Type, pokemon.Types[1]);

         if (firstTypeMultiplier < secondTypeMultiplier) firstTypeMultiplier = secondTypeMultiplier;
      }
      return firstTypeMultiplier;
   }

   public static string GetColorHex(TextColor textColor) => textColor switch 
   {
      TextColor.Red => "FF4040",
      TextColor.Orange => "F88158",
      TextColor.Yellow => "E9D66B",
      TextColor.Green => "76CD26",
      TextColor.Blue => "6495ED",
      TextColor.Purple => "CA9BF7",
      _ => "FFFFFF",
   };

   public static string GetStatusConditionMessage(StatusCondition statusCondition) => statusCondition switch
   {
      StatusCondition.Burn => "Burn",
      StatusCondition.Freeze => "Frozen",
      StatusCondition.Paralysis => "Paralyzed",
      StatusCondition.Poison => "Poisoned",
      StatusCondition.BadlyPoisoned => "Badly Poisoned",
      StatusCondition.Sleep => "Asleep",
      StatusCondition.Confuse => "Confused",
      _ => ""
   };
}