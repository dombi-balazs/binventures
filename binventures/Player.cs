using Godot;
using System;

public partial class Player : CharacterBody3D
{
    private float _speed = 10.0f;
    private MeshInstance3D _meshInstance;

    public override void _Ready()
    {
        CollisionShape3D collision = new CollisionShape3D();
        collision.Shape = new CylinderShape3D { Radius = 0.4f, Height = 1.0f };
        AddChild(collision);

        this.MotionMode = MotionModeEnum.Grounded;
        _meshInstance = new MeshInstance3D();
        CylinderMesh cylinderMesh = new CylinderMesh { RadialSegments = 6, Height = 0.8f, TopRadius = 0.7f, BottomRadius = 0.7f };
        _meshInstance.Mesh = cylinderMesh;

        StandardMaterial3D material = new StandardMaterial3D();
        material.AlbedoColor = new Color(0, 1, 1); 
        material.EmissionEnabled = true;
        material.Emission = new Color(0, 1, 1);
        material.EmissionEnergyMultiplier = 1.2f; 
        material.RimEnabled = true;
        material.Rim = 1.0f;
        material.RimTint = 0.0f; 

        cylinderMesh.Material = material;
        AddChild(_meshInstance);

        Camera3D camera = new Camera3D();
        camera.Position = new Vector3(0, 12, 0);
        camera.RotationDegrees = new Vector3(-90, 0, 0);
        camera.Current = true;
        AddChild(camera);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameUI.IsPuzzleActive || GameUI.IsGameOver || GameUI.IsMainMenuActive) return;

        if (_meshInstance != null) _meshInstance.RotateY(3.0f * (float)delta);

        Vector3 direction = Vector3.Zero;
        if (Input.IsPhysicalKeyPressed(Key.W) || Input.IsPhysicalKeyPressed(Key.Up)) direction.Z -= 1;
        if (Input.IsPhysicalKeyPressed(Key.S) || Input.IsPhysicalKeyPressed(Key.Down)) direction.Z += 1;
        if (Input.IsPhysicalKeyPressed(Key.A) || Input.IsPhysicalKeyPressed(Key.Left)) direction.X -= 1;
        if (Input.IsPhysicalKeyPressed(Key.D) || Input.IsPhysicalKeyPressed(Key.Right)) direction.X += 1;

        Velocity = direction.Normalized() * _speed;
        MoveAndSlide();
    }
}