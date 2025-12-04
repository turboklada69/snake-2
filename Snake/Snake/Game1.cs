using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Snake
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Texture2D rectTex;
        private SpriteFont font;

        private int gridSize = 20;
        private int columns = 30;
        private int rows = 20;

        private List<Point> snake = new List<Point>();
        private Point direction = new Point(1, 0);

        private Point apple;

        private double moveTimer = 0;
        private double moveInterval = 0.12f;

        private int score = 0;
        private Random rand = new Random();

        private List<Particle> particles = new List<Particle>();
        private List<Point> obstacles = new List<Point>();

        // --- NOVÉ proměnné pro plynulý pohyb ---
        private Vector2 smoothPosition;    // aktuální pozice hlavy hada (floatová)
        private Vector2 targetPosition;    // cílová pozice hlavy na gridu (Point převedený na Vector2)
        private float moveProgress = 1f;   // kolik procent cesty mezi dvěma políčky je ujeto (0-1)

        // --- NOVÉ proměnné pro animovanou hlavu ---
        private float headOpenTimer = 0f;
        private bool headOpen = false;

        // --- NOVÉ proměnné pro efekt smrti ---
        private bool isDead = false;
        private double deathTimer = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = columns * gridSize;
            graphics.PreferredBackBufferHeight = rows * gridSize;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            snake.Clear();
            snake.Add(new Point(columns / 2, rows / 2));

            GenerateWalls();
            GenerateObstacles(20);
            SpawnApple();

            // Inicializace plynulého pohybu
            smoothPosition = new Vector2(snake[0].X * gridSize, snake[0].Y * gridSize);
            targetPosition = smoothPosition;
            moveProgress = 1f;

            isDead = false;
            deathTimer = 0;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            rectTex = new Texture2D(GraphicsDevice, 1, 1);
            rectTex.SetData(new Color[] { Color.White });

            font = Content.Load<SpriteFont>("DefaultFont");
        }

        protected override void Update(GameTime gameTime)
        {
            if (isDead)
            {
                deathTimer += gameTime.ElapsedGameTime.TotalSeconds;
                UpdateParticles(gameTime);

                if (deathTimer > 1.0) // 1 sekunda smrti, pak restart
                {
                    ResetGame();
                }
                return;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            HandleInput();

            moveTimer += gameTime.ElapsedGameTime.TotalSeconds;

            // --- plynulý pohyb ---
            if (moveProgress < 1f)
            {
                float speed = 1f / (float)moveInterval;
                moveProgress += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (moveProgress > 1f) moveProgress = 1f;

                // Interpolace mezi starou a novou pozicí
                smoothPosition = Vector2.Lerp(smoothPosition, targetPosition, moveProgress);
            }
            else if (moveTimer >= moveInterval)
            {
                moveTimer = 0;
                MoveSnake();
                moveProgress = 0f;
                // Nastav nové cílové pozice pro plynulý pohyb
                targetPosition = new Vector2(snake[0].X * gridSize, snake[0].Y * gridSize);
            }

            // --- animace hlavy ---
            headOpenTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (headOpenTimer >= 0.3f)
            {
                headOpen = !headOpen;
                headOpenTimer = 0f;
            }

            UpdateParticles(gameTime);

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            KeyboardState k = Keyboard.GetState();

            if (k.IsKeyDown(Keys.Up) && direction.Y != 1)
                direction = new Point(0, -1);
            else if (k.IsKeyDown(Keys.Down) && direction.Y != -1)
                direction = new Point(0, 1);
            else if (k.IsKeyDown(Keys.Left) && direction.X != 1)
                direction = new Point(-1, 0);
            else if (k.IsKeyDown(Keys.Right) && direction.X != -1)
                direction = new Point(1, 0);
        }

        private void MoveSnake()
        {
            Point head = snake[0];
            Point newHead = new Point(head.X + direction.X, head.Y + direction.Y);

            if (obstacles.Contains(newHead) || snake.Contains(newHead))
            {
                // Zahaj efekt smrti
                isDead = true;
                deathTimer = 0;
                CreateParticles(newHead, 40);
                return;
            }

            snake.Insert(0, newHead);

            if (newHead == apple)
            {
                score++;
                CreateParticles(apple, 10);
                SpawnApple();
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }
        }

        private void SpawnApple()
        {
            Point p;
            do
            {
                p = new Point(rand.Next(1, columns - 1), rand.Next(1, rows - 1));
            } while (snake.Contains(p) || obstacles.Contains(p));

            apple = p;
        }

        private void GenerateWalls()
        {
            obstacles.Clear();

            for (int x = 0; x < columns; x++)
            {
                obstacles.Add(new Point(x, 0));
                obstacles.Add(new Point(x, rows - 1));
            }

            for (int y = 1; y < rows - 1; y++)
            {
                obstacles.Add(new Point(0, y));
                obstacles.Add(new Point(columns - 1, y));
            }
        }

        private void GenerateObstacles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Point p;
                do
                {
                    p = new Point(rand.Next(1, columns - 1), rand.Next(1, rows - 1));
                } while (snake.Contains(p) || obstacles.Contains(p));

                obstacles.Add(p);
            }
        }

        private void ResetGame()
        {
            snake.Clear();
            snake.Add(new Point(columns / 2, rows / 2));
            score = 0;
            particles.Clear();

            obstacles.Clear();
            GenerateWalls();
            GenerateObstacles(20);
            SpawnApple();

            smoothPosition = new Vector2(snake[0].X * gridSize, snake[0].Y * gridSize);
            targetPosition = smoothPosition;
            moveProgress = 1f;

            isDead = false;
            deathTimer = 0;
        }

        private void CreateParticles(Point pos, int count)
        {
            for (int i = 0; i < count; i++)
            {
                particles.Add(new Particle(
                    new Vector2(pos.X * gridSize + gridSize / 2, pos.Y * gridSize + gridSize / 2),
                    new Vector2((float)(rand.NextDouble() - 0.5) * 80, (float)(rand.NextDouble() - 0.5) * 80),
                    0.5f
                ));
            }
        }

        private void UpdateParticles(GameTime time)
        {
            float dt = (float)time.ElapsedGameTime.TotalSeconds;

            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Life -= dt;
                p.Position += p.Velocity * dt;

                if (p.Life <= 0)
                    particles.RemoveAt(i);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // --- Blikající zdi ---
            float pulse = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) * 0.2 + 0.8);
            Color wallColor = new Color(pulse, pulse, pulse);

            // Překážky (zdi + kameny)
            foreach (Point o in obstacles)
            {
                bool isWall = (o.X == 0 || o.X == columns - 1 || o.Y == 0 || o.Y == rows - 1);
                Color color = isWall ? wallColor : Color.Gray;

                spriteBatch.Draw(rectTex,
                    new Rectangle(o.X * gridSize, o.Y * gridSize, gridSize, gridSize),
                    color);
            }

            // --- Had ---

            // Vykreslíme tělo hadu (vše kromě hlavy)
            for (int i = 1; i < snake.Count; i++)
            {
                Point s = snake[i];
                spriteBatch.Draw(rectTex,
                    new Rectangle(s.X * gridSize, s.Y * gridSize, gridSize, gridSize),
                    Color.LimeGreen);
            }

            // Vykreslení hlavy s animací a očima

            // Pozice hlavy (plynulá)
            Rectangle headRect = new Rectangle(
                (int)smoothPosition.X,
                (int)smoothPosition.Y,
                gridSize,
                gridSize);

            // Barva hlavy mění se podle otevření pusy
            Color headColor = headOpen ? Color.LimeGreen : Color.Green;

            spriteBatch.Draw(rectTex, headRect, headColor);

            // Oči (dle směru)
            int eyeSize = 4;
            int eyeOffsetX = 6;
            int eyeOffsetY = 5;

            Vector2 leftEye = new Vector2();
            Vector2 rightEye = new Vector2();

            if (direction == new Point(1, 0)) // doprava
            {
                leftEye = new Vector2(headRect.X + eyeOffsetX, headRect.Y + eyeOffsetY);
                rightEye = new Vector2(headRect.X + eyeOffsetX, headRect.Y + eyeOffsetY + eyeSize + 2);
            }
            else if (direction == new Point(-1, 0)) // doleva
            {
                leftEye = new Vector2(headRect.X + gridSize - eyeOffsetX - eyeSize, headRect.Y + eyeOffsetY);
                rightEye = new Vector2(headRect.X + gridSize - eyeOffsetX - eyeSize, headRect.Y + eyeOffsetY + eyeSize + 2);
            }
            else if (direction == new Point(0, -1)) // nahoru
            {
                leftEye = new Vector2(headRect.X + 5, headRect.Y + eyeOffsetY);
                rightEye = new Vector2(headRect.X + gridSize - 9, headRect.Y + eyeOffsetY);
            }
            else if (direction == new Point(0, 1)) // dolů
            {
                leftEye = new Vector2(headRect.X + 5, headRect.Y + gridSize - eyeOffsetY - eyeSize);
                rightEye = new Vector2(headRect.X + gridSize - 9, headRect.Y + gridSize - eyeOffsetY - eyeSize);
            }

            spriteBatch.Draw(rectTex, new Rectangle((int)leftEye.X, (int)leftEye.Y, eyeSize, eyeSize), Color.Black);
            spriteBatch.Draw(rectTex, new Rectangle((int)rightEye.X, (int)rightEye.Y, eyeSize, eyeSize), Color.Black);

            // --- Jablko ---
            spriteBatch.Draw(rectTex,
                new Rectangle(apple.X * gridSize, apple.Y * gridSize, gridSize, gridSize),
                Color.Red);

            // --- Particles ---
            foreach (var p in particles)
            {
                spriteBatch.Draw(rectTex, new Rectangle((int)p.Position.X, (int)p.Position.Y, 4, 4), Color.Yellow);
            }

            // --- Skóre ---
            spriteBatch.DrawString(font, $"Skore: {score}", new Vector2(10, 10), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private class Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Life;

            public Particle(Vector2 pos, Vector2 vel, float life)
            {
                Position = pos;
                Velocity = vel;
                Life = life;
            }
        }
    }
}
