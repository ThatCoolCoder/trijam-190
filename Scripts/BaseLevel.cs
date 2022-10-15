using Godot;
using System;

public class BaseLevel : Node2D
{
	private Player player;
	private Timer timer;
	public override void _Ready()
	{
		timer = GetNode<Timer>("Timer");

		player = GetNode<Player>("Player");
		player.OnDied += () => timer.Start();
		player.OnWon += () =>
		{
			GetNode<AudioStreamPlayer>("AudioStreamPlayer").Play();
			timer.Start();
		};

	}

	private void OnTimerTimeout()
	{
		if (player.State == Player.PlayerState.Dead)
		{
			LevelManager.RestartCurrentLevel(GetTree());
		}
		else if (player.State == Player.PlayerState.Won)
		{
			LevelManager.LoadNextLevel(GetTree());
		}
	}
}
