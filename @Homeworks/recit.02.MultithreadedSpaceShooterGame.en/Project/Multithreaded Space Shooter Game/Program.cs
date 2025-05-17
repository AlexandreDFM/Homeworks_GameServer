//using System;
//using System.Threading;

//class SpaceShooterGame
//{
//    static bool isRunning = true;       // Game running status
//    static int playerPosition = 10;     // Player position
//    static int enemyPosition = 30;      // Enemy position
//    static int playerHealth = 100;      // Player health
//    static int enemyHealth = 100;       // Enemy health
//    static object lockObject = new object(); // Lock object for thread safety

//    static void Main(string[] args)
//    {
//        Console.WriteLine("Welcome to Space Shooter!");
//        Console.WriteLine("Press 'A' to move left, 'D' to move right, 'F' to fire. Press 'Q' to quit.");
//        Console.Write("Game starts within ");
//        for (int i = 3; i > 0; i--)
//        {
//            Console.Write(i + " ");
//            Thread.Sleep(500);
//        }
//        Console.WriteLine(" Go~~~!");

//        Thread enemyThread = new Thread(new ThreadStart(EnemyBehavior));
//        enemyThread.Start();  // Start enemy behavior thread

//        while (isRunning && playerHealth > 0 && enemyHealth > 0)
//        {
//            // Player controls
//            if (Console.KeyAvailable)
//            {
//                var key = Console.ReadKey(true).Key;
//                lock (lockObject)
//                {
//                    switch (key)
//                    {
//                        case ConsoleKey.A:
//                            MovePlayerLeft();
//                            break;
//                        case ConsoleKey.D:
//                            MovePlayerRight();
//                            break;
//                        case ConsoleKey.F:
//                            FireWeapon();
//                            break;
//                        case ConsoleKey.Q:
//                            isRunning = false;
//                            break;
//                    }
//                }
//            }

//            // Simulate game frame update
//            Thread.Sleep(500);
//            UpdateGame();
//        }

//        isRunning = false;
//        enemyThread.Join();  // Ensure enemy thread stops before ending game
//        EndGame();
//    }

//    // Moves the player left
//    static void MovePlayerLeft()
//    {
//        if (playerPosition > 0)
//        {
//            playerPosition--;
//            Console.WriteLine(">>> Player moved left to position " + playerPosition);
//        }
//    }

//    // Moves the player right
//    static void MovePlayerRight()
//    {
//        if (playerPosition < 20)
//        {
//            playerPosition++;
//            Console.WriteLine(">>> Player moved right to position " + playerPosition);
//        }
//    }

//    // Fires player's weapon
//    static void FireWeapon()
//    {
//        Console.WriteLine("Player fired!");
//        if (Math.Abs(playerPosition - enemyPosition) < 5)
//        {
//            lock (lockObject)
//            {
//                enemyHealth -= 10;
//                Console.WriteLine("Hit! Enemy health: " + enemyHealth);
//            }
//        }
//        else
//        {
//            Console.WriteLine("Missed! Enemy is too far.");
//        }
//    }

//    // Enemy behavior thread: Moves enemy and attacks player
//    static void EnemyBehavior()
//    {
//        Random random = new Random();
//        while (isRunning && enemyHealth > 0)
//        {
//            lock (lockObject)
//            {
//                // Random enemy movement
//                int moveDirection = random.Next(0, 3);
//                if (moveDirection == 0 && enemyPosition > 0)
//                {
//                    enemyPosition--;
//                }
//                else if (moveDirection == 1 && enemyPosition < 20)
//                {
//                    enemyPosition++;
//                }

//                Console.WriteLine("> Enemy moved to position " + enemyPosition);

//                // Enemy attack if close to the player
//                if (Math.Abs(playerPosition - enemyPosition) < 5)
//                {
//                    playerHealth -= 10;
//                    Console.WriteLine("Enemy attacked! Player health: " + playerHealth);
//                }
//            }

//            Thread.Sleep(500); // Enemy action delay
//        }
//    }

//    // Game update loop
//    static void UpdateGame()
//    {
//        Console.WriteLine("Player Health: " + playerHealth + " | Enemy Health: " + enemyHealth);
//    }

//    // End game logic
//    static void EndGame()
//    {
//        if (playerHealth <= 0)
//        {
//            Console.WriteLine("Game Over! You were defeated.");
//        }
//        else if (enemyHealth <= 0)
//        {
//            Console.WriteLine("Victory! You defeated the enemy.");
//        }
//        else
//        {
//            Console.WriteLine("Game exited.");
//        }
//    }
//}


// Step 3


using System;
using System.Threading;

class SpaceShooterGame
{
    static bool isRunning = true;       // Game running status
    static int playerPosition = 10;     // Player position
    static int enemy1Position = 30;     // Enemy 1 position
    static int enemy2Position = 0;      // Enemy 2 position
    static int playerHealth = 100;      // Player health
    static int enemy1Health = 100;      // Enemy 1 health
    static int enemy2Health = 100;      // Enemy 2 health
    static object lockObject = new object(); // Lock object for thread safety

    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to Space Shooter!");
        Console.WriteLine("Press 'A' to move left, 'D' to move right, 'F' to fire. Press 'Q' to quit.");
        Console.Write("Game starts within ");
        for (int i = 3; i > 0; i--)
        {
            Console.Write(i + " ");
            Thread.Sleep(500);
        }
        Console.WriteLine(" Go~~~!");

        // Start enemy behavior threads
        Thread enemyThread1 = new Thread(new ThreadStart(Enemy1Behavior));
        Thread enemyThread2 = new Thread(new ThreadStart(Enemy2Behavior));
        enemyThread1.Start();
        enemyThread2.Start();

        while (isRunning && playerHealth > 0 && (enemy1Health > 0 || enemy2Health > 0))
        {
            // Player controls
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                lock (lockObject)
                {
                    switch (key)
                    {
                        case ConsoleKey.A:
                            MovePlayerLeft();
                            break;
                        case ConsoleKey.D:
                            MovePlayerRight();
                            break;
                        case ConsoleKey.F:
                            FireWeapon();
                            break;
                        case ConsoleKey.Q:
                            isRunning = false;
                            break;
                    }
                }
            }

            // Simulate game frame update
            Thread.Sleep(100);
            UpdateGame();
        }

        isRunning = false;
        enemyThread1.Join();  // Ensure enemy 1 thread stops before ending game
        enemyThread2.Join();  // Ensure enemy 2 thread stops before ending game
        EndGame();
    }

    // Moves the player left
    static void MovePlayerLeft()
    {
        if (playerPosition > 0)
        {
            playerPosition--;
            Console.WriteLine(">>> Player moved left to position " + playerPosition);
        }
    }

    // Moves the player right
    static void MovePlayerRight()
    {
        if (playerPosition < 20)
        {
            playerPosition++;
            Console.WriteLine(">>> Player moved right to position " + playerPosition);
        }
    }

    // Fires player's weapon
    static void FireWeapon()
    {
        Console.WriteLine("Player fired!");
        if (Math.Abs(playerPosition - enemy1Position) < 5)
        {
            lock (lockObject)
            {
                enemy1Health -= 10;
                Console.WriteLine("Hit! Enemy 1 health: " + enemy1Health);
            }
        }
        else if (Math.Abs(playerPosition - enemy2Position) < 5)
        {
            lock (lockObject)
            {
                enemy2Health -= 10;
                Console.WriteLine("Hit! Enemy 2 health: " + enemy2Health);
            }
        }
        else
        {
            Console.WriteLine("Missed! Enemy is too far.");
        }
    }

    // Enemy 1 behavior thread: Moves enemy 1 and attacks player
    static void Enemy1Behavior()
    {
        Random random = new Random();
        while (isRunning && enemy1Health > 0)
        {
            lock (lockObject)
            {
                // Random enemy movement
                int moveDirection = random.Next(0, 3);
                if (moveDirection == 0 && enemy1Position > 0)
                {
                    enemy1Position--;
                }
                else if (moveDirection == 1 && enemy1Position < 20)
                {
                    enemy1Position++;
                }

                Console.WriteLine("> Enemy 1 moved to position " + enemy1Position);

                // Enemy attack if close to the player
                if (Math.Abs(playerPosition - enemy1Position) < 5)
                {
                    playerHealth -= 10;
                    Console.WriteLine("Enemy 1 attacked! Player health: " + playerHealth);
                }
            }

            Thread.Sleep(500); // Enemy action delay
        }
    }

    // Enemy 2 behavior thread: Moves enemy 2 and attacks player
    static void Enemy2Behavior()
    {
        Random random = new Random();
        while (isRunning && enemy2Health > 0)
        {
            lock (lockObject)
            {
                // Random enemy movement
                int moveDirection = random.Next(0, 3);
                if (moveDirection == 0 && enemy2Position > 0)
                {
                    enemy2Position--;
                }
                else if (moveDirection == 1 && enemy2Position < 20)
                {
                    enemy2Position++;
                }

                Console.WriteLine("> Enemy 2 moved to position " + enemy2Position);

                // Enemy attack if close to the player
                if (Math.Abs(playerPosition - enemy2Position) < 5)
                {
                    playerHealth -= 10;
                    Console.WriteLine("Enemy 2 attacked! Player health: " + playerHealth);
                }
            }

            Thread.Sleep(500); // Enemy action delay
        }
    }

    // Game update loop
    static void UpdateGame()
    {
        Console.WriteLine("Player Health: " + playerHealth + " | Enemy 1 Health: " + enemy1Health + " | Enemy 2 Health: " + enemy2Health);
    }

    // End game logic
    static void EndGame()
    {
        if (playerHealth <= 0)
        {
            Console.WriteLine("Game Over! You were defeated.");
        }
        else if (enemy1Health <= 0 && enemy2Health <= 0)
        {
            Console.WriteLine("Victory! You defeated both enemies.");
        }
        else
        {
            Console.WriteLine("Game exited.");
        }
    }
}