using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduSyncApi.Models;

public class Course
{
    public Guid CourseId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? MediaUrl { get; set; }

    public Guid? InstructorId { get; set; }

    public virtual ICollection<Assessment> Assessment { get; set; } = new List<Assessment>();

    public virtual UserModel? Instructor { get; set; }
}