using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileManager3
{
    public class FileManagerViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<FileItem> leftPanelFiles;
        private ObservableCollection<FileItem> rightPanelFiles;
        private FileItem selectedLeftItem;
        private FileItem selectedRightItem;
        private string currentLeftPath;
        private string currentRightPath;
        private Stack<string> leftPathHistory;
        private Stack<string> rightPathHistory;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<FileItem> LeftPanelFiles
        {
            get => leftPanelFiles;
            set
            {
                leftPanelFiles = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FileItem> RightPanelFiles
        {
            get => rightPanelFiles;
            set
            {
                rightPanelFiles = value;
                OnPropertyChanged();
            }
        }

        public FileItem SelectedLeftItem
        {
            get => selectedLeftItem;
            set
            {
                selectedLeftItem = value;
                OnPropertyChanged();
            }
        }

        public FileItem SelectedRightItem
        {
            get => selectedRightItem;
            set
            {
                selectedRightItem = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenFileCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand MoveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BackCommand { get; }

        public FileManagerViewModel()
        {
            LeftPanelFiles = new ObservableCollection<FileItem>();
            RightPanelFiles = new ObservableCollection<FileItem>();
            leftPathHistory = new Stack<string>();
            rightPathHistory = new Stack<string>();
            
            currentLeftPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            currentRightPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            OpenFileCommand = new RelayCommand(OpenFile);
            CopyCommand = new RelayCommand(CopyFile);
            MoveCommand = new RelayCommand(MoveFile);
            DeleteCommand = new RelayCommand(DeleteFile);
            BackCommand = new RelayCommand(GoBack);

            LoadFiles();
        }

        private void LoadFiles()
        {
            LoadDirectoryContents(currentLeftPath, LeftPanelFiles);
            LoadDirectoryContents(currentRightPath, RightPanelFiles);
        }

        private void LoadDirectoryContents(string path, ObservableCollection<FileItem> collection)
        {
            collection.Clear();

            // Додаємо ".." для навігації вгору
            if (path != Path.GetPathRoot(path))
            {
                collection.Add(new FileItem { Name = "..", IsDirectory = true });
            }

            // Додаємо директорії
            foreach (var dir in Directory.GetDirectories(path))
            {
                collection.Add(new FileItem
                {
                    Name = Path.GetFileName(dir),
                    Path = dir,
                    IsDirectory = true,
                    Modified = Directory.GetLastWriteTime(dir)
                });
            }

            // Додаємо файли
            foreach (var file in Directory.GetFiles(path))
            {
                collection.Add(new FileItem
                {
                    Name = Path.GetFileName(file),
                    Path = file,
                    Size = new FileInfo(file).Length,
                    Modified = File.GetLastWriteTime(file)
                });
            }
        }

        private void OpenFile()
        {
            var selectedItem = SelectedLeftItem ?? SelectedRightItem;
            if (selectedItem != null)
            {
                if (selectedItem.IsDirectory)
                {
                    if (selectedItem.Name == "..")
                    {
                        GoBack();
                    }
                    else
                    {
                        if (selectedItem == SelectedLeftItem)
                        {
                            leftPathHistory.Push(currentLeftPath);
                            currentLeftPath = selectedItem.Path;
                            LoadDirectoryContents(currentLeftPath, LeftPanelFiles);
                        }
                        else
                        {
                            rightPathHistory.Push(currentRightPath);
                            currentRightPath = selectedItem.Path;
                            LoadDirectoryContents(currentRightPath, RightPanelFiles);
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = selectedItem.Path,
                        UseShellExecute = true
                    });
                }
            }
        }

        private void GoBack()
        {
            if (leftPathHistory.Count > 0)
            {
                currentLeftPath = leftPathHistory.Pop();
                LoadDirectoryContents(currentLeftPath, LeftPanelFiles);
            }
            if (rightPathHistory.Count > 0)
            {
                currentRightPath = rightPathHistory.Pop();
                LoadDirectoryContents(currentRightPath, RightPanelFiles);
            }
        }

        private void CopyFile()
        {
            var sourceItem = SelectedLeftItem ?? SelectedRightItem;
            var targetPath = sourceItem == SelectedLeftItem ? currentRightPath : currentLeftPath;

            if (sourceItem != null)
            {
                try
                {
                    if (sourceItem.IsDirectory)
                    {
                        CopyDirectory(sourceItem.Path, Path.Combine(targetPath, sourceItem.Name));
                    }
                    else
                    {
                        File.Copy(sourceItem.Path, Path.Combine(targetPath, sourceItem.Name), true);
                    }
                    LoadFiles();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Помилка при копіюванні: {ex.Message}", "Помилка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void MoveFile()
        {
            var sourceItem = SelectedLeftItem ?? SelectedRightItem;
            var targetPath = sourceItem == SelectedLeftItem ? currentRightPath : currentLeftPath;

            if (sourceItem != null)
            {
                try
                {
                    if (sourceItem.IsDirectory)
                    {
                        Directory.Move(sourceItem.Path, Path.Combine(targetPath, sourceItem.Name));
                    }
                    else
                    {
                        File.Move(sourceItem.Path, Path.Combine(targetPath, sourceItem.Name));
                    }
                    LoadFiles();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Помилка при переміщенні: {ex.Message}", "Помилка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void DeleteFile()
        {
            var selectedItem = SelectedLeftItem ?? SelectedRightItem;
            if (selectedItem != null)
            {
                try
                {
                    if (selectedItem.IsDirectory)
                    {
                        Directory.Delete(selectedItem.Path, true);
                    }
                    else
                    {
                        File.Delete(selectedItem.Path);
                    }
                    LoadFiles();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Помилка при видаленні: {ex.Message}", "Помилка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
