// PreviewWindow.xaml.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BatchRenamerApp.Models;

namespace BatchRenamerApp.Views
{
    public partial class PreviewWindow : Window
    {
        public bool ApplyRequested { get; private set; }

        public PreviewWindow(IList<FileItemModel> files, DestinationOperationMode mode, string destinationFolder)
        {
            InitializeComponent();

            PreviewDataGrid.ItemsSource = files;

            int selectedCount = files.Count(f => f.IsSelected);
            int totalCount = files.Count;
            SummaryTextBlock.Text = $"Showing {selectedCount} selected of {totalCount} filtered file(s). Uncheck any file to exclude it from batch renaming.";

            switch (mode)
            {
                case DestinationOperationMode.RenameInSameFolder:
                    OperationModeBadge.Text = "Mode: Rename in Same Folder";
                    break;
                case DestinationOperationMode.MoveToOtherFolder:
                    OperationModeBadge.Text = $"Mode: Move to '{destinationFolder}'";
                    break;
                case DestinationOperationMode.CopyToOtherFolder:
                    OperationModeBadge.Text = $"Mode: Copy to '{destinationFolder}'";
                    break;
            }
        }

        private void ApplyRenameButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyRequested = true;
            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyRequested = false;
            DialogResult = false;
            Close();
        }
    }
}

