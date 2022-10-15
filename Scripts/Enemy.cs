using Godot;
using System;

public class Enemy : KinematicBody2D
{
	[Export] public bool Patrolling { get; set; } = true;
	[Export] public bool Dazed { get; set; } = false;
	[Export] public float PatrolSpeed { get; set; } = 120;
	[Export] public float ChasePlayerSpeed { get; set; } = 120;
	[Export] public float ChasePlayerAcceleration { get; set; } = 120;

	private Player player;
	public Vector2 velocity = Vector2.Zero;

	private PackedScene prefab = ResourceLoader.Load<PackedScene>("res://Scenes/Enemy.tscn");
	private Timer t;

	public override void _Ready()
	{
		t = GetNode<Timer>("Timer");
		player = GetTree().GetNodesInGroup("Player")[0] as Player;
	}

	public override void _PhysicsProcess(float delta)
	{
		if (Dazed)
		{
			velocity *= 0.97f;
			velocity = MoveAndSlide(velocity, Vector2.Zero);
			if (t.TimeLeft == 0) Dazed = false;

		}
		else
		{
			if (Patrolling)
			{
				Patrol(delta);
			}
			else
			{
				ChasePlayer(delta);
			}
		}
	}

	private void Patrol(float delta)
	{
		velocity = Vector2.Right.Rotated(Rotation) * PatrolSpeed;
		MoveAndSlide(velocity, Vector2.Zero);
		for (int i = 0; i < GetSlideCount(); i++)
		{
			var collision = GetSlideCollision(i);
			var collider = collision.Collider as Node2D;
			var ang = Mathf.Rad2Deg((collision.Position - GlobalPosition).Angle());
			var delta2 = (ang - RotationDegrees) % 360;
			var angleOk = Mathf.Abs(delta2) < 45;
			if ((collider.IsInGroup("Walls") || collider.IsInGroup("Enemy")) && angleOk)
			{
				RotationDegrees = (RotationDegrees + 180) % 360;
				break;
			}
		}
	}

	private void ChasePlayer(float delta)
	{
		velocity += ChasePlayerAcceleration * (player.GlobalPosition - GlobalPosition).Normalized();
		Rotation = (player.GlobalPosition - GlobalPosition).Angle();
		velocity = velocity.LimitLength(ChasePlayerSpeed);
		velocity = MoveAndSlide(velocity, Vector2.Zero);
	}

	public void StartToSplit(float angle)
	{
		var vel = 200;
		var offset = 20;

		var newVel = Vector2.Up.Rotated(angle + Mathf.Pi / 2) * vel;
		var newPos = Position + newVel.Normalized() * offset;
		var instance = prefab.Instance<Enemy>();
		instance.Position = newPos;
		instance.velocity = newVel;
		instance.Dazed = true;
		instance.Patrolling = false;
		GetParent().AddChild(instance);
		instance.GetNode<Timer>("Timer").Start();


		velocity = Vector2.Up.Rotated(angle - Mathf.Pi / 2) * vel;
		Position += newVel.Normalized() * offset;
		Dazed = true;
		Patrolling = false;
		t.Start();
	}
}
