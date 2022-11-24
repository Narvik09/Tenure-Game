using Godot;
using System;

public class StartScreen : CanvasLayer
{
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
        GetNode<Label>("Description").Hide();
        GetNode<Label>("Title").Hide();
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
        GD.Print("Multiplayer pressed!!!");
        HideButtons();
        GetTree().ChangeScene("res://NetworkSetup.tscn");
    }
}
