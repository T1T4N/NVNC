using System;
using System.Drawing;

namespace NVNC.Utils
{
    public class Rectangle2 : IEquatable<Rectangle2>
    {
        public bool Contains(Rectangle2 other)
        {
            return (other.X >= X && other.X < Width) &&
                   (other.Y >= Y && other.Y < Height) &&
                   (other.X + other.Width <= Width) &&
                   (other.Y + other.Height <= Height);
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Top { get { return Y; } }
        public int Bottom { get { return Y + Height; } }
        public int Left { get { return X; } }
        public int Right { get { return X + Width; } }
        public bool IsSolidColor { get; set; }
        public int SolidColor { get; private set; }
        public Rectangle2(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle2(Rectangle r)
        {
            X = r.X;
            Y = r.Y;
            Width = r.Width;
            Height = r.Height;
        }
        public Rectangle2(Point s, Size d)
        {
            X = s.X;
            Y = s.Y;
            Width = d.Width;
            Height = d.Height;
        }

        public void SetSolidColor(int color)
        {
            SolidColor = color;
            IsSolidColor = true;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        #region Equality 
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Rectangle2) obj);
        }
        public bool Equals(Rectangle2 other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
                return hashCode;
            }
        }
        #endregion
        public override string ToString()
        {
            return String.Format("{0}:{1} - {2}:{3}", X, Y, Width, Height);
        }
    }
}
