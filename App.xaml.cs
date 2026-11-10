// App.xaml.cs
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
using System.Windows;
using BatchRenamerApp.Models;
using BatchRenamerApp.Services;
using BatchRenamerApp.Views;

namespace BatchRenamerApp
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args != null && e.Args.Any(a => string.Equals(a, "--self-test", StringComparison.OrdinalIgnoreCase)))
            {
                int exitCode = RunSelfTests();
                Shutdown(exitCode);
                return;
            }

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        private static int RunSelfTests()
        {
            try
            {
                // 1. Test AGENTS.md Sample 1: "text/text/sequence number" -> "photo-" "0001".jpg from "IMG_0012.jpg"
                var sample1Parts = new List<RenameRulePart>
                {
                    new RenameRulePart { PartType = RulePartType.Text, TextValue = "photo" },
                    new RenameRulePart { PartType = RulePartType.Text, TextValue = "-" },
                    new RenameRulePart { PartType = RulePartType.SequenceNumber, StartNumber = 1, TotalDigits = 4 }
                };
                string res1 = RenameEngine.GenerateNewFileName("IMG_0012.jpg", sample1Parts, 0);
                if (res1 != "photo-0001.jpg")
                    throw new Exception($"Sample 1 failed: expected 'photo-0001.jpg', got '{res1}'");

                // 2. Test AGENTS.md Sample 2: "text/sequence number/text/date" -> "Mark-001-2026-11-08.jpg" from "IMG_0012.jpg"
                var sample2Parts = new List<RenameRulePart>
                {
                    new RenameRulePart { PartType = RulePartType.Text, TextValue = "Mark-" },
                    new RenameRulePart { PartType = RulePartType.SequenceNumber, StartNumber = 1, TotalDigits = 3 },
                    new RenameRulePart { PartType = RulePartType.Text, TextValue = "-" },
                    new RenameRulePart { PartType = RulePartType.Date, DateFormat = DateRuleFormat.ISO_Date, ReferenceDate = new DateTime(2026, 11, 8) }
                };
                string res2 = RenameEngine.GenerateNewFileName("IMG_0012.jpg", sample2Parts, 0);
                if (res2 != "Mark-001-2026-11-08.jpg")
                    throw new Exception($"Sample 2 failed: expected 'Mark-001-2026-11-08.jpg', got '{res2}'");

                // 3. Test Default preset configuration: "File" + "_" + SequenceNumber(149, 3 digits) -> "File_149.jpg"
                var defaultPresets = RenamePreset.GetBuiltInPresets();
                string res3 = RenameEngine.GenerateNewFileName("IMG_0012.jpg", defaultPresets[0].Parts, 0);
                if (res3 != "File_149.jpg")
                    throw new Exception($"Default preset failed: expected 'File_149.jpg', got '{res3}'");

                // 4. Test Sequence Letter & all 5 Date Formats (ISO date, MMDDYYYY, YYYYMMDD, YYYYMM, YYYY-MM)
                DateTime testDate = new DateTime(2026, 11, 8);
                if (RenameEngine.FormatDate(testDate, DateRuleFormat.ISO_Date) != "2026-11-08")
                    throw new Exception("ISO_Date format check failed.");
                if (RenameEngine.FormatDate(testDate, DateRuleFormat.MMDDYYYY) != "11082026")
                    throw new Exception("MMDDYYYY format check failed.");
                if (RenameEngine.FormatDate(testDate, DateRuleFormat.YYYYMMDD) != "20261108")
                    throw new Exception("YYYYMMDD format check failed.");
                if (RenameEngine.FormatDate(testDate, DateRuleFormat.YYYYMM) != "202611")
                    throw new Exception("YYYYMM format check failed.");
                if (RenameEngine.FormatDate(testDate, DateRuleFormat.YYYY_MM) != "2026-11")
                    throw new Exception("YYYY_MM format check failed.");

                if (RenameEngine.GenerateSequenceLetter("A", 0) != "A" ||
                    RenameEngine.GenerateSequenceLetter("A", 2) != "C" ||
                    RenameEngine.GenerateSequenceLetter("a", 25) != "z")
                    throw new Exception("SequenceLetter check failed.");

                // 5. Test FileFilterService (Size, Extension, Date Modified, Date Created)
                var testFiles = new List<FileItemModel>
                {
                    new FileItemModel { OriginalFileName = "IMG_0012.jpg", Extension = ".jpg", SizeBytes = 500 * 1024, DateCreated = new DateTime(2026, 11, 1), DateModified = new DateTime(2026, 11, 8) },
                    new FileItemModel { OriginalFileName = "notes.txt", Extension = ".txt", SizeBytes = 10 * 1024, DateCreated = new DateTime(2026, 10, 1), DateModified = new DateTime(2026, 10, 5) }
                };
                var filtered = FileFilterService.ApplyFilter(testFiles, new FileFilterCriteria
                {
                    ExtensionFilter = ".jpg",
                    MinSizeKB = 100,
                    MaxSizeKB = 1000,
                    CreatedFrom = new DateTime(2026, 11, 1),
                    ModifiedFrom = new DateTime(2026, 11, 8)
                });
                if (filtered.Count != 1 || filtered[0].OriginalFileName != "IMG_0012.jpg")
                    throw new Exception("FileFilterService check failed.");

                Console.WriteLine("SELF-TEST PASSED: All Batch Renamer FX requirements verified successfully.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("SELF-TEST FAILED: " + ex.Message);
                return 1;
            }
        }
    }
}
