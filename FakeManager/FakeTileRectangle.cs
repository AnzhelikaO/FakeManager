#region Using
using System;
using Terraria;
using OTAPI.Tile;
#endregion
namespace FakeManager
{
    public class FakeTileRectangle : ICloneable
    {
        #region Data

        public TileProvider Tile;
        public int X, Y, Width, Height;
        public bool Enabled = true;
        public FakeCollection Collection = null;

        public bool IsPersonal => Collection.IsPersonal;

        #endregion
        #region Constructor [TileProvider]

        public FakeTileRectangle(FakeCollection Collection, int X, int Y, int Width, int Height, TileProvider Tile = null)
        {
            this.Collection = Collection;
            this.Tile = new TileProvider(Width, Height);
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    if (this.Tile[i, j] == null)
                        this.Tile[i, j] = new Tile();
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
            if (Tile != null)
                for (int i = X; i < X + Width; i++)
                    for (int j = Y; j < Y + Height; j++)
                    {
                        ITile t = Tile[i, j];
                        if (t != null)
                            this.Tile[i - X, j - Y].CopyFrom(t);
                    }
        }

        #endregion
        #region Constructor [ITile[,]]

        public FakeTileRectangle(FakeCollection Collection, int X, int Y, int Width, int Height, ITile[,] Tile)
            : this(Collection, X, Y, Width, Height)
        {
            for (int i = X; i < X + Width; i++)
                for (int j = Y; j < Y + Height; j++)
                {
                    ITile t = Tile[i, j];
                    if (t != null)
                        this.Tile[i - X, j - Y].CopyFrom(t);
                }
        }

        #endregion

        #region operator[,]

        public ITile this[int x, int y]
        {
            get => Tile[x, y];
            set => Tile[x, y] = value;
        }

        #endregion

        #region SetXYWH

        public void SetXYWH(int X, int Y, int Width, int Height)
        {
            this.X = X;
            this.Y = Y;
            if ((this.Width != Width) || (this.Height != Height))
            {
                TileProvider newTile = new TileProvider(Width, Height);
                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++)
                        if (i < this.Width && j < this.Height)
                            newTile[i, j] = Tile[i, j];
                        else
                            newTile[i, j] = new Tile();
                Tile = newTile;
                this.Width = Width;
                this.Height = Height;
            }
        }

        #endregion

        #region Intersect

        public void Intersect(int X, int Y, int Width, int Height,
            out int RX, out int RY, out int RWidth, out int RHeight)
        {
            int ex1 = this.X + this.Width;
            int ex2 = X + Width;
            int ey1 = this.Y + this.Height;
            int ey2 = Y + Height;
            int maxSX = (this.X > X) ? this.X : X;
            int maxSY = (this.Y > Y) ? this.Y : Y;
            int minEX = (ex1 < ex2) ? ex1 : ex2;
            int minEY = (ey1 < ey2) ? ey1 : ey2;
            RX = maxSX;
            RY = maxSY;
            RWidth = minEX - maxSX;
            RHeight = minEY - maxSY;
        }

        #endregion
        #region IsIntersecting

        public bool IsIntersecting(int X, int Y, int Width, int Height) =>
            ((X < (this.X + this.Width)) && (this.X < (X + Width))
            && (Y < (this.Y + this.Height)) && (this.Y < (Y + Height)));

        #endregion

        #region Apply

        public void Apply(ITile[,] Tiles, int AbsoluteX, int AbsoluteY)
        {
            Intersect(AbsoluteX, AbsoluteY, Tiles.GetLength(0), Tiles.GetLength(1),
                out int intersectionX, out int intersectionY,
                out int intersectionWidth, out int intersectionHeight);
            int intersectionXLimit = (intersectionX + intersectionWidth);
            int intersectionYLimit = (intersectionY + intersectionHeight);
            for (int X = intersectionX; X < intersectionXLimit; X++)
                for (int Y = intersectionY; Y < intersectionYLimit; Y++)
                {
                    ITile tile = Tile[X - this.X, Y - this.Y];
                    if (tile != null)
                        Tiles[(X - AbsoluteX), (Y - AbsoluteY)] = tile;
                }
        }

        #endregion

        public object Clone() =>
            new FakeTileRectangle(Collection, X, Y, Width, Height, Tile);
    }
}