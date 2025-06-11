using Godot;

namespace PokemonTD;

public partial class InformationInterface : CanvasLayer
{
    [Export]
    private CustomButton _exitButton;

    [Export]
    private Label _statusConditionNotesLabel;

    [Export]
    private Label _keybindNotesLabel;

    [Export]
    private Label _extraNotesLabel;

    [Export]
    private VideoStreamPlayer _videoStreamPlayer;

    [Export]
    private Label _videoTitle;

    [Export]
    private Label _videoCaption;

    [Export]
    private CustomButton _nextButton;

    [Export]
    private CustomButton _previousButton;

    private int _pageIndex;
    private int _extraPages;
    private int _maxPageCount;
    private int _videoCount;

    public bool FromMainMenu; // For Pokemon Stage

    public override void _Ready()
    {
        Control parent = _videoStreamPlayer.GetParentOrNull<Control>();
        _extraPages = parent.GetChildCount() - 1; // Include the video stream player to get the extra pages
        
        // Index starts at 0, so subtract the total from 1
        _videoCount = PokemonVideos.Instance.Videos.Count - 1;
        _maxPageCount = _videoCount + _extraPages;
        _exitButton.Pressed += () =>
        {
            if (!PokemonTD.HasSelectedStarter)
            {
                PokemonSettings.Instance.HasShowedTutorial = true;

                Node starterSelectionInterface = PokemonTD.PackedScenes.GetStarterSelectionInterface();
                AddSibling(starterSelectionInterface);
                QueueFree();
                return;
            }
            else
            {
                PokemonSettings.Instance.HasShowedTutorial = true;
            }

            SettingsInterface settingsInterface = PokemonTD.PackedScenes.GetSettingsInterface();
            settingsInterface.FromMainMenu = FromMainMenu;
            AddSibling(settingsInterface);
            QueueFree();
        };

        _nextButton.Pressed += () =>
        {
            _pageIndex++;
            if (_pageIndex > _maxPageCount) _pageIndex = 0;
            if (_pageIndex == _maxPageCount && !PokemonSettings.Instance.HasShowedTutorial)
            {
                _exitButton.Visible = true;
                _previousButton.Visible = true;
            }
            SetPage();
        };

        _previousButton.Pressed += () =>
        {
            _pageIndex--;
            if (_pageIndex < 0) _pageIndex = _maxPageCount;
            SetPage();
        };

        if (!PokemonSettings.Instance.HasShowedTutorial)
        {
            _exitButton.Visible = false;
            _previousButton.Visible = false;
        }
        SetPage();
    }

    private void SetPage()
    {
        _videoStreamPlayer.Visible = _pageIndex <= _videoCount;

        SetExtraPages();

        if (_pageIndex > _videoCount) return;

        PokemonVideo pokemonVideo = PokemonVideos.Instance.Videos[_pageIndex];
        _videoTitle.Text = $"{pokemonVideo.Title}";
        _videoCaption.Text = $"{pokemonVideo.Caption}";

        VideoStreamTheora video = PokemonVideos.Instance.GetVideoStream(_pageIndex);
        _videoStreamPlayer.Stream = video;
        _videoStreamPlayer.Play();
    }

    private void SetExtraPages()
    {
        int extraPageIndex = _pageIndex - _videoCount;
        _statusConditionNotesLabel.Visible = extraPageIndex == 1;
        _keybindNotesLabel.Visible = extraPageIndex == 2;
        _extraNotesLabel.Visible = extraPageIndex == 3;

        if (extraPageIndex < 0) return;

        string videoTitle = extraPageIndex switch
        {
            1 => "Status Conditions",
            2 => "Keybinds",
            3 => "Extra Notes",
            _ => ""
        };

        _videoTitle.Text = videoTitle;
        _videoCaption.Text = "";
    }
}
