using Godot;
using GC = Godot.Collections;

namespace PokémonTD;

public partial class PokémonData : Node
{
	public new string Name { private set; get; }

	public string NationalNumber;
	public string Species;
   public string Description;
	public float Height;
	public float Weight;
   public Texture2D Sprite;
	public int BaseExperienceYield { private set; get; }

	public int HP { private set; get; }
	public int Attack { private set; get; }
	public int Defense { private set; get; }
	public int SpecialAttack { private set; get; }
	public int SpecialDefense { private set; get; }
	public int Speed { private set; get; }

	public int MinExperience;
	public int MaxExperience = 100;

	public void SetData(GC.Dictionary<string, Variant> pokémonData)
	{
		Name = pokémonData["Name"].As<string>();
		Species = pokémonData["Species"].As<string>();
		Description = pokémonData["Description"].As<string>();
		Height = pokémonData["Height"].As<float>();
		Weight = pokémonData["Weight"].As<float>();
      Sprite = PokémonTD.GetPokémonSprite(pokémonData["Name"].As<string>());
		BaseExperienceYield = pokémonData["Base Experience Yield"].As<int>();
	}

	public void SetStats(GC.Dictionary<string, Variant> pokémonStats)
	{
		HP = pokémonStats["HP"].As<int>();
		Attack = pokémonStats["Attack"].As<int>();
		Defense = pokémonStats["Defense"].As<int>();
		SpecialAttack = pokémonStats["Special Attack"].As<int>();
		SpecialDefense = pokémonStats["Special Defense"].As<int>();
		Speed = pokémonStats["Speed"].As<int>();
	}
}
