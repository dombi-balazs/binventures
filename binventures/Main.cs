using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node3D
{
	public const float CellSize = 2.0f;
	public static AStarGrid2D PathGrid;
	
	private const int MazeWidth = 51; 
	private const int MazeHeight = 51;
	private int[,] _mazeMap;
	private GameUI _ui;
	private Vector3 _playerSpawnPos;

	private List<BinaryObstacle> _obstacles = new List<BinaryObstacle>();
	private AudioStreamPlayer _musicPlayer;

	public override void _Ready()
	{
		SetupEnvironment();
		GenerateProceduralMaze();

		PathGrid = new AStarGrid2D();
		PathGrid.Region = new Rect2I(0, 0, MazeWidth, MazeHeight);
		PathGrid.CellSize = new Vector2(CellSize, CellSize);
		PathGrid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		PathGrid.Update();

		BuildMazeFromMap();
		SetupMusic();
	}

	private void SetupMusic()
	{
		_musicPlayer = new AudioStreamPlayer();
		_musicPlayer.Stream = GD.Load<AudioStream>("res://zene.mp3");
		_musicPlayer.Bus = "Master";
		AddChild(_musicPlayer);
	}

	private void SetupEnvironment()
	{
		WorldEnvironment worldEnv = new WorldEnvironment();
		Godot.Environment env = new Godot.Environment();
		env.BackgroundMode = Godot.Environment.BGMode.Color;
		env.BackgroundColor = new Color(0.01f, 0.01f, 0.03f);
		env.GlowEnabled = true;
		env.GlowIntensity = 1.5f;
		env.GlowBlendMode = Godot.Environment.GlowBlendModeEnum.Additive;
		worldEnv.Environment = env;
		AddChild(worldEnv);
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel") || Input.IsKeyPressed(Key.Escape))
			GetTree().Quit();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("music_toggle") || Input.IsKeyPressed(Key.M))
		{
			if (_musicPlayer.Playing) _musicPlayer.Stop();
			else _musicPlayer.Play();
		}
	}

	private void GenerateProceduralMaze()
	{
		_mazeMap = new int[MazeHeight, MazeWidth];
		for (int z = 0; z < MazeHeight; z++)
			for (int x = 0; x < MazeWidth; x++)
				_mazeMap[z, x] = 1;

		Stack<Vector2I> stack = new Stack<Vector2I>();
		Vector2I current = new Vector2I(1, 1);
		_mazeMap[current.Y, current.X] = 0;
		stack.Push(current);

		Random rnd = new Random();
		Vector2I[] directions = { new Vector2I(0, -2), new Vector2I(0, 2), new Vector2I(-2, 0), new Vector2I(2, 0) };

		while (stack.Count > 0)
		{
			current = stack.Pop();
			List<Vector2I> unvisitedNeighbors = new List<Vector2I>();

			foreach (var dir in directions)
			{
				int nx = current.X + dir.X;
				int ny = current.Y + dir.Y;
				if (nx > 0 && nx < MazeWidth - 1 && ny > 0 && ny < MazeHeight - 1 && _mazeMap[ny, nx] == 1)
					unvisitedNeighbors.Add(dir);
			}

			if (unvisitedNeighbors.Count > 0)
			{
				stack.Push(current);
				Vector2I chosenDir = unvisitedNeighbors[rnd.Next(unvisitedNeighbors.Count)];
				_mazeMap[current.Y + chosenDir.Y / 2, current.X + chosenDir.X / 2] = 0;
				_mazeMap[current.Y + chosenDir.Y, current.X + chosenDir.X] = 0;
				stack.Push(new Vector2I(current.X + chosenDir.X, current.Y + chosenDir.Y));
			}
		}

		_mazeMap[1, 1] = 3; 
		_mazeMap[MazeHeight - 2, MazeWidth - 2] = 5; 

		int obstaclesPlaced = 0;
		while (obstaclesPlaced < 15)
		{
			int rx = rnd.Next(1, MazeWidth - 1);
			int rz = rnd.Next(1, MazeHeight - 1);
			if (_mazeMap[rz, rx] == 0 && !(rx == 1 && rz == 1))
			{
				_mazeMap[rz, rx] = 2; 
				obstaclesPlaced++;
			}
		}
	}

	private void BuildMazeFromMap()
{
	_ui = new GameUI();
	AddChild(_ui);
	CreateFloor();

	for (int z = 0; z < MazeHeight; z++)
	{
		for (int x = 0; x < MazeWidth; x++)
		{
			Vector3 position = new Vector3(x * CellSize, 0, z * CellSize);
			switch (_mazeMap[z, x])
			{
				case 1: CreateWall(position); PathGrid.SetPointSolid(new Vector2I(x, z), true); break;
				case 2: CreateObstacle(position + new Vector3(0, 1, 0)); break;
				case 3:
{
	Vector3 enemySpawnPos = position + new Vector3(0, 1, 0);
	Vector3 playerSpawnPos = FindSafePlayerSpawn(x, z);

	CreatePlayer(playerSpawnPos);
	CreateShorcut(enemySpawnPos);
	break;
}
				case 5: CreateExit(position + new Vector3(0, 1, 0)); break;
			}
		}
	}
}

private Vector3 FindSafePlayerSpawn(int startX, int startZ)
{
	Vector2I[] offsets =
	{
		new Vector2I(1, 0),
		new Vector2I(0, 1),
		new Vector2I(-1, 0),
		new Vector2I(0, -1)
	};

	foreach (var offset in offsets)
	{
		int x = startX + offset.X;
		int z = startZ + offset.Y;

		if (x > 0 && z > 0 && x < MazeWidth - 1 && z < MazeHeight - 1 && _mazeMap[z, x] == 0)
		{
			return new Vector3(x * CellSize, 1, z * CellSize);
		}
	}

	return new Vector3(startX * CellSize, 1, startZ * CellSize);
}
	private void SpawnEnemySafely()
{
	int targetX = 1; 
	int targetZ = 2;

	CreateShorcut(new Vector3(targetX * CellSize, 1, targetZ * CellSize));
}

	private void CreateFloor()
	{
		MeshInstance3D floor = new MeshInstance3D();
		PlaneMesh planeMesh = new PlaneMesh { Size = new Vector2(150, 150) };
		floor.Mesh = planeMesh;
		floor.Position = new Vector3(20, -0.1f, 20);
		StandardMaterial3D material = new StandardMaterial3D { AlbedoColor = new Color(0.01f, 0.01f, 0.03f) };
		planeMesh.Material = material;
		AddChild(floor);
	}

	private void CreateWall(Vector3 position)
	{
		Node3D wallComplex = new Node3D();
		wallComplex.Position = position;
		AddChild(wallComplex);

		StaticBody3D wallBody = new StaticBody3D();
		CollisionShape3D collision = new CollisionShape3D { Shape = new BoxShape3D { Size = new Vector3(CellSize, 3, CellSize) } };
		wallBody.AddChild(collision);
		wallComplex.AddChild(wallBody);

		MeshInstance3D meshInstance = new MeshInstance3D();
		BoxMesh boxMesh = new BoxMesh { Size = new Vector3(CellSize, 3, CellSize) };
		meshInstance.Mesh = boxMesh;
		StandardMaterial3D darkMaterial = new StandardMaterial3D { AlbedoColor = new Color(0.1f, 0.1f, 0.12f) };
		boxMesh.Material = darkMaterial;
		wallBody.AddChild(meshInstance);

		MeshInstance3D neonBorder = new MeshInstance3D();
		BoxMesh neonMesh = new BoxMesh { Size = new Vector3(CellSize, 0.1f, CellSize) };
		neonBorder.Mesh = neonMesh;
		neonBorder.Position = new Vector3(0, 1.55f, 0);
		StandardMaterial3D neonMaterial = new StandardMaterial3D { 
			AlbedoColor = new Color(0, 0, 0), 
			EmissionEnabled = true, 
			Emission = new Color(0.3f, 0, 0.5f), 
			EmissionEnergyMultiplier = 6.0f 
		};
		neonMesh.Material = neonMaterial;
		wallComplex.AddChild(neonBorder);
	}

	private void CreateObstacle(Vector3 position)
	{
		BinaryObstacle obstacle = new BinaryObstacle();
		obstacle.Position = position;
		obstacle.PuzzleTriggered += _ui.ShowPuzzle;
		_obstacles.Add(obstacle);
		AddChild(obstacle);
	}

	private void CreatePlayer(Vector3 position)
	{
		Player player = new Player();
		player.Name = "Player";
		player.Position = position;
		AddChild(player);
	}

	private void CreateShorcut(Vector3 position)
	{
		ShorcutEnemy shorcut = new ShorcutEnemy();
		shorcut.Position = position;
		AddChild(shorcut);
	}

	private void CreateExit(Vector3 position)
	{
		ExitZone exit = new ExitZone();
		exit.Position = position;
		AddChild(exit);
	}
}
