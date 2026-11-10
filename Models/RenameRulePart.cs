// RenameRulePart.cs
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
using System.Windows;

namespace BatchRenamerApp.Models
{
    public enum RulePartType
    {
        Text,
        SequenceNumber,
        SequenceLetter,
        Date
    }

    public enum DateRuleFormat
    {
        ISO_Date,   // YYYY-MM-DD (ISO 8601)
        MMDDYYYY,   // MMDDYYYY
        YYYYMMDD,   // YYYYMMDD
        YYYYMM,     // YYYYMM
        YYYY_MM     // YYYY-MM
    }

    public enum DestinationOperationMode
    {
        RenameInSameFolder,
        MoveToOtherFolder,
        CopyToOtherFolder
    }

    public class RenameRulePart : INotifyPropertyChanged
    {
        private RulePartType _partType = RulePartType.Text;
        private string _textValue = string.Empty;
        private int _startNumber = 1;
        private int _totalDigits = 3;
        private string _startLetter = "A";
        private DateRuleFormat _dateFormat = DateRuleFormat.ISO_Date;
        private DateTime _referenceDate = DateTime.Today;
        private bool _useFileModifiedDate = false;
        private bool _canRemove = true;
        private bool _canAdd = true;

        public RulePartType PartType
        {
            get => _partType;
            set
            {
                if (_partType != value)
                {
                    _partType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TextVisibility));
                    OnPropertyChanged(nameof(SequenceNumberVisibility));
                    OnPropertyChanged(nameof(SequenceLetterVisibility));
                    OnPropertyChanged(nameof(DateVisibility));
                }
            }
        }

        public string TextValue
        {
            get => _textValue;
            set
            {
                if (_textValue != value)
                {
                    _textValue = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        public int StartNumber
        {
            get => _startNumber;
            set
            {
                int clamped = value < 0 ? 0 : value;
                if (_startNumber != clamped)
                {
                    _startNumber = clamped;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StartNumberText));
                }
            }
        }

        public string StartNumberText
        {
            get => _startNumber.ToString();
            set
            {
                if (int.TryParse(value, out int parsed) && parsed >= 0)
                {
                    StartNumber = parsed;
                }
            }
        }

        public int TotalDigits
        {
            get => _totalDigits;
            set
            {
                int clamped = Math.Max(1, Math.Min(10, value));
                if (_totalDigits != clamped)
                {
                    _totalDigits = clamped;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalDigitsTag));
                }
            }
        }

        public string TotalDigitsTag
        {
            get => _totalDigits.ToString();
            set
            {
                if (int.TryParse(value, out int parsed))
                {
                    TotalDigits = parsed;
                }
            }
        }

        public string StartLetter
        {
            get => _startLetter;
            set
            {
                string cleaned = string.IsNullOrWhiteSpace(value) ? "A" : value.Trim();
                if (_startLetter != cleaned)
                {
                    _startLetter = cleaned;
                    OnPropertyChanged();
                }
            }
        }

        public DateRuleFormat DateFormat
        {
            get => _dateFormat;
            set
            {
                if (_dateFormat != value)
                {
                    _dateFormat = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime ReferenceDate
        {
            get => _referenceDate;
            set
            {
                if (_referenceDate != value)
                {
                    _referenceDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseFileModifiedDate
        {
            get => _useFileModifiedDate;
            set
            {
                if (_useFileModifiedDate != value)
                {
                    _useFileModifiedDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CanRemove
        {
            get => _canRemove;
            set
            {
                if (_canRemove != value)
                {
                    _canRemove = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CanAdd
        {
            get => _canAdd;
            set
            {
                if (_canAdd != value)
                {
                    _canAdd = value;
                    OnPropertyChanged();
                }
            }
        }

        public Visibility TextVisibility => _partType == RulePartType.Text ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SequenceNumberVisibility => _partType == RulePartType.SequenceNumber ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SequenceLetterVisibility => _partType == RulePartType.SequenceLetter ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DateVisibility => _partType == RulePartType.Date ? Visibility.Visible : Visibility.Collapsed;

        public RenameRulePart Clone()
        {
            return new RenameRulePart
            {
                PartType = this.PartType,
                TextValue = this.TextValue,
                StartNumber = this.StartNumber,
                TotalDigits = this.TotalDigits,
                StartLetter = this.StartLetter,
                DateFormat = this.DateFormat,
                ReferenceDate = this.ReferenceDate,
                UseFileModifiedDate = this.UseFileModifiedDate
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

