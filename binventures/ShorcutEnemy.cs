using Godot;
using System;

public partial class ShorcutEnemy : CharacterBody3D
{
    private float _normalSpeed = 2.0f; 
    private float _creepingSpeed = 0.5f; 
    
    private Player _targetPlayer;
    private bool _isChasing = false;
    private bool _gameStarted = false;
    private bool _isWaiting = true;
    
    private MeshInstance3D _meshInstance;
    private StandardMaterial3D _crystalMaterial;
    private Timer _freezeTimer;

    public override void _Ready()
    {
        CollisionShape3D collision = new CollisionShape3D();
        collision.Shape = new BoxShape3D { Size = new Vector3(0.6f, 0.6f, 0.6f) };
        AddChild(collision);

        _meshInstance = new MeshInstance3D();
        BoxMesh crystalMesh = new BoxMesh { 
            Size = new Vector3(1.0f, 1.0f, 1.0f),
            SubdivideDepth = 2, SubdivideHeight = 2, SubdivideWidth = 2 
        };
        _meshInstance.Mesh = crystalMesh;

        _crystalMaterial = new StandardMaterial3D();
        _crystalMaterial.AlbedoColor = new Color(1, 0, 0); 
        _crystalMaterial.EmissionEnabled = true;
        _crystalMaterial.Emission = new Color(1, 0, 0);
        _crystalMaterial.EmissionEnergyMultiplier = 0.2f; 
        _crystalMaterial.RimEnabled = true;
        _crystalMaterial.Rim = 1.0f;
        _crystalMaterial.RimTint = 0.0f; 

        crystalMesh.Material = _crystalMaterial;
        AddChild(_meshInstance);

        _targetPlayer = GetParent().GetNodeOrNull<Player>("Player");
        if (_targetPlayer != null) _isChasing = true;

        _freezeTimer = new Timer();
        _freezeTimer.WaitTime = 30.0f;
        _freezeTimer.OneShot = true;
        _freezeTimer.Timeout += OnFreezeTimerTimeout;
        AddChild(_freezeTimer);
    }

    private void OnFreezeTimerTimeout()
    {
        _isWaiting = false;
        if (_crystalMaterial != null) _crystalMaterial.EmissionEnergyMultiplier = 1.2f;
    }

    public void FreezeFor30Seconds()
    {
        _freezeTimer.Stop();
        _freezeTimer.Start();
        _isWaiting = true;
        Velocity = Vector3.Zero;
        if (_crystalMaterial != null) _crystalMaterial.EmissionEnergyMultiplier = 0.2f;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameUI.IsGameOver || GameUI.IsMainMenuActive) return;

        if (!_gameStarted)
        {
            _freezeTimer.Start();
            _gameStarted = true;
        }

        _meshInstance.RotateX(0.03f);
        _meshInstance.RotateY(0.05f);
        _meshInstance.RotateZ(0.02f);

        if (_isWaiting) return;

        if (_isChasing && _targetPlayer != null && Main.PathGrid != null)
        {
            if (GlobalPosition.DistanceTo(_targetPlayer.GlobalPosition) < 0.8f)
            {
                GameUI.Instance?.ShowGameOver(false);
                return;
            }

            float currentSpeed = GameUI.IsPuzzleActive ? _creepingSpeed : _normalSpeed;

            if (GameUI.IsPuzzleActive)
            {
                Velocity = (_targetPlayer.GlobalPosition - GlobalPosition).Normalized() * currentSpeed;
            }
            else
            {
                Vector2I myPos = new Vector2I(Mathf.RoundToInt(GlobalPosition.X / Main.CellSize), Mathf.RoundToInt(GlobalPosition.Z / Main.CellSize));
                Vector2I playerPos = new Vector2I(Mathf.RoundToInt(_targetPlayer.GlobalPosition.X / Main.CellSize), Mathf.RoundToInt(_targetPlayer.GlobalPosition.Z / Main.CellSize));
                var path = Main.PathGrid.GetIdPath(myPos, playerPos);
                
                if (path.Count > 1)
                {
                    Vector3 nextTarget = new Vector3(path[1].X * Main.CellSize, GlobalPosition.Y, path[1].Y * Main.CellSize);
                    Velocity = (nextTarget - GlobalPosition).Normalized() * currentSpeed;
                }
            }
            MoveAndSlide();
        }
    }
}