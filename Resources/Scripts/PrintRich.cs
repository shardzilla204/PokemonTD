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

   // Adds new lines for readability
   public static void PrintLine(TextColor textColor, string text)
   {
      if (!PokemonTD.AreConsoleMessagesEnabled) return;

      string textColorString = GetColorHex(textColor);
      GD.PrintRich($"[color={textColorString}]{text}[/color]");
      GD.Print();
   }

   public static void PrintTeam(TextColor textColor)
	{
      if (!PokemonTD.AreConsoleMessagesEnabled) return;

      string textColorString = GetColorHex(textColor);

      List<Pokemon> PokemonTeam = PokemonTD.PokemonTeam.Pokemon;

		GD.PrintRich($"[color={textColorString}]New Team:[/color]");
		for (int i = 0; i < PokemonTeam.Count; i++)
		{
			GD.PrintRich($"[color={textColorString}]\t{i + 1}: {PokemonTeam[i].Name}[/color]");
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

   public static void PrintEffectiveness(TextColor textColor, EffectiveType effectiveType)
   {
      string effectiveMessage = effectiveType switch
      {
         EffectiveType.SuperEffective => "And Was Super Effective",
         EffectiveType.Effective => "And Was Effective",
         EffectiveType.NotEffective => "And Was Not Effective",
         EffectiveType.NoEffect => "And Had No Effect",
         _ => "Effective" 
      };

      PrintLine(textColor, effectiveMessage);
   }

   public static string GetColorHex(TextColor textColor) => textColor switch 
   {
      TextColor.Red => "FF0000",
      TextColor.Orange => "F88158",
      TextColor.Yellow => "E9D66B",
      TextColor.Green => "76CD26",
      TextColor.Blue => "6495ED",
      TextColor.Purple => "CA9BF7",
      _ => "FFFFFF",
   };
}