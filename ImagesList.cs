using System.Collections.Generic;

namespace RandomImage
{
    public class Files
    {
        public int CurrentIndex { get; private set; }

        private List<string> _filesList;

        public IReadOnlyList<string> FilesList
        {
            get
            {
                return _filesList.AsReadOnly();
            }
        }

        public Files()
        {
            _filesList = new List<string>();
        }

        public void Add(string file)
        {
            if (!_filesList.Contains(file))
                _filesList.Add(file);
        }

        public void AddRange(IEnumerable<string> files)
        {
            _filesList.AddRange(files);
        }

        public string GetCurrentFile()
        {
            return _filesList[CurrentIndex];
        }

        public string GetNextFile()
        {
            IncIndex();
            return GetCurrentFile();
        }

        public string GetPrevFile()
        {
            DecIndex();
            return GetCurrentFile();
        }

        public void IncIndex()
        {
            if (CurrentIndex == _filesList.Count - 1)
                CurrentIndex = 0;
            else
                CurrentIndex++;
        }

        public void DecIndex()
        {
            if (CurrentIndex == 0)
                CurrentIndex = _filesList.Count - 1;
            else
                CurrentIndex--;
        }

        public void Shuffle()
        {
            _filesList.Shuffle();
        }

        public int Count { get { return _filesList.Count; } }
    }

    public class Directory
    {
        public string Name { get; set; }

        private Files _files;

        public Files Files
        {
            get
            {
                return _files ?? (_files = new Files());
            }
        }
    }

    internal class ImagesCollection
    {
        private List<Directory> _directoriesList;
        private int _currentIndex;
        private int _globalImagesCount;
        private const int _countOfFilesInDirectoryToUnite = 20;

        public ImagesCollection()
        {
            _directoriesList = new List<Directory>();
        }

        public string Current()
        {
            return _directoriesList[_currentIndex].Files.GetCurrentFile();
        }

        public string Next()
        {
            _directoriesList[_currentIndex].Files.IncIndex();
            IncIndex();
            return _directoriesList[_currentIndex].Files.GetCurrentFile();
        }

        private void IncIndex()
        {
            if (_currentIndex == _directoriesList.Count - 1)
                _currentIndex = 0;
            else
                _currentIndex++;
        }

        public string Prev()
        {
            DecIndex();
            return _directoriesList[_currentIndex].Files.GetPrevFile();
        }

        private void DecIndex()
        {
            if (_currentIndex == 0)
                _currentIndex = _directoriesList.Count - 1;
            else
                _currentIndex--;
        }

        public void Shuffle()
        {
            _directoriesList.Shuffle();
            _directoriesList.Shuffle();
            _directoriesList.Shuffle();

            foreach (var directory in _directoriesList)
            {
                directory.Files.Shuffle();
                directory.Files.Shuffle();
                directory.Files.Shuffle();
            }
        }

        public void Reorganize()
        {
            if (_directoriesList.Count < 2)
                return;

            Directory unitedResultDirectory = new Directory();
            List<Directory> directoriesToRemove = new List<Directory>();

            foreach (Directory directory in _directoriesList)
            {
	            if (directory.Files.Count >= _countOfFilesInDirectoryToUnite) continue;
	            unitedResultDirectory.Files.AddRange(directory.Files.FilesList);
	            directoriesToRemove.Add(directory);
            }
            foreach (Directory directory in directoriesToRemove)
                _directoriesList.Remove(directory);

            unitedResultDirectory.Name = "Union";
            _directoriesList.Insert(0, unitedResultDirectory);
        }

        public void Clear()
        {
            _currentIndex = 0;
            _globalImagesCount = 0;
            _directoriesList.Clear();
        }

        public void Add(string directoryName, string filePath)
        {
            Directory directory;
            int indx = _directoriesList.FindIndex(x => { return x.Name.Equals(directoryName); });

            if (indx == -1)
            {
                directory = new Directory();
                directory.Name = directoryName;
                _directoriesList.Add(directory);
				// why we don't use indx anywhere?
                indx = _directoriesList.Count - 1;
            }
            else
                directory = _directoriesList[indx];

            ++_globalImagesCount;
            directory.Files.Add(filePath);
        }

        public void AddRange(string directoryName, IReadOnlyList<string> files)
        {
            foreach (var file in files)
                Add(directoryName, file);
        }

        public int Count
        {
            get
            {
                return _globalImagesCount;
            }
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }
    }
}