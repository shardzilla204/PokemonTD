using Godot;

namespace PokemonTD;

public partial class PokemonEnemy : TextureRect
{
	[Export]
	private TextureProgressBar _healthBar;

	[Export]
	private VisibleOnScreenNotifier2D _screenNotifier;

	[Export]
	private Area2D _area;

	public Pokemon Pokemon;
	public StatusEffect StatusEffect;
	public bool IsCatchable = false;
	public VisibleOnScreenNotifier2D ScreenNotifier => _screenNotifier;

	public override void _Ready()
	{
		if (PokemonTD.AreLevelsRandomized) Pokemon.Level = PokemonTD.GetRandomLevel(PokemonTD.MinPokemonEnemyLevel, PokemonTD.MaxPokemonEnemyLevel);

		if (PokemonTD.IsCaptureModeEnabled)
		{
			EnableCaptureMode();
		}
		else
		{
			_healthBar.Value = Pokemon.HP;
			_healthBar.MaxValue = Pokemon.HP;
		}

		ScreenNotifier.ScreenExited += () =>
		{
			string passedMessage = $"{Pokemon.Name} Has Breached The Defenses";
			PrintRich.PrintLine(TextColor.Yellow, passedMessage);

			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEnemyPassed, this);
			QueueFree();

			PokemonTD.AudioManager.PlayPokemonCry(Pokemon, true);
		};

		_area.AreaEntered += OnAreaEntered;
		_area.AreaExited += OnAreaExited;
	}

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return IsCatchable;
    }

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		string capturedMessage = $"{Pokemon.Name} Has Been Captured";
		PrintRich.PrintLine(TextColor.Yellow, capturedMessage);
		PokemonTD.AddStageConsoleMessage(TextColor.Yellow, capturedMessage);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEnemyCaptured, this);

		PokemonTD.AudioManager.PlayPokemonCry(Pokemon, true);
	}

    private void OnAreaEntered(Area2D area)
	{
		StageSlot stageSlot = area.GetParentOrNull<StageSlot>();
		int pokemonEnemyQueue = stageSlot.PokemonEnemyQueue.Count;
		stageSlot.PokemonEnemyQueue.Insert(pokemonEnemyQueue, this);
	}

	private void OnAreaExited(Area2D area)
	{
		StageSlot stageSlot = area.GetParentOrNull<StageSlot>();
		stageSlot.PokemonEnemyQueue.Remove(this);
	}

	public void SetPokemon(Pokemon pokemon)
	{
		Pokemon = pokemon;
		Texture = pokemon is null ? null : pokemon.Sprite;
	}

	public void DamagePokemon(int damage, int teamSlotID)
	{
		_healthBar.Value -= damage;

		CheckIsCatchable();
		CheckHasFainted(teamSlotID);
	}

	private void CheckIsCatchable()
	{
		float capturePercentThreshold = 0.25f;

		if (_healthBar.Value > _healthBar.MaxValue * capturePercentThreshold) return;

		IsCatchable = true;
		MouseFilter = MouseFilterEnum.Stop;
		_healthBar.TintProgress = new Color(0.5f, 0, 0); // Red Color

		string catchMessage = $"{Pokemon.Name} Is Now Catchable!";
		PrintRich.PrintLine(TextColor.Yellow, catchMessage);
	}

	private void CheckHasFainted(int teamSlotID)
	{
		if (_healthBar.Value > 0) return;

		PokemonTD.AddPokeDollars(Pokemon);

		string faintMessage = $"{Pokemon.Name} Has Fainted";
		PrintRich.PrintLine(TextColor.Yellow, faintMessage);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEnemyFainted, this, teamSlotID);
		QueueFree();

		PokemonTD.AudioManager.PlayPokemonFaint();
	}

	private void EnableCaptureMode()
	{
		IsCatchable = true;
		
		_healthBar.Value = 1;
		_healthBar.MaxValue = 1;
		_healthBar.TintProgress = new Color(0.5f, 0, 0); // Red Color
	}
}
