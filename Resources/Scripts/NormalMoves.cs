using Godot;

namespace PokémonTD;

public partial class NormalMoves 
{
   public static void Barrage(PokémonEnemy pokémonEnemy)
   {
      PokémonMove pokémonMove = PokémonTD.PokémonMoveset.GetPokémonMove("Barrage");
      int minCount = 2;
      int maxCount = 5;

      RandomNumberGenerator RNG = new RandomNumberGenerator();
      int randomCount = RNG.RandiRange(minCount, maxCount);

      for (int i = 0; i < randomCount; i++)
      {
         
      }
   }

   public static void Bind()
   {

   }  
}