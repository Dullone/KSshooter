using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace KSshooter.Classes
{
    public class Level
    {
        List<Room> roomlist;
        Room activeRoom;
        Player _player;
        Room startRoom;
        Vector2 startLocation;

        public delegate void ActiveRoomChangedEventCallback(Level sender, EventArgs e);
        public event ActiveRoomChangedEventCallback ActiveRoomChanged;

        //Constructors
        public Level()
        {
            roomlist = new List<Room>();
            activeRoom = null;
        }

        public Level(XmlDocument xml, ContentManager content, Player player)
        {
            _player = player;
            roomlist = new List<Room>();
            XmlNode levelNode = xml.SelectSingleNode("level");
            //room info
            string startingRoom = levelNode.SelectSingleNode("startroom").InnerText;
            float x = (float)Convert.ToDouble(levelNode.SelectSingleNode("startlocationx").InnerText);
            float y = (float)Convert.ToDouble(levelNode.SelectSingleNode("startlocationy").InnerText);
            startLocation = new Vector2(x, y);

            XmlNode roomsElement = levelNode.SelectSingleNode("rooms");
            XmlNodeList rooms = roomsElement.SelectNodes("room");
            foreach (XmlNode room in rooms)
            {
                roomlist.Add(new Room(room, this, content));
            }
            //set starting room
            foreach (Room room in roomlist)
            {
                if (startingRoom == room.Name)
                    startRoom = room;
            }
            activeRoom = startRoom;
            //link exits to destination room
            foreach (Room room in roomlist)
            {
                foreach (RoomExit exit in room.Exits)
                {
                    foreach (Room room2 in roomlist)
                    {
                        if (room2.Name == exit.toRoomName)
                            exit.toRoom = room2;
                    }
                }
            }
        }

        //Properties
        public Player player
        {
            get { return _player; }
            set { _player = value; }
        }
        
        public Room ActiveRoom
        {
            set 
            {
                if (value != activeRoom)
                {
                    activeRoom = value;
                    if(ActiveRoomChanged != null)
                        ActiveRoomChanged(this, new EventArgs());
                }
            }

            get { return activeRoom; }
        }

        public Vector2 StartLocation
        {
            get { return startLocation; }
        }

        //Methods
        public void addRoomToLevel(Room room)
        {
            //don't add the room if its already in the list
            if (roomlist.Contains(room))
                return;
            roomlist.Add(room);
        }

        public void ChangeRoom(Room room)
        {
            activeRoom = room;
        }

        public Room getRoomByIndex(int idx)
        {
            return roomlist[idx];
        }
    }
}
