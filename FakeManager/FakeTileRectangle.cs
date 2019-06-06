#region Using
using System;
using Terraria;
using OTAPI.Tile;
using System.Collections.Generic;
using System.Linq;
#endregion
namespace FakeManager
{
    public class FakeTileRectangle : ICloneable
    {
        #region Data

        public FakeTileProvider Tile;
        public int X, Y, Width, Height;
        public bool Enabled = true;
        public FakeCollection Collection;
        private Dictionary<int, Sign> FakeSigns;
        private Sign SignPlaceholder = new Sign() { x = -1, y = -1 };
        private Dictionary<int, Chest> FakeChests;
        private Chest ChestPlaceholder = new Chest() { x = -1, y = -1 };

        public bool IsPersonal => Collection.IsPersonal;

        #endregion
        #region Constructor [TileProvider]

        public FakeTileRectangle(FakeCollection Collection, int X, int Y,
            int Width, int Height, ITileCollection Tile = null)
        {
            this.Collection = Collection;
            this.Tile = new FakeTileProvider(Width, Height);
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
            this.FakeSigns = new Dictionary<int, Sign>();
            this.FakeChests = new Dictionary<int, Chest>();
        }

        #endregion
        #region Constructor [ITile[,]]

        public FakeTileRectangle(FakeCollection Collection,
            int X, int Y, int Width, int Height, ITile[,] Tile)
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
                FakeTileProvider newTile = new FakeTileProvider(Width, Height);
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
        #region Update
        public void Update() { }

        #endregion

        #region AddSign

        public void AddSign(Sign Sign)
        {
            if (Sign == null)
                throw new ArgumentNullException(nameof(Sign), "Sign is null.");

            lock (FakeSigns)
            {
                KeyValuePair<int, Sign>[] signs = FakeSigns.Where(s =>
                    ((s.Value.x == Sign.x) && (s.Value.y == Sign.y))).ToArray();
                if (signs.Length == 0)
                {
                    int index = -1;
                    for (int i = 999; i >= 0; i--)
                        if (Main.sign[i] == null)
                        {
                            index = i;
                            break;
                        }
                    if (index == -1)
                        throw new Exception("Could not add a sign.");
                    Main.sign[index] = SignPlaceholder;
                    FakeSigns.Add(index, Sign);
                }
                else
                    FakeSigns[signs[0].Key] = Sign;
            }
        }

        #endregion
        #region RemoveSign

        public bool RemoveSign(Sign Sign)
        {
            if (Sign == null)
                throw new ArgumentNullException(nameof(Sign), "Sign is null.");
            return RemoveSign(Sign.x, Sign.y);
        }

        public bool RemoveSign(int X, int Y)
        {
            lock (FakeSigns)
            {
                KeyValuePair<int, Sign>[] signs = FakeSigns.Where(s =>
                    ((s.Value.x == X) && (s.Value.y == Y))).ToArray();
                if (signs.Length != 1)
                    return false;
                int index = signs[0].Key;
                Main.sign[index] = null;
                FakeSigns.Remove(index);
                return true;
            }
        }

        #endregion
        #region AddChest

        public void AddChest(Chest Chest)
        {
            if (Chest == null)
                throw new ArgumentNullException(nameof(Chest), "Chest is null.");

            lock (FakeChests)
            {
                KeyValuePair<int, Chest>[] chests = FakeChests.Where(s =>
                    ((s.Value.x == Chest.x) && (s.Value.y == Chest.y))).ToArray();
                if (chests.Length == 0)
                {
                    int index = -1;
                    for (int i = 999; i >= 0; i--)
                        if (Main.chest[i] == null)
                        {
                            index = i;
                            break;
                        }
                    if (index == -1)
                        throw new Exception("Could not add a chest.");
                    Main.chest[index] = ChestPlaceholder;
                    FakeChests.Add(index, Chest);
                }
                else
                    FakeChests[chests[0].Key] = Chest;
            }
        }

        #endregion
        #region RemoveChest

        public bool RemoveChest(Chest Chest)
        {
            if (Chest == null)
                throw new ArgumentNullException(nameof(Chest), "Chest is null.");
            return RemoveChest(Chest.x, Chest.y);
        }

        public bool RemoveChest(int X, int Y)
        {
            lock (FakeChests)
            {
                KeyValuePair<int, Chest>[] chests = FakeChests.Where(s =>
                    ((s.Value.x == X) && (s.Value.y == Y))).ToArray();
                if (chests.Length != 1)
                    return false;
                int index = chests[0].Key;
                Main.chest[index] = null;
                FakeChests.Remove(index);
                return true;
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

        #region ApplyTiles

        public void ApplyTiles(ITile[,] Tiles, int AbsoluteX, int AbsoluteY)
        {
            Intersect(AbsoluteX, AbsoluteY, Tiles.GetLength(0), Tiles.GetLength(1),
                out int x1, out int y1, out int w, out int h);
            int x2 = (x1 + w), y2 = (y1 + h);

            for (int i = x1; i < x2; i++)
                for (int j = y1; j < y2; j++)
                {
                    ITile tile = Tile[i - X, j - Y];
                    if (tile != null)
                        Tiles[(i - AbsoluteX), (j - AbsoluteY)] = tile;
                }
        }

        #endregion
        #region ApplySigns

        public void ApplySigns(Dictionary<int, Sign> Signs,
            int AbsoluteX, int AbsoluteY, int Width, int Height)
        {
            Intersect(AbsoluteX, AbsoluteY, Width, Height,
                out int x1, out int y1, out int w, out int h);
            int x2 = (x1 + w), y2 = (y1 + h);

            lock (FakeSigns)
                foreach (KeyValuePair<int, Sign> pair in FakeSigns)
                {
                    int x = pair.Value.x, y = pair.Value.y;
                    if ((x >= x1) && (x < x2) && (y >= y1) && (y < y2))
                        Signs.Add(pair.Key, pair.Value);
                }
        }

        #endregion
        #region ApplyChests

        public void ApplyChests(Dictionary<int, Chest> Chests,
            int AbsoluteX, int AbsoluteY, int Width, int Height)
        {
            Intersect(AbsoluteX, AbsoluteY, Width, Height,
                out int x1, out int y1, out int w, out int h);
            int x2 = (x1 + w), y2 = (y1 + h);

            lock (FakeChests)
                foreach (KeyValuePair<int, Chest> pair in FakeChests)
                {
                    int x = pair.Value.x, y = pair.Value.y;
                    if ((x >= x1) && (x < x2) && (y >= y1) && (y < y2))
                        Chests.Add(pair.Key, pair.Value);
                }
        }

        #endregion

        public object Clone() =>
            new FakeTileRectangle(Collection, X, Y, Width, Height, Tile);
    }
}