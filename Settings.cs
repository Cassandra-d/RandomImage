using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RandomImage
{
    [Serializable]
    public class Settings
    {
        public List<string> LastPlaces { get; }
        public bool SearchInSubdirectories { get; set; }
        public bool AutomaticalyCopyImageToClipboard { get; set; }
        public bool CheckForAlreadyUsedImages { get; set; }
        public string CurrentPlace
        {
            get
            {
                if (LastPlaces.Count != 0)
                    return LastPlaces.First();
                return String.Empty;
            }
        }

        [XmlIgnore]
        public HashSet<string> UsedImages { get; set; }

        public Settings()
        {
            LastPlaces = new List<string>();
            UsedImages = new HashSet<string>();
            SearchInSubdirectories = false;
            CheckForAlreadyUsedImages = false;
        }

        public void AddPlace(string place)
        {
            if (string.IsNullOrEmpty(place))
                throw new ArgumentException("place should not be empty");

            LastPlaces.Remove(place);
            LastPlaces.Insert(0, place);
        }

        internal void ClearUsedImages()
        {
            UsedImages.Clear();
        }
    }
}