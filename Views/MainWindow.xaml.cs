// MainWindow.xaml.cs
// Batch Renamer FX
// Copyright (c) 2026 Mohammad Hafiz bin Ismail (mypapit@gmail.com), https://mypapit.net
//
// This file is part of Batch Renamer FX.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BatchRenamerApp.Models;
using BatchRenamerApp.Services;

namespace BatchRenamerApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly List<RenamePreset> _presets = new List<RenamePreset>();
        private readonly ObservableCollection<RenameRulePart> _activeRuleParts = new ObservableCollection<RenameRulePart>();
        private readonly List<FileItemModel> _allFolderFiles = new List<FileItemModel>();
        private readonly ObservableCollection<FileItemModel> _filteredFiles = new ObservableCollection<FileItemModel>();

        private bool _isLoadingPreset = false;
        private string _currentPresetBaseName = "Default";
        private bool _isPresetModified = false;

        public MainWindow()
        {
            InitializeComponent();

            RulePartsItemsControl.ItemsSource = _activeRuleParts;
            FilesDataGrid.ItemsSource = _filteredFiles;

            InitializePresets();
            LoadSampleFilesInternal();
            RefreshPreviews();
        }

        private void InitializePresets()
        {
            _presets.Clear();
            _presets.AddRange(RenamePreset.GetBuiltInPresets());
            RefreshPresetComboBox(0);
        }

        private void RefreshPresetComboBox(int selectIndex)
        {
            _isLoadingPreset = true;
            PresetComboBox.Items.Clear();

            for (int i = 0; i < _presets.Count; i++)
            {
                string displayName = _presets[i].Name;
                if (i == selectIndex && _isPresetModified)
                {
                    displayName += " (Modified)";
                }
                PresetComboBox.Items.Add(displayName);
            }

            if (selectIndex >= 0 && selectIndex < PresetComboBox.Items.Count)
            {
                PresetComboBox.SelectedIndex = selectIndex;
            }
            _isLoadingPreset = false;
        }

        private void ApplyPreset(RenamePreset preset)
        {
            _isLoadingPreset = true;
            _currentPresetBaseName = preset.Name;
            _isPresetModified = false;

            switch (preset.DestinationMode)
            {
                case DestinationOperationMode.RenameInSameFolder:
                    RenameSameFolderRadio.IsChecked = true;
                    break;
                case DestinationOperationMode.MoveToOtherFolder:
                    MoveOtherFolderRadio.IsChecked = true;
                    break;
                case DestinationOperationMode.CopyToOtherFolder:
                    CopyOtherFolderRadio.IsChecked = true;
                    break;
            }

            DestinationFolderTextBox.Text = preset.DestinationFolder ?? string.Empty;

            _activeRuleParts.Clear();
            foreach (var part in preset.Parts.Take(RenameEngine.MaxRuleParts))
            {
                var cloned = part.Clone();
                cloned.PropertyChanged += OnRulePartPropertyChanged;
                _activeRuleParts.Add(cloned);
            }

            if (_activeRuleParts.Count == 0)
            {
                var defaultPart = new RenameRulePart { PartType = RulePartType.Text, TextValue = "File" };
                defaultPart.PropertyChanged += OnRulePartPropertyChanged;
                _activeRuleParts.Add(defaultPart);
            }

            UpdateRulePartButtonsState();
            _isLoadingPreset = false;
            RefreshPreviews();
        }

        private void MarkPresetModified()
        {
            if (_isLoadingPreset)
                return;

            if (!_isPresetModified)
            {
                _isPresetModified = true;
                int idx = PresetComboBox.SelectedIndex;
                if (idx >= 0)
                {
                    RefreshPresetComboBox(idx);
                }
            }
        }

        private void OnRulePartPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isLoadingPreset)
                return;

            MarkPresetModified();
            RefreshPreviews();
        }

        private void UpdateRulePartButtonsState()
        {
            bool canRemove = _activeRuleParts.Count > 1;
            bool canAdd = _activeRuleParts.Count < RenameEngine.MaxRuleParts;

            foreach (var part in _activeRuleParts)
            {
                part.CanRemove = canRemove;
                part.CanAdd = canAdd;
            }

            if (PartsCountTextBlock != null)
            {
                PartsCountTextBlock.Text = $"  ({_activeRuleParts.Count} of {RenameEngine.MaxRuleParts} max parts active)";
            }
        }

        private DestinationOperationMode GetSelectedDestinationMode()
        {
            if (MoveOtherFolderRadio?.IsChecked == true)
                return DestinationOperationMode.MoveToOtherFolder;
            if (CopyOtherFolderRadio?.IsChecked == true)
                return DestinationOperationMode.CopyToOtherFolder;
            return DestinationOperationMode.RenameInSameFolder;
        }

        private void RefreshPreviews()
        {
            if (_activeRuleParts == null || SampleRuleOutputTextBlock == null)
                return;

            // Update live sample banner for 'IMG_0012.jpg'
            string sampleResult = RenameEngine.GenerateNewFileName("IMG_0012.jpg", _activeRuleParts, 0, new DateTime(2026, 11, 8));
            SampleRuleOutputTextBlock.Text = sampleResult;

            // Update all filtered files
            RenameEngine.UpdatePreviews(
                _filteredFiles,
                _activeRuleParts,
                GetSelectedDestinationMode(),
                DestinationFolderTextBox?.Text ?? string.Empty);

            if (FileCountStatusTextBlock != null)
            {
                int selectedCount = _filteredFiles.Count(f => f.IsSelected);
                FileCountStatusTextBlock.Text = $"{selectedCount} of {_filteredFiles.Count} filtered file(s) selected ({_allFolderFiles.Count} total in folder)";
            }
        }

        private void LoadSampleFilesInternal()
        {
            _allFolderFiles.Clear();

            var sampleItems = new[]
            {
                new FileItemModel
                {
                    IsSelected = true,
                    OriginalFileName = "IMG_0012.jpg",
                    FullPath = @"C:\SamplePhotos\IMG_0012.jpg",
                    Extension = ".jpg",
                    SizeBytes = 1450 * 1024,
                    DateCreated = new DateTime(2026, 11, 1, 9, 15, 0),
                    DateModified = new DateTime(2026, 11, 8, 14, 30, 0)
                },
                new FileItemModel
                {
                    IsSelected = true,
                    OriginalFileName = "IMG_0013.jpg",
                    FullPath = @"C:\SamplePhotos\IMG_0013.jpg",
                    Extension = ".jpg",
                    SizeBytes = 1820 * 1024,
                    DateCreated = new DateTime(2026, 11, 2, 10, 20, 0),
                    DateModified = new DateTime(2026, 11, 8, 15, 10, 0)
                },
                new FileItemModel
                {
                    IsSelected = true,
                    OriginalFileName = "IMG_0014.jpg",
                    FullPath = @"C:\SamplePhotos\IMG_0014.jpg",
                    Extension = ".jpg",
                    SizeBytes = 920 * 1024,
                    DateCreated = new DateTime(2026, 11, 3, 11, 45, 0),
                    DateModified = new DateTime(2026, 11, 8, 16, 00, 0)
                },
                new FileItemModel
                {
                    IsSelected = true,
                    OriginalFileName = "DSC_0205.png",
                    FullPath = @"C:\SamplePhotos\DSC_0205.png",
                    Extension = ".png",
                    SizeBytes = 3200 * 1024,
                    DateCreated = new DateTime(2026, 11, 5, 8, 00, 0),
                    DateModified = new DateTime(2026, 11, 9, 8, 30, 0)
                },
                new FileItemModel
                {
                    IsSelected = true,
                    OriginalFileName = "vacation_clip.mp4",
                    FullPath = @"C:\SamplePhotos\vacation_clip.mp4",
                    Extension = ".mp4",
                    SizeBytes = 12400 * 1024,
                    DateCreated = new DateTime(2026, 10, 25, 18, 00, 0),
                    DateModified = new DateTime(2026, 10, 26, 12, 00, 0)
                }
            };

            foreach (var item in sampleItems)
            {
                item.PropertyChanged += OnFileItemPropertyChanged;
                _allFolderFiles.Add(item);
            }

            if (SourceFolderTextBox != null)
            {
                SourceFolderTextBox.Text = "(Sample Demonstration List — Click 'Browse Folder...' to load a real folder)";
            }

            if (_presets.Count > 0 && _activeRuleParts.Count == 0)
            {
                ApplyPreset(_presets[0]);
                // Show "Default (Modified)" initially as shown in reference-ui.jpg
                _isPresetModified = true;
                RefreshPresetComboBox(0);
            }

            ApplyCurrentFilters();
        }

        private void OnFileItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileItemModel.IsSelected))
            {
                RefreshPreviews();
            }
        }

        private void LoadFilesFromDirectory(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                FluentDialogWindow.ShowInfo(this, "Folder Not Found", "The selected folder does not exist. Please choose a valid directory.");
                return;
            }

            try
            {
                var dirInfo = new DirectoryInfo(folderPath);
                var fileInfos = dirInfo.GetFiles("*", SearchOption.TopDirectoryOnly)
                                       .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                                       .ToList();

                _allFolderFiles.Clear();
                foreach (var fi in fileInfos)
                {
                    var model = new FileItemModel
                    {
                        IsSelected = true,
                        OriginalFileName = fi.Name,
                        FullPath = fi.FullName,
                        Extension = fi.Extension,
                        SizeBytes = fi.Length,
                        DateCreated = fi.CreationTime,
                        DateModified = fi.LastWriteTime
                    };
                    model.PropertyChanged += OnFileItemPropertyChanged;
                    _allFolderFiles.Add(model);
                }

                SourceFolderTextBox.Text = folderPath;
                if (string.IsNullOrWhiteSpace(DestinationFolderTextBox.Text))
                {
                    DestinationFolderTextBox.Text = folderPath;
                }

                ApplyCurrentFilters();
                StatusBarTextBlock.Text = $"Loaded {_allFolderFiles.Count} file(s) from '{folderPath}'.";
            }
            catch (Exception ex)
            {
                FluentDialogWindow.ShowInfo(this, "Error Loading Folder", "Failed to read the contents of the selected folder.", "Close", ex.Message);
            }
        }

        private void ApplyCurrentFilters()
        {
            if (_filteredFiles == null)
                return;

            double? minKb = null;
            if (FilterMinSizeTextBox != null && double.TryParse(FilterMinSizeTextBox.Text, out double parsedMin) && parsedMin >= 0)
                minKb = parsedMin;

            double? maxKb = null;
            if (FilterMaxSizeTextBox != null && double.TryParse(FilterMaxSizeTextBox.Text, out double parsedMax) && parsedMax >= 0)
                maxKb = parsedMax;

            var criteria = new FileFilterCriteria
            {
                ExtensionFilter = FilterExtensionTextBox?.Text,
                MinSizeKB = minKb,
                MaxSizeKB = maxKb,
                ModifiedFrom = FilterModifiedFromPicker?.SelectedDate,
                ModifiedTo = FilterModifiedToPicker?.SelectedDate,
                CreatedFrom = FilterCreatedFromPicker?.SelectedDate,
                CreatedTo = FilterCreatedToPicker?.SelectedDate
            };

            var results = FileFilterService.ApplyFilter(_allFolderFiles, criteria);

            _filteredFiles.Clear();
            foreach (var f in results)
            {
                _filteredFiles.Add(f);
            }

            RefreshPreviews();
        }

        // --- UI Event Handlers ---

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingPreset)
                return;

            int idx = PresetComboBox.SelectedIndex;
            if (idx >= 0 && idx < _presets.Count)
            {
                ApplyPreset(_presets[idx]);
                RefreshPresetComboBox(idx);
                StatusBarTextBlock.Text = $"Loaded preset '{_presets[idx].Name}'.";
            }
        }

        private void SavePresetButton_Click(object sender, RoutedEventArgs e)
        {
            string proposedName = $"Custom Preset {_presets.Count + 1}";
            bool confirmed = FluentDialogWindow.ShowConfirm(
                this,
                "Save Preset",
                $"Do you want to save the current filename rule configuration as '{proposedName}'?",
                "Save Preset",
                "Cancel");

            if (!confirmed)
                return;

            var newPreset = new RenamePreset
            {
                Name = proposedName,
                DestinationMode = GetSelectedDestinationMode(),
                DestinationFolder = DestinationFolderTextBox.Text,
                Parts = _activeRuleParts.Select(p => p.Clone()).ToList()
            };

            _presets.Add(newPreset);
            _isPresetModified = false;
            RefreshPresetComboBox(_presets.Count - 1);
            StatusBarTextBlock.Text = $"Saved preset '{proposedName}'.";
        }

        private void DeletePresetButton_Click(object sender, RoutedEventArgs e)
        {
            int idx = PresetComboBox.SelectedIndex;
            if (idx < 0 || idx >= _presets.Count)
                return;

            if (_presets.Count <= 1)
            {
                FluentDialogWindow.ShowInfo(this, "Cannot Delete Preset", "At least one preset must remain in the presets list.");
                return;
            }

            string presetName = _presets[idx].Name;
            bool confirmed = FluentDialogWindow.ShowConfirm(
                this,
                "Delete Preset",
                $"Are you sure you want to delete the preset '{presetName}'?",
                "Delete Preset",
                "Cancel");

            if (!confirmed)
                return;

            _presets.RemoveAt(idx);
            int nextIdx = Math.Max(0, idx - 1);
            ApplyPreset(_presets[nextIdx]);
            RefreshPresetComboBox(nextIdx);
            StatusBarTextBlock.Text = $"Deleted preset '{presetName}'.";
        }

        private void DestinationModeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoadingPreset)
                return;

            MarkPresetModified();
            RefreshPreviews();
        }

        private void BrowseDestinationButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select Destination Folder for Batch Renamer FX";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DestinationFolderTextBox.Text = dialog.SelectedPath;
                    if (RenameSameFolderRadio.IsChecked == true)
                    {
                        CopyOtherFolderRadio.IsChecked = true;
                    }
                    RefreshPreviews();
                }
            }
        }

        private void DestinationFolderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingPreset)
                return;

            MarkPresetModified();
            RefreshPreviews();
        }

        private void AddRulePartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_activeRuleParts.Count >= RenameEngine.MaxRuleParts)
            {
                FluentDialogWindow.ShowInfo(this, "Maximum Parts Reached", "A maximum of 4 configurable filename rule parts is allowed.");
                return;
            }

            int insertIndex = _activeRuleParts.Count;
            if (sender is Button btn && btn.Tag is RenameRulePart currentPart)
            {
                int currentIdx = _activeRuleParts.IndexOf(currentPart);
                if (currentIdx >= 0)
                    insertIndex = currentIdx + 1;
            }

            var newPart = new RenameRulePart
            {
                PartType = RulePartType.Text,
                TextValue = "-"
            };
            newPart.PropertyChanged += OnRulePartPropertyChanged;
            _activeRuleParts.Insert(insertIndex, newPart);

            UpdateRulePartButtonsState();
            MarkPresetModified();
            RefreshPreviews();
        }

        private void RemoveRulePartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_activeRuleParts.Count <= 1)
                return;

            if (sender is Button btn && btn.Tag is RenameRulePart currentPart)
            {
                currentPart.PropertyChanged -= OnRulePartPropertyChanged;
                _activeRuleParts.Remove(currentPart);
                UpdateRulePartButtonsState();
                MarkPresetModified();
                RefreshPreviews();
            }
        }

        private void RulePartControl_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingPreset)
                return;

            MarkPresetModified();
            RefreshPreviews();
        }

        private void RulePartText_Changed(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingPreset)
                return;

            MarkPresetModified();
            RefreshPreviews();
        }

        private void RulePartDate_Changed(object? sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingPreset)
                return;

            MarkPresetModified();
            RefreshPreviews();
        }

        private void RulePartCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoadingPreset)
                return;

            MarkPresetModified();
            RefreshPreviews();
        }

        private void BrowseSourceFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select Source Folder Containing Files to Batch Rename";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadFilesFromDirectory(dialog.SelectedPath);
                }
            }
        }

        private void LoadSampleFilesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSampleFilesInternal();
            StatusBarTextBlock.Text = "Loaded sample demonstration files (including IMG_0012.jpg).";
        }

        private void FilterCriteria_Changed(object sender, TextChangedEventArgs e)
        {
            ApplyCurrentFilters();
        }

        private void FilterDate_Changed(object? sender, SelectionChangedEventArgs e)
        {
            ApplyCurrentFilters();
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            FilterExtensionTextBox.Text = string.Empty;
            FilterMinSizeTextBox.Text = string.Empty;
            FilterMaxSizeTextBox.Text = string.Empty;
            FilterModifiedFromPicker.SelectedDate = null;
            FilterModifiedToPicker.SelectedDate = null;
            FilterCreatedFromPicker.SelectedDate = null;
            FilterCreatedToPicker.SelectedDate = null;
            ApplyCurrentFilters();
            StatusBarTextBlock.Text = "Cleared all file filters.";
        }

        private void SelectAllFilesButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var f in _filteredFiles)
                f.IsSelected = true;
            RefreshPreviews();
        }

        private void SelectNoneFilesButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var f in _filteredFiles)
                f.IsSelected = false;
            RefreshPreviews();
        }

        private void InvertSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var f in _filteredFiles)
                f.IsSelected = !f.IsSelected;
            RefreshPreviews();
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshPreviews();
            var previewWin = new PreviewWindow(_filteredFiles, GetSelectedDestinationMode(), DestinationFolderTextBox.Text)
            {
                Owner = this
            };

            if (previewWin.ShowDialog() == true && previewWin.ApplyRequested)
            {
                PromptAndExecuteBatchRename();
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshPreviews();
            PromptAndExecuteBatchRename();
        }

        private void PromptAndExecuteBatchRename()
        {
            var selected = _filteredFiles.Where(f => f.IsSelected).ToList();
            if (selected.Count == 0)
            {
                FluentDialogWindow.ShowInfo(this, "No Files Selected", "Please select at least one file from the list before running Batch Rename.");
                return;
            }

            DestinationOperationMode mode = GetSelectedDestinationMode();
            string actionDescription = mode switch
            {
                DestinationOperationMode.RenameInSameFolder => "rename the selected files in place (within the same folder)",
                DestinationOperationMode.MoveToOtherFolder => $"move and rename the selected files to '{DestinationFolderTextBox.Text}'",
                DestinationOperationMode.CopyToOtherFolder => $"copy and rename the selected files to '{DestinationFolderTextBox.Text}'",
                _ => "rename the selected files"
            };

            string primaryActionLabel = mode switch
            {
                DestinationOperationMode.RenameInSameFolder => "Rename Files",
                DestinationOperationMode.MoveToOtherFolder => "Move & Rename",
                DestinationOperationMode.CopyToOtherFolder => "Copy & Rename",
                _ => "Rename Files"
            };

            string sampleLines = string.Join("\n", selected.Take(5).Select(f => $"{f.OriginalFileName}  →  {f.PreviewNewFileName}"));
            if (selected.Count > 5)
            {
                sampleLines += $"\n... and {selected.Count - 5} more file(s).";
            }

            bool permitted = FluentDialogWindow.ShowConfirm(
                this,
                "Confirm Batch Rename",
                $"Do you grant permission to {actionDescription} ({selected.Count} file(s) selected)?",
                primaryActionLabel,
                "Cancel",
                sampleLines);

            if (!permitted)
            {
                StatusBarTextBlock.Text = "Batch rename cancelled by user.";
                return;
            }

            // Check if currently loaded items are virtual sample items
            bool isVirtualDemo = selected.All(f => !File.Exists(f.FullPath));
            if (isVirtualDemo)
            {
                foreach (var f in selected)
                {
                    f.OriginalFileName = f.PreviewNewFileName;
                    f.Status = mode == DestinationOperationMode.CopyToOtherFolder
                        ? "Copied & Renamed (Sample Mode)"
                        : "Renamed (Sample Mode)";
                }
                RefreshPreviews();
                StatusBarTextBlock.Text = $"Applied batch rename to {selected.Count} sample file(s).";
                FluentDialogWindow.ShowInfo(
                    this,
                    "Batch Rename Complete",
                    $"Successfully applied batch rename rules to {selected.Count} sample entry/entries. Use 'Browse Folder...' to run on a folder on disk.",
                    "Close");
                return;
            }

            var execResult = RenameEngine.ExecuteBatch(
                _filteredFiles,
                _activeRuleParts,
                mode,
                DestinationFolderTextBox.Text);

            StatusBarTextBlock.Text = $"Batch operation complete: {execResult.SuccessCount} succeeded, {execResult.FailedCount} failed, {execResult.SkippedCount} skipped.";

            if (execResult.Errors.Count > 0)
            {
                FluentDialogWindow.ShowInfo(
                    this,
                    "Batch Rename Completed with Warnings",
                    $"Processed {execResult.SuccessCount} file(s) successfully, with {execResult.FailedCount} error(s).",
                    "Close",
                    string.Join("\n", execResult.Errors.Take(8)));
            }
            else
            {
                FluentDialogWindow.ShowInfo(
                    this,
                    "Batch Rename Complete",
                    $"Successfully processed {execResult.SuccessCount} file(s).",
                    "Close");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            bool confirmExit = FluentDialogWindow.ShowConfirm(
                this,
                "Exit Batch Renamer FX",
                "Are you sure you want to close Batch Renamer FX?",
                "Exit Application",
                "Cancel");

            if (confirmExit)
            {
                Close();
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var aboutWin = new AboutWindow
            {
                Owner = this
            };
            aboutWin.ShowDialog();
        }
    }
}
