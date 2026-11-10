// RenameEngine.cs
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BatchRenamerApp.Models;

namespace BatchRenamerApp.Services
{
    public static class RenameEngine
    {
        public const int MaxRuleParts = 4;

        /// <summary>
        /// Generates the new filename for a single file at a given 0-based sequence index.
        /// Always appends the original filename's extension.
        /// </summary>
        public static string GenerateNewFileName(
            string originalFileName,
            IList<RenameRulePart> ruleParts,
            int sequenceIndex,
            DateTime? fileModifiedDate = null)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
                return string.Empty;

            string extension = Path.GetExtension(originalFileName);
            if (ruleParts == null || ruleParts.Count == 0)
                return originalFileName;

            var sb = new StringBuilder();
            int count = Math.Min(ruleParts.Count, MaxRuleParts);

            for (int i = 0; i < count; i++)
            {
                var part = ruleParts[i];
                switch (part.PartType)
                {
                    case RulePartType.Text:
                        sb.Append(part.TextValue ?? string.Empty);
                        break;

                    case RulePartType.SequenceNumber:
                        long number = (long)part.StartNumber + sequenceIndex;
                        int digits = Math.Max(1, part.TotalDigits);
                        string numStr = number.ToString(CultureInfo.InvariantCulture).PadLeft(digits, '0');
                        sb.Append(numStr);
                        break;

                    case RulePartType.SequenceLetter:
                        sb.Append(GenerateSequenceLetter(part.StartLetter, sequenceIndex));
                        break;

                    case RulePartType.Date:
                        DateTime dt = (part.UseFileModifiedDate && fileModifiedDate.HasValue)
                            ? fileModifiedDate.Value
                            : part.ReferenceDate;
                        sb.Append(FormatDate(dt, part.DateFormat));
                        break;
                }
            }

            string baseName = SanitizeFileNameBase(sb.ToString());
            if (string.IsNullOrEmpty(baseName))
            {
                baseName = Path.GetFileNameWithoutExtension(originalFileName);
            }

            return baseName + extension;
        }

