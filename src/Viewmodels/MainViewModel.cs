using HyperDev.src.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HyperDev {
    public class MainViewModel {
        public ObservableCollection<Feature> Features { get; }
        public ObservableCollection<Slot> Slots { get; }
        public Feature SelectedFeature { get; set; }
        public ICommand AssignCommand { get; }

        public ICommand LoadProjectCommand { get; }

        private readonly GitHubProjectService? _gitHubProjectService;


        public MainViewModel()
        {
            _gitHubProjectService = MauiProgram.Services.GetRequiredService<GitHubProjectService>();

            Features = new ObservableCollection<Feature>
            {
                new Feature { Name = "Feature 1" },
                new Feature { Name = "Feature 2" }
            };

            Slots = new ObservableCollection<Slot>
            {
                new Slot { Title = "Slot 1" },
                new Slot { Title = "Slot 2" }
            };

            AssignCommand = new Command<Slot>(AssignFeatureToSlot);

        }

        private void AssignFeatureToSlot(Slot slot)
        {
            if(slot != null && SelectedFeature != null && !slot.IsLocked)
            {
                slot.AssignedFeature = SelectedFeature;
            }
        }

    }

    public class Feature {
        public string Name { get; set; }
    }

    public class Slot : BindableObject {
        private Feature _assignedFeature;
        private bool _isLocked;

        public string Title { get; set; }

        public Feature AssignedFeature
        {
            get => _assignedFeature;
            set
            {
                _assignedFeature = value;
                OnPropertyChanged();
            }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                _isLocked = value;
                OnPropertyChanged();
            }
        }
    }
}