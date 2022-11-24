using Godot;
using System;

public class StartScreen : CanvasLayer
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    // Called when the node enters the scene tree for the first time.
    [Export]
    public PackedScene Main;

    public override void _Ready()
    {

    }

    // public void OnStartButtonPressed()
    // {
    //     GetNode<Button>("StartButton").Hide();
    //     EmitSignal("StartGame");
    // }

    public void HideButtons()
    {
        GetNode<Button>("AttackerStartButton").Hide();
        GetNode<Button>("DefenderStartButton").Hide();
        GetNode<Button>("MultiplayerStartButton").Hide();
    }

    public void SetScene(string option, string scenePath)
    {
        GD.Print($"{option} pressed!!!");
        var mainScene = (PackedScene)ResourceLoader.Load(scenePath);
        var instance = (Main)mainScene.Instance();
        instance.Option = option;
        AddChild(instance);
    }

    public void OnAttackerStartButtonDown()
    {
        SetScene("Attacker", "res://Main.tscn");
        HideButtons();
    }

    public void OnDefenderStartButtonDown()
    {
        SetScene("Defender", "res://Main.tscn");
        HideButtons();
    }

    public void OnMultiplayerStartButtonDown()
    {
        SetScene("Multiplayer", "res://NetworkSetup.tscn");
        HideButtons();
    }
}
