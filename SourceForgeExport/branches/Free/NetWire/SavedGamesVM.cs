using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NetWireUltimate.Annotations;

namespace NetWireUltimate
{
    public class SavedGamesVM : INotifyPropertyChanged, IDisposable
    {
        readonly SavesDataContext _context;
        public SavedGamesVM()
        {
            const string file = "isostore:/Saves.sdf";

            try
            {
                _context = new SavesDataContext(file);

                if (!_context.DatabaseExists())
                {
                    _context.CreateDatabase();
                }
                Saves = new ObservableCollection<SavedGame>(_context.SavedGames);
            }
            catch
            {
                if (_context != null)
                {
                    _context.DeleteDatabase();
                    _context.CreateDatabase();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<SavedGame> Saves { get; private set; }

        public void Save(string name, byte[] data)
        {
            var sg = new SavedGame { Id = Guid.NewGuid(), Name = name, Data = data };
            _context.SavedGames.InsertOnSubmit(sg);
            _context.SubmitChanges();
            Saves.Add(sg);
        }

        public void Drop(SavedGame sg)
        {
            _context.SavedGames.DeleteOnSubmit(sg);
            Saves.Remove(sg);
            _context.SubmitChanges();
        }

        public void Dispose()
        {
            if (_context != null)
                _context.Dispose();
        }
    }
}