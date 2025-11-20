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

        private int gridSize = 20; // velikost jednoho čtverce
        private int columns = 30;  // počet sloupců
        private int rows = 20;     // počet řádků

        private List<Point> snake = new List<Point>();
        private Point direction = new Point(1, 0);
        private Point apple;

        private double moveTimer = 0;
        private double moveInterval = 0.12f;

        private int score = 0;
        private Random rand = new Random();

        private List<Particle> particles = new List<Particle>();
        private List<Point> obstacles = new List<Point>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Nastavení okna
            graphics.PreferredBackBufferWidth = columns * gridSize;
            graphics.PreferredBackBufferHeight = rows * gridSize;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // Had uprostřed
            snake.Clear();
            snake.Add(new Point(columns / 2, rows / 2));

            // Okrajové zdi
            GenerateWalls();

            // Náhodné překážky uvnitř mapy
            GenerateObstacles(20);

            SpawnApple();

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
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            HandleInput();

            moveTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (moveTimer >= moveInterval)
            {
                moveTimer = 0;
                MoveSnake();
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
                ResetGame();
                return;
            }

            snake.Insert(0, newHead);

            if (newHead == apple)
            {
                score++;
                CreateParticles(apple);
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

            // Horní a spodní hrany
            for (int x = 0; x < columns; x++)
            {
                obstacles.Add(new Point(x, 0));
                obstacles.Add(new Point(x, rows - 1));
            }

            // Levá a pravá hrana
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
        }

        private void CreateParticles(Point pos)
        {
            for (int i = 0; i < 10; i++)
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

            // Skin hada podle skóre
            Color snakeColor = Color.LimeGreen;
            if (score >= 10 && score < 20) snakeColor = Color.CornflowerBlue;
            else if (score >= 20 && score < 30) snakeColor = Color.MediumPurple;
            else if (score >= 30) snakeColor = new Color(rand.Next(255), rand.Next(255), rand.Next(255));

            // Překážky
            foreach (Point o in obstacles)
            {
                spriteBatch.Draw(rectTex,
                    new Rectangle(o.X * gridSize, o.Y * gridSize, gridSize, gridSize),
                    Color.Gray);
            }

            // Had
            foreach (Point s in snake)
            {
                spriteBatch.Draw(rectTex,
                    new Rectangle(s.X * gridSize, s.Y * gridSize, gridSize, gridSize),
                    snakeColor);
            }

            // Jablko
            spriteBatch.Draw(rectTex,
                new Rectangle(apple.X * gridSize, apple.Y * gridSize, gridSize, gridSize),
                Color.Red);

            // Particle efekty
            foreach (var p in particles)
            {
                spriteBatch.Draw(rectTex, new Rectangle((int)p.Position.X, (int)p.Position.Y, 4, 4), Color.Yellow);
            }

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
