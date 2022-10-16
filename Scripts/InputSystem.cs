using Godot;
using System;

public class InputSystem : Node
{
    Vector2? inputDirection;
    // var inputActivation;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        PauseMode = Node.PauseModeEnum.Process;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        inputDirection = GetInputDirection();
        // inputActivation = GetInputActivation();
    }

    /// <summary>
    /// Gets the direction vector based on user input
    /// </summary>
    /// <returns>Direction vector</returns>
    public Vector2 GetInputDirection()
    {
        var horizontal = ((Input.IsActionPressed("ui_right")) ? 1 : 0) - ((Input.IsActionPressed("ui_left")) ? 1 : 0);
        var vertical = ((Input.IsActionPressed("ui_down")) ? 1 : 0) - ((Input.IsActionPressed("ui_up")) ? 1 : 0);
        return new Vector2(horizontal, (horizontal == 0) ? vertical : 0);
    }

    /// <summary>
    /// Neutralize inputs 
    /// </summary>
    public void NeutralizeInputs()
    {
        inputDirection = null;
    }

    /// <summary>
    /// Disable all inputs until a given trigger. Useful for letting scene transitions or 
    /// menu animations to finish. 
    /// </summary>
    /// <param name="waitForThisObject">GD object (mostly node) to wait for</param>
    /// <param name="toFinishThis">Signal that object finshes</param>
    public async void DisableInputUntil(Godot.Object waitForThisObject, string toFinishThis)
    {
        NeutralizeInputs();
        SetProcess(false);
        await ToSignal(waitForThisObject, toFinishThis);
        SetProcess(true);
    }

    /// <summary>
    /// Used for "game over" to disable all inputs
    /// </summary>
    public void DisableInput()
    {
        NeutralizeInputs();
        SetProcess(false);
    }
}