        public static string FormatDate(DateTime date, DateRuleFormat format)
        {
            switch (format)
            {
                case DateRuleFormat.ISO_Date:
                    return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                case DateRuleFormat.MMDDYYYY:
                    return date.ToString("MMddyyyy", CultureInfo.InvariantCulture);
                case DateRuleFormat.YYYYMMDD:
                    return date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                case DateRuleFormat.YYYYMM:
                    return date.ToString("yyyyMM", CultureInfo.InvariantCulture);
                case DateRuleFormat.YYYY_MM:
                    return date.ToString("yyyy-MM", CultureInfo.InvariantCulture);
                default:
                    return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
        }

        public static string GenerateSequenceLetter(string startLetter, int offset)
        {
            char firstChar = 'A';
            bool isLower = false;

            if (!string.IsNullOrWhiteSpace(startLetter))
            {
                char c = startLetter.Trim()[0];
                if (char.IsLetter(c))
                {
                    isLower = char.IsLower(c);
                    firstChar = char.ToUpperInvariant(c);
                }
            }

            int startIndex = firstChar - 'A';
            int targetIndex = Math.Max(0, startIndex + offset);

            // Convert 0-based index to Excel-style alphabetical sequence (A..Z, AA..ZZ)
            var result = new StringBuilder();
            int n = targetIndex;
            while (n >= 0)
            {
                int remainder = n % 26;
                char letter = (char)((isLower ? 'a' : 'A') + remainder);
                result.Insert(0, letter);
                n = (n / 26) - 1;
            }

            return result.ToString();
        }

        private static string SanitizeFileNameBase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (Array.IndexOf(invalidChars, c) < 0)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Computes preview filenames and statuses for all items in the list.
        /// </summary>
        public static void UpdatePreviews(
            IList<FileItemModel> files,
            IList<RenameRulePart> ruleParts,
            DestinationOperationMode destinationMode,
            string destinationFolder)
        {
            if (files == null)
                return;

            var seenNewNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int seqIndex = 0;

            foreach (var file in files)
            {
                if (!file.IsSelected)
                {
                    file.PreviewNewFileName = file.OriginalFileName;
                    file.PreviewDestinationPath = file.FullPath;
                    file.Status = "Skipped (Unselected)";
                    continue;
                }

                string newName = GenerateNewFileName(file.OriginalFileName, ruleParts, seqIndex, file.DateModified);
                file.PreviewNewFileName = newName;

                string targetDir = destinationMode == DestinationOperationMode.RenameInSameFolder
                    ? (Path.GetDirectoryName(file.FullPath) ?? string.Empty)
                    : destinationFolder;

                file.PreviewDestinationPath = string.IsNullOrWhiteSpace(targetDir)
                    ? newName
                    : Path.Combine(targetDir, newName);

                if (!seenNewNames.Add(newName))
                {
                    file.Status = "Conflict: Duplicate new name";
                }
                else
                {
                    switch (destinationMode)
                    {
                        case DestinationOperationMode.RenameInSameFolder:
                            file.Status = string.Equals(file.OriginalFileName, newName, StringComparison.Ordinal)
                                ? "Unchanged"
                                : "Ready to Rename";
                            break;
                        case DestinationOperationMode.MoveToOtherFolder:
                            file.Status = "Ready to Move & Rename";
                            break;
                        case DestinationOperationMode.CopyToOtherFolder:
                            file.Status = "Ready to Copy & Rename";
                            break;
                    }
                }

                seqIndex++;
            }
        }

        /// <summary>
        /// Executes the batch operation on selected files after user confirmation.
        /// </summary>
        public static BatchExecutionResult ExecuteBatch(
            IList<FileItemModel> files,
            IList<RenameRulePart> ruleParts,
            DestinationOperationMode destinationMode,
            string destinationFolder)
        {
            UpdatePreviews(files, ruleParts, destinationMode, destinationFolder);

            var result = new BatchExecutionResult();
            var selectedFiles = files.Where(f => f.IsSelected).ToList();

            if (selectedFiles.Count == 0)
            {
                result.Errors.Add("No files are selected for batch renaming.");
                return result;
            }

            if (destinationMode != DestinationOperationMode.RenameInSameFolder)
            {
                if (string.IsNullOrWhiteSpace(destinationFolder))
                {
                    result.Errors.Add("Please select a valid Destination Folder.");
                    return result;
                }

                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }
            }

            foreach (var file in selectedFiles)
            {
                try
                {
                    if (!File.Exists(file.FullPath))
                    {
                        file.Status = "Error: Source file not found";
                        result.FailedCount++;
                        result.Errors.Add($"{file.OriginalFileName}: Source file does not exist.");
                        continue;
                    }

                    string targetDir = destinationMode == DestinationOperationMode.RenameInSameFolder
                        ? (Path.GetDirectoryName(file.FullPath) ?? string.Empty)
                        : destinationFolder;

                    string targetPath = Path.Combine(targetDir, file.PreviewNewFileName);

                    if (string.Equals(file.FullPath, targetPath, StringComparison.OrdinalIgnoreCase))
                    {
                        file.Status = "Unchanged";
                        result.SkippedCount++;
                        continue;
                    }

                    if (File.Exists(targetPath))
                    {
                        file.Status = "Error: Target already exists";
                        result.FailedCount++;
                        result.Errors.Add($"{file.PreviewNewFileName}: Target file already exists.");
                        continue;
                    }

                    switch (destinationMode)
                    {
                        case DestinationOperationMode.RenameInSameFolder:
                        case DestinationOperationMode.MoveToOtherFolder:
                            File.Move(file.FullPath, targetPath);
                            file.FullPath = targetPath;
                            file.OriginalFileName = file.PreviewNewFileName;
                            file.Status = destinationMode == DestinationOperationMode.RenameInSameFolder
                                ? "Renamed"
                                : "Moved & Renamed";
                            result.SuccessCount++;
                            break;

                        case DestinationOperationMode.CopyToOtherFolder:
                            File.Copy(file.FullPath, targetPath, overwrite: false);
                            file.Status = "Copied & Renamed";
                            result.SuccessCount++;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    file.Status = $"Error: {ex.Message}";
                    result.FailedCount++;
                    result.Errors.Add($"{file.OriginalFileName}: {ex.Message}");
                }
            }

            return result;
        }
    }

    public class BatchExecutionResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; } = new List<string>();
    }
}

