// FileFilterService.cs
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
using System.Linq;
using BatchRenamerApp.Models;

namespace BatchRenamerApp.Services
{
    public class FileFilterCriteria
    {
        public string? ExtensionFilter { get; set; }
        public double? MinSizeKB { get; set; }
        public double? MaxSizeKB { get; set; }
        public DateTime? ModifiedFrom { get; set; }
        public DateTime? ModifiedTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }

    public static class FileFilterService
    {
        public static List<FileItemModel> ApplyFilter(IEnumerable<FileItemModel> allFiles, FileFilterCriteria criteria)
        {
            if (allFiles == null)
                return new List<FileItemModel>();

            if (criteria == null)
                return allFiles.ToList();

            IEnumerable<FileItemModel> query = allFiles;

            // 1. Filter by Extension (supports comma or semicolon separated extensions, e.g. ".jpg, .png" or "jpg")
            string? extFilter = criteria.ExtensionFilter;
            if (extFilter != null && !string.IsNullOrWhiteSpace(extFilter))
            {
                var allowedExtensions = extFilter
                    .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(ext => ext.Trim().StartsWith(".") ? ext.Trim() : "." + ext.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (allowedExtensions.Count > 0)
                {
                    query = query.Where(f => allowedExtensions.Contains(f.Extension));
                }
            }

            // 2. Filter by Size (KB)
            if (criteria.MinSizeKB.HasValue)
            {
                long minBytes = (long)(criteria.MinSizeKB.Value * 1024);
                query = query.Where(f => f.SizeBytes >= minBytes);
            }

            if (criteria.MaxSizeKB.HasValue)
            {
                long maxBytes = (long)(criteria.MaxSizeKB.Value * 1024);
                query = query.Where(f => f.SizeBytes <= maxBytes);
            }

            // 3. Filter by Date Modified
            if (criteria.ModifiedFrom.HasValue)
            {
                DateTime fromDate = criteria.ModifiedFrom.Value.Date;
                query = query.Where(f => f.DateModified.Date >= fromDate);
            }

            if (criteria.ModifiedTo.HasValue)
            {
                DateTime toDate = criteria.ModifiedTo.Value.Date;
                query = query.Where(f => f.DateModified.Date <= toDate);
            }

            // 4. Filter by Date Created
            if (criteria.CreatedFrom.HasValue)
            {
                DateTime fromDate = criteria.CreatedFrom.Value.Date;
                query = query.Where(f => f.DateCreated.Date >= fromDate);
            }

            if (criteria.CreatedTo.HasValue)
            {
                DateTime toDate = criteria.CreatedTo.Value.Date;
                query = query.Where(f => f.DateCreated.Date <= toDate);
            }

            return query.ToList();
        }
    }
}

