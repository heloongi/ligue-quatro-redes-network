using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public int rows = 6;
    public int columns = 7;
    public Button[,] gridButtons;
    public GameObject gridParent;
    public TMP_Text statusText;
    public Color player1Color = Color.red;
    public Color player2Color = Color.yellow;
    public Button restartButton;

    private int[,] board; // Estado do tabuleiro no servidor
    private int currentPlayer = 1;
    private bool gameOver = false;

    void Start()
    {
        SetupGrid();
        restartButton.onClick.AddListener(() =>
        {
            if (IsServer) RestartGameServerRpc();
        });
        // Inicialização do jogo só no host
        if (IsServer)
        {
            RestartGameServerRpc();
        }
    }

    void SetupGrid()
    {
        gridButtons = new Button[rows, columns];
        int i = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Button b = gridParent.transform.GetChild(i).GetComponent<Button>();
                int colIndex = c;
                b.onClick.AddListener(() =>
                {
                    if (IsOwner && !gameOver)
                        OnColumnClick(colIndex);
                });
                gridButtons[r, c] = b;
                i++;
            }
        }
    }

    // Jogador local tenta jogar — envia para servidor
    public void OnColumnClick(int col)
    {
        SubmitMoveServerRpc(col);
    }

    // Executado no servidor: processa jogada
    [ServerRpc(RequireOwnership = false)]
    void SubmitMoveServerRpc(int col)
    {
        if (gameOver) return;

        for (int row = rows - 1; row >= 0; row--)
        {
            if (board[row, col] == 0)
            {
                board[row, col] = currentPlayer;

                UpdateBoardClientRpc(row, col, currentPlayer);

                if (CheckVictory(row, col))
                {
                    gameOver = true;
                    UpdateStatusClientRpc($"Jogador {currentPlayer} venceu!");
                    SetButtonsInteractableClientRpc(false);
                }
                else
                {
                    currentPlayer = 3 - currentPlayer;
                    UpdateStatusClientRpc($"Vez do Jogador {currentPlayer}");
                }

                return;
            }
        }
    }

    [ClientRpc]
    void UpdateBoardClientRpc(int row, int col, int player)
    {
        var img = gridButtons[row, col].GetComponent<Image>();
        img.color = player == 1 ? player1Color : player2Color;
        gridButtons[row, col].interactable = false;
    }

    [ClientRpc]
    void UpdateStatusClientRpc(string message)
    {
        statusText.text = message;
    }

    [ClientRpc]
    void SetButtonsInteractableClientRpc(bool interactable)
    {
        foreach (var b in gridButtons)
        {
            if (b != null)
                b.interactable = interactable;
        }
    }

    bool CheckVictory(int lastCol, int lastRow)
    {
        int player = board[lastRow, lastCol];
        return (CountDirection(lastRow, lastCol, -1, 0) + CountDirection(lastRow, lastCol, 1, 0) >= 3) ||
               (CountDirection(lastRow, lastCol, 0, -1) + CountDirection(lastRow, lastCol, 0, 1) >= 3) ||
               (CountDirection(lastRow, lastCol, -1, -1) + CountDirection(lastRow, lastCol, 1, 1) >= 3) ||
               (CountDirection(lastRow, lastCol, -1, 1) + CountDirection(lastRow, lastCol, 1, -1) >= 3);
    }

    int CountDirection(int row, int col, int dRow, int dCol)
    {
        int count = 0;
        int player = board[row, col];

        for (int i = 1; i < 4; i++)
        {
            int r = row + dRow * i;
            int c = col + dCol * i;
            if (r < 0 || r >= rows || c < 0 || c >= columns) break;
            if (board[r, c] != player) break;
            count++;
        }

        return count;
    }

    [ServerRpc(RequireOwnership = false)]
    void RestartGameServerRpc()
    {
        board = new int[rows, columns];
        currentPlayer = 1;
        gameOver = false;

        ResetBoardClientRpc();

        UpdateStatusClientRpc("Vez do Jogador 1");
        SetButtonsInteractableClientRpc(true);
    }

    [ClientRpc]
    void ResetBoardClientRpc()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                var img = gridButtons[r, c].GetComponent<Image>();
                img.color = Color.white;
                gridButtons[r, c].interactable = true;
            }
        }
    }
}
