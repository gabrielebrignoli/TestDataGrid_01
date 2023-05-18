using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestDataGrid_01.Data;


namespace TestDataGrid_01.ViewModels
{
    public class DocumentViewModel 
    {
        public int ID { get; set; } // Change property name to ID
        public string Source { get; set; } // Change property name to Source
        public string Target { get; set; } // Change property name to Target
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<DocumentViewModel> _dataItems;
        private AppDbContext _databaseContext;

        public ObservableCollection<DocumentViewModel> DataItems
        {
            get => _dataItems;
            set
            {
                _dataItems = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            _databaseContext = new AppDbContext();
            _dataItems = new ObservableCollection<DocumentViewModel>(); // Make sure to initialize the DataItems collection
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}