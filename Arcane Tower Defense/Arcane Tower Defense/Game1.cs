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

namespace Arcane_Tower_Defense
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 

    //screen is 600 by 600 allowing for 15 x 15 for tiles

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        List<Vector2> path;
        bool[,] validSpaces;

        MouseState mouse;
        bool isClicking;

        Texture2D tile, pathTile, screen, selScreen, upgrScreen, towOutline, darkOut;
        Texture2D health, healthBar, hit;
        Texture2D fireTower, iceTower, earthTower, thunderTower;
        Texture2D towRng;
        Dictionary<String,Texture2D> towerImages;
        SpriteFont bigFont, infoFont;

        //wave holds the name of the next monsters, list is four in total so i guess an array mightve been more suitable
        List<String> nextWave;
        List<Monster> nextWaveMons;
        //holds all the monsters currently on the field
        List<Monster> monsters;
        List<Monster> monstersToSpawn;
        //timer for how long to wait before spawning a new monster
        int monTimer;

        List<Tower> towers;
        List<Shot> shots;

        class Hit
        {
            public AnimatedSprite hitTex;
            Vector2 loc;
            //monster to follow if applicable
            Monster mon;
            public double angle;

            public Hit(AnimatedSprite tex, Monster mo)
            {
                hitTex = tex;
                mon = mo;
                angle = RNG.RandInt(1, 360);
            }

            public Hit(AnimatedSprite tex, Vector2 lo)
            {
                hitTex = tex;
                loc = lo;
                angle = RNG.RandInt(1, 360);
            }

            public Vector2 GetLoc()
            {
                if (mon == null) return loc;
                else return mon.loc;
            }
        }

        List<Hit> hits;

        //the number of the current wave
        int curWaveNum;

        int lives;
        int gold;

        enum Phase
        {
            TowerSelect,
            TowerUpgrade
        }
        //string that holds the tower the player is currently placing
        //only relevant under TowerSelect
        String curTowerType;
        Tower curTower;
        Tower selected;
        

        Phase phase;

        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;
            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            base.Initialize();
            IsMouseVisible = true;
            path = new List<Vector2>();
            int x = 1, y = RNG.RandInt(2, 14);
            path.Add(new Vector2(x,y));
            x++;
            path.Add(new Vector2(x, y));
            //list of possible directions the path could go
            List<Vector2> directionList = new List<Vector2>();
            validSpaces = new bool[15,15];
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    validSpaces[i, j] = true;
                }
            }
            while (x != 15)
            {

                //random path development for a time
                while (path.Count <= 50)
                {
                    directionList.Clear();
                    if (IsValidPathTile(new Vector2(x - 1, y))) directionList.Add(new Vector2(x - 1, y));
                    if (IsValidPathTile(new Vector2(x + 1, y))) directionList.Add(new Vector2(x + 1, y));
                    if (IsValidPathTile(new Vector2(x, y - 1))) directionList.Add(new Vector2(x, y - 1));
                    if (IsValidPathTile(new Vector2(x, y + 1))) directionList.Add(new Vector2(x, y + 1));
                    if (IsValidPathTile(new Vector2(2*path[path.Count-1].X-path[path.Count-2].X, 2*path[path.Count-1].Y-path[path.Count-2].Y)))
                    {
                        //a<n is essentially a scale for the probability of straight lines
                        for (int a = 0; a<14; a++) directionList.Add(new Vector2(2 * path[path.Count - 1].X - path[path.Count - 2].X, 2 * path[path.Count - 1].Y - path[path.Count - 2].Y));
                    }
                    //if at dead end, reset entirely
                    if (directionList.Count == 0)
                    {
                        path.Clear();
                        x = 1;
                        y = RNG.RandInt(2, 14);
                        path.Add(new Vector2(x, y));
                        x++;
                        path.Add(new Vector2(x, y));
                        continue;
                    }
                    int i = RNG.RandInt(0, directionList.Count - 1);
                    x = (int)directionList[i].X;
                    y = (int)directionList[i].Y;
                    path.Add(new Vector2(x, y));
                }

                while (x != 15)
                {
                    if (x == 14)
                    {
                        x = 15;
                        path.Add(new Vector2(x, y));
                    }
                    else if (IsValidPathTile(new Vector2(x + 1, y)))
                    {
                        x++;
                        path.Add(new Vector2(x, y));
                    }
                    else
                    {
                        path.Clear();
                        x = 1;
                        y = RNG.RandInt(2, 14);
                        path.Add(new Vector2(x, y));
                        x++;
                        path.Add(new Vector2(x, y));
                        break;
                    }
                }
            }
            for (int i = 0; i < path.Count; i++)
            {
                validSpaces[(int) path[i].X - 1, (int) path[i].Y - 1] = false;
            }

            //final "tile" so that enemies know where to go
            path.Add(new Vector2(16, path[path.Count - 1].Y));

            //ignore the actual list for now
            nextWave = new List<String> { "None", "None", "None", "None" };
            nextWaveMons = new List<Monster> { null, null, null, null };
            curWaveNum = 0;
            monsters = new List<Monster>();
            monstersToSpawn = new List<Monster>();
            monTimer = 0;
            towers = new List<Tower>();
            shots = new List<Shot>();
            hits = new List<Hit>();

            gold = 1000;
            lives = 20;

            phase = Phase.TowerSelect;
            curTowerType = "None";

            towerImages = new Dictionary<String, Texture2D>();
            towerImages.Add("Fire", fireTower);
            towerImages.Add("Ice", iceTower);
            towerImages.Add("Earth", earthTower);
            towerImages.Add("Thunder", thunderTower);

            //overwrites the specific monsters listed before
            generateMonsters();

        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tile = Content.Load<Texture2D>("Tile");
            pathTile = Content.Load<Texture2D>("PathTile");
            screen = Content.Load<Texture2D>("RightScreen");
            selScreen = Content.Load<Texture2D>("TowerSelect");
            upgrScreen = Content.Load<Texture2D>("TowerUpgrade");

            towOutline = Content.Load<Texture2D>("TowerOutline");
            darkOut = Content.Load<Texture2D>("DarkOut");

            health = Content.Load<Texture2D>("Health");
            healthBar = Content.Load<Texture2D>("HealthBar");

            fireTower = Content.Load<Texture2D>("FireTower");
            iceTower = Content.Load<Texture2D>("IceTower");
            earthTower = Content.Load<Texture2D>("EarthTower");
            thunderTower = Content.Load<Texture2D>("ThunderTower");

            hit = Content.Load<Texture2D>("Hit");
            

            bigFont = Content.Load<SpriteFont>("SpriteFont1");
            infoFont = Content.Load<SpriteFont>("InfoFont");
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

            // TODO: Add your update logic here

            base.Update(gameTime);

            mouse = Mouse.GetState();
            
            
            if (mouse.MiddleButton == ButtonState.Pressed)
            {
                
                
            }
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                isClicking = true;
                Console.WriteLine(mouse.X + " " + mouse.Y);
            }
            if (mouse.RightButton == ButtonState.Pressed)
            {
                curTowerType = "None";
                phase = Phase.TowerSelect;
                selected = null;
            }
            if (isClicking && mouse.LeftButton == ButtonState.Released )
            {
                if (new Rectangle(620, 520, 780, 64).Contains(mouse.X, mouse.Y) && curTowerType.Equals("None"))
                {
                    generateMonsters();
                    curWaveNum++;
                }
                else
                {
                    
                    foreach (Tower tow in towers)
                    {
                        if (new Rectangle((int)tow.loc.X - 20, (int)tow.loc.Y - 20,40,40).Contains(mouse.X, mouse.Y))
                        {
                            curTowerType = "None";
                            selected = tow;
                            towRng = new Texture2D(graphics.GraphicsDevice, selected.curShot.ran.length, selected.curShot.ran.length);
                            towRng.SetData(selected.curShot.ran.img);
                            phase = Phase.TowerUpgrade;
                        }
                    }
                }
                switch (phase)
                {
                    case (Phase.TowerSelect):
                        {
                            try
                            {
                                if (curTowerType.Equals("None"))
                                {
                                    if (new Rectangle(640, 60, 40, 40).Contains(mouse.X, mouse.Y) && gold >= 25)
                                    {
                                        curTowerType = "Fire";
                                        curTower = new Tower("Fire");
                                    }
                                    else if (new Rectangle(720, 60, 40, 40).Contains(mouse.X, mouse.Y) && gold >= 25)
                                    {
                                        curTowerType = "Ice";
                                        curTower = new Tower("Ice");
                                    }
                                    else if (new Rectangle(640, 140, 40, 40).Contains(mouse.X, mouse.Y) && gold >= 25)
                                    {
                                        curTowerType = "Earth";
                                        curTower = new Tower("Earth");
                                    }
                                    else if (new Rectangle(720, 140, 40, 40).Contains(mouse.X, mouse.Y) && gold >= 25)
                                    {
                                        curTowerType = "Thunder";
                                        curTower = new Tower("Thunder");
                                    }
                                    if (!(curTowerType.Equals("None")))
                                    {
                                        towRng = new Texture2D(graphics.GraphicsDevice, curTower.curShot.ran.length, curTower.curShot.ran.length);
                                        towRng.SetData(curTower.curShot.ran.img);
                                    }
                                }
                                else if (validSpaces[(int)(mouse.X / 40.0), (int)(mouse.Y / 40.0)])
                                {
                                    
                                    validSpaces[(int)(mouse.X / 40.0), (int)(mouse.Y / 40.0)] = false;
                                    curTower.setLoc(new Vector2((int)(mouse.X / 40.0) * 40 + 20, (int)(mouse.Y / 40.0) * 40 + 20));

                                    if (curTower.Equals(selected))
                                    {
                                        gold -= (int)(curTower.cost / 4.0);
                                        curTower = null;
                                        curTowerType = "None";
                                        phase = Phase.TowerUpgrade;
                                    }
                                    else
                                    {
                                        gold -= curTower.cost;
                                        towers.Add(curTower);
                                        if (gold >= curTower.cost) curTower = new Tower(curTower.type);
                                        else curTowerType = "None";
                                        
                                    }
                                    
                                }
                            }
                            catch
                            {
                                Console.WriteLine("Error 1");
                            }
                        }
                        break;
                    case (Phase.TowerUpgrade):
                        {
                            if (new Rectangle(625, 136, 148, 32).Contains(mouse.X, mouse.Y))
                            {
                                if (gold >= selected.upgrCost)
                                {
                                    gold -= selected.upgrCost;
                                    selected.Upgrade();
                                    towRng = new Texture2D(graphics.GraphicsDevice, selected.curShot.ran.length, selected.curShot.ran.length);
                                    towRng.SetData(selected.curShot.ran.img);
                                }
                            }
                            else if (new Rectangle(627, 180, 65, 27).Contains(mouse.X, mouse.Y))
                            {
                                gold += (int)(selected.cost / 2.0);
                                validSpaces[(int)(selected.loc.X / 40.0), (int)(selected.loc.Y / 40.0)] = true;
                                towers.Remove(selected);
                                selected = null;
                                curTower = null;
                                phase = Phase.TowerSelect;
                            }
                            else if (new Rectangle(705, 180, 65, 27).Contains(mouse.X, mouse.Y) && gold >= (int)(selected.cost/4.0))
                            {

                                curTower = selected;
                                curTowerType = selected.type;
                                validSpaces[(int)(selected.loc.X / 40.0), (int)(selected.loc.Y / 40.0)] = true;
                                phase = Phase.TowerSelect;
                            }
                            else if (new Rectangle(686, 34, 40, 40).Contains(mouse.X, mouse.Y))
                            {
                                selected.SetShot(1);
                                towRng = new Texture2D(graphics.GraphicsDevice, selected.curShot.ran.length, selected.curShot.ran.length);
                                towRng.SetData(selected.curShot.ran.img);
                            }
                            else if (new Rectangle(741, 34, 40, 40).Contains(mouse.X, mouse.Y) && !(selected.towShot2.name == null))
                            {
                                selected.SetShot(2);
                                towRng = new Texture2D(graphics.GraphicsDevice, selected.curShot.ran.length, selected.curShot.ran.length);
                                towRng.SetData(selected.curShot.ran.img);
                            }
                        }
                        break;
                }
            }

            monsters.Sort();

            foreach (Tower tow in towers)
            {
                if (tow.delay > 0) tow.delay--;
                if (tow.delay2 > 0) tow.delay2--;
                if (tow.curShot.name.Equals("Flamethrower") && tow.curShot.param[1] != null && tow.delay2 == 0)
                {
                    Shot shot = new Shot((Shot)tow.curShot.param[1]);
                    shot.shotTex.setTexture(Content.Load<Texture2D>(shot.shotTex.textureString));
                    shot.hitTex.setTexture(Content.Load<Texture2D>(shot.hitTex.textureString));
                    shots.Add(shot);
                    tow.delay2 = tow.curShot.cooldown2;
                    tow.count++;
                    if (tow.count >= (int)tow.curShot.param[0])
                    {
                        tow.count = 0;
                        tow.curShot.param[1] = null;
                        tow.delay = tow.curShot.cooldown;
                    }
                }
                else if (tow.delay == 0)
                {
                    foreach (Monster mon in monsters)
                    {
                        if (tow.RngContains(mon.loc))
                        {
                            
                            Shot shot = new Shot(tow, tow.loc, mon.loc);
                            shot.loc = tow.loc;
                            

                            
                            switch (tow.type)
                            {
                                case ("Ice"):
                                    {
                                        Vector2 monDir = new Vector2(0, 0);
                                        try
                                        {
                                            //check if monster will be outside tower range when it tries to shoot
                                            Vector2 temp = new Vector2((path[(int)((mon.distTraveled + 20 + ((mon.loc - tow.loc).Length() / (float)shot.spd) * mon.spd) / 40.0)].X - 1) * 40 + 20, (path[(int)((mon.distTraveled + 20 + ((mon.loc - tow.loc).Length() / (float)shot.spd) * mon.spd) / 40.0)].Y - 1) * 40 + 20);
                                            if ((temp - tow.loc).Length() > tow.towShot1.shotLife)
                                                continue;
                                            else monDir = temp - mon.loc;
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Outside tower range");
                                            continue;
                                        }

                                        monDir.Normalize();
                                        shot.target = (mon.loc + (monDir * ((mon.loc - tow.loc).Length() / (float)shot.spd) * mon.spd) - tow.loc);
                                        shot.target.Normalize();
                                        shot.target *= 610;
                                        shot.target += tow.loc;
                                    }
                                    break;
                                case("Fire"):
                                    {
                                        switch (tow.curShotNum)
                                        {
                                            case (1):
                                                {
                                                    
                                                }
                                                break;
                                            case (2):
                                                {
                                                    
                                                    tow.curShot.param[1] = shot;
                                                    
                                                }
                                                break;
                                        }
                                        int ran = RNG.RandInt(-3, 3);
                                        if (shot.direction.Y > .50)
                                        {
                                            shot.loc = new Vector2(tow.loc.X + ran, tow.loc.Y);
                                            shot.target = new Vector2(tow.loc.X + ran, 640);
                                        }
                                        else if (shot.direction.Y < -.50)
                                        {
                                            shot.loc = new Vector2(tow.loc.X + ran, tow.loc.Y);
                                            shot.target = new Vector2(tow.loc.X + ran, -40);
                                        }
                                        else if (shot.direction.X > .50)
                                        {
                                            shot.loc = new Vector2(tow.loc.X, tow.loc.Y + ran);
                                            shot.target = new Vector2(640, tow.loc.Y + ran);
                                        }
                                        else if (shot.direction.X < -.50)
                                        {
                                            shot.loc = new Vector2(tow.loc.X, tow.loc.Y + ran);
                                            shot.target = new Vector2(-40, tow.loc.Y + ran);
                                        }
                                    }
                                    break;
                                case ("Earth"):
                                    {
                                        //earth bullet wont dissipate upon hitting a "target"
                                        shot.target = new Vector2(-40, -40);
                                        //nothing?
                                    }
                                    break;
                                case ("Thunder"):
                                    {
                                        switch (tow.curShotNum)
                                        {
                                            case (1):
                                                {
                                                    shot.loc = mon.loc - new Vector2(0, 5);

                                                    
                                                }
                                                break;
                                            case (2):
                                                {

                                                }
                                                break;
                                                
                                        }
                                        shot.SetMonTarget(mon);
                                    }
                                    break;
                            }
                            shot.shotTex.setTexture(Content.Load<Texture2D>(shot.shotTex.textureString));
                            shot.hitTex.setTexture(Content.Load<Texture2D>(shot.hitTex.textureString));
                            shots.Add(shot);
                            tow.delay = tow.curShot.cooldown;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < shots.Count; i++)
            {
                //if (!(shots[i].hasHit))
                //{
                    shots[i].Update();
                    if ((shots[i].loc - shots[i].target).Length() <= 6)
                    {
                       
                            hits.Add(new Hit(shots[i].hitTex, shots[i].loc));
                            shots.RemoveAt(i);
                            i--;
                        
                        continue;
                    }
                    if (shots[i].distTraveled >= shots[i].lifetime)
                    {
                        shots.RemoveAt(i);
                        i--;
                        continue;
                    }
                    for(int j = 0; j<monsters.Count;j++)
                    {
                        Monster mon = monsters[j];
                        
                        if ((mon.loc - shots[i].loc).Length() <= shots[i].range && !(shots[i].hitMons.Contains(mon)))
                        {
                            if (shots[i].dmg - mon.def > 0) mon.HP -= (shots[i].dmg-mon.def);
                            if (mon.HP <= 0)
                            {
                                gold += mon.gold;
                                monsters.Remove(mon);
                                j--;
                            }
                            shots[i].hits--;
                            shots[i].hitMons.Add(mon);
                            if (shots[i].hits <= 0)
                            {
                                hits.Add(new Hit(shots[i].hitTex, mon));
                                shots.RemoveAt(i);
                                i--;
                                break;
                            }
                            else
                            {
                                AnimatedSprite temp = new AnimatedSprite("hit", 1, 4, 4);
                                temp.setTexture(hit);
                                temp.setColor(shots[i].hitColor);
                                Hit h = new Hit(temp, mon);
                                hits.Add(h);
                            }
                        }
                        
                        
                    }

                    
                //}
            }

            for (int i = 0; i<monsters.Count;i++)
            {
                Monster mon = monsters[i];
                try
                {
                    mon.MoveToward(new Vector2((path[(int)((mon.distTraveled + 20) / 40.0)].X - 1) * 40 + 20, (path[(int)((mon.distTraveled + 20) / 40.0)].Y - 1) * 40 + 20));
                    

                }
                catch
                {
                    lives--;
                    monsters.Remove(mon);
                    i--;
                }
                
            }

            if (monstersToSpawn.Count != 0 && monTimer <= 0)
            {
                if (monstersToSpawn.Count != 1) monTimer = monstersToSpawn[0].spawnSpeed;
                monsters.Add(monstersToSpawn[0]);
                monstersToSpawn.RemoveAt(0);
                //monsters[monsters.Count - 1].drawLoc = new Vector2(path[0].X * 40, path[0].Y * 40);
                monsters[monsters.Count - 1].loc = new Vector2(0, (path[0].Y-1) * 40 + 20);
                monsters[monsters.Count - 1].sprite.setTexture(Content.Load<Texture2D>(monsters[monsters.Count - 1].sprite.textureString));

            }
            if (monTimer > 0) monTimer--;
            if (mouse.LeftButton != ButtonState.Pressed) isClicking = false;
        }









        //DRAWING BEGINS HERE




        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
            spriteBatch.Begin();
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (path.Contains(new Vector2(i+1,j+1)))
                    {
                        spriteBatch.Draw(pathTile, new Vector2(i * 40, j * 40), Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw(tile, new Vector2(i * 40, j * 40),Color.White);
                    }
                }
            }

            

            foreach (Tower tow in towers)
            {
                spriteBatch.Draw(towerImages[tow.type], new Vector2(tow.loc.X - 20, tow.loc.Y - 20), Color.White);
            }

            spriteBatch.End();

            foreach (Monster mon in monsters)
            {
                mon.sprite.Draw(spriteBatch,new Vector2(mon.loc.X-20,mon.loc.Y-20));
                spriteBatch.Begin();
                spriteBatch.Draw(healthBar, new Vector2(mon.loc.X - 10, mon.loc.Y + 15), Color.White);
                spriteBatch.Draw(health, new Vector2(mon.loc.X - 10, mon.loc.Y + 15), new Rectangle(0,0,(int)(health.Width*mon.HP/(double)mon.maxHP),5),Color.White);
                spriteBatch.End();
            }

            for (int j = 0; j<shots.Count;j++)
            {
                Shot shot = shots[j];
                if ((shot.lifetime - shot.distTraveled) / ((double)(shot.spd) / 20.0) < 12) shot.shotTex.setAlpha((float)((shot.lifetime - shot.distTraveled) / ((double)(shot.spd) / 20.0) / 12.0));
                //if (shot.distTraveled / (float)shot.lifetime > .9) shot.shotTex.setAlpha((float)((1 - shot.distTraveled / (float)shot.lifetime) / .10));

                if (shot.shotTow.type.Equals("Earth"))
                {
                    shot.shotTex.Draw(spriteBatch, shot.loc, (int)(shot.range * 2), (int)(shot.range * 2));
                }
                else if (shot.name.Equals("Flamethrower"))
                {
                    shot.shotTex.Draw(spriteBatch,shot.loc,(float)(RNG.RandInt(1,360)/360.0*Math.PI*2));
                }
                else if (shot.name.Equals("Static Tracer"))
                {
                    spriteBatch.Begin();
                    int temp = 1 + (int)(shot.distTraveled)%100;
                    if (temp > 50) temp = 100 - temp;
                    float alpha = (float)(temp / 100.0);
                    spriteBatch.Draw(Content.Load<Texture2D>("ThunderShot2Target"), shot.monTarget.loc - new Vector2(20,20), Color.White * alpha);
                    spriteBatch.End();
                }
                {
                    double angle;

                    if (shot.direction.X == 0 && Math.Abs(shot.direction.Y + 1) <= .002) angle = Math.PI;
                    else angle = (float)Math.Acos(shot.direction.Y) * Math.Sign(shot.direction.X);
                    shot.shotTex.Draw(spriteBatch, shot.loc, (float)(Math.PI - angle));
                }
            }

            for (int j = 0; j < hits.Count; j++)
            {
                if (hits[j].hitTex.textureString.Equals("ThunderHit1")) hits[j].hitTex.Draw(spriteBatch, hits[j].GetLoc(), 0);
                else hits[j].hitTex.Draw(spriteBatch,hits[j].GetLoc(),(float)(hits[j].angle/360.0*Math.PI*2));
                if (hits[j].hitTex.currentFrame == hits[j].hitTex.totalFrames - 1)
                {
                    hits.RemoveAt(j);
                    j--;
                }
            }

            spriteBatch.Begin();
            spriteBatch.Draw(screen, new Vector2(600, 0), Color.White);
            spriteBatch.DrawString(bigFont, "" + gold, new Vector2(630, 6), Color.Black);
            spriteBatch.DrawString(bigFont, "" + lives, new Vector2(733, 6), Color.Black);
            switch (phase)
            {
                case (Phase.TowerSelect):
                    {
                        spriteBatch.Draw(selScreen, new Vector2(600, 0), Color.White);
                        if (!(curTowerType.Equals("None")))
                        {
                            mouse = Mouse.GetState();
                            try
                            {


                                if (mouse.X >= 600)
                                {
                                    spriteBatch.Draw(towerImages[curTowerType], new Vector2(mouse.X - 20, mouse.Y - 20), Color.White);
                                    spriteBatch.Draw(towRng, new Vector2((float)(mouse.X - curTower.curShot.ran.length / 2.0), (float)(mouse.Y - curTower.curShot.ran.length / 2.0)), Color.White);
                                }
                                else if (validSpaces[(int)(mouse.X / 40.0), (int)(mouse.Y / 40.0)] == true)
                                {
                                    spriteBatch.Draw(towerImages[curTowerType], new Vector2((int)(mouse.X / 40.0) * 40, (int)(mouse.Y / 40.0) * 40), Color.White);
                                    spriteBatch.Draw(towRng, new Vector2((float)((int)(mouse.X / 40.0) * 40 - curTower.curShot.ran.length / 2.0 + 20), (float)((int)(mouse.Y / 40.0) * 40 - curTower.curShot.ran.length / 2.0 + 20)), Color.White);

                                }

                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.WriteLine("Out of bounds");
                            }

                        }
                        else
                        {
                            if (new Rectangle(640, 60, 40, 40).Contains(mouse.X, mouse.Y))
                            {
                                ShowInfo("Cost: 25 gold\n\nA tower with fairly high damage. Its main asset is its shots' piercing ability which let it go through multiple enemies at once, though the exact number it can go through is determined by upgrades. Can only hit in four directions.");
                            }
                            else if (new Rectangle(720, 60, 40, 40).Contains(mouse.X, mouse.Y))
                            {
                                ShowInfo("Cost: 25 gold\n\nA tower with variability. Its first shot is very fast but does low damage per shot, so it fails against enemies with high defense. Its second shot has slowing and freezing effects, making it a good support.");
                            }
                            else if (new Rectangle(640, 140, 40, 40).Contains(mouse.X, mouse.Y))
                            {
                                ShowInfo("Cost: 25 gold \n\n A tower specializing in hitting multiple enemies. Range is generally low and cooldown and is fairly average, but it is incredibly effective against groups of slimes");
                            }
                            else if (new Rectangle(720, 140, 40, 40).Contains(mouse.X, mouse.Y))
                            {
                                ShowInfo("Cost: 25 gold \n\n A tower with huge range. The tower's large range makes placement easy, although there is a middle area which it cannot hit. Damage is incredibly high but offset by long cooldowns. Upgrading will add various supporting effects.");
                            }
                        }

                    }
                    break;
                case (Phase.TowerUpgrade):
                    {
                        
                        
                        //spriteBatch.Draw(towOutline, new Vector2(selected.loc.X - 21, selected.loc.Y - 21), Color.White);
                        spriteBatch.Draw(towRng, selected.loc-new Vector2((float)(selected.curShot.ran.length/2.0),(float)(selected.curShot.ran.length/2.0)), Color.White);
                        spriteBatch.Draw(screen, new Vector2(600, 0), Color.White);
                        spriteBatch.DrawString(bigFont, "" + gold, new Vector2(630, 6), Color.Black);
                        spriteBatch.DrawString(bigFont, "" + lives, new Vector2(733, 6), Color.Black);
                        spriteBatch.Draw(upgrScreen, new Vector2(600, 0), Color.White);
                        try
                        {
                            spriteBatch.Draw(Content.Load<Texture2D>(selected.type + "Shot" + 1 + "Portrait"), new Vector2(686, 34), Color.White);
                            spriteBatch.Draw(Content.Load<Texture2D>(selected.type + "Shot" + 2 + "Portrait"), new Vector2(741, 34), Color.White);
                        }
                        catch (Exception e)
                        {
                        }
                        if (selected.towShot2.name == null) spriteBatch.Draw(darkOut, new Vector2(741, 34), Color.White);
                        spriteBatch.Draw(towOutline, new Vector2(685 + 55 * (selected.curShotNum - 1), 33), Color.White);
                        mouse = Mouse.GetState();
                        if (new Rectangle(625, 136, 148, 32).Contains(mouse.X, mouse.Y))
                        {
                            ShowInfo(selected.upgrInfo);
                        }
                        else if (new Rectangle(627, 180, 65, 27).Contains(mouse.X, mouse.Y))
                        {
                            ShowInfo("Sell for " + (int)(selected.cost / 2.0) + " gold?");
                        }
                        else if (new Rectangle(705, 180, 65, 27).Contains(mouse.X, mouse.Y))
                        {
                            ShowInfo("Move for " + (int)(selected.cost / 4.0) + " gold?");
                        }
                        else
                        {
                            ShowInfo("Value: " + selected.cost + "\nCost of Next Upgrade: " + selected.upgrCost + "\n\nRange: " + Math.Round((selected.curShot.ran.length - 42) / 80.0, 1) + "\nCooldown: " + selected.curShot.cooldown + "\n\nShot: " + selected.curShot.name + "\nDmg: " + selected.curShot.shotDmg + "\nHits: " + selected.curShot.shotHits);
                        }
                    }
                    break;

            }

            spriteBatch.End();

            foreach (Monster mon in nextWaveMons)
            {
                if (mon != null)
                mon.sprite.Draw(spriteBatch, new Vector2(mon.loc.X , mon.loc.Y));
                
            }
        }

        protected void ShowInfo(String text)
        {
            float strLength = 0;
            int strIndex=0;
            String word;
            for(int i = 0; i < text.Length;i++)
            {
                word = "";
                strIndex = i;
                while (!(text.Substring(i, 1).Equals("\n") || text.Substring(i, 1).Equals(" ")))
                {
                    word += text.Substring(i, 1);
                    if (i + 1 >= text.Length) break;
                    i++;
                }
                if (text.Substring(i, 1).Equals(" "))
                {
                    strLength += infoFont.MeasureString(" ").X;
                }
                if (text.Substring(i, 1).Equals("\n"))
                {
                    i++;
                    strLength = 0;
                }
                else
                {
                    if (strLength + infoFont.MeasureString(word).X > 130)
                    {
                        text = text.Substring(0, strIndex) + "\n" + text.Substring(strIndex);
                        strLength = infoFont.MeasureString(word).X;
                        i += 2;
                    }
                    else strLength += infoFont.MeasureString(word).X;
                } 
            }
            spriteBatch.DrawString(infoFont, text, new Vector2(628, 227), Color.Black);
        }
        //checks if a "Tile" is valid for being next in the path
        protected bool IsValidPathTile(Vector2 vec)
        {

            if (vec.X == 1 || vec.X == 15 || vec.Y == 1 || vec.Y == 15) return false;
            List<Vector2> pa = new List<Vector2>(path);
            pa.RemoveAt(pa.Count - 1);
            pa.RemoveAt(pa.Count - 1);
            if (pa.Contains(new Vector2(vec.X - 1, vec.Y)) || pa.Contains(new Vector2(vec.X + 1, vec.Y)) || pa.Contains(new Vector2(vec.X, vec.Y - 1)) || pa.Contains(new Vector2(vec.X, vec.Y + 1)) || pa.Contains(new Vector2(vec.X - 1, vec.Y + 1)) || pa.Contains(new Vector2(vec.X + 1, vec.Y - 1)) || pa.Contains(new Vector2(vec.X - 1, vec.Y - 1)) || pa.Contains(new Vector2(vec.X+1, vec.Y + 1))) return false;
            
            else return true;
        }

        //uses next wave, adds to monstersToSpawn
        protected void generateMonsters()
        {
            
            List<Monster> tempMons = new List<Monster>();
            int j = 0;
            for (int i = 0; i < nextWave.Count; i++)
            {

                switch (nextWave[i])
                {
                    case ("Slime"):
                        {
                            j  = RNG.RandInt(4,5);
                           
                        }
                        break;
                    case ("Yellow Slime"):
                        {
                            j = RNG.RandInt(5, 6);

                        }
                        break;
                    case ("Red Slime"):
                        {
                            j = RNG.RandInt(5, 6);

                        }
                        break;
                    case ("Heart Slime"):
                        {
                            j = RNG.RandInt(3, 4);

                        }
                        break;
                    case ("Steel Slime"):
                        {
                            j = RNG.RandInt(3, 4);

                        }
                        break;
                    case ("Group Slime"):
                        {
                            j = 3;
                        }
                        break;
                    case ("None"):
                        {
                            j = 0;
                        }
                        break;
                }
                for (int a = 0; a < j; a++)
                {
                    //adds the monster with level equal to the current waves number
                    tempMons.Add(new Monster(nextWave[i],curWaveNum));
                }
                
            }
            while (tempMons.Count != 0)
            {
                j = RNG.RandInt(0, tempMons.Count - 1);
                monstersToSpawn.Add(tempMons[j]);
                if (tempMons[j].name.Equals("Group Slime"))
                {
                    monstersToSpawn.Add(new Monster("Group Slime",curWaveNum));
                    monstersToSpawn.Add(new Monster("Group Slime", curWaveNum));
                    monstersToSpawn.Add(new Monster("Group Slime", curWaveNum));
                    monstersToSpawn.Add(new Monster("Group Slime", curWaveNum));
                }
                tempMons.RemoveAt(j);
            }
            
            List<int> randInts = new List<int> { RNG.RandInt(1, 20), RNG.RandInt(1, 20), RNG.RandInt(1, 20) };
            for (int i = 0; i < 3; i++)
            {
                if (randInts[i] < 5)
                {
                    nextWave[i] = "Slime";
                }
                else if (randInts[i] < 8)
                {
                    nextWave[i] = "Yellow Slime";
                }
                else if (randInts[i] < 11)
                {
                    nextWave[i] = "Red Slime";
                }
                else if (randInts[i] < 14)
                {
                    nextWave[i] = "Heart Slime";
                }
                else if (randInts[i] < 17)
                {
                    nextWave[i] = "Steel Slime";
                }
                else
                {
                    nextWave[i] = "Group Slime";
                }
            }
            for (int i = 0; i < 3; i++)
            {
                if (!(nextWave[i].Equals("None")))
                {
                    nextWaveMons[i] = new Monster(nextWave[i], curWaveNum);
                    nextWaveMons[i].sprite.setTexture(Content.Load<Texture2D>(nextWaveMons[i].sprite.textureString));
                    nextWaveMons[i].loc = new Vector2(630 + (i * 30), 470);
                }
            }

            
        }
    }
}
