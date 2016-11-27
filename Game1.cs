
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
using System.Xml;
using KSshooter.Classes;
using KSshooter.Classes.BinTree;
using System.Diagnostics;
using System.Threading;
using PerformanceUtility.GameDebugTools;

namespace KSshooter
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        Level levelOne;
        Vector2 veiwPortOffSet;
        
        //debug
        Vector2 lastViewPortOffset;
        Player player;
        KeyboardState KeyboardPreviousState;
        KeyboardState KeyboardCurrentState;
        MouseState MouseCurrentState;
        MouseState MousePreviousState;

        //pause
        bool pause;

        //gameover
        bool isGameOver = false;

        //textures
        Texture2D Circle;

        //framerate
        double dframerate;
        double dframeratemin = 10000;
        double dframeratelastmin = 0;

        //camera
        Camera2d cam;
        const float ZOOMDEFAULT = 1.0f;

        //bullets
        List<Bullet> Bullets;
        Texture2D BulletTexture;
        const int BULLETFIRERATE = 200; //in milliseconds
        double lastBullet;

        //enemies
        List<Enemy> removeEnemies;
        bool UpdatedAstar = false;

        //Pet
        Pet pet;

        //UI
        UI ui;

        //fonts
        SpriteFont spritefont;
        SpriteFont fontArial14Regular;
        SpriteFont font;

        //Mouse cursor
        Texture2D MouseCursorTexture;
        
        #region Temp Variables
        //*TEMP* variables
        Vector2 tempLoc;
        Vector2 offset;
        Rectangle tempRec;
        #endregion //temp variables        

        //timestep
        TimeSpan timestep;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            //set display size
            graphics.PreferredBackBufferHeight = 700;
            graphics.PreferredBackBufferWidth = 1000;
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
            //graphics.IsFullScreen = true;
            timestep = new TimeSpan(0, 0, 0, 0, 6);

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
            //PresentationParameters par = GraphicsDevice.PresentationParameters;
            //par.PresentationInterval = PresentInterval.Immediate;
            //GraphicsDevice.Reset(par);
            Vector2 offset = new Vector2(0, 0);
            Vector2 tempLoc = new Vector2(0, 0);

            //debug stuff
            DebugSystem.Initialize(this, "Font");

            GraphicsDevice.SamplerStates[0] = new SamplerState() { Filter = TextureFilter.Anisotropic };
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

            KeyboardCurrentState = Keyboard.GetState();
            MouseCurrentState = Mouse.GetState();
            Bullets = new List<Bullet>(50);
            Bullets = new List<Bullet>(50);
            removeEnemies = new List<Enemy>();
            lastBullet = 0;
            pause = false;
            tempRec = new Rectangle();
            this.IsMouseVisible = false;

            pet = new Pet(Content.Load<Texture2D>("PetUnicorn"), 20);
            fontArial14Regular = Content.Load<SpriteFont>("Arial14regular");
            font = Content.Load<SpriteFont>("Font");
            
            //initialize player
            player = new Player(Content.Load<Texture2D>("Pawn"), new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2));
            player.Damaged += new CharacterObject.DamagedEventHandler(player_Damaged);
            player.LevelUp += new Player.LevelUpEventHandler(player_LevelUp);
            player.ObjectDeath += new CharacterObject.ObjectDeathEventHandler(player_ObjectDeath);

            //set up Level
            XmlDocument xml = new XmlDocument();
            string levelFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\levels\LevelTwo.xml";
            try
            {
                xml.Load(levelFile);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Problem loading level, game will exit.  " + e.Message, "Error");
                this.Exit();
            }
            levelOne = new Level(xml, Content, player);
            player.Position = levelOne.StartLocation;
            levelOne.ActiveRoomChanged += new Level.ActiveRoomChangedEventCallback(levelOne_ActiveRoomChanged);
            levelOne_ActiveRoomChanged(levelOne, new EventArgs()); //set up events

            //bullets
            BulletTexture = Content.Load<Texture2D>("BulletBlack");
            for (int i = 0; i < Bullets.Capacity; i++)
            {
                Bullet bullet = new Bullet(BulletTexture);
                bullet.Alive = false;
                Bullets.Add(bullet);
            }

            player.inRoom = levelOne.ActiveRoom;
            //initialize camera
            cam = new Camera2d();
            cam.Pos = new Vector2(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height/2);
            //load font
            spritefont = Content.Load<SpriteFont>("Text");
            //load mouse cursor
            MouseCursorTexture = Content.Load<Texture2D>("TargetCursor");

            //textures
            Circle = Content.Load<Texture2D>("Circle");
            //set initial viewportoffsetveiwPortOffSet = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            veiwPortOffSet = new Vector2(player.screenPosition.X - player.PositionX, player.screenPosition.Y - player.PositionY);

            //ui
            ui = new UI(levelOne, Content.Load<SpriteFont>("UIfont"), GraphicsDevice.Viewport, Content);
            ui.CooldownBackground = Content.Load<Texture2D>("OutlineBox");

            //Test text
            FloatingText aText = new FloatingText("Welcome to KSShooter", spritefont, levelOne.StartLocation, 5000, FloatingText.FontEffects.fade);
            aText._color = new Color(40, 155, 40, 200);
            //aText.AddShadowEffect(new Vector2(3, 3), new Color(0, 0, 0, 100));
            ui.AddFloatingText(aText);
        }

        private string loadLevel()
        {
            System.Windows.Forms.OpenFileDialog levelFile = new System.Windows.Forms.OpenFileDialog();
            levelFile.FileName = @"C:\Users\Rory\Documents\Visual Studio 2010\Projects\KSlevelDesigner\KSlevelDesigner\bin\Debug\AstarTestLevel2.xml";
            levelFile.Filter = "XML|*.xml";
            levelFile.ShowDialog();
            return levelFile.FileName;
        }

        void player_ObjectDeath(object Sender, ObjectDeathEventArgs e)
        {
            GameOver();
        }

        void player_LevelUp(Player sender, LevelUpEventArgs e)
        {
            //SpriteFont font = Content.Load<SpriteFont>("");
            Vector2 strLength = fontArial14Regular.MeasureString("Level Up!");
            FloatingText levelUPtext = new FloatingText("Level up!", fontArial14Regular, new Vector2(player.PositionX + player.HitRectangle.Width / 2 - strLength.X / 2, player.PositionY + player.HitRectangle.Height / 2 - strLength.Y), 2000, FloatingText.FontEffects.None);
            levelUPtext.AddTextMovement(levelUPtext.StartLocation + new Vector2(0, -40), FloatingText.TextMovemenType.sin);
            levelUPtext._color = new Color(40,200,40);
            levelUPtext.AddShadowEffect(new Vector2(1, 1), new Color(0, 0, 0, 155));
            ui.AddFloatingText(levelUPtext);
        }

        void levelOne_ActiveRoomChanged(Level sender, EventArgs e)
        {
            foreach (Enemy enemy in sender.ActiveRoom.Enemies)
            {
                enemy.Damaged += new Enemy.DamagedEventHandler(enemy_Damaged);
                enemy.ObjectDeath += new CharacterObject.ObjectDeathEventHandler(enemy_ObjectDeath);
            }
        }

        void enemy_ObjectDeath(object Sender, ObjectDeathEventArgs e)
        {
            Enemy enemy = Sender as Enemy;
            removeEnemies.Add(enemy);
            player.ExperiencePoints += enemy.XPvalue;
        }

        void player_Damaged(object sender, DamageEventArgs e)
        {
            character_Damaged(sender, e);
        }

        void enemy_Damaged(object sender, DamageEventArgs e)
        {
            character_Damaged(sender, e);
        }

        void character_Damaged(Object sender, DamageEventArgs e)
        {
            SpriteFont font = Content.Load<SpriteFont>("DamageFont");
            CharacterObject character = sender as CharacterObject;
            FloatingText fText = new FloatingText(e.damageTaken.ToString("0"), font, character.Position, 1000);
            fText._color = Color.Red;
            fText.AddTextMovement(new Vector2(character.PositionX, character.PositionY - 15), FloatingText.TextMovemenType.sin);
            fText.AddShadowEffect(new Vector2(1, 1), Color.Black);
            ui.AddFloatingText(fText);
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
#if DEBUG
            DebugSystem.Instance.FpsCounter.Visible = true;
            DebugSystem.Instance.TimeRuler.Visible = true;
            DebugSystem.Instance.TimeRuler.ShowLog = true;
#endif //debug

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (gameTime.ElapsedGameTime <= timestep)
            {
                Thread.Sleep(timestep - gameTime.ElapsedGameTime);
                MyUpdate(gameTime);
            }
            else
            {
                MyUpdate(gameTime);
            }
            

            base.Update(gameTime);

        }

        protected void MyUpdate(GameTime gameTime)
        {
            //Debug stuff
#if DEBUG
            DebugSystem.Instance.TimeRuler.StartFrame();
            DebugSystem.Instance.TimeRuler.BeginMark("Update", Color.Blue);
#endif //debug
            //keyboard and mouse states
            KeyboardPreviousState = KeyboardCurrentState;
            KeyboardCurrentState = Keyboard.GetState();
            MousePreviousState = MouseCurrentState;
            MouseCurrentState = Mouse.GetState();

            UpdatedAstar = false;

            //reset
            if (KeyboardCurrentState.IsKeyDown(Keys.Escape) == true)
            {
                this.Exit();
            }

            if (isGameOver)
            {
                base.Update(gameTime);
#if DEBUG
                DebugSystem.Instance.TimeRuler.EndMark("Update");
#endif //debug
                return;
            }

            if (KeyboardCurrentState.IsKeyDown(Keys.C) == true && KeyboardPreviousState.IsKeyDown(Keys.C) == false)
            {
                ui.ToggleCharacterSheet();
                if (ui.ShowCharacterSheet == true)
                    pause = true;
                else
                    pause = false;
            }

            //pause
            if (KeyboardCurrentState.IsKeyDown(Keys.Space) == true && KeyboardPreviousState.IsKeyDown(Keys.Space) == false)
            {
                ui.UnDarkenScreen();
                pause = !pause;
            }
            if (pause == true)
            {
                ui.DarkenScreen();
                base.Update(gameTime);
#if DEBUG
                DebugSystem.Instance.TimeRuler.EndMark("Update");
#endif //deubug
                return;
            }

            lastViewPortOffset = veiwPortOffSet;
            //Updaterate | framerate
            dframerate = 1 / gameTime.ElapsedGameTime.TotalSeconds;

            //Player movement
            tempLoc.X = player.Position.X;
            tempLoc.Y = player.Position.Y;
            offset.X = offset.Y = 0;

            if (KeyboardCurrentState.IsKeyDown(Keys.Up) == true || KeyboardCurrentState.IsKeyDown(Keys.W))
            {
                double off = (-1 * player.Speed * (gameTime.ElapsedGameTime.TotalSeconds));
                offset.Y += (float)off;
            }
            if (KeyboardCurrentState.IsKeyDown(Keys.Down) == true || KeyboardCurrentState.IsKeyDown(Keys.S))
            {
                double off = (player.Speed * (gameTime.ElapsedGameTime.TotalSeconds));
                offset.Y += (float)off;
            }
            if (KeyboardCurrentState.IsKeyDown(Keys.Left) == true || KeyboardCurrentState.IsKeyDown(Keys.A))
            {
                double off = (float)(-1 * player.Speed * (gameTime.ElapsedGameTime.TotalSeconds));
                offset.X += (float)off;
            }
            if (KeyboardCurrentState.IsKeyDown(Keys.Right) == true || KeyboardCurrentState.IsKeyDown(Keys.D))
            {
                double off = (player.Speed * (gameTime.ElapsedGameTime.TotalSeconds));
                offset.X += (float)off;
            }
            if (offset.X != 0 && offset.Y != 0) //in the case movement on more than one axis
            {
                offset *= 0.707f; // = cos(π/4)*offset.X, sin(π/4)*offset.Y
            }

            //Camera
            if (KeyboardCurrentState.IsKeyDown(Keys.PageUp) == true)
            {
                cam.Zoom += 0.01f;
            }
            else if (KeyboardCurrentState.IsKeyDown(Keys.PageDown) == true)
            {
                cam.Zoom -= 0.01f;
            }

            if (KeyboardCurrentState.IsKeyDown(Keys.End) == true)
            {
                cam.Zoom = 0.5f;
            }
            if (KeyboardCurrentState.IsKeyDown(Keys.Home) == true)
            {
                cam.Zoom = ZOOMDEFAULT;
            }
            bool teleported = false;
            //Abilities
            if (KeyboardCurrentState.IsKeyDown(Keys.D1) == true && KeyboardPreviousState.IsKeyDown(Keys.D1) == false)
            {
                float tmpoffsetX = player.screenPosition.X - MouseCurrentState.X;
                float tmpoffsetY = player.screenPosition.Y - MouseCurrentState.Y;
                //do teleport
                if (player.TeleportAbility(new Vector2(MouseCurrentState.X - veiwPortOffSet.X, MouseCurrentState.Y - veiwPortOffSet.Y)) == true)
                {
                    //reposition
                    offset.X -= tmpoffsetX;
                    offset.Y -= tmpoffsetY;
                    teleported = true;
                }
            }
            if (KeyboardCurrentState.IsKeyDown(Keys.D2) == true)
            {
                pet.Reset();
                player.SummonCreature(pet);
            }

            //shoot
            if ((KeyboardCurrentState.IsKeyDown(Keys.R) && !KeyboardPreviousState.IsKeyDown(Keys.R)) || (MouseCurrentState.LeftButton == ButtonState.Pressed))
            {
                if (lastBullet > BULLETFIRERATE)
                {
                    Bullet bullet = null;
                    //find a dead bullet we can use.
                    foreach (Bullet bul in Bullets)
                    {
                        if (bul.Alive == false)
                        {
                            bullet = bul;
                            break;
                        }

                    }
                    if (bullet != null) // if all the bullets are alive... don't fire
                    {
                        bullet.ShootBullet(player.Position, PointToward(new Vector2(player.screenPosition.X, player.screenPosition.Y), new Vector2(MouseCurrentState.X, MouseCurrentState.Y)));
                        lastBullet = 0;
                    }
                }
            }
            lastBullet += gameTime.ElapsedGameTime.TotalMilliseconds;

            foreach (Bullet bul in Bullets)
            {
                if (bul.Alive == true)
                {
                    bul.Update(gameTime);
                    foreach (Enemy en in levelOne.ActiveRoom.Enemies)
                    {
                        if (bul.HitDetection(en.HitRectangle) == true)
                        {
                            bul.AttackTarget(en);
                        }

                    }
                }
            }

            bool hitFound = false;
            if (teleported == false)
            {
                //hit detection stuff
                tempRec.X = (int)(player.PositionX + offset.X);
                tempRec.Y = (int)(player.PositionY + offset.Y);
                tempRec.Width = player.HitRectangle.Width;
                tempRec.Height = player.HitRectangle.Height;
                //check tile hit

                player.HitDetectionWithTiles(tempRec, ref offset);
            }

            //check enemy hit
            foreach (Enemy en in levelOne.ActiveRoom.Enemies)
            {
                if (en.HitDetection(tempRec) == true)
                {
                    player.fixHitOverlap(en.HitRectangle, ref offset);
                }
            }
            //check for hit with exit
            foreach (RoomExit exit in levelOne.ActiveRoom.Exits)
            {
                if (player.HitDetection(exit.HitRectangle) == true)
                {
                    ChangeRoom(exit.toRoom, exit.toLocation);
                }
            }

            //if no hit is found, update all the screen draw loctions
            if (hitFound == false)
            {
                veiwPortOffSet.X -= offset.X;
                veiwPortOffSet.Y -= offset.Y;
                player.Position += offset;
            }

            //updates
            player.Update(gameTime);
            levelOne.ActiveRoom.Update(gameTime);
            foreach (Enemy en in levelOne.ActiveRoom.Enemies)
            {
                en.Update(gameTime, ref UpdatedAstar);
                //en.Shoot(en.Target.Position, new Bullet(Content.Load<Texture2D>("RedArrow")));
            }
            ui.Update(gameTime);
            foreach (Enemy e in removeEnemies)
            {
                levelOne.ActiveRoom.RemoveEnemy(e);
            }
            removeEnemies.Clear();

#if DEBUG
            DebugSystem.Instance.TimeRuler.EndMark("Update");
#endif //debug
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.PreferMultiSampling = true;
            //Debug Stuff
#if DEBUG
            DebugSystem.Instance.TimeRuler.BeginMark("Draw", Color.Red);
#endif //debug

            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, cam.get_transformation(GraphicsDevice));
            //spriteBatch.Begin();
            //draw Tiles
            foreach (Tile tile in levelOne.ActiveRoom.tiles)
            {
                tile.draw(spriteBatch, veiwPortOffSet, GraphicsDevice.Viewport.Bounds);
            }

            //foreach (Tile tile in levelOne.ActiveRoom.tiles)
            //{
            //    tile.drawOverlay(spriteBatch, veiwPortOffSet, GraphicsDevice.Viewport.Bounds);
            //}

            //draw enemies
            foreach (Enemy e in levelOne.ActiveRoom.Enemies)
            {
                e.draw(spriteBatch, veiwPortOffSet, GraphicsDevice.Viewport.Bounds);
            }

            //draw Bullets
            foreach (Bullet bul in Bullets)
            {
                if (bul.Alive == true)
                {
                    bul.draw(spriteBatch, veiwPortOffSet, GraphicsDevice.Viewport.Bounds);
                }
            }
            //enemy bullets
            foreach (Bullet bul in levelOne.ActiveRoom.EnemyBullets)
            {
                bul.draw(spriteBatch, veiwPortOffSet, GraphicsDevice.Viewport.Bounds);
            }

            //draw player
            player.draw(spriteBatch, veiwPortOffSet, GraphicsDevice.Viewport.Bounds);
            if (player.Pet != null)
                player.Pet.draw(spriteBatch, veiwPortOffSet, GraphicsDevice.Viewport.Bounds);
            
            //draw framerate
            //if (dframerate < dframeratemin || dframeratelastmin > 2000)
            //{
            //    dframeratemin = dframerate;
            //    dframeratelastmin = 0;
            //}
            //dframeratelastmin += gameTime.ElapsedGameTime.TotalMilliseconds;
            //spriteBatch.DrawString(spritefont, "FrameRate: " + dframerate.ToString("000.0") + "  Min FrameRate: " + dframeratemin.ToString("00.0"), new Vector2(0, 0), Color.White);

            //draw Garbage collection
            //spriteBatch.DrawString(spritefont, "collection: " + GC.CollectionCount(GC.GetGeneration(player)), new Vector2(40, 100), Color.White);
            spriteBatch.End();

            spriteBatch.Begin(); //this time without the camera
            //draw UI
            ui.Draw(spriteBatch, veiwPortOffSet);         

            //Text Teleport circle
            //spriteBatch.Draw(Circle, new Vector2(player.ScreenPosition.X - 165, player.ScreenPosition.Y - 165), new Color(0,0,155,125));

            //debug
            //Vector2 offsetDiff = veiwPortOffSet - lastViewPortOffset;
            //spriteBatch.DrawString(spritefont, "Offset X:" + offsetDiff.X + " Offset Y:" + offsetDiff.Y, new Vector2(300, 0), Color.White);

            //draw Mousecursor (must be last)
            spriteBatch.Draw(MouseCursorTexture, new Rectangle(MouseCurrentState.X - MouseCursorTexture.Width / 2, MouseCurrentState.Y - MouseCursorTexture.Height/2, MouseCursorTexture.Width, MouseCursorTexture.Height), Color.White);

            spriteBatch.End();
            if (isGameOver)
            {
                GameOver();
            }
            base.Draw(gameTime);
#if DEBUG
            DebugSystem.Instance.TimeRuler.EndMark("Draw");
#endif //debug
        }

        public static Vector2 PointToward(Vector2 start, Vector2 toward)
        {
            Vector2 Direction;
            Direction = new Vector2(toward.X - start.X, toward.Y - start.Y);
            return Direction;
        }

        public void ChangeRoom(Room toRoom, Vector2 toLocation)
        {
            //chnange room
            levelOne.ActiveRoom = toRoom;
            veiwPortOffSet = player.screenPosition - toLocation;
            player.Position = toLocation;
            player.inRoom = levelOne.ActiveRoom;

            foreach (Bullet bul in Bullets)
            {
                bul.Alive = false;
            }

            ui.ChangedRoom();

            if (player.Pet != null) //move pet too
            {
                player.Pet.Position = player.Position + new Vector2(1, 0);
                player.Pet.inRoom = levelOne.ActiveRoom;
            }
        }

        private void GameOver()
        {
            this.IsMouseVisible = true;
            spriteBatch.Begin();
            spriteBatch.Draw(Content.Load<Texture2D>("DarkBlend"), new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            Texture2D gameOver = Content.Load<Texture2D>("GameOver");
            Texture2D restart = Content.Load<Texture2D>("Restart");
            Color restartColor = new Color(150, 150, 150);
            Rectangle restartRec = new Rectangle(GraphicsDevice.Viewport.Width / 2 - restart.Width / 2, GraphicsDevice.Viewport.Height / 2 + gameOver.Height / 2, restart.Width, restart.Height);
            if (restartRec.Intersects(new Rectangle(MouseCurrentState.X, MouseCurrentState.Y, 1, 1)) == true)
            {
                restartColor = Color.White;
                if (MouseCurrentState.LeftButton == ButtonState.Pressed || KeyboardCurrentState.IsKeyDown(Keys.R))
                {
                    ResetGame();
                    isGameOver = false;
                    return;
                }
            }
            if (KeyboardCurrentState.IsKeyDown(Keys.R))
            {
                ResetGame();
                isGameOver = false;
                return;
            }

            spriteBatch.Draw(gameOver, new Vector2(GraphicsDevice.Viewport.Width/2 - gameOver.Width/2, GraphicsDevice.Viewport.Height/2 - gameOver.Height/2),Color.White);
            spriteBatch.Draw(restart, restartRec, restartColor);
            isGameOver = true;
            spriteBatch.End();
        }

        private void ResetGame()
        {
            this.LoadContent();
        }

        public static int roundUp(float num)
        {
            if (num > 0)
                return ((int)num + 1);
            if (num < 0)
                return ((int)num - 1);
            return 0;
        }

        public static bool CheckIfOnscreen(Vector2 pos, int width, int height, Rectangle screen)
        {
            return
                !(pos.Y + height < screen.Top ||
                pos.Y > screen.Bottom ||
                pos.X > screen.Right ||
                pos.X + width < screen.Left);
        }
    }

    static class MyStateObjects
    {
        public static BlendState BlendSubtract = new BlendState()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,

            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.ReverseSubtract,
        };
    }
}
