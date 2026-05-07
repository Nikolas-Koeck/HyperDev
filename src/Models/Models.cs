namespace HyperDev.src.Models {
    public class Feature {
        public string Name { get; set; }
    }

    public class Slot {
        public string Title { get; set; }
        public Feature AssignedFeature { get; set; }
        public bool IsLocked { get; set; }
    }
}
