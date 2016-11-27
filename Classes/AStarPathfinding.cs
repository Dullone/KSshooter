using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.IO;

namespace KSshooter.Classes
{
    public class AStarPathfinding
    {
        const int HVmovement = 10;
        const int Dmovement = 14;
        const int nodeSize = 30;
        const int XKeyMulitply = 10000;
        SortedList<int,AstarNode> openList;
        SortedList<int,AstarNode> closedList;
        BinaryHeap<AstarNode> openListBH;

        AstarNode pathEnd = null;

        Tile[,] nodes;

        //Heuristic method
        enum Heuristic { Manhattan, Euclidian };
        Heuristic heursitic;

        //debug
        int iterations;
        int findOpen;
        int findClosed;
        StreamWriter log;
        Stopwatch time;

        public AStarPathfinding(Tile[,] nodeArray)
        {
            nodes = nodeArray;
            openList = new SortedList<int,AstarNode>(500);
            closedList = new SortedList<int,AstarNode>(500);
            openListBH = new BinaryHeap<AstarNode>();
            time = new Stopwatch();
            heursitic = Heuristic.Euclidian;
        }

        public List<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            //If the end lies on a hitable node, don't bother trying to find a path
            if (nodes[(int)(end.X / 30), (int)(end.Y / 30)].hit == true)
            {
                return null;
            }
            openList.Clear();
            closedList.Clear();
            openListBH.Clear();
            pathEnd = null;
            AstarNode startNode = new AstarNode();
            startNode.G = 0;
            startNode.location = start;
            startNode.H = HeuristicEstimate(start, end);
            AddToOpenList(startNode);
            //debug
            iterations = 0;
            findClosed = 0;
            findOpen = 0;
            time.Restart();
            //end debug
            Iterate(end);
            //log = new StreamWriter("log.txt", true);
            //log.WriteLine("iterations: " + iterations);
            //log.WriteLine("Iteratetime Ticks: " + time.Elapsed.Ticks);
            //log.Close();
            if (pathEnd == null)
                return new List<Vector2>();
            return ConstructPathArray(start);
        }

        private List<Vector2> ConstructPathArray(Vector2 start)
        {
            List<Vector2> path = new List<Vector2>();

            AstarNode currentLoc = pathEnd;
            AstarNode  prev = null;
            while (currentLoc.location != start)
            {
                path.Add(currentLoc.location);
                prev = currentLoc;
                currentLoc = currentLoc.parent;
            }

            return path;
        }

        private void AddToOpenList(AstarNode node)
        {
            openList.Add(Ckey(node.location), node);
            openListBH.Add(node);
        }

        private AstarNode FindLowest()
        {
            if (openListBH.Count == 0)
                return null;
            return openListBH.Remove();
        }

        private int Ckey(Vector2 loc)
        {
            return (int)(loc.X * XKeyMulitply + loc.Y);
        }

        private bool IsInClosedList(Vector2 node)
        {
            findClosed++; //debug
            return closedList.ContainsKey(Ckey(node));
        }

        private AstarNode IsInOpenList(Vector2 node)
        {
            findOpen++;//debug
            if(openList.ContainsKey(Ckey(node)))
            {
                return openList[Ckey(node)];
            }
            return null;
        }

        private void Iterate(Vector2 end)
        {
            do
            {
                iterations++;
                //get losest f from list 
                AstarNode lowestF = FindLowest();
                if (lowestF == null) //no path found
                    return;
                if (lowestF.location == end) //path found
                {
                    pathEnd = lowestF;
                    return;
                }
                openList.Remove(Ckey(lowestF.location));
                closedList.Add(Ckey(lowestF.location), lowestF);

                bool leftClear = true;
                bool upClear = true;
                bool belowclear = true;
                bool rightclear = true;
                //For each of 8 adjacent Squares:
                //upper middle
                upClear = CheckNode(lowestF, new Vector2(lowestF.location.X, lowestF.location.Y - 30), end, 10);
                //left
                leftClear = CheckNode(lowestF, new Vector2(lowestF.location.X - 30, lowestF.location.Y), end, 10);
                //right
                rightclear = CheckNode(lowestF, new Vector2(lowestF.location.X + 30, lowestF.location.Y), end, 10);
                //lower middle
                belowclear = CheckNode(lowestF, new Vector2(lowestF.location.X, lowestF.location.Y + 30), end, 10);
                //upper left
                if (upClear && leftClear)
                    CheckNode(lowestF, new Vector2(lowestF.location.X - 30, lowestF.location.Y - 30), end, 14);
                //upper right
                if (upClear && rightclear)
                    CheckNode(lowestF, new Vector2(lowestF.location.X + 30, lowestF.location.Y - 30), end, 14);
                //lower left
                if (leftClear && belowclear)
                    CheckNode(lowestF, new Vector2(lowestF.location.X - 30, lowestF.location.Y + 30), end, 14);
                //lower right
                if (belowclear && rightclear)
                    CheckNode(lowestF, new Vector2(lowestF.location.X + 30, lowestF.location.Y + 30), end, 14);
            } while (true); //Loop... TO INFINITIY
            
        }

        private bool CheckNode(AstarNode lowestF, Vector2 check, Vector2 end, int weight)
        {
            if (check.X < 0 || check.X/30 > nodes.GetLength(0) - 1 || check.Y < 0 || check.Y/30 > nodes.GetLength(1) - 1)
            {
                return false;
            }
            else
            {
                //if not walkable or in closed list, ignore
                if (nodes[(int)(check.X / nodeSize), (int)(check.Y / nodeSize)].hit == true)
                    return false;
                if (IsInClosedList(nodes[(int)(check.X / nodeSize), (int)(check.Y / nodeSize)].Location) == true)
                    return true;
            }
            AstarNode InListCheck = IsInOpenList(nodes[(int)(check.X / nodeSize), (int)(check.Y / nodeSize)].Location);
            if (InListCheck == null)
            {
                //add to open list
                AstarNode newN = new AstarNode();
                newN.G = lowestF.G + weight;
                newN.H = HeuristicEstimate(nodes[(int)(check.X / nodeSize), (int)(check.Y / nodeSize)].Location, end);
                newN.parent = lowestF;
                newN.location = nodes[(int)(check.X / nodeSize), (int)(check.Y / nodeSize)].Location;
                AddToOpenList(newN);
            }
            else // is in open list, adjust
            {
                if (InListCheck.G > lowestF.G + weight)
                {
                    InListCheck.parent = lowestF;
                    InListCheck.G = lowestF.G + weight;
                }
            }
            return true;
        }

        private int HeuristicEstimate(Vector2 start, Vector2 end)
        {
            if (heursitic == Heuristic.Manhattan)
            {
                float distance = Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);
                distance = distance / 30;
                return (int)distance * 10;
            }
            else
            {
                return (int)Math.Abs(Vector2.Distance(start, end));
            }
        }
    }

    public class AstarNode : IComparable<AstarNode>
    {
        private int f = 0; //total path score, G + h
        private int g = 0; //movement cost from start
        private int h = 0; //heuristic estimate of distance from goal
        public AstarNode parent = null;
        public Vector2 location;

        public int F
        {
            get { return f; }
        }

        public int G
        {
            get { return g; }
            set
            {
                g = value;
                f = h + g;
            }
        }

        public int H
        {
            get { return h; }
            set
            {
                h = value;
                f = h + g;
            }
        }

        public int CompareTo(AstarNode other)
        {
            return F - other.F;
        }
    }
}
