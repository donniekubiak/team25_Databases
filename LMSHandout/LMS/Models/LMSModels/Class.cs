using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Enrolleds = new HashSet<Enrolled>();
        }

        public uint ClassId { get; set; }
        public ushort Year { get; set; }
        public string Season { get; set; } = null!;
        public string Location { get; set; } = null!;
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public uint CourseId { get; set; }
        public string ProfessorId { get; set; } = null!;

        public virtual Course Course { get; set; } = null!;
        public virtual Professor Professor { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Enrolled> Enrolleds { get; set; }
    }
}
