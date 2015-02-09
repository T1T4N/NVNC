using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using NVNC.Utils.ScreenTree;

namespace NVNC.Utils
{
    /// <summary>
    /// A QuadTree structure for keeping information about the screen pixels
    /// </summary>
    public class ScreenHandler
    {
        private QuadTree previous, current;
        private bool firstScreen;
        public int[] LastPixels { get; private set; }
        public Rectangle2 Bounds { get; set; }
        private IEqualityComparer<QuadNode> Comparator { get; set; }

        /// <summary>
        /// Creates a screen handler to get the changed parts of the screen since the last check.
        /// </summary>
        /// <param name="screen">A rectangle that represents which part of the screen will be handled.</param>
        /// <param name="hashOnlyCompare">If true, pixel data will be checked only by its HashCode, otherwise it will be checked with the implemented Equals method</param>
        public ScreenHandler(Rectangle screen, bool hashOnlyCompare)
        {
            if (hashOnlyCompare)
                Comparator = new HashComparer();
            else Comparator = new FullComparer();

            Rectangle2 rect = new Rectangle2(screen);
            Bounds = rect;

            //One of the most expensive operations, getting the screen capture. Should be used as less as possible
            int[] pixels = PixelGrabber.GrabPixels(PixelGrabber.CreateScreenCapture(screen));
            LastPixels = pixels;

            int minTHeight = Bounds.Height / 6;
            int minTWidth = Bounds.Width / 8;
            current = new QuadTree(rect, pixels /*, minTHeight, minTWidth*/ );
            previous = current;
            firstScreen = true;
        }
        public ScreenHandler(Rectangle2 screen, bool hashOnlyCompare) : this(screen.ToRectangle(), hashOnlyCompare) { }

        public ICollection<QuadNode> GetChange()
        {
            HashSet<QuadNode> ret = new HashSet<QuadNode>();
            //If it is the first change request, send the whole screen
            //So the client has something to paint
            if (firstScreen)
            {
                firstScreen = false;
                RefreshCurrent();
                ret.Add(current.Root);
                return ret;
            }
            RefreshCurrent();

            Stopwatch r = Stopwatch.StartNew();
            GetChangeR(ret, previous.Root, current.Root);
            previous = current;
            r.Stop();
            //Trace.WriteLine("Changes processed in: " + r.ElapsedMilliseconds);

            //A workaround since the Framebuffer Update must send some data
            //Encode the first pixel of the screen (top-left) and send it
            if (ret.Count == 0)
                ret.Add(QuadNode.EmptyNode());

            return ret;
        }

        private void GetChangeR(HashSet<QuadNode> ret, QuadNode pRoot, QuadNode cRoot)
        {
            if (pRoot == null || cRoot == null) return;
            if (Comparator.Equals(cRoot, pRoot)) return;

            if (!cRoot.CanExpand()) //If we hit a leaf node
                ret.Add(cRoot);
            else
            {
                int countAdded = 0;
                for (int i = 0; i < 4; i++)
                {
                    GetChangeR(ret, pRoot[(Direction)i], cRoot[(Direction)i]);

                    if (ret.Contains(cRoot[(Direction)i]))
                    {
                        countAdded++;
                    }
                }

                //If all four subnodes have been added, remove them and add the parent node instead
                if (countAdded == 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        ret.Remove(cRoot[(Direction)i]);
                    }
                    ret.Add(cRoot);
                }
            }
        }

        private void RefreshCurrent()
        {
            Stopwatch o = Stopwatch.StartNew();
            Bitmap b = PixelGrabber.CreateScreenCapture(Bounds.ToRectangle());

            Stopwatch c = Stopwatch.StartNew();
            int[] pixels = PixelGrabber.GrabPixels(b);
            c.Stop();
            Trace.WriteLine("Refresh grab done in: " + c.ElapsedMilliseconds + "ms");

            LastPixels = pixels;
            current = new QuadTree(Bounds, pixels);

            o.Stop();
            Trace.WriteLine("Complete refresh done in: " + o.ElapsedMilliseconds + "ms");
        }

        private class HashComparer : IEqualityComparer<QuadNode>
        {
            public bool Equals(QuadNode x, QuadNode y)
            {
                return x.DataHash == y.DataHash;
            }

            public int GetHashCode(QuadNode obj)
            {
                return obj.GetHashCode();
            }
        }
        private class FullComparer : IEqualityComparer<QuadNode>
        {
            public bool Equals(QuadNode x, QuadNode y)
            {
                if (x.DataHash == y.DataHash)
                {
                    return x.Equals(y);
                }
                return false;
            }

            public int GetHashCode(QuadNode obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
