using Godot;
using System;

public class StartScreen : CanvasLayer
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    public void OnStartButtonPressed()
    {
        GetNode<Button>("StartButton").Hide();
        EmitSignal("StartGame");
    }

    public void OnAttackerStartButtonDown()
    {
        GD.Print("Attacker pressed!!!");

    }

    public void OnDefenderStartButtonDown()
    {
        GD.Print("Defender pressed!!!");
    }

    public void OnMultiplayerStartButtonDown()
    {
        GD.Print("Multiplayer pressed!!!");
    }
}
