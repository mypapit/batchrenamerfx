// FluentDialogWindow.xaml.cs
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

using System.Windows;

namespace BatchRenamerApp.Views
{
    public partial class FluentDialogWindow : Window
    {
        public FluentDialogWindow(
            string title,
            string message,
            string primaryButtonText,
            string? secondaryButtonText = null,
            string? detailsText = null)
        {
            InitializeComponent();

            Title = title;
            DialogTitleTextBlock.Text = title;
            DialogMessageTextBlock.Text = message;
            PrimaryActionButton.Content = primaryButtonText;

            if (!string.IsNullOrWhiteSpace(detailsText))
            {
                DialogDetailsTextBlock.Text = detailsText;
                DetailsBorder.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(secondaryButtonText))
            {
                SecondaryActionButton.Visibility = Visibility.Collapsed;
                PrimaryActionButton.Margin = new Thickness(0);
            }
            else
            {
                SecondaryActionButton.Content = secondaryButtonText;
                SecondaryActionButton.Visibility = Visibility.Visible;
            }
        }

        public static bool ShowConfirm(
            Window owner,
            string title,
            string message,
            string primaryButtonText = "Apply Changes",
            string secondaryButtonText = "Cancel",
            string? detailsText = null)
        {
            var dlg = new FluentDialogWindow(title, message, primaryButtonText, secondaryButtonText, detailsText)
            {
                Owner = owner
            };
            return dlg.ShowDialog() == true;
        }

        public static void ShowInfo(
            Window owner,
            string title,
            string message,
            string closeButtonText = "Close",
            string? detailsText = null)
        {
            var dlg = new FluentDialogWindow(title, message, closeButtonText, null, detailsText)
            {
                Owner = owner
            };
            dlg.ShowDialog();
        }

        private void PrimaryActionButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void SecondaryActionButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

