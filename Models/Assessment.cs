using System;
using System.Collections.Generic;

namespace EduSyncApi.Models;

public partial class Assessment
{
    public Guid AssessmentId { get; set; }

    public string? Title { get; set; }

    public string? Question { get; set; }

    public int? MaxScore { get; set; }

    public Guid? CourseId { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<ResultModel> ResultModel { get; set; } = new List<ResultModel>();
}
