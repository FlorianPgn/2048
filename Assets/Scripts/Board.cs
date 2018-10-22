using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Board : MonoBehaviour
{
    public enum Direction
    {
        LEFT, UP, RIGHT, DOWN
    }

    private int[,] _board = new int[4, 4];
    private bool[,] _boardMerge = new bool[4, 4];
    private Vector2[,] _tilesPositions = new Vector2[4, 4];
    private Tile[,] _tiles = new Tile[4, 4];
    private int _score = 0;

    public TextMeshProUGUI DebugBoard;
    public GameObject GameOverPanel;
    public GameObject BackgroundTiles;
    public GameObject TilePrefab;
    public GameObject TilePanel;
    public TextMeshProUGUI ScoreText;

    // Use this for initialization
    void Start()
    {
        RectTransform[] tiles = BackgroundTiles.GetComponentsInChildren<RectTransform>();
        int index = 0;
        foreach (RectTransform rectT in tiles)
        {
            if (rectT == BackgroundTiles.GetComponent<RectTransform>())
                continue;
            _tilesPositions[index / 4, index % 4] = rectT.transform.position;
            index++;
        }
        SpawnTile();
        DebugBoard.text = BoardToString();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            KeyPressed(Direction.LEFT);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            KeyPressed(Direction.UP);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            KeyPressed(Direction.RIGHT);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            KeyPressed(Direction.DOWN);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            DebugBoard.gameObject.SetActive(DebugBoard.gameObject.activeSelf ? false : true);
        }
    }

    private void KeyPressed(Direction direction)
    {
        if (IsGameOver())
        {
            GameOverPanel.SetActive(true);
        }
        else
        {
            if (Move(direction))
                OnSwap();
        }
    }

    // Add a random tile in the board
    private void SpawnTile()
    {
        List<Vector2> emptyTiles = new List<Vector2>();
        for (int i = 0; i < _board.GetLength(0); i++)
        {
            for (int j = 0; j < _board.GetLength(1); j++)
            {
                if (_board[i, j] == 0)
                    emptyTiles.Add(new Vector2(i, j));
            }
        }

        int newTileIndex = (int)Random.Range(0, emptyTiles.Count - 1);
        int newTileValue = Random.Range(0, 1) < 0.9f ? 2 : 4;
        int x = (int)emptyTiles[newTileIndex].x;
        int y = (int)emptyTiles[newTileIndex].y;
        _board[x, y] = newTileValue;

        CreateTileUI(x, y, newTileValue);
    }

    private void CreateTileUI(int x, int y, int val)
    {
        GameObject tileObj = Instantiate(TilePrefab);
        tileObj.transform.SetParent(TilePanel.transform);
        tileObj.transform.localScale = Vector3.one;
        Tile tile = tileObj.GetComponent<Tile>();
        tile.Position = _tilesPositions[x, y];
        tile.Value = val;
        _tiles[x, y] = tile;
    }

    private void MoveTileUI(int x, int y, int val)
    {
        _tiles[x, y].Position = _tilesPositions[x, y];
        _tiles[x, y].Value = val;
    }

    // Move the tiles in direction chosen
    private bool Move(Direction direction)
    {
        bool moveMade = false;
        _boardMerge = new bool[4, 4];

        int n;
        switch (direction)
        {
            case Direction.LEFT:
                // For each tile
                for (int i = 0; i < _board.GetLength(0); i++)
                {
                    for (int j = 1; j < _board.GetLength(1); j++)
                    {
                        // If the tile is empty : next
                        if (_board[i, j] == 0)
                            continue;

                        n = 0;
                        while (j - n != 0 && !MoveTile(i, j - n, i, j - n - 1))
                        {
                            n++;
                            moveMade = true;
                        };
                    }
                }
                break;
            case Direction.UP:
                for (int i = 1; i < _board.GetLength(0); i++)
                {
                    for (int j = 0; j < _board.GetLength(1); j++)
                    {
                        // If the tile is empty : next
                        if (_board[i, j] == 0)
                            continue;

                        n = 0;
                        while (i - n != 0 && !MoveTile(i - n, j, i - n - 1, j))
                        {
                            n++;
                            moveMade = true;
                        };
                    }
                }
                break;
            case Direction.RIGHT:
                for (int i = 3; i >= 0; i--)
                {
                    for (int j = 2; j >= 0; j--)
                    {
                        // If the tile is empty : next
                        if (_board[i, j] == 0)
                            continue;

                        n = 0;
                        while (j + n != 3 && !MoveTile(i, j + n, i, j + n + 1))
                        {
                            n++;
                            moveMade = true;
                        };
                    }
                }
                break;
            case Direction.DOWN:
                for (int i = 2; i >= 0; i--)
                {
                    for (int j = 3; j >= 0; j--)
                    {
                        // If the tile is empty : next
                        if (_board[i, j] == 0)
                            continue;

                        n = 0;
                        while (i + n != 3 && !MoveTile(i + n, j, i + n + 1, j))
                        {
                            n++;
                            moveMade = true;
                        };
                    }
                }
                break;
        }

        return moveMade;
    }

    // Return true if the tile did her last authorized move
    private bool MoveTile(int i, int j, int nextTileX, int nextTileY)
    {
        // Tile displacement
        if (_board[nextTileX, nextTileY] == 0)
        {
            _board[nextTileX, nextTileY] = _board[i, j];
            _board[i, j] = 0;

            _tiles[nextTileX, nextTileY] = _tiles[i, j];
            _tiles[i, j] = null;
            _tiles[nextTileX, nextTileY].Position = _tilesPositions[nextTileX, nextTileY];
            return false;
        }
        // Tile merging
        else if (_board[nextTileX, nextTileY] == _board[i, j])
        {
            // Test si la prochaine case a déjà mergé
            if (_boardMerge[nextTileX, nextTileY] || _boardMerge[i, j])
                return false;

            // Data update
            _board[nextTileX, nextTileY] *= 2;
            _boardMerge[nextTileX, nextTileY] = true;
            _board[i, j] = 0;

            // Update UI
            Destroy(_tiles[i, j].gameObject);
            _tiles[nextTileX, nextTileY].Position = _tilesPositions[nextTileX, nextTileY];
            _tiles[nextTileX, nextTileY].Value = _board[nextTileX, nextTileY];

            // Scoring
            _score += _board[nextTileX, nextTileY];


            return false;
        }
        return true;
    }

    private void OnSwap()
    {
        SpawnTile();
        DebugBoard.text = BoardToString();
        UpdateScore();
    }

    private bool IsGameOver()
    {
        // For each tile
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int tileValue = _board[i, j];
                if (tileValue == 0)
                    return false;

                // For each neighbor tiles
                for (int k = -1; k <= 1; k += 2)
                {
                    int ki = i + k;
                    int kj = j + k;
                    if (kj > 0 && kj < 4 && _board[i, kj] == tileValue)
                        return false;
                    if (ki > 0 && ki < 4 && _board[ki, j] == tileValue)
                        return false;
                }
            }
        }
        return true;
    }

    public void Restart()
    {
        _board = new int[4, 4];
        _boardMerge = new bool[4, 4];
        _tilesPositions = new Vector2[4, 4];
        _score = 0;
        foreach (var item in _tiles)
        {
            if(item != null)
                Destroy(item.gameObject);
        }
        _tiles = new Tile[4, 4];
        GameOverPanel.SetActive(false);
        Start();
    }

    private void UpdateTileUI()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {

                MoveTileUI(i, j, _board[i, j]);
            }
        }
    }

    private void UpdateScore()
    {

        ScoreText.SetText(_score.ToString());
    }

    // Return the board in a formatted string
    private string BoardToString()
    {
        string boardDisplay = "-------------";
        for (int i = 0; i < _board.GetLength(0); i++)
        {
            boardDisplay += "\n ";
            for (int j = 0; j < _board.GetLength(1); j++)
            {
                boardDisplay += _board[i, j] + "  ";
            }
        }

        boardDisplay += "\n-------------";
        return boardDisplay;
    }
}
