using Godot;
using System;

public class Player : KinematicBody2D
{
	public enum PlayerState
	{
		Alive,
		Dead,
		Won
	}

	[Export] public float Acceleration { get; set; } = 250f;
	[Export] public float GroundFriction { get; set; } = 380f;
	[Export] public float MaxSpeed { get; set; } = 150f;

	private Vector2 velocity = Vector2.Zero;

	private PackedScene bullet = ResourceLoader.Load<PackedScene>("res://Scenes/Bullet.tscn");

	public event Action OnWon;
	public event Action OnDied;

	public PlayerState State = PlayerState.Alive;

	public override void _PhysicsProcess(float delta)
	{
		if (State == PlayerState.Alive)
		{
			Move(delta);
			ShootGun();
			HandleCollisions();
		}
	}

	private void Move(float delta)
	{
		var acceleration = CheckKeybinds();
		velocity += acceleration * delta;
		FeelFloorFriction(acceleration, delta);
		velocity = velocity.LimitLength(MaxSpeed);
		velocity = MoveAndSlide(velocity, Vector2.Zero);
		Rotation = velocity.Angle() + Mathf.Pi / 2;
	}

	private Vector2 CheckKeybinds()
	{
		var acceleration = Vector2.Zero;
		if (Input.IsActionPressed("up")) acceleration.y -= Acceleration;
		if (Input.IsActionPressed("down")) acceleration.y += Acceleration;
		if (Input.IsActionPressed("left")) acceleration.x -= Acceleration;
		if (Input.IsActionPressed("right")) acceleration.x += Acceleration;
		return acceleration;

	}

	private void FeelFloorFriction(Vector2 acceleration, float delta)
	{
		if (acceleration.x == 0) velocity.x = Utils.ConvergeValue(velocity.x, 0, delta * GroundFriction);
		if (acceleration.y == 0) velocity.y = Utils.ConvergeValue(velocity.y, 0, delta * GroundFriction);
	}

	private void ShootGun()
	{
		var timer = GetNode<Timer>("ReloadTimer");
		if (Input.IsActionJustPressed("shoot") && timer.TimeLeft == 0)
		{
			GD.Print("Shooting");


			var instance = bullet.Instance<Node2D>();
			instance.Rotation = (GetGlobalMousePosition() - GlobalPosition).Angle();

			instance.Position = GlobalPosition + Vector2.Right.Rotated(instance.Rotation) * 20;
			GetParent().AddChild(instance);
			timer.Start();
		}
	}

	private void HandleCollisions()
	{
		for (int i = 0; i < GetSlideCount(); i++)
		{
			var collider = GetSlideCollision(i).Collider as Node2D;
			if (collider.IsInGroup("Enemy")) Die();
			if (collider.IsInGroup("Finish"))
			{
				OnWon?.Invoke();
				State = PlayerState.Won;
			}
		}
	}

	private void Die()
	{
		State = PlayerState.Dead;
		OnDied?.Invoke();
	}
}
