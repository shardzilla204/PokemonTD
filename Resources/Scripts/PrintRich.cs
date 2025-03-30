using System.Collections.Generic;
using Godot;

namespace PokémonTD;

public enum TextColor
{
   Red, // Error
   Orange, // Action
   Yellow, // General Event
   Green, // Success
   Blue,
   Purple, // Pokémon Event
}

public partial class PrintRich
{
   public static void Print(TextColor textColor, string text)
   {
      if (!PokémonTD.AreConsoleMessagesEnabled) return;

      string textColorString = GetColorHex(textColor);
      GD.PrintRich($"[color={textColorString}]{text}[/color]");
   }

   // Adds new lines for better readability
   public static void PrintLine(TextColor textColor, string text)
   {
      if (!PokémonTD.AreConsoleMessagesEnabled) return;

      string textColorString = GetColorHex(textColor);
      GD.PrintRich($"[color={textColorString}]{text}[/color]");
      GD.Print();
   }

   public static void PrintTeam(TextColor textColor)
	{
      if (!PokémonTD.AreConsoleMessagesEnabled) return;

      string textColorString = GetColorHex(textColor);

      List<Pokémon> pokémonTeam = PokémonTD.PokémonTeam.Pokémon;

		GD.PrintRich($"[color={textColorString}]New Team:[/color]");
		for (int i = 0; i < pokémonTeam.Count; i++)
		{
			GD.PrintRich($"[color={textColorString}]\t{i + 1}: {pokémonTeam[i].Name}[/color]");
		}
      GD.Print();
	}

   public static void PrintStats(TextColor textColor, Pokémon pokémon)
   {
      if (!PokémonTD.AreConsoleMessagesEnabled) return;

      string textColorString = GetColorHex(textColor);

      GD.PrintRich($"[color={textColorString}]Level {pokémon.Level} {pokémon.Name} Stats:[/color]");
      GD.PrintRich($"[color={textColorString}]HP: {pokémon.HP}[/color]");
      GD.PrintRich($"[color={textColorString}]Attack: {pokémon.Attack}[/color]");
      GD.PrintRich($"[color={textColorString}]Defense: {pokémon.Defense}[/color]");
      GD.PrintRich($"[color={textColorString}]Special Attack: {pokémon.SpecialAttack}[/color]");
      GD.PrintRich($"[color={textColorString}]Special Defense: {pokémon.SpecialDefense}[/color]");
      GD.PrintRich($"[color={textColorString}]Speed: {pokémon.Speed}[/color]");
      GD.Print();
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