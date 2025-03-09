using System;
using System.Globalization;

namespace Eevee.Fixed
{
    public struct Rectangle : IEquatable<Rectangle>, IComparable<Rectangle>
    {
        public Fixed64 MinX;
        public Fixed64 MinY;
        public Fixed64 Width;
        public Fixed64 Height;

        public Rectangle(Fixed64 x, Fixed64 y, Fixed64 width, Fixed64 height)
        {
            MinX = x;
            MinY = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Vector2D position, Vector2D size)
        {
            MinX = position.X;
            MinY = position.Y;
            Width = size.X;
            Height = size.Y;
        }

        public Rectangle(Rectangle source)
        {
            MinX = source.MinX;
            MinY = source.MinY;
            Width = source.Width;
            Height = source.Height;
        }

        public static Rectangle zero => new Rectangle(Fixed64.Zero, Fixed64.Zero, Fixed64.Zero, Fixed64.Zero);

        public static Rectangle MinMaxRect(Fixed64 xmin, Fixed64 ymin, Fixed64 xmax, Fixed64 ymax)
        {
            return new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        public void Set(Fixed64 x, Fixed64 y, Fixed64 width, Fixed64 height)
        {
            MinX = x;
            MinY = y;
            Width = width;
            Height = height;
        }

        public Fixed64 x
        {
            get { return MinX; }
            set { MinX = value; }
        }

        public Fixed64 y
        {
            get { return MinY; }
            set { MinY = value; }
        }

        public Vector2D position
        {
            get { return new Vector2D(MinX, MinY); }
            set
            {
                MinX = value.X;
                MinY = value.Y;
            }
        }

        public Vector2D center
        {
            get { return new Vector2D(x + Width / 2f, y + Height / 2f); }
            set
            {
                MinX = value.X - Width / 2f;
                MinY = value.Y - Height / 2f;
            }
        }

        public Vector2D min
        {
            get { return new Vector2D(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2D max
        {
            get { return new Vector2D(xMax, yMax); }
            set
            {
                xMax = value.X;
                yMax = value.Y;
            }
        }

        public Fixed64 width
        {
            get { return Width; }
            set { Width = value; }
        }

        public Fixed64 height
        {
            get { return Height; }
            set { Height = value; }
        }

        public Vector2D size
        {
            get { return new Vector2D(Width, Height); }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public Fixed64 X
        {
            get { return MinX; }
            set
            {
                Fixed64 oldxmax = xMax;
                MinX = value;
                Width = oldxmax - MinX;
            }
        }
        public Fixed64 Y
        {
            get { return MinY; }
            set
            {
                Fixed64 oldymax = yMax;
                MinY = value;
                Height = oldymax - MinY;
            }
        }
        public Fixed64 xMax
        {
            get { return Width + MinX; }
            set { Width = value - MinX; }
        }
        public Fixed64 yMax
        {
            get { return Height + MinY; }
            set { Height = value - MinY; }
        }

        public bool Contains(Vector2D point)
        {
            return (point.X >= X) && (point.X < xMax) && (point.Y >= Y) && (point.Y < yMax);
        }

        // Returns true if the /x/ and /y/ components of /point/ is a point inside this rectangle.
        public bool Contains(Vector3D point)
        {
            return (point.X >= X) && (point.X < xMax) && (point.Y >= Y) && (point.Y < yMax);
        }

        public bool Contains(Vector3D point, bool allowInverse)
        {
            if (!allowInverse)
            {
                return Contains(point);
            }

            bool xAxis = width < 0f && (point.X <= X) && (point.X > xMax) || width >= 0f && (point.X >= X) && (point.X < xMax);
            bool yAxis = height < 0f && (point.Y <= Y) && (point.Y > yMax) || height >= 0f && (point.Y >= Y) && (point.Y < yMax);
            return xAxis && yAxis;
        }

        // Swaps min and max if min was greater than max.
        private static Rectangle OrderMinMax(Rectangle rect)
        {
            if (rect.X > rect.xMax)
            {
                Fixed64 temp = rect.X;
                rect.X = rect.xMax;
                rect.xMax = temp;
            }

            if (rect.Y > rect.yMax)
            {
                Fixed64 temp = rect.Y;
                rect.Y = rect.yMax;
                rect.yMax = temp;
            }

            return rect;
        }

        public bool Overlaps(Rectangle other)
        {
            return (other.xMax > X && other.X < xMax && other.yMax > Y && other.Y < yMax);
        }

        public bool Overlaps(Rectangle other, bool allowInverse)
        {
            Rectangle self = this;
            if (allowInverse)
            {
                self = OrderMinMax(self);
                other = OrderMinMax(other);
            }

            return self.Overlaps(other);
        }

        public static Vector2D NormalizedToPoint(Rectangle rectangle, Vector2D normalizedRectCoordinates)
        {
            return new Vector2D(Maths.Lerp(rectangle.x, rectangle.xMax, normalizedRectCoordinates.X), Maths.Lerp(rectangle.y, rectangle.yMax, normalizedRectCoordinates.Y));
        }

        public static Vector2D PointToNormalized(Rectangle rectangle, Vector2D point)
        {
            return new Vector2D(Maths.InverseLerp(rectangle.x, rectangle.xMax, point.X), Maths.InverseLerp(rectangle.y, rectangle.yMax, point.Y));
        }

        // Returns true if the rectangles are different.
        public static bool operator !=(Rectangle lhs, Rectangle rhs)
        {
            // Returns true in the presence of NaN values.
            return !(lhs == rhs);
        }

        // Returns true if the rectangles are the same.
        public static bool operator ==(Rectangle lhs, Rectangle rhs)
        {
            // Returns false in the presence of NaN values.
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (width.GetHashCode() << 2) ^ (y.GetHashCode() >> 2) ^ (height.GetHashCode() >> 1);
        }

        public int CompareTo(Rectangle other)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(object other)
        {
            if (!(other is Rectangle))
                return false;

            return Equals((Rectangle)other);
        }

        public bool Equals(Rectangle other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && width.Equals(other.width) && height.Equals(other.height);
        }

        public override string ToString()
        {
            return ToString(null, CultureInfo.InvariantCulture.NumberFormat);
        }

        public string ToString(string format)
        {
            return ToString(format, CultureInfo.InvariantCulture.NumberFormat);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "F2";
            return string.Format("(x:{0}, y:{1}, width:{2}, height:{3})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), width.ToString(format, formatProvider), height.ToString(format, formatProvider));
        }
    }
}