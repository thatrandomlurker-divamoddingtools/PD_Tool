﻿namespace KKdBaseLib
{
    public struct Vector4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vector4(float X, float Y, float Z, float W)
        { this.X = X; this.Y = Y; this.Z = Z; this.W = W; }

        public float Length => (X * X + Y * Y + Z * Z + W * W).Sqrt();
        public Vector4 Normalized => this = Length == 0 ? new Vector4() : this / Length;

        public static Vector4 operator +(Vector4 left, Vector4 right)
        { left.X += right.X; left.Y += right.Y; left.Z += right.Z; left.W += right.W; return left; }
        public static Vector4 operator -(Vector4 left, Vector4 right)
        { left.X -= right.X; left.Y -= right.Y; left.Z -= right.Z; left.W -= right.W; return left; }
        public static Vector4 operator -(Vector4 vec)
        { vec.X = -vec.X; vec.Y = -vec.Y; vec.Z = -vec.Z; vec.W = -vec.W; return vec; }
        public static Vector4 operator *(Vector4   vec,   float scale)
        { vec.X *= scale  ; vec.Y *= scale  ; vec.Z *= scale  ; vec.W *= scale  ; return vec; }
        public static Vector4 operator *(  float scale, Vector4   vec)
        { vec.X *= scale  ; vec.Y *= scale  ; vec.Z *= scale  ; vec.W *= scale  ; return vec; }
        public static Vector4 operator *(Vector4   vec, Vector4 scale)
        { vec.X *= scale.X; vec.Y *= scale.Y; vec.Z *= scale.Z; vec.W *= scale.W; return vec; }
        public static Vector4 operator /(Vector4   vec,   float scale)
        { vec.X /= scale  ; vec.Y /= scale  ; vec.Z /= scale  ; vec.W /= scale  ; return vec; }
        public static Vector4 operator /(Vector4   vec, Vector4 scale)
        { vec.X /= scale.X; vec.Y /= scale.Y; vec.Z /= scale.Z; vec.W /= scale.W; return vec; }
        public static bool operator ==(Vector4 A, Vector4 B) =>  A.Equals(B);
        public static bool operator !=(Vector4 A, Vector4 B) => !A.Equals(B);

        public static double Distance       (Vector4 left, Vector4 right) =>
            ((right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) * (right.Y - left.Y) +
             (right.Z - left.Z) * (right.Z - left.Z) + (right.W - left.W) * (right.W - left.W)).Sqrt();
        public static double DistanceSquared(Vector4 left, Vector4 right) =>
            (right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) * (right.Y - left.Y) +
            (right.Z - left.Z) * (right.Z - left.Z) + (right.W - left.W) * (right.W - left.W);
        public static double Dot(Vector4 left, Vector4 right) =>
            left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
        public static Vector4 Lerp(Vector4 a, Vector4 b,   float blend) =>
            new Vector4 { X = blend   * (b.X - a.X) + a.X,
                          Y = blend   * (b.Y - a.Y) + a.Y,
                          Z = blend   * (b.Z - a.Z) + a.Z,
                          W = blend   * (b.W - a.W) + a.W };
        public static Vector4 Lerp(Vector4 a, Vector4 b, Vector4 blend) =>
            new Vector4 { X = blend.X * (b.X - a.X) + a.X,
                          Y = blend.Y * (b.Y - a.Y) + a.Y,
                          Z = blend.Z * (b.Z - a.Z) + a.Z,
                          W = blend.W * (b.W - a.W) + a.W };
        public Vector4 Round(     ) =>
            new Vector4 { X = X.Round( ), Y = Y.Round( ), Z = Z.Round( ), W = W.Round( ) };
        public Vector4 Round(int d) =>
            new Vector4 { X = X.Round(d), Y = Y.Round(d), Z = Z.Round(d), W = W.Round(d) };

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ W.GetHashCode();
                return hashCode;
            }
        }
        public override bool Equals(object obj) =>
            obj is Vector4 vec ? Equals(vec) : false;
        public bool Equals(Vector4 other) =>
            X == other.X && Y == other.Y && Z == other.Z && W == other.W;

        public override string ToString() => $"({X}; {Y}, {Z}, {W})";
        public string ToString(int d) => $"({X.Round(d)}; {Y.Round(d)}, {Z.Round(d)}, {W.Round(d)})";
    }
}
