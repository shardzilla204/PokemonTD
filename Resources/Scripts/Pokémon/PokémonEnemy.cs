using Godot;

namespace PokémonTD;

public partial class PokémonEnemy : TextureRect
{
	[Signal]
	public delegate void FaintedEventHandler(PokémonEnemy pokémonEnemy);

	[Signal]
	public delegate void CapturedEventHandler(PokémonEnemy pokémonEnemy);

	[Export]
	private TextureProgressBar _healthBar;

	[Export]
	private VisibleOnScreenNotifier2D _screenNotifier;

	[Export]
	private Area2D _area;

	public Pokémon Pokémon;
	public StatusEffect StatusEffect;
	public bool IsCatchable = false;
	public VisibleOnScreenNotifier2D ScreenNotifier => _screenNotifier;

	public override void _Ready()
	{
		if (PokémonTD.AreLevelsRandomized) Pokémon.Level = PokémonTD.GetRandomLevel(PokémonTD.MinPokémonEnemyLevel, PokémonTD.MaxPokémonEnemyLevel);

		if (PokémonTD.IsCaptureModeEnabled)
		{
			EnableCaptureMode();
		}
		else
		{
			_healthBar.Value = Pokémon.HP;
			_healthBar.MaxValue = Pokémon.HP;
		}

		ScreenNotifier.ScreenExited += () =>
		{
			PokémonTD.Signals.EmitSignal(Signals.SignalName.PokémonEnemyPassed, this);
			QueueFree();

			string passedMessage = $"{Pokémon.Name} Has Breached The Defenses";
			PrintRich.PrintLine(TextColor.Yellow, passedMessage);
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
		string passedMessage = $"{Pokémon.Name} Has Been Captured";
		PrintRich.PrintLine(TextColor.Yellow, passedMessage);

		EmitSignal(SignalName.Captured, this);
		PokémonTD.Signals.EmitSignal(Signals.SignalName.PokémonEnemyCaptured, this);
	}

    private void OnAreaEntered(Area2D area)
	{
		StageSlot stageSlot = area.GetParentOrNull<StageSlot>();
		int pokémonEnemyQueue = stageSlot.PokémonEnemyQueue.Count;
		stageSlot.PokémonEnemyQueue.Insert(pokémonEnemyQueue, this);
	}

	private void OnAreaExited(Area2D area)
	{
		StageSlot stageSlot = area.GetParentOrNull<StageSlot>();
		stageSlot.PokémonEnemyQueue.Remove(this);
	}

	public void SetPokémon(Pokémon pokémon)
	{
		Pokémon = pokémon;
		Texture = pokémon is null ? null : pokémon.Sprite;
	}

	public void DamagePokémon(int damage)
	{
		_healthBar.Value -= damage;

		CheckIsCatchable();
		CheckHasFainted();
	}

	private void CheckIsCatchable()
	{
		float capturePercentThreshold = 0.25f;

		if (_healthBar.Value > _healthBar.MaxValue * capturePercentThreshold) return;

		IsCatchable = true;

		_healthBar.TintProgress = new Color(0.5f, 0, 0); // Red Color

		string catchMessage = $"{Pokémon.Name} Is Now Catchable!";
		PrintRich.PrintLine(TextColor.Yellow, catchMessage);
	}

	private void EnableCaptureMode()
	{
		IsCatchable = true;
		
		_healthBar.Value = 1;
		_healthBar.MaxValue = 1;
		_healthBar.TintProgress = new Color(0.5f, 0, 0); // Red Color
	}

	private void CheckHasFainted()
	{
		if (_healthBar.Value > 0) return;

		PokémonTD.AddPokéDollars(Pokémon);

		string faintMessage = $"{Pokémon.Name} Has Fainted";
		PrintRich.PrintLine(TextColor.Yellow, faintMessage);

		EmitSignal(SignalName.Fainted, this);
		PokémonTD.Signals.EmitSignal(Signals.SignalName.PokémonEnemyFainted, this);
		QueueFree();
	}
}
