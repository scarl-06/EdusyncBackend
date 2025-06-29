using System;
using System.Collections.Generic;

namespace EduSyncApi.Models;

public partial class UserModel
{
    public Guid UserId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    public string? PasswordHash { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<ResultModel> ResultModel { get; set; } = new List<ResultModel>();
}
