using Godot;
using System;

public class TileMap : Godot.TileMap
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    TileSet _tileset;
    [Export]
    public int Rows = 20;
    [Export]
    public int Columns = 20;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // _tileset = GetTileset();
        int temp_x = Rows / 2 + 1;
        int temp_y = Columns / 2 + 1;
        for (int i = -temp_x; i <= temp_x; i++)
        {
            for (int j = -temp_y; j <= temp_y; j++)
            {
                if (i == -temp_x || i == temp_x || j == -temp_y || j == temp_y)
                {
                    SetCell(i, j, TileSet.FindTileByName("borderGrass"));
                }
                else if (j == -temp_y + 1)
                {
                    SetCell(i, j, TileSet.FindTileByName("waterTile"));

                }
                else
                {
                    SetCell(i, j, TileSet.FindTileByName("grassTile"));
                }
            }
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
