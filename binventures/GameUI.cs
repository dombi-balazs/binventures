using Godot;
using System;

public partial class GameUI : CanvasLayer
{
    public static GameUI Instance { get; private set; }
    public static bool IsMainMenuActive = true; 
    public static bool IsPuzzleActive = false; 
    public static bool IsGameOver = false;

    private ColorRect _dimmer;
    private VBoxContainer _mainMenuContainer;
    private VBoxContainer _gameOverContainer;
    private VBoxContainer _puzzleContainer;
    private Label _notificationLabel;
    private LineEdit _inputField;
    private BinaryObstacle _currentObstacle;
    private Timer _hideNotificationTimer;

    private LabelSettings CreateVaporLabelSettings(Color glowColor, int size)
    {
        var settings = new LabelSettings();
        settings.FontSize = size;
        settings.FontColor = new Color(1, 1, 1);
        settings.OutlineSize = 8;
        settings.OutlineColor = glowColor;
        return settings;
    }

    public override void _Ready()
    {
        Instance = this;
        IsMainMenuActive = true;
        IsPuzzleActive = false;
        IsGameOver = false;

        Control mainLayout = new Control();
        mainLayout.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(mainLayout);

        _dimmer = new ColorRect();
        _dimmer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _dimmer.Color = new Color(0.1f, 0, 0.2f, 0.8f);
        mainLayout.AddChild(_dimmer);

        // 3. FŐMENÜ
        _mainMenuContainer = CreateCenterContainer(mainLayout);
        
        Label title = new Label();
        title.Text = "BINVENTURES";
        title.LabelSettings = CreateVaporLabelSettings(new Color(0, 1, 1), 72); // Cián glow
        title.HorizontalAlignment = HorizontalAlignment.Center;
        _mainMenuContainer.AddChild(title);

        Label subtitle = new Label();
        subtitle.Text = "Retro Áramkör Szimulátor";
        subtitle.LabelSettings = CreateVaporLabelSettings(new Color(1, 0, 1), 32); // Magenta glow
        subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        _mainMenuContainer.AddChild(subtitle);

        AddMenuButton(_mainMenuContainer, "JÁTÉK INDÍTÁSA", OnStartPressed);
        AddMenuButton(_mainMenuContainer, "KILÉPÉS", OnQuitPressed);

        // 4. GAME OVER
        _gameOverContainer = CreateCenterContainer(mainLayout);
        _gameOverContainer.Visible = false;

        Label goTitle = new Label();
        goTitle.Text = "RENDSZER LEÁLLT";
        goTitle.Name = "GameOverTitle";
        goTitle.LabelSettings = CreateVaporLabelSettings(new Color(1, 0, 0), 64);
        goTitle.HorizontalAlignment = HorizontalAlignment.Center;
        _gameOverContainer.AddChild(goTitle);

        AddMenuButton(_gameOverContainer, "ÚJRAINDÍTÁS", () => GetTree().ReloadCurrentScene());
        AddMenuButton(_gameOverContainer, "KILÉPÉS", OnQuitPressed);

        // 5. JÁTÉKOS UI
        _puzzleContainer = new VBoxContainer();
        _puzzleContainer.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
        _puzzleContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _puzzleContainer.CustomMinimumSize = new Vector2(600, 200);
        _puzzleContainer.Position = new Vector2(0, 50); 
        _puzzleContainer.Visible = false;
        mainLayout.AddChild(_puzzleContainer);

        _notificationLabel = new Label();
        _notificationLabel.LabelSettings = CreateVaporLabelSettings(new Color(0, 1, 1), 32);
        _notificationLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _puzzleContainer.AddChild(_notificationLabel);

        _inputField = new LineEdit();
        _inputField.PlaceholderText = "Bináris kód...";
        _inputField.Alignment = HorizontalAlignment.Center;
        _inputField.Visible = false;
        _inputField.FocusMode = Control.FocusModeEnum.None;
        _inputField.TextSubmitted += OnTextSubmitted;
        _puzzleContainer.AddChild(_inputField);

        _hideNotificationTimer = new Timer { WaitTime = 3.0f, OneShot = true };
        _hideNotificationTimer.Timeout += HideNotification;
        AddChild(_hideNotificationTimer);
    }

    private VBoxContainer CreateCenterContainer(Control parent)
    {
        VBoxContainer container = new VBoxContainer();
        container.SetAnchorsPreset(Control.LayoutPreset.Center); 
        container.SetBegin(new Vector2(-300, -200)); 
        container.SetEnd(new Vector2(300, 200));
        container.Alignment = BoxContainer.AlignmentMode.Center;
        parent.AddChild(container);
        return container;
    }

    private void AddMenuButton(VBoxContainer parent, string text, Action action)
    {
        Button btn = new Button();
        btn.Text = text;
        btn.CustomMinimumSize = new Vector2(300, 60);
        btn.Pressed += action;
        btn.AddThemeColorOverride("font_color", new Color(0, 1, 1));
        parent.AddChild(btn);
    }

    public override void _Process(double delta)
    {
        if (IsPuzzleActive && _inputField.Visible && !_inputField.HasFocus())
            _inputField.GrabFocus();
    }

    private void OnStartPressed() 
    { 
        IsMainMenuActive = false; 
        _mainMenuContainer.Visible = false; 
        _dimmer.Visible = false; 
        _puzzleContainer.Visible = true; 
        ShowTimedNotification("Rendszer aktív...");
    }

    private void OnQuitPressed() => GetTree().Quit();

    public void ShowGameOver(bool isWin = false)
    {
        IsGameOver = true; 
        _puzzleContainer.Visible = false; 
        _dimmer.Visible = true; 
        _gameOverContainer.Visible = true;

        var title = _gameOverContainer.GetNode<Label>("GameOverTitle");
        title.Text = isWin ? "PLAYER HAS WON!" : "RENDSZER LEÁLLT";
        title.LabelSettings.OutlineColor = isWin ? new Color(0, 1, 0) : new Color(1, 0, 0);
    }

    public void ShowPuzzle(int decimalNumber, BinaryObstacle obstacle)
    {
        IsPuzzleActive = true;
        _currentObstacle = obstacle;
        _notificationLabel.LabelSettings.OutlineColor = new Color(1, 0, 1); // Magenta Glow az akadályhoz
        _notificationLabel.Text = $"FIGYELEM: Elszabadult elektron!\nKonvertáld: {decimalNumber}";
        
        _inputField.Visible = true;
        _inputField.FocusMode = Control.FocusModeEnum.All;
        _inputField.GrabFocus();
        _hideNotificationTimer.Stop();
    }

    private void ShowTimedNotification(string text)
    {
        _notificationLabel.LabelSettings.OutlineColor = new Color(0, 1, 1); // Cián glow a sikerhez
        _notificationLabel.Text = text;
        _inputField.Visible = false;
        _hideNotificationTimer.Start();
    }

    private void HideNotification() => _notificationLabel.Text = "";

    private void OnTextSubmitted(string newText)
    {
        if (_currentObstacle != null)
        {
            bool isCorrect = false;
            try
            {
                if (Convert.ToInt32(newText, 2) == _currentObstacle.TargetDecimal) 
                {
                    isCorrect = true;
                }
            }
            catch { isCorrect = false; }

            if (isCorrect)
            {
                ShowTimedNotification("STABILIZÁLVA!\nShorcut leállítva 30 mp-re.");
                _inputField.Visible = false;
                _inputField.ReleaseFocus();
                _inputField.FocusMode = Control.FocusModeEnum.None;
                IsPuzzleActive = false; 
                
                var enemy = GetTree().Root.FindChild("ShorcutEnemy", true, false) as ShorcutEnemy;
                enemy?.FreezeFor30Seconds();
                
                _currentObstacle.Resolve();
                _currentObstacle = null;
            }
            else
            {
                _notificationLabel.LabelSettings.OutlineColor = new Color(1, 0, 0); // Vörös glow a hibára
                _notificationLabel.Text = $"HIBÁS VÁLASZ!\nKonvertáld: {_currentObstacle.TargetDecimal}";
                _inputField.Clear();
            }
        }
    }
}