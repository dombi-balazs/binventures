using Godot;
using System;

public partial class ExitZone : Area3D
{
    public override void _Ready()
    {
        // 1. Ütközési zóna (Kicsit nagyobbra vesszük, hogy könnyen bele lehessen lépni)
        CollisionShape3D collision = new CollisionShape3D();
        collision.Shape = new BoxShape3D { Size = new Vector3(2.0f, 2.0f, 2.0f) };
        AddChild(collision);

        // 2. Zöld neon oszlop kinézete
        MeshInstance3D meshInstance = new MeshInstance3D();
        CylinderMesh cylinderMesh = new CylinderMesh { RadialSegments = 8, TopRadius = 0.8f, BottomRadius = 0.8f, Height = 3.0f };
        meshInstance.Mesh = cylinderMesh;

        StandardMaterial3D material = new StandardMaterial3D();
        material.AlbedoColor = new Color(0, 0.2f, 0); // Sötétzöld test
        material.EmissionEnabled = true;
        material.Emission = new Color(0, 1, 0); // Élénk neon zöld fény
        material.EmissionEnergyMultiplier = 4.0f;
        cylinderMesh.Material = material;
        AddChild(meshInstance);

        // Összekötjük az ütközés eseményét
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Player)
        {
            GameUI.Instance?.ShowGameOver(true);
            QueueFree(); 
        }
    }
}