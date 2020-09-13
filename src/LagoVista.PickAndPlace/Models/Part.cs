using LagoVista.Core.Models;
using System;

namespace LagoVista.PickAndPlace.Models
{
    public class Part : ModelBase
    {
        public int Count { get; set; }

        private String _packageId;
        public String PackageId
        {
            get => _packageId;
            set => Set(ref _packageId, value);
        }

        private String _packageName;
        public String PackageName
        {
            get => _packageName;
            set => Set(ref _packageName, value);
        }

        public String LibraryName { get; set; }

        private string _value;
        public string Value
        {
            get => _value;
            set { Set(ref _value, value); }
        }

        public double Height { get; set; }

        public string PartNumber { get; set; }


        private string _feederId;
        public String FeederId
        {
            get { return _feederId; }
            set
            {
                _feederId = value;
                Set(ref _feederId, value);
                RaisePropertyChanged(nameof(IsConfigured));
            }
        }

        public bool IsConfigured
        {
            get { return !String.IsNullOrEmpty(FeederId) && RowNumber.HasValue && !String.IsNullOrEmpty(PackageId); }
        }

        private int? _rowNumber;
        public int? RowNumber
        {
            get { return _rowNumber; }
            set
            {
                Set(ref _rowNumber, value);
                RaisePropertyChanged(nameof(IsConfigured));
            }
        }

        private bool _shouldPlace;
        public bool ShouldPlace
        {
            get { return _shouldPlace; }
            set { Set(ref _shouldPlace, value); }
        }

        private bool _wasPlaced;
        public bool WasPlaced
        {
            get { return _wasPlaced; }
            set { Set(ref _wasPlaced, value); }
        }

        public string Display
        {
            get
            {
                return $"{PackageName} - {Value}";
            }
        }
    }
}
