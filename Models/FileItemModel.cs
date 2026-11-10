// FileItemModel.cs
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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BatchRenamerApp.Models
{
    public class FileItemModel : INotifyPropertyChanged
    {
        private bool _isSelected = true;
        private string _originalFileName = string.Empty;
        private string _fullPath = string.Empty;
        private string _extension = string.Empty;
        private long _sizeBytes;
        private DateTime _dateCreated;
        private DateTime _dateModified;
        private string _previewNewFileName = string.Empty;
        private string _previewDestinationPath = string.Empty;
        private string _status = "Ready";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public string OriginalFileName
        {
            get => _originalFileName;
            set
            {
                if (_originalFileName != value)
                {
                    _originalFileName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FullPath
        {
            get => _fullPath;
            set
            {
                if (_fullPath != value)
                {
                    _fullPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Extension
        {
            get => _extension;
            set
            {
                if (_extension != value)
                {
                    _extension = value;
                    OnPropertyChanged();
                }
            }
        }

        public long SizeBytes
        {
            get => _sizeBytes;
            set
            {
                if (_sizeBytes != value)
                {
                    _sizeBytes = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SizeFormatted));
                }
            }
        }

        public string SizeFormatted
        {
            get
            {
                if (_sizeBytes < 1024)
                    return $"{_sizeBytes} B";
                if (_sizeBytes < 1024 * 1024)
                    return $"{_sizeBytes / 1024.0:F1} KB";
                return $"{_sizeBytes / (1024.0 * 1024.0):F2} MB";
            }
        }

        public DateTime DateCreated
        {
            get => _dateCreated;
            set
            {
                if (_dateCreated != value)
                {
                    _dateCreated = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime DateModified
        {
            get => _dateModified;
            set
            {
                if (_dateModified != value)
                {
                    _dateModified = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PreviewNewFileName
        {
            get => _previewNewFileName;
            set
            {
                if (_previewNewFileName != value)
                {
                    _previewNewFileName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PreviewDestinationPath
        {
            get => _previewDestinationPath;
            set
            {
                if (_previewDestinationPath != value)
                {
                    _previewDestinationPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

