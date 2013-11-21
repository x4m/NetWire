using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace NetWireUltimate
{
    public class SavesDataContext : DataContext
    {
        public SavesDataContext(string fileOrConnection) : base(fileOrConnection)
        {
        }

        public Table<SavedGame> SavedGames { get{return GetTable<SavedGame>();} }
    }

    [Table]
    public class SavedGame
    {
        [Column(IsDbGenerated = true,IsPrimaryKey = true)]
        public Guid Id { get; set; }
        [Column]
        public string Name { get; set; }
        [Column]
        public byte[] Data { get; set; }
    }
}