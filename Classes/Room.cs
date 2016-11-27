using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Xml;
using Microsoft.Xna.Framework.Content;

namespace KSshooter.Classes
{
    /// <summary>
    /// structure used to store room exits
    /// </summary>
    public class RoomExit : HitableObject
    {
        public RoomExit()
        {
            hit = true;
            hitRec = new Rectangle(0, 0, 30, 30);
        }
        public Room toRoom; //the exit leads to this room
        public string toRoomName;
        public Vector2 toLocation; //where the exit leads to
        Vector2 _location; //location of the exit

        public Vector2 location
        {
            get { return _location; }
            set 
            {
                hitRec.X = (int)value.X;
                hitRec.Y = (int)value.Y;
                _location = value;
            }
        }
    }

    public class Room
    {
        const int TileWidth = 30;
        const int TileHeight = 30;

        string name;
        Level _level;
        Vector2 location; //location within the level
        Vector2 roomDimensions;
        public Tile[,] tiles;
        public List<HitableObject> tilesWithHit;
        List<RoomExit> exits; //exits from this room
        List<Enemy> enemies;

        //Astar pathfinding for room
        AStarPathfinding aStarPathfinding;

        //Bullets
        List<Bullet> playerBullets;
        List<Bullet> enemyBullets;
        List<Bullet> deadBullets;

        //Constructor
        public Room(Level alevel)
        {
            _level = alevel;
            Initialize();
        }

        public Room(XmlNode roomElement, Level alevel, ContentManager content)
        {
            _level = alevel;
            Initialize();

            name = roomElement.SelectSingleNode("name").InnerText;
            location = new Vector2((float)Convert.ToDouble(roomElement.SelectSingleNode("x").InnerText), (float)Convert.ToDouble(roomElement.SelectSingleNode("y").InnerText));
            int rows, cols;
            XmlNode nodesElement = roomElement.SelectSingleNode("nodelist");
            XmlNodeList nodes = nodesElement.SelectNodes("node");
            rows = Convert.ToInt32(nodes[nodes.Count - 1].SelectSingleNode("x").InnerText) + 1;
            cols = Convert.ToInt32(nodes[nodes.Count - 1].SelectSingleNode("y").InnerText) + 1;
            tiles = new Tile[rows / TileWidth + 1, cols / TileHeight + 1];
            roomDimensions = new Vector2(rows / TileWidth, cols / TileWidth);
        //Load nodes
            int x, y;
            string file;
            string hitd;
            Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
            tilesWithHit = new List<HitableObject>();
            exits = new List<RoomExit>();
            foreach (XmlNode node in nodes)
            {
                x = Convert.ToInt32(node.SelectSingleNode("x").InnerText);
                y = Convert.ToInt32(node.SelectSingleNode("y").InnerText);
                file = node.SelectSingleNode("floortexture").InnerText;
                hitd = node.SelectSingleNode("hit").InnerText;

                string contentName = FileNameFromPath.GetFileNameFromPath(file);
                if (textures.ContainsKey(contentName) == false)
                {
                    //create and add
                    Texture2D tex = content.Load<Texture2D>(contentName);
                    textures.Add(contentName, tex);
                }
                Tile tilebox = new Tile(textures[contentName]);
                //add in hit
                tilebox.HitChanged += new HitableObject.HitChangedEventHandler(tilebox_HitChanged);
                tilebox.hit = Convert.ToBoolean(hitd);
                Vector2 loc = new Vector2(x, y);
                tilebox.Location = loc;
                tiles[x / TileWidth, y / TileHeight] = tilebox;

                //overlay
                XmlNode overlay = node.SelectSingleNode("overlaytexture");
                if(overlay != null)
                {
                    contentName = FileNameFromPath.GetFileNameFromPath(overlay.InnerText);
                    if (textures.ContainsKey(contentName) == false)
                    {
                        //create and add
                        Texture2D tex = content.Load<Texture2D>(contentName);
                        textures.Add(contentName, tex);
                    }
                    tilebox.Overlay = textures[contentName];
                }
                //enemies
                XmlNode enemy = node.SelectSingleNode("enemy");
                if (enemy != null)
                {
                    Enemy e;
                    contentName = FileNameFromPath.GetFileNameFromPath(enemy.SelectSingleNode("texture").InnerText);
                    if (textures.ContainsKey(contentName) == false)
                    {
                        //create and add
                        Texture2D tex = content.Load<Texture2D>(contentName);
                        textures.Add(contentName, tex);
                    }
                    switch (enemy.SelectSingleNode("type").InnerText)
                    {
                        default:
                            e = new Enemy(textures[contentName], 10, 1, 5, _level.player, this, Enemy.AItype.AstarPathfindingMelee ,500);
                            e.Position = loc;
                            enemies.Add(e);
                            break;
                    }
                    XmlNode idlepath = enemy.SelectSingleNode("path");
                    if (idlepath != null)
                    {
                        XmlNodeList pathX = idlepath.SelectNodes("x");
                        XmlNodeList pathY = idlepath.SelectNodes("y");
                        for (int i = 0; i < pathX.Count; i++)
                        {
                            e.addIdlePathWaypoint(new Vector2(Convert.ToInt32(pathX[i].InnerText), Convert.ToInt32(pathY[i].InnerText)));
                        }
                    }
                }
                //room exits
                XmlNode roomExitElement = node.SelectSingleNode("roomexit");
                if (roomExitElement != null)
                {
                    RoomExit roomExit = new RoomExit();
                    roomExit.location = loc;
                    roomExit.toRoomName = roomExitElement.SelectSingleNode("toroom").InnerText;
                    XmlNode toLocationElement = roomExitElement.SelectSingleNode("tolocation");
                    float exitx = Convert.ToInt32(toLocationElement.SelectSingleNode("x").InnerText);
                    float exity = Convert.ToInt32(toLocationElement.SelectSingleNode("y").InnerText);
                    roomExit.toLocation = new Vector2(exitx, exity);
                    exits.Add(roomExit);
                }
            }
            aStarPathfinding = new AStarPathfinding(tiles);
        }

        private void Initialize()
        {
            location = new Vector2();
            roomDimensions = new Vector2();
            level.addRoomToLevel(this);
            tilesWithHit = new List<HitableObject>();
            enemies = new List<Enemy>();
            playerBullets = new List<Bullet>(15);
            enemyBullets = new List<Bullet>(15);
            deadBullets = new List<Bullet>(10);
        }

        //Properties
        public Tile[,] Tiles
        {
            get
            {
               return tiles;
            }
        }

        public AStarPathfinding ASTARpathfinding
        {
            get { return aStarPathfinding; }
        }

        public string Name
        {
            get { return name; }
        }

        public Level level
        {
            get { return _level; }
        }

        public List<Enemy> Enemies
        {
            get { return enemies; }
        }

        public List<RoomExit> Exits
        {
            get { return exits; }
        }

        public List<Bullet> EnemyBullets
        {
            get { return enemyBullets; }
        }


        //Methods
        public Room AddEnemyToRoom(Enemy enemy)
        {
            enemies.Add(enemy);
            return this;
        }

        public void AddEnemyBullet(Bullet aBullet)
        {
            enemyBullets.Add(aBullet);
            aBullet.ObjectDeath += new Bullet.ObjectDeathEventHandler(aBullet_ObjectDeath);
        }

        void aBullet_ObjectDeath(object Sender, ObjectDeathEventArgs e)
        {
            deadBullets.Add(Sender as Bullet);
        }

        public void RemoveEnemy(Enemy en)
        {
            enemies.Remove(en);
        }

        public void Update(GameTime gameTime)
        {
            foreach(Bullet bul in enemyBullets)
            {
                bul.Update(gameTime);
                if (bul.HitDetection(level.player.HitRectangle))
                    bul.AttackTarget(level.player);
            }
            foreach (Bullet bul in deadBullets)
            {
                enemyBullets.Remove(bul);
            }
            deadBullets.Clear();
            foreach(Bullet bul in playerBullets)
            {
                bul.Update(gameTime);
            }
        }

        public void LoadRoomFromXML(string filename, ContentManager content)
        {
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(filename);

                XmlNodeList RoomNodes = xml.GetElementsByTagName("LevelInfo");

                //Convert THEN cast :P
                location.X = (float)Convert.ToInt32(RoomNodes[0].SelectSingleNode("locationx").InnerText);
                location.Y = (float)Convert.ToInt32(RoomNodes[0].SelectSingleNode("locationy").InnerText);

                XmlNodeList TileNodes = xml.GetElementsByTagName("Tile");
                //create array
                int rows = Convert.ToInt32(TileNodes[TileNodes.Count - 1].SelectSingleNode("x").InnerText) + 1;
                int cols = Convert.ToInt32(TileNodes[TileNodes.Count - 1].SelectSingleNode("y").InnerText) + 1;
                tiles = new Tile[rows,cols];
                roomDimensions.X = rows;
                roomDimensions.Y = cols;

                //get tile data out of file
                int x, y;
                string file;
                string hitd;
                Dictionary<string, Texture2D> textures = new Dictionary<string,Texture2D>();
                int counter = 0;
                foreach (XmlNode node in TileNodes)
                {
                    x = Convert.ToInt32(node.SelectSingleNode("x").InnerText);
                    y = Convert.ToInt32(node.SelectSingleNode("y").InnerText);
                    file = node.SelectSingleNode("filename").InnerText;
                    hitd = node.SelectSingleNode("hit").InnerText;

                    string contentName = FileNameFromPath.GetFileNameFromPath(file);
                    if (textures.ContainsKey(contentName) == false)
                    {
                        //create and add
                        Texture2D tex = content.Load<Texture2D>(contentName);
                        textures.Add(contentName, tex);
                    }

                    Tile tilebox = new Tile(textures[contentName]);
                    tilebox.HitChanged += new HitableObject.HitChangedEventHandler(tilebox_HitChanged);
                    tilebox.hit = Convert.ToBoolean(hitd);
                    Vector2 loc = new Vector2(x * TileWidth, y * TileHeight);
                    tilebox.Location = loc;
                    tiles[x,y] = tilebox;
                    counter++;
                }
            }
            catch (LoadRoomException e)
            {
                throw new LoadRoomException();
            }
        }

        void tilebox_HitChanged(HitableObject sender, HitChangedEventArgs e)
        {
            //this event is only triggered if the previous hit is different from current
            if (sender.hit == false)//remove
            {
                tilesWithHit.Remove(sender);
            }
            else if (sender.hit == true)
            {
                tilesWithHit.Add(sender);
            }
        }
    }

    class LoadRoomException : Exception
    {
        public LoadRoomException()
            : base("Invalid Room File.")
        {
        }
    }
}
