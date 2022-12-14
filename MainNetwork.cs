using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MainNetwork : Node2D
{
#pragma warning disable 649
    [Export]
    public PackedScene SoldierScene;
    [Export]
    public PackedScene TileMapScene;
#pragma warning restore 649

    [Export]
    public int MaxCount = 15;
    public int AliveSoldiers = 0;
    public int MaxLevel = 15;
    public bool KillLeft = true;
    public string GameWonBy = "None";
    public RandomNumberGenerator rng;
    private object _lock;

    [RemoteSyncAttribute]
    List<Node> soldiers;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rng = new RandomNumberGenerator();
        _lock = new object();
        rng.Randomize();
        GetNode<Label>("GameOver").Hide();
        GetNode<Label>("GameInst").Hide();
        InitializeBoard();
        StartP1Turn();
    }

    [RemoteAttribute]
    public void UpdateSoldiers()
    {
        RsetUnreliable("soldiers", soldiers);
    }

    // Randomly initialises soldiers on the board.
    public void InitializeBoard()
    {
        var tile = (TileMap)TileMapScene.Instance();
        if (!GetTree().IsNetworkServer())
        {
            GD.Print(GetTree().GetNetworkPeer().GetConnectionStatus());
            GD.Print("Performing an RPC...");
            RpcId(1, "UpdateSoldiers");
            foreach (Soldier soldier in soldiers)
            {
                soldier.Show();
            }
            return;
        }

        // A soldier reaching this level will win the game for the attacker.
        // Levels start from 0.
        MaxLevel = tile.Rows - 1;
        int minForAttacker = (1 << (tile.Rows - 1));

        GD.Print("Rows: " + tile.Rows + ", Columns: " + tile.Columns);
        GD.Print("Max. possible level of a soldier: " + MaxLevel);

        // The higher the leeway, the bigger the attacker's advantage.
        int leewayAttacker = rng.RandiRange(0, minForAttacker / 8);
        int totalSum = minForAttacker + leewayAttacker;
        int[] soldiersPerRow = new int[tile.Rows];

        GD.Print("Total sum of configuration: " + totalSum);

        // Pre-emptively placing two soldiers on the penultimate row if necessary.
        // Satisfies the minimum total weight required to allow the attacker to win.
        if (totalSum >= minForAttacker)
        {
            soldiersPerRow[tile.Rows - 2] = 2;
            AliveSoldiers = 2;
        }

        for (int i = tile.Rows - 2; i >= 0; i--)
        {
            if (((1 << i) & totalSum) > 0)
            {
                soldiersPerRow[i]++;
                AliveSoldiers++;
            }
        }

        // Pushing some soldiers downwards (doubling them to maintain the same total weight sum).
        // We try to push the penultimate soldiers down more.
        // We also try to limit the number of splits.
        for (int i = tile.Rows - 2; i > 0; i--)
        {
            int numInRow = soldiersPerRow[i];
            for (int j = 0; j < numInRow; j++)
            {
                if (soldiersPerRow[i - 1] + 2 < tile.Columns)
                {
                    if (2 * AliveSoldiers < 3 * tile.Rows &&
                    (rng.RandiRange(0, 2) == 0 || (i == tile.Rows - 2 && rng.RandiRange(0, 3) <= 2)))
                    {
                        soldiersPerRow[i]--;
                        soldiersPerRow[i - 1] += 2;
                        AliveSoldiers++;
                    }
                }
            }
        }

        // Distributing soldiers randomly in each row.
        for (int i = 0; i < tile.Rows; i++)
        {
            bool[] taken = new bool[tile.Columns];
            int numVacancies = tile.Columns;

            while (numVacancies > tile.Columns - soldiersPerRow[i])
            {
                int pos = rng.RandiRange(0, tile.Rows) % numVacancies;
                int encountered = 0;

                for (int j = 0; j < tile.Columns; j++)
                {
                    if (taken[j])
                    {
                        continue;
                    }
                    if (encountered < pos)
                    {
                        encountered++;
                        continue;
                    }

                    // Add a new soldier.
                    taken[j] = true;
                    var soldier = (Soldier)SoldierScene.Instance();
                    soldier.level = i;
                    soldier.AddToGroup("soldiers");
                    AddChild(soldier);

                    // Position the soldier.
                    int x = (-tile.Columns / 2 + j) * 64;
                    int y = (tile.Rows / 2 - i - 1) * 64 - 4;
                    soldier.Position = new Vector2(x, y);

                    GD.Print("Soldier Position : " + soldier.Position);
                    GD.Print("Soldier Level    : " + soldier.level);

                    break;
                }
                numVacancies--;
            }
        }
    }

    public void StartP1Turn()
    {
        GetNode<Button>("P1Done").Hide();
        GetNode<Button>("P2Left").Hide();
        GetNode<Button>("P2Right").Hide();
        GetNode<Button>("P2Done").Hide();
        GetNode<CheckButton>("CheckButton").Hide();
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        foreach (Soldier soldier in soldiers)
        {
            soldier.state = 0;
            soldier._Ready();
        }
        var message = GetNode<Label>("GameInst");
        // Instructions displayed during the game
        message.Text = "Attacker's turn. Click on the players to make them face either left or right. The defender will remove the set facing left or right. Choose wisely!!!";
        message.Show();
        GetNode<Button>("P1Done").Show();
    }

    public async void OnP1DoneButtonDown()
    {
        // Get the number of soldiers in each direction. 
        int cntLeft = 0, cntRight = 0;
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        foreach (Soldier soldier in soldiers)
        {
            // flipping all the unchosen players to the right
            var sprite = soldier.GetNode<Sprite>("Position2D/Sprite");
            if (soldier.right)
            {
                sprite.Frame = 6;
                cntRight++;
            }
            else
            {
                cntLeft++;
                if (sprite.Frame == 0)
                {
                    sprite.Frame = 6;
                    sprite.FlipH = !sprite.FlipH;
                }
            }
            soldier.state = 1;
        }
        GD.Print("Number of left facing: " + cntLeft);
        GD.Print("Number of right facing: " + cntRight);
        var message = GetNode<Label>("GameInst");
        message.Text = "Defender's Turn. Choose the set of players facing left or right to remove them. The remaining players will advance forward by one step. All the best!";
        message.Show();
        GetNode<Button>("P1Done").Hide();
        GetNode<Button>("P2Left").Show();
        GetNode<Button>("P2Right").Show();
        GetNode<Button>("P2Done").Show();
    }

    public void OnP2LeftButtonDown()
    {
        KillLeft = true;
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        // coloring the players belonging to a set when button is pressed. 
        foreach (Soldier soldier in soldiers)
        {
            if (soldier.right == !KillLeft)
            {
                soldier.GetNode<Sprite>("Position2D/Sprite").SelfModulate = new Color("#5ac3f1");
            }
            else
            {
                soldier.GetNode<Sprite>("Position2D/Sprite").SelfModulate = new Color(1, 1, 1);
            }
        }
        var message = GetNode<Label>("GameInst");
        message.Text = "Are you sure? If so, press the 'Done' button!";
        message.Show();
    }

    public void OnP2RightButtonDown()
    {
        KillLeft = false;
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        // coloring the players belonging to a set when button is pressed. 
        foreach (Soldier soldier in soldiers)
        {
            if (soldier.right == !KillLeft)
            {
                soldier.GetNode<Sprite>("Position2D/Sprite").SelfModulate = new Color("#d75af1");
            }
            else
            {
                soldier.GetNode<Sprite>("Position2D/Sprite").SelfModulate = new Color(1, 1, 1);
            }
        }
        var message = GetNode<Label>("GameInst");
        message.Text = "Are you sure? If so, press the 'Done' button!";
        message.Show();
    }

    public async void GameOver(string option)
    {
        GD.Print($"{option} wins!");
        var label = GetNode<Label>("GameOver");
        label.Text = $"Game Over!\n{option} Wins!!!";
        label.Show();
        GetNode<Button>("P1Done").Hide();
        GetNode<Button>("P2Left").Hide();
        GetNode<Button>("P2Right").Hide();
        GetNode<Button>("P2Done").Hide();
        GetNode<Label>("GameInst").Hide();
        await ToSignal(GetTree().CreateTimer(2), "timeout");
        GetTree().ChangeScene("res://StartScreen.tscn");
    }

    public async void PlayOrRemove(Soldier soldier)
    {
        bool destroy = false;
        if (soldier.right == !KillLeft)
        {
            destroy = true;
        }
        if (destroy)
        {
            soldier.RemoveFromGroup("soldier");
            RemoveChild(soldier);
            soldier.QueueFree();
            AliveSoldiers--;
            if (AliveSoldiers == 0)
            {
                GameWonBy = "Defender";
            }
        }
        else
        {
            soldier.level++;
            if (soldier.level == MaxLevel)
            {
                GameWonBy = "Attacker";
            }
            Vector2 targetPos = soldier.Position;
            targetPos.y = targetPos.y - 64;
            Tween tween = soldier.GetNode<Tween>("Tween");
            AnimationPlayer animationPlayer = soldier.GetNode<AnimationPlayer>("AnimationPlayer");
            animationPlayer.Play("walk_up");
            tween.InterpolateProperty(soldier, "position", soldier.Position, targetPos, animationPlayer.CurrentAnimationLength);
            tween.Start();
            await ToSignal(animationPlayer, "animation_finished");
            soldier.Position = targetPos;
        }
    }

    public void OnP2DoneButtonDown()
    {
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        foreach (Soldier soldier in soldiers)
        {
            PlayOrRemove(soldier);
        }
        if (GameWonBy != "None")
        {
            GameOver(GameWonBy);
            return;
        }
        var message = GetNode<Label>("GameInst");
        message.Text = "The players are moving forward. Wait for your turn...";
        message.Show();
        StartP1Turn();
    }
}