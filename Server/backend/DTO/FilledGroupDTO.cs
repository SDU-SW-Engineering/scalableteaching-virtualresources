﻿namespace ScalableTeaching.DTO
{
    public class FilledGroupDTO
    {
        public string GroupName { get; set; }
        public Guid CourseID { get; set; }
        public List<string> Users { get; set; }

        public override string ToString()
        {
            return $"FilledGroupDTO{{GroupName:{GroupName}, CourseID:{CourseID.ToString()}, Users:{string.Join(", ", Users)}}}";

        }
    }
}
