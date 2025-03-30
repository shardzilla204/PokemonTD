using System;
using Godot;

namespace PokémonTD;

public enum CoinFace
{
   Heads,
   Tails
}

public enum StatusEffectType
{
   None,
   Burn,
   Confuse,
   Freeze,
   Sleep,
   Paralysis,
   Poison
}

public partial class MoveEffectManager : Node
{

   public void ApplyStatusEffect(StatusEffectType statusEffectType)
   {
      switch (statusEffectType)
      {
         case StatusEffectType.Burn:
            ApplyBurnStatusEffect();
            break;

         case StatusEffectType.Confuse:
            ApplyConfuseStatusEffect();
            break;

         case StatusEffectType.Freeze:
            ApplyFreezeStatusEffect();
            break;

         case StatusEffectType.Sleep:
            ApplySleepStatusEffect();
            break;

         case StatusEffectType.Paralysis:
            ApplyParalysisStatusEffect();
            break;

         case StatusEffectType.Poison:
            ApplyPoisonStatusEffect();
            break;

         default: 
            return;
      }
   }

   public void ApplyBurnStatusEffect()
   {

   }

   public void ApplyConfuseStatusEffect()
   {

   }

   public void ApplyFreezeStatusEffect()
   {

   }

   public void ApplySleepStatusEffect()
   {
      
   }

   public void ApplyParalysisStatusEffect()
   {

   }

   public void ApplyPoisonStatusEffect()
   {

   }

   public CoinFace GetRandomCoinFace()
   {
      RandomNumberGenerator RNG = new RandomNumberGenerator();
      CoinFace randomCoinFace = (CoinFace) RNG.RandiRange((int) CoinFace.Heads, (int) CoinFace.Tails);

      return randomCoinFace;
   }
}