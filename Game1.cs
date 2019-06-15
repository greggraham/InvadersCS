using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace InvadersCS
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private const int _width = 639;
        private const int _height = 426;

        private Random _rand = new Random();

        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _shipTexture;
        private Texture2D _starsTexture;
        private Texture2D _saucerTexture;
        private Texture2D _plasmaTexture;
        private SpriteFont _font;

        private SoundEffect _shoot;
        private SoundEffect _explosion;

        private const int _numSaucers = 5;
        private const int _gameOverInvasions = 10;
        private float _shipSpeed = 200;
        private float _plasmaSpeed = 300;
        private float _saucerSpeed = 60;

        private int _score = 0;
        private int _invasions = 0;

        private Vector2 _shipPosition;
        private Vector2 _plasmaPosition;
        private Vector2[] _saucerPositions = new Vector2[_numSaucers];

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = _width,
                PreferredBackBufferHeight = _height
            };

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _shipPosition = new Vector2(_width / 2, _height - 80);
            _plasmaPosition = new Vector2(-10, -10);
            var saucer_y = -10;
            for (var i = 0; i < _saucerPositions.Length; i++)
            {
                _saucerPositions[i] = new Vector2(_rand.Next(_width), saucer_y);
                saucer_y -= 80;
            }

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _starsTexture = Content.Load<Texture2D>("stars");
            _shipTexture = Content.Load<Texture2D>("ship");
            _saucerTexture = Content.Load<Texture2D>("saucer");
            _plasmaTexture = Content.Load<Texture2D>("plasma");
            _font = Content.Load<SpriteFont>("Score");

            _shoot = Content.Load<SoundEffect>("shoot");
            _explosion = Content.Load<SoundEffect>("explosion");
            Song song = Content.Load<Song>("Slammin27");
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            if (_invasions < _gameOverInvasions)
            {
                var kstate = Keyboard.GetState();

                if (kstate.IsKeyDown(Keys.Left) && _shipPosition.X > -8)
                {
                    _shipPosition.X -= _shipSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (kstate.IsKeyDown(Keys.Right) && _shipPosition.X < _width + 8)
                {
                    _shipPosition.X += _shipSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (kstate.IsKeyDown(Keys.Space) && _plasmaPosition.Y < _height * 0.6f)
                {
                    _plasmaPosition.X = _shipPosition.X;
                    _plasmaPosition.Y = _shipPosition.Y - 6;
                    _shoot.Play();
                }

                if (IsOnScreen(_plasmaPosition))
                {
                    _plasmaPosition.Y -= _plasmaSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    for (var i = 0; i < _saucerPositions.Length; i++)
                    {
                        if (Collision(_saucerTexture, _saucerPositions[i], _plasmaTexture, _plasmaPosition))
                        {
                            _score++;
                            _saucerPositions[i] = ResetSaucer(_saucerPositions[i]);
                            _plasmaPosition = new Vector2(-10, -10);
                            _explosion.Play();
                        }
                    }
                }

                for (var i = 0; i < _saucerPositions.Length; i++)
                {
                    _saucerPositions[i].Y += _saucerSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if ((_saucerPositions[i].Y - _saucerTexture.Height / 2) > _height)
                    {
                        _invasions++;
                        _saucerPositions[i] = ResetSaucer(_saucerPositions[i]);
                        continue;
                    }

                    if (_rand.Next(2) == 1)
                    {
                        _saucerPositions[i].X += _saucerSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (_saucerPositions[i].X > _width)
                        {
                            _saucerPositions[i].X = _width;
                        }
                    }
                    else
                    {
                        _saucerPositions[i].X -= _saucerSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (_saucerPositions[i].X < 0)
                        {
                            _saucerPositions[i].X = 0;
                        }
                    }
                }
            }
            base.Update(gameTime);
        }

        private Vector2 ResetSaucer(Vector2 pos)
        {
            pos.X = _rand.Next(_width);
            pos.Y = _rand.Next(110) - 100;
            return pos;
        }


        private bool Collision(Texture2D t1, Vector2 p1, Texture2D t2, Vector2 p2)
        {
            var top1 = p1.Y - t1.Height / 2;
            var top2 = p2.Y - t2.Height / 2;
            var bottom1 = p1.Y + t1.Height / 2;
            var bottom2 = p2.Y + t2.Height / 2;
            var left1 = p1.X - t1.Width / 2;
            var left2 = p2.X - t2.Width / 2;
            var right1 = p1.X + t1.Width / 2;
            var right2 = p2.X + t2.Width / 2;

            var vertical1 = top1 < bottom2 && top1 > top2;
            var vertical2 = top2 < bottom1 && top2 > top1;
            var verticalOverlap = vertical1 || vertical2;

            var horizontal1 = left1 < right2 && left1 > left2;
            var horizontal2 = left2 < right1 && left2 > left1;
            var horizontalOverlap = horizontal1 || horizontal2;

            return verticalOverlap && horizontalOverlap;
        }

        /// <summary>
        /// This is used to draw a sprite centered at the given position
        /// </summary>
        /// <param name="texture">The sprite texture to be drawn</param>
        /// <param name="pos">The position of the sprite center</param>
        private void DrawCentered(Texture2D texture, Vector2 pos)
        {
            if (IsOnScreen(pos))
            {
                _spriteBatch.Draw(texture, pos, null, Color.White, 0,
                                 new Vector2(texture.Width / 2, texture.Height / 2),
                                 Vector2.One, SpriteEffects.None, 0);
            }
        }

        /// <summary>
        /// Check to see if a given position is considered to be on the screen.
        /// </summary>
        /// <param name="pos">The position to be checked</param>
        /// <returns>true if on screen</returns>
        private bool IsOnScreen(Vector2 pos)
        {
            return pos.X > -12 && pos.Y > -12 &&
                   pos.X < _width + 12 && pos.Y < _height + 12;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            _spriteBatch.Draw(_starsTexture, new Vector2(0, 0), Color.White);
            _spriteBatch.DrawString(_font, $"score={_score}, invasions={_invasions}",
                                   new Vector2(10, _height - 30), Color.Lime);
            if (_invasions >= _gameOverInvasions)
            {
                _spriteBatch.DrawString(_font, "Game Over", new Vector2(277, _height / 2), Color.Lime);
            }
            else
            {
                DrawCentered(_shipTexture, _shipPosition);
                DrawCentered(_plasmaTexture, _plasmaPosition);
                foreach (var pos in _saucerPositions)
                {
                    DrawCentered(_saucerTexture, pos);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
