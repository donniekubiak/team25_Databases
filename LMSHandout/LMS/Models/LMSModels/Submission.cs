using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public uint SubmissionId { get; set; }
        public uint AssignmentId { get; set; }
        public DateTime Time { get; set; }
        public string Contents { get; set; } = null!;
        public uint Score { get; set; }
        public string Uid { get; set; } = null!;

        public virtual Assignment Assignment { get; set; } = null!;
        public virtual Student UidNavigation { get; set; } = null!;
    }
}
