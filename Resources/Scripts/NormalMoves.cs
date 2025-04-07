using Godot;

namespace PokemonTD;

public partial class NormalMoves 
{
   public static void Barrage(PokemonEnemy pokemonEnemy)
   {
      PokemonMove pokemonMove = PokemonTD.PokemonMoveset.GetPokemonMove("Barrage");
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