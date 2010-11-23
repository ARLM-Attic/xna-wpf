using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace XnaGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        protected GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model m_model;
        float m_rot;
        Texture2D m_logo;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
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

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            m_model = Content.Load<Model>("dude");
            foreach (ModelMesh _mesh in m_model.Meshes)
            {
                foreach (BasicEffect _effect in _mesh.Effects)
                {
                    _effect.View = Matrix.CreateLookAt(new Vector3(-30, 75, 75), new Vector3(0, 50, 0), Vector3.Up);
                    _effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                                                                            this.GraphicsDevice.Viewport.AspectRatio,
                                                                            0.1f,
                                                                            100000.0f);
                }
            }

            m_logo = Content.Load<Texture2D>("logoXna");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            m_rot += (float)(MathHelper.ToRadians(45) * gameTime.ElapsedGameTime.TotalSeconds);


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach (ModelMesh _mesh in this.m_model.Meshes)
            {
                foreach (BasicEffect _effect in _mesh.Effects)
                {
                    _effect.World = Matrix.CreateRotationY(m_rot);
                }
                _mesh.Draw();
            }

            //spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Opaque);
            //spriteBatch.Draw(m_logo, new Rectangle(0, 0,
            //                                      (int)(this.graphics.PreferredBackBufferWidth * 0.30f),
            //                                      (int)(this.graphics.PreferredBackBufferHeight * 0.50f)),
            //                                      Color.White);
            //spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
