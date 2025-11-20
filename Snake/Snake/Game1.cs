using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SnakeMono
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

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 600;
            graphics.PreferredBackBufferHeight = 400;
        }

        protected override void Initialize()
        {
            snake.Clear();
            snake.Add(new Point(columns / 2, rows / 2));

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

            // Kolize s hranou
            if (newHead.X < 0 || newHead.X >= columns ||
                newHead.Y < 0 || newHead.Y >= rows)
            {
                ResetGame();
                return;
            }

            // Kolize se sebou
            if (snake.Contains(newHead))
            {
                ResetGame();
                return;
            }

            snake.Insert(0, newHead);

            // Jablko
            if (newHead == apple)
            {
                score++;
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
                p = new Point(rand.Next(columns), rand.Next(rows));
            }
            while (snake.Contains(p));

            apple = p;
        }

        private void ResetGame()
        {
            snake.Clear();
            snake.Add(new Point(columns / 2, rows / 2));
            direction = new Point(1, 0);
            score = 0;
            SpawnApple();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            foreach (Point s in snake)
            {
                spriteBatch.Draw(rectTex,
                    new Rectangle(s.X * gridSize, s.Y * gridSize, gridSize, gridSize),
                    Color.LimeGreen);
            }

            spriteBatch.Draw(rectTex,
                new Rectangle(apple.X * gridSize, apple.Y * gridSize, gridSize, gridSize),
                Color.Red);

            spriteBatch.DrawString(font, $"Skore: {score}", new Vector2(10, 10), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
