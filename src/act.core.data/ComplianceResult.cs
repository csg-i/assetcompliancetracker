using System;
using System.Collections.Generic;

namespace act.core.data
{
    public class ComplianceResult
    {
        public ComplianceResult()
        {
            Errors = new HashSet<ComplianceResultError>();
            Tests = new HashSet<ComplianceResultTest>();
        }

        public long Id { get; set; }

        public Guid ResultId { get; set; }

        public long InventoryItemId { get; set; }

        /// <summary>
        ///     Date and Time
        /// </summary>
        public DateTime EndTime { get; set; }

        public ComplianceStatusConstant Status { get; set; }

        public bool OperatingSystemTestPassed { get; set; }

        /// <summary>
        ///     Date Only (No Time)
        /// </summary>
        public DateTime EndDate { get; set; }

        public Node Node { get; set; }

        public ICollection<ComplianceResultTest> Tests { get; set; }
        public ICollection<ComplianceResultError> Errors { get; set; }
    }
}