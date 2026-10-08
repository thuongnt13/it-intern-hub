using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class Evaluation
{
    public long Id { get; set; }

    public long InternshipRecordId { get; set; }

    public byte EvaluationRound { get; set; }

    public decimal DisciplineScore { get; set; }

    public decimal TechnicalScore { get; set; }

    public decimal SoftSkillScore { get; set; }

    public decimal FinalScore { get; set; }

    public string Comments { get; set; } = null!;

    public string? ImprovementPoints { get; set; }

    public bool HiringRecommendation { get; set; }

    public string EvaluatedBy { get; set; } = null!;

    public DateTime EvaluatedAt { get; set; }

    public virtual Record InternshipRecord { get; set; } = null!;
}
