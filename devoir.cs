using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

// Classe de base pour les jeux de plateau
public abstract class BoardGame
{
    public int Id { get; set; }
    public abstract void Play();
}

// Classe représentant un joueur
public class Player
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
}

// Classe représentant une partie de Tic-Tac-Toe
public class TicTacToeGame : BoardGame
{
    public int Id { get; set; }
    public int Player1Id { get; set; }
    public int Player2Id { get; set; }
    public int? WinnerId { get; set; }

    [ForeignKey("Player1Id")]
    public Player Player1 { get; set; }

    [ForeignKey("Player2Id")]
    public Player Player2 { get; set; }

    [ForeignKey("WinnerId")]
    public Player Winner { get; set; }

    public string[,] Board { get; set; } = new string[3, 3];

    public override void Play()
    {
        int currentPlayer = 1;
        bool gameEnd = false;

        while (!gameEnd)
        {
            DisplayBoard();

            Console.WriteLine($"Joueur {currentPlayer}, c'est à vous de jouer. Choisissez une case (1-9) :");
            int choice = int.Parse(Console.ReadLine());

            int row = (choice - 1) / 3;
            int col = (choice - 1) % 3;

            if (Board[row, col] != "-")
            {
                Console.WriteLine("Cette case est déjà occupée. Veuillez choisir une autre case.");
                continue;
            }

            Board[row, col] = currentPlayer == 1 ? "X" : "O";

            if (CheckWin(currentPlayer == 1 ? "X" : "O"))
            {
                WinnerId = currentPlayer == 1 ? Player1Id : Player2Id;
                gameEnd = true;
                Console.WriteLine($"Joueur {currentPlayer} a gagné la partie !");
            }
            else if (IsGameOver())
            {
                gameEnd = true;
                Console.WriteLine("Match nul !");
            }

            currentPlayer = currentPlayer == 1 ? 2 : 1;
        }

        DisplayBoard();
    }

    private void DisplayBoard()
    {
        Console.WriteLine("Plateau de jeu :");
        int count = 1;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Console.Write(Board[i, j] == "-" ? count.ToString() : Board[i, j]);
                Console.Write(" ");
                count++;
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    private bool CheckWin(string mark)
    {
        for (int i = 0; i < 3; i++)
        {
            if (Board[i, 0] == mark && Board[i, 1] == mark && Board[i, 2] == mark)
                return true;
        }

        for (int j = 0; j < 3; j++)
        {
            if (Board[0, j] == mark && Board[1, j] == mark && Board[2, j] == mark)
                return true;
        }

        if (Board[0, 0] == mark && Board[1, 1] == mark && Board[2, 2] == mark)
            return true;
        if (Board[0, 2] == mark && Board[1, 1] == mark && Board[2, 0] == mark)
            return true;

        return false;
    }

    private bool IsGameOver()
    {
        return Board.Cast<string>().All(cell => cell != "-");
    }
}

public class TicTacToeContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<TicTacToeGame> Games { get; set; }
}

public class TicTacToeRepository
{
    public void SaveGame(TicTacToeGame game)
    {
        using (var context = new TicTacToeContext())
        {
            context.Games.Add(game);
            context.SaveChanges();
        }
    }

    public List<Player> GetPlayers()
    {
        using (var context = new TicTacToeContext())
        {
            return context.Players.ToList();
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Bienvenue dans le jeu Tic-Tac-Toe avec Entity Framework 6!");

        TicTacToeRepository repository = new TicTacToeRepository();

        // Créer et sauvegarder deux joueurs
        Player player1 = new Player { Name = "Joueur 1", Score = 0 };
        Player player2 = new Player { Name = "Joueur 2", Score = 0 };

        repository.SaveGame(new TicTacToeGame { Player1 = player1, Player2 = player2 });

        // Afficher les joueurs
        var players = repository.GetPlayers();
        Console.WriteLine("Liste des joueurs :");
        foreach (var player in players)
        {
            Console.WriteLine($"ID : {player.Id}, Nom : {player.Name}, Score : {player.Score}");
        }

        Console.WriteLine("Appuyez sur une touche pour commencer une partie...");
        Console.ReadKey();

        TicTacToeGame game = new TicTacToeGame { Player1 = player1, Player2 = player2 };
        game.Play();

        Console.WriteLine("Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
