using Godot;
using System;

public class Bullet : Area2D
{
	[Export] public float speed = 400;

	public override void _PhysicsProcess(float delta)
	{
		Position += Vector2.Right.Rotated(Rotation) * speed * delta;
	}

	public void Die(Node body)
	{
		if (body.IsInGroup("Enemy"))
		{
			(body as Enemy).StartToSplit(Rotation);
		}
		if (!body.IsInGroup("Player"))
		{
			GetParent().AddChild(ResourceLoader.Load<PackedScene>("res://Scenes/Boom.tscn").Instance());
			QueueFree();
		}

	}
}
