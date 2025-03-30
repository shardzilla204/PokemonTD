using Godot;
using Godot.Collections;

namespace PokémonTD;

public partial class StageTeamSlot : NinePatchRect
{
	[Signal]
	public delegate void DraggingEventHandler(bool isDragging);

	[Export]
	private TextureRect _genderSprite;

	[Export]
	private Label _pokémonName;

	[Export]
	private TextureRect _pokémonSprite;

	[Export]
	private Label _pokémonLevel;

	[Export]
	private TextureProgressBar _experienceBar;

	[Export]
	private CustomButton _moveButton;

	[Export]
	private Label _moveName;

	private bool _isFilled;
	private bool _isDragging;
	private bool _isPokémonOut;

	public int ID = 0;

	public Pokémon Pokémon;

	public override void _Ready()
	{
		_moveButton.Pressed += OnMoveButtonPressed;

		if (Pokémon == null) return;

		UpdateControls();
	}

	public override void _Notification(int what)
	{
		if (what != NotificationDragEnd || !_isDragging) return;

		_isDragging = false;
		EmitSignal(SignalName.Dragging, false);

		if (IsDragSuccessful()) PokémonTD.Signals.EmitSignal(Signals.SignalName.PokémonOnStage, ID);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		_isDragging = true; 
		EmitSignal(SignalName.Dragging, true);
		
		SetDragPreview(GetDragPreview());
		return GetDragData();
	}

	private Dictionary<string, Variant> GetDragData()
	{
		return new Dictionary<string, Variant>()
		{
			{ "TeamSlotID", ID },
			{ "FromTeamSlot", true },
			{ "Pokémon", Pokémon }
		};
	}

	// Updates text, textures and progress bars
	private void UpdateControls()
	{
		_pokémonName.Text = Pokémon != null ? $"{Pokémon.Name}" : "";
		_pokémonSprite.Texture = Pokémon != null ? Pokémon.Sprite : null;
		_pokémonLevel.Text = Pokémon != null ? $"Lvl. {Pokémon.Level}": null;

		_experienceBar.Value = Pokémon.MinExperience;
		_experienceBar.MaxValue = Pokémon.MaxExperience;

		_genderSprite.Texture = GetGenderSprite();

		UpdateMoveButton();
	}

	private Texture2D GetGenderSprite()
	{
		string filePath = $"res://Assets/Images/Gender/{Pokémon.Gender}Icon.png";

		return ResourceLoader.Load<Texture2D>(filePath);
	}

	private Control GetDragPreview()
	{
		int minValue = 85;
		Vector2 minSize = new Vector2(minValue, minValue);
		TextureRect textureRect = new TextureRect()
		{
			CustomMinimumSize = minSize,
			Texture = Pokémon.Sprite,
			TextureFilter = TextureFilterEnum.Nearest,
			Position = -minSize / 2
		};

		Control control = new Control();
		control.AddChild(textureRect);

		return control;
	}

	private void OnPokémonMoveChanged(int teamSlotID, PokémonMove pokémonMove)
	{
		if (ID != teamSlotID) return;

		string changedPokémonMoveMessage = $"{Pokémon.Name}'s Move Is Now {pokémonMove.Name}";
		PrintRich.PrintLine(TextColor.Purple, changedPokémonMoveMessage);
		
		Pokémon.CurrentMove = pokémonMove;
		UpdateMoveButton();
	}

   	private void OnMoveButtonPressed()
	{
		PokémonTD.Signals.EmitSignal(Signals.SignalName.ChangeMovesetPressed);

		MovesetInterface movesetInterface = PokémonTD.PackedScenes.GetMovesetInterface();
		movesetInterface.Pokémon = Pokémon;
		movesetInterface.TeamSlotID = ID;

		movesetInterface.PokémonMoveChanged += OnPokémonMoveChanged;
		
		StageInterface stageInterface = GetParentOrNull<Node>().GetOwnerOrNull<StageInterface>();
		PokémonStage pokémonStage = stageInterface.GetParentOrNull<PokémonStage>();
		pokémonStage.AddChild(movesetInterface);
	}

	private void UpdateStats()
	{
		Pokémon.HP = PokémonTD.PokémonManager.GetPokémonHP(Pokémon);
		Pokémon.Attack = PokémonTD.PokémonManager.GetOtherPokémonStat(Pokémon, PokémonStat.Attack);
		Pokémon.Defense = PokémonTD.PokémonManager.GetOtherPokémonStat(Pokémon, PokémonStat.Defense);
		Pokémon.SpecialAttack = PokémonTD.PokémonManager.GetOtherPokémonStat(Pokémon, PokémonStat.SpecialAttack);
		Pokémon.SpecialDefense = PokémonTD.PokémonManager.GetOtherPokémonStat(Pokémon, PokémonStat.SpecialDefense);
		Pokémon.Speed = PokémonTD.PokémonManager.GetOtherPokémonStat(Pokémon, PokémonStat.Speed);
	}

	private void UpdateMoveButton()
	{
		Color typeColor = PokémonTD.PokémonTypes.GetTypeColor(Pokémon.CurrentMove.Type);
		Control texture = _moveButton.GetChild<Control>(0);
		texture.SelfModulate = typeColor;

		_moveName.Text = Pokémon.CurrentMove.Name;
	}

	// ? EXP Formula
	// EXP = b * L / 7
	// b = Pokémon Enemy Experience Yield
	// L = Pokémon Enemy Level
	public int GetExperience(PokémonEnemy pokémonEnemy)
	{
		return Mathf.RoundToInt(pokémonEnemy.Pokémon.ExperienceYield * pokémonEnemy.Pokémon.Level / 7);
	}

	public void AddExperience(int experience)
	{
		if (Pokémon.Level >= PokémonTD.MaxPokémonLevel) return;

		_experienceBar.Value += experience;
		Pokémon.MinExperience = (int) _experienceBar.Value;

		string pokémonGainedExperienceMessage = $"{Pokémon.Name} Gained {experience} EXP";
		PrintRich.PrintLine(TextColor.Purple, pokémonGainedExperienceMessage);

		if (_experienceBar.Value >= _experienceBar.MaxValue) GetLevels();
	}

	private void GetLevels()
	{
		while (Pokémon.MinExperience >= Pokémon.MaxExperience)
		{
			Pokémon.Level++;

			_experienceBar.Value -= _experienceBar.MaxValue;
			_experienceBar.MaxValue = Pokémon.GetExperienceRequired();

			Pokémon.MinExperience = (int) _experienceBar.Value;
			Pokémon.MaxExperience = (int) _experienceBar.MaxValue;

			PokémonTD.Signals.EmitSignal(Signals.SignalName.PokémonLeveledUp, Pokémon);

			UpdateControls();
			UpdateStats();

			string pokémonCanLevelUpMessage = $"{Pokémon.Name} Has Leveled Up To Level {Pokémon.Level}";
			PrintRich.PrintLine(TextColor.Purple, pokémonCanLevelUpMessage); ;
		}
	}
}
