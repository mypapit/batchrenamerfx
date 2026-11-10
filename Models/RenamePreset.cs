// RenamePreset.cs
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

namespace BatchRenamerApp.Models
{
    public class RenamePreset
    {
        public string Name { get; set; } = "Default";
        public DestinationOperationMode DestinationMode { get; set; } = DestinationOperationMode.RenameInSameFolder;
        public string DestinationFolder { get; set; } = string.Empty;
        public List<RenameRulePart> Parts { get; set; } = new List<RenameRulePart>();

        public RenamePreset Clone()
        {
            return new RenamePreset
            {
                Name = this.Name,
                DestinationMode = this.DestinationMode,
                DestinationFolder = this.DestinationFolder,
                Parts = this.Parts.Select(p => p.Clone()).ToList()
            };
        }

        public static List<RenamePreset> GetBuiltInPresets()
        {
            return new List<RenamePreset>
            {
                // Default preset: Text("File") + Text("_") + SequenceNumber(149, Three Digits) -> File_149.jpg
                new RenamePreset
                {
                    Name = "Default",
                    DestinationMode = DestinationOperationMode.RenameInSameFolder,
                    Parts = new List<RenameRulePart>
                    {
                        new RenameRulePart { PartType = RulePartType.Text, TextValue = "File" },
                        new RenameRulePart { PartType = RulePartType.Text, TextValue = "_" },
                        new RenameRulePart { PartType = RulePartType.SequenceNumber, StartNumber = 149, TotalDigits = 3 }
                    }
                },
                // Matches AGENTS.md Sample 1: "text/text/sequence number" -> "photo-" "0001".jpg
                new RenamePreset
                {
                    Name = "Sample 1: Photo Sequence (photo-0001)",
                    DestinationMode = DestinationOperationMode.RenameInSameFolder,
                    Parts = new List<RenameRulePart>
                    {
                        new RenameRulePart { PartType = RulePartType.Text, TextValue = "photo" },
                        new RenameRulePart { PartType = RulePartType.Text, TextValue = "-" },
                        new RenameRulePart { PartType = RulePartType.SequenceNumber, StartNumber = 1, TotalDigits = 4 }
                    }
                },
                // Matches AGENTS.md Sample 2: "text/sequence number/text/date" -> "Mark-" "001" "-" "2026-11-08".jpg
                new RenamePreset
                {
                    Name = "Sample 2: Dated Mark (Mark-001-2026-11-08)",
                    DestinationMode = DestinationOperationMode.RenameInSameFolder,
                    Parts = new List<RenameRulePart>
                    {
                        new RenameRulePart { PartType = RulePartType.Text, TextValue = "Mark-" },
                        new RenameRulePart { PartType = RulePartType.SequenceNumber, StartNumber = 1, TotalDigits = 3 },
                        new RenameRulePart { PartType = RulePartType.Text, TextValue = "-" },
                        new RenameRulePart
                        {
                            PartType = RulePartType.Date,
                            DateFormat = DateRuleFormat.ISO_Date,
                            ReferenceDate = new DateTime(2026, 11, 8),
                            UseFileModifiedDate = false
                        }
                    }
                },
                // Demonstrates Sequence Letter rule
                new RenamePreset
                {
                    Name = "Sample 3: Appendix Letter (Doc-A-202611)",
                    DestinationMode = DestinationOperationMode.RenameInSameFolder,
                    Parts = new List<RenameRulePart>
                    {
                        new RenameRulePart { PartType = RulePartType.Text, TextValue = "Doc-" },
                        new RenameRulePart { PartType = RulePartType.SequenceLetter, StartLetter = "A" },
                        new RenameRulePart { PartType = RulePartType.Text, TextValue = "-" },
                        new RenameRulePart
                        {
                            PartType = RulePartType.Date,
                            DateFormat = DateRuleFormat.YYYYMM,
                            ReferenceDate = new DateTime(2026, 11, 8)
                        }
                    }
                }
            };
        }
    }
}
