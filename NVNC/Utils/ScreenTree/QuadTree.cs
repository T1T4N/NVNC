using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NVNC.Utils.ScreenTree
{
    public class QuadTree
    {
        public readonly Dictionary<Rectangle2, QuadNode> LocationToNode;
        private Rectangle2 ScreenSize { get; set; }
        private int[] ScreenPixels { get; set; }
        public QuadNode Root { get; private set; }
        //private object syncLock = new object();
        public QuadTree(Rectangle2 screenSize, int[] pixels, int minTileHeight = 64, int minTileWidth = 64)
        {
            Stopwatch t = Stopwatch.StartNew();
            QuadNode.MIN_HEIGHT = minTileHeight;    //Any better way ?
            QuadNode.MIN_WIDTH = minTileWidth;

            ScreenSize = screenSize;
            ScreenPixels = pixels;
            LocationToNode = new Dictionary<Rectangle2, QuadNode>();

            Root = new QuadNode(screenSize, pixels);

            //Very time consuming method ! Needs optimization
            //Root.Expand();

            Stopwatch a = Stopwatch.StartNew();
            HashSet<QuadNode> lowestLevel = BFSLowest();
            a.Stop();
            Trace.WriteLine("Found lowest in:" + a.ElapsedMilliseconds + "ms");

            a = Stopwatch.StartNew();
            int numberOfTasks = lowestLevel.Count * 4;
            ManualResetEvent signal = new ManualResetEvent(false);

            var arr = lowestLevel.ToArray();
            for (int iter = 0; iter < arr.Length; iter++)
            {
                QuadNode currNode = arr[iter];
                for (int i = 0; i < 4; i++)
                {
                    int li = i;
                    //ThreadPool.QueueUserWorkItem(func =>
                    //{
                    Dictionary<int, long> occurances = new Dictionary<int, long>();
                    long h = 1;
                    long maxO = -1;
                    long maxV = -1;

                    for (long j = 0; j < currNode.childrenData[li].Length; j++)
                    {
                        int px = currNode.childrenData[li][j];
                        h = (h * ((px + j) % QuadNode.Q)) % QuadNode.Q;

                        int val = px;
                        if (!occurances.ContainsKey(val))
                            occurances.Add(val, 0);
                        occurances[val]++;

                        if (occurances[val] > maxO)
                        {
                            maxO = occurances[val];
                            maxV = val;
                        }
                    }

                    currNode.childrenHashes[li] = h;
                    long diff = occurances.Count;

                    //Calculates the percentage of different pixels in the rectangle
                    //If it is less than 10 in 1024, the tile is considered to be filled with a solid color
                    //The solid color used for filling is the color which occured the most times
                    float percDiff = (float)diff / currNode.childrenData[li].Length;
                    if (percDiff < 0.01)
                    {
                        currNode.childrenRect[li].SetSolidColor((int)maxV);
                        currNode.childrenHashes[li] = ((long)Math.Pow(maxV * maxO * diff, 3)) % QuadNode.Q;
                        //idk if the previous hash would be better or this one
                    }
                    currNode.DataHash = (currNode.DataHash + currNode.childrenHashes[li]) % QuadNode.Q;

                    if (Interlocked.Decrement(ref numberOfTasks) == 0)
                    {
                        signal.Set();
                    }
                    //});
                }
            }
            signal.WaitOne();
            a.Stop();
            Trace.WriteLine("Processed nodes in:" + a.ElapsedMilliseconds + "ms");

            Root.CalculateHash();
            //IEnumerable<QuadNode> nodes = GetChildren(Root);
            //foreach (QuadNode ch in nodes)
            //{
            //LocationToNode.Add(ch.Bounds, ch);
            //}
            t.Stop();
            Trace.WriteLine("QuadTree construction: " + t.ElapsedMilliseconds + "ms");
        }

        private HashSet<QuadNode> BFSLowest()
        {
            HashSet<QuadNode> ret = new HashSet<QuadNode>();
            Queue<QuadNode> q = new Queue<QuadNode>();
            q.Enqueue(Root);
            while (q.Count > 0)
            {
                QuadNode c = q.Dequeue();
                if (c.CanExpand())
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int li = i;
                        Direction d = (Direction)li;
                        c[d] = new QuadNode(c.childrenRect[li], c.childrenData[li]);
                        q.Enqueue(c[d]);
                    }
                }
                else
                {
                    ret.Add(c);
                }
            }
            return ret;
        }
        private IEnumerable<QuadNode> GetChildren(QuadNode root)
        {
            HashSet<QuadNode> ret = new HashSet<QuadNode>();
            GetChildren(root, ret);
            return ret;
        }

        private void GetChildren(QuadNode root, ICollection<QuadNode> ret)
        {
            if (root != null)
            {
                ret.Add(root);
                for (int i = 0; i < 4; i++)
                {
                    QuadNode ch = root[(Direction)i];
                    if (ch != null)
                    {
                        GetChildren(ch, ret);
                    }
                }
            }
        }

        public QuadNode this[Rectangle2 location]
        {
            get { return LocationToNode[location]; }
        }

        public override string ToString()
        {
            return Root.DataHash.ToString();
        }
    }

    public enum Direction
    {
        NW = 0,
        NE = 1,
        SW = 2,
        SE = 3
    }

}