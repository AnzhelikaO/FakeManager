#region Using
using OTAPI.Tile;
using System;
#endregion
namespace FakeManager
{
    public class FakeTileProvider : ITileCollection, IDisposable
    {
        private StructTile[,] data;
        private int _Width, _Height;
        public int Width => _Width;
        public int Height => _Height;

        #region Constructor

        public FakeTileProvider(int Width, int Height)
        {
            _Width = Width;
            _Height = Height;
            data = new StructTile[Width, Height];
        }
        
        #endregion

        #region operator[,]

        public ITile this[int X, int Y]
        {
            get => new TileReference(data, X, Y);

            set => (new TileReference(data, X, Y)).CopyFrom(value);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            if (data == null)
                return;
            for (int x = 0; x < data.GetLength(0); x++)
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    data[x, y].bTileHeader = 0;
                    data[x, y].bTileHeader2 = 0;
                    data[x, y].bTileHeader3 = 0;
                    data[x, y].frameX = 0;
                    data[x, y].frameY = 0;
                    data[x, y].liquid = 0;
                    data[x, y].type = 0;
                    data[x, y].wall = 0;
                }
            data = null;
        }

        #endregion
    }
}