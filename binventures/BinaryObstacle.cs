using Godot;
using System;

public partial class BinaryObstacle : Area3D
{
    public int TargetDecimal { get; private set; }
    public string CorrectBinary { get; private set; }
    public bool IsPlayerNear { get; private set; } = false;

    [Signal]
    public delegate void PuzzleTriggeredEventHandler(int decimalNumber, BinaryObstacle obstacle);

    public override void _Ready()
    {
        Random random = new Random();
        TargetDecimal = random.Next(0, 100); 
        
        CorrectBinary = Convert.ToString(TargetDecimal, 2).PadLeft(8, '0');

        CollisionShape3D collision = new CollisionShape3D();
        collision.Shape = new CylinderShape3D { Radius = 2.0f, Height = 2.0f };
        AddChild(collision);

        MeshInstance3D meshInstance = new MeshInstance3D();
        PrismMesh prismMesh = new PrismMesh { Size = new Vector3(1, 1.5f, 1) };
        meshInstance.Mesh = prismMesh;

        StandardMaterial3D material = new StandardMaterial3D();
        material.AlbedoColor = new Color(0, 0, 0);
        material.EmissionEnabled = true;
        material.Emission = new Color(1, 0, 1);
        material.EmissionEnergyMultiplier = 4.0f;
        prismMesh.Material = material;

        AddChild(meshInstance);

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Player)
        {
            IsPlayerNear = true;
            EmitSignal(SignalName.PuzzleTriggered, TargetDecimal, this);
        }
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is Player) IsPlayerNear = false;
    }

    public void Resolve()
    {
        QueueFree();
    }
}