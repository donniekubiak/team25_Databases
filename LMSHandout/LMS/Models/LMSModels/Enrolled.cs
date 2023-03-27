using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Enrolled
    {
        public uint EnrollmentId { get; set; }
        public string Uid { get; set; } = null!;
        public uint ClassId { get; set; }
        public string Grade { get; set; } = null!;

        public virtual Class Class { get; set; } = null!;
        public virtual Student UidNavigation { get; set; } = null!;
    }
}
