using ITInternHub.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITInternHub.Models;

public partial class ITInternHubDbContext : DbContext
{
    public ITInternHubDbContext()
    {
    }

    public ITInternHubDbContext(DbContextOptions<ITInternHubDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<CompanyProfile> CompanyProfiles { get; set; }

    public virtual DbSet<Evaluation> Evaluations { get; set; }

    public virtual DbSet<Industry> Industries { get; set; }

    public virtual DbSet<Interview> Interviews { get; set; }

    public virtual DbSet<JobPost> JobPosts { get; set; }

    public virtual DbSet<JobSkill> JobSkills { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Record> Records { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<StatusLog> StatusLogs { get; set; }

    public virtual DbSet<StudentProfile> StudentProfiles { get; set; }

    public virtual DbSet<StudentSkill> StudentSkills { get; set; }

    public virtual DbSet<Term> Terms { get; set; }

    public virtual DbSet<UniversityOfficer> UniversityOfficers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<WeeklyReport> WeeklyReports { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=DESKTOP-5H0MUCA\\SQLEXPRESS;Database=ITInternHubDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("applications", "application");

            entity.HasIndex(e => new { e.JobPostId, e.Status, e.MatchScore }, "IX_applications_screening").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.JobPostId, e.StudentId }, "UQ_applications_submission").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AppliedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("applied_at");
            entity.Property(e => e.CoverLetter).HasColumnName("cover_letter");
            entity.Property(e => e.CvUrl)
                .HasMaxLength(500)
                .HasColumnName("cv_url");
            entity.Property(e => e.JobPostId).HasColumnName("job_post_id");
            entity.Property(e => e.MatchScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("match_score");
            entity.Property(e => e.RecruiterNote).HasColumnName("recruiter_note");
            entity.Property(e => e.Status)
                .HasDefaultValue((byte)1)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.JobPost).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobPostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_app_job");

            entity.HasOne(d => d.Student).WithMany(p => p.Applications)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_app_student");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs", "auth");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("ip_address");
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.TableName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("table_name");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_audit_user");
        });

        modelBuilder.Entity<CompanyProfile>(entity =>
        {
            entity.ToTable("company_profiles", "company");

            entity.HasIndex(e => e.TaxCode, "UQ_company_profiles_tax").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_company_profiles_user").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.BusinessLicenseUrl)
                .HasMaxLength(500)
                .HasColumnName("business_license_url");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.CompanyEmail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("company_email");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(255)
                .HasColumnName("company_name");
            entity.Property(e => e.CompanyPhone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("company_phone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EmployeeScale)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("employee_scale");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .HasColumnName("logo_url");
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500)
                .HasColumnName("rejection_reason");
            entity.Property(e => e.TaxCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tax_code");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VerificationStatus).HasColumnName("verification_status");
            entity.Property(e => e.WebsiteUrl)
                .HasMaxLength(255)
                .HasColumnName("website_url");

            entity.HasOne(d => d.User).WithOne(p => p.CompanyProfile)
                .HasForeignKey<CompanyProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_company_profiles_user");
        });

        modelBuilder.Entity<Evaluation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_internship_evaluations");

            entity.ToTable("evaluations", "internship");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.DisciplineScore)
                .HasColumnType("decimal(3, 1)")
                .HasColumnName("discipline_score");
            entity.Property(e => e.EvaluatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("evaluated_at");
            entity.Property(e => e.EvaluatedBy)
                .HasMaxLength(150)
                .HasColumnName("evaluated_by");
            entity.Property(e => e.EvaluationRound)
                .HasDefaultValue((byte)1)
                .HasColumnName("evaluation_round");
            entity.Property(e => e.FinalScore)
                .HasColumnType("decimal(3, 1)")
                .HasColumnName("final_score");
            entity.Property(e => e.HiringRecommendation).HasColumnName("hiring_recommendation");
            entity.Property(e => e.ImprovementPoints).HasColumnName("improvement_points");
            entity.Property(e => e.InternshipRecordId).HasColumnName("internship_record_id");
            entity.Property(e => e.SoftSkillScore)
                .HasColumnType("decimal(3, 1)")
                .HasColumnName("soft_skill_score");
            entity.Property(e => e.TechnicalScore)
                .HasColumnType("decimal(3, 1)")
                .HasColumnName("technical_score");

            entity.HasOne(d => d.InternshipRecord).WithMany(p => p.Evaluations)
                .HasForeignKey(d => d.InternshipRecordId)
                .HasConstraintName("FK_eval_record");
        });

        modelBuilder.Entity<Industry>(entity =>
        {
            entity.ToTable("industries", "catalog");

            entity.HasIndex(e => e.Name, "UQ_industries_name").IsUnique();

            entity.HasIndex(e => e.Slug, "UQ_industries_slug").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IconUrl)
                .HasMaxLength(500)
                .HasColumnName("icon_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Slug)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("slug");
        });

        modelBuilder.Entity<Interview>(entity =>
        {
            entity.ToTable("interviews", "application");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.CandidateNote).HasColumnName("candidate_note");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.Format)
                .HasDefaultValue((byte)1)
                .HasColumnName("format");
            entity.Property(e => e.InterviewResult).HasColumnName("interview_result");
            entity.Property(e => e.InterviewerName)
                .HasMaxLength(255)
                .HasColumnName("interviewer_name");
            entity.Property(e => e.LocationOrLink)
                .HasMaxLength(500)
                .HasColumnName("location_or_link");
            entity.Property(e => e.StartTime).HasColumnName("start_time");

            entity.HasOne(d => d.Application).WithMany(p => p.Interviews)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("FK_interview_application");
        });

        modelBuilder.Entity<JobPost>(entity =>
        {
            entity.ToTable("job_posts", "company");

            entity.HasIndex(e => new { e.IndustryId, e.Status, e.City }, "IX_job_posts_filter");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApplicationCount).HasColumnName("application_count");
            entity.Property(e => e.Benefits).HasColumnName("benefits");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IndustryId).HasColumnName("industry_id");
            entity.Property(e => e.IsUnpaid).HasColumnName("is_unpaid");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500)
                .HasColumnName("rejection_reason");
            entity.Property(e => e.Requirements).HasColumnName("requirements");
            entity.Property(e => e.SalaryMax)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("salary_max");
            entity.Property(e => e.SalaryMin)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("salary_min");
            entity.Property(e => e.Slug)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("slug");
            entity.Property(e => e.Status)
                .HasDefaultValue((byte)1)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.ViewCount).HasColumnName("view_count");
            entity.Property(e => e.WorkingAddress)
                .HasMaxLength(500)
                .HasColumnName("working_address");
            entity.Property(e => e.WorkingMode)
                .HasDefaultValue((byte)1)
                .HasColumnName("working_mode");

            entity.HasOne(d => d.Company).WithMany(p => p.JobPosts)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_job_posts_company");

            entity.HasOne(d => d.Industry).WithMany(p => p.JobPosts)
                .HasForeignKey(d => d.IndustryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_job_posts_industry");
        });

        modelBuilder.Entity<JobSkill>(entity =>
        {
            entity.HasKey(e => new { e.JobPostId, e.SkillId });

            entity.ToTable("job_skills", "company");

            entity.HasIndex(e => new { e.SkillId, e.PriorityLevel }, "IX_job_skills_lookup");

            entity.Property(e => e.JobPostId).HasColumnName("job_post_id");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.PriorityLevel).HasColumnName("priority_level");

            entity.HasOne(d => d.JobPost).WithMany(p => p.JobSkills)
                .HasForeignKey(d => d.JobPostId)
                .HasConstraintName("FK_js_job");

            entity.HasOne(d => d.Skill).WithMany(p => p.JobSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_js_skill");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3213E83F1C99F97A");

            entity.ToTable("notifications", "auth");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("notification_type");
            entity.Property(e => e.RedirectUrl)
                .HasMaxLength(500)
                .HasColumnName("redirect_url");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__notificat__user___3C34F16F");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions", "auth");

            entity.HasIndex(e => e.Code, "UQ_permissions_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("action_name");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("module");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_student_projects");

            entity.ToTable("projects", "student");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DemoUrl)
                .HasMaxLength(500)
                .HasColumnName("demo_url");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GithubUrl)
                .HasMaxLength(500)
                .HasColumnName("github_url");
            entity.Property(e => e.Role)
                .HasMaxLength(100)
                .HasColumnName("role");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.Technologies)
                .HasMaxLength(500)
                .HasColumnName("technologies");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Student).WithMany(p => p.Projects)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK_student_projects_student");
        });

        modelBuilder.Entity<Record>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_internship_records");

            entity.ToTable("records", "internship");

            entity.HasIndex(e => new { e.StudentId, e.InternshipTermId }, "UQ_records_student_term").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualEndDate).HasColumnName("actual_end_date");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.ExpectedEndDate).HasColumnName("expected_end_date");
            entity.Property(e => e.FinalReportUrl)
                .HasMaxLength(500)
                .HasColumnName("final_report_url");
            entity.Property(e => e.InternshipTermId).HasColumnName("internship_term_id");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(150)
                .HasColumnName("job_title");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasDefaultValue((byte)1)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SupervisorEmail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("supervisor_email");
            entity.Property(e => e.SupervisorName)
                .HasMaxLength(150)
                .HasColumnName("supervisor_name");
            entity.Property(e => e.SupervisorPhone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("supervisor_phone");

            entity.HasOne(d => d.Application).WithMany(p => p.Records)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("FK_records_application");

            entity.HasOne(d => d.Company).WithMany(p => p.Records)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_records_company");

            entity.HasOne(d => d.InternshipTerm).WithMany(p => p.Records)
                .HasForeignKey(d => d.InternshipTermId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_records_term");

            entity.HasOne(d => d.Student).WithMany(p => p.Records)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_records_student");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles", "auth");

            entity.HasIndex(e => e.Code, "UQ_roles_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsRegistrable)
                .HasDefaultValue(true)
                .HasColumnName("is_registrable");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.ToTable("role_permissions", "auth");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .HasConstraintName("FK_rp_permission");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_rp_role");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("skills", "catalog");

            entity.HasIndex(e => new { e.IndustryId, e.IsActive }, "IX_skills_industry");

            entity.HasIndex(e => e.Name, "UQ_skills_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.IconUrl)
                .HasMaxLength(500)
                .HasColumnName("icon_url");
            entity.Property(e => e.IndustryId).HasColumnName("industry_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsHot).HasColumnName("is_hot");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.Industry).WithMany(p => p.Skills)
                .HasForeignKey(d => d.IndustryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_skills_industry");
        });

        modelBuilder.Entity<StatusLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_application_status_logs");

            entity.ToTable("status_logs", "application");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.ChangedByUserId).HasColumnName("changed_by_user_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.NewStatus).HasColumnName("new_status");
            entity.Property(e => e.PreviousStatus).HasColumnName("previous_status");

            entity.HasOne(d => d.Application).WithMany(p => p.StatusLogs)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("FK_asl_application");

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.StatusLogs)
                .HasForeignKey(d => d.ChangedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_asl_user");
        });

        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.ToTable("student_profiles", "student");

            entity.HasIndex(e => e.StudentCode, "UQ_student_profiles_code").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_student_profiles_user").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdmissionYear).HasColumnName("admission_year");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.ClassName)
                .HasMaxLength(50)
                .HasColumnName("class_name");
            entity.Property(e => e.CvUrl)
                .HasMaxLength(500)
                .HasColumnName("cv_url");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Gender)
                .HasDefaultValue((byte)2)
                .HasColumnName("gender");
            entity.Property(e => e.GithubUrl)
                .HasMaxLength(500)
                .HasColumnName("github_url");
            entity.Property(e => e.Gpa)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("gpa");
            entity.Property(e => e.JobSeekingStatus)
                .HasDefaultValue((byte)1)
                .HasColumnName("job_seeking_status");
            entity.Property(e => e.LinkedinUrl)
                .HasMaxLength(500)
                .HasColumnName("linkedin_url");
            entity.Property(e => e.Major)
                .HasMaxLength(150)
                .HasColumnName("major");
            entity.Property(e => e.PortfolioUrl)
                .HasMaxLength(500)
                .HasColumnName("portfolio_url");
            entity.Property(e => e.StudentCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("student_code");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.StudentProfile)
                .HasForeignKey<StudentProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_student_profiles_user");
        });

        modelBuilder.Entity<StudentSkill>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.SkillId });

            entity.ToTable("student_skills", "student");

            entity.HasIndex(e => new { e.SkillId, e.ProficiencyLevel }, "IX_student_skills_lookup");

            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.ProficiencyLevel).HasColumnName("proficiency_level");
            entity.Property(e => e.YearsOfExperience)
                .HasColumnType("decimal(3, 1)")
                .HasColumnName("years_of_experience");

            entity.HasOne(d => d.Skill).WithMany(p => p.StudentSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ss_skill");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentSkills)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK_ss_student");
        });

        modelBuilder.Entity<Term>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_internship_terms");

            entity.ToTable("terms", "internship");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcademicYear)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("academic_year");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Semester).HasColumnName("semester");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
        });

        modelBuilder.Entity<UniversityOfficer>(entity =>
        {
            entity.ToTable("university_officers", "internship");

            entity.HasIndex(e => e.OfficerCode, "UQ_uo_code").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_uo_user").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FacultyDepartment)
                .HasMaxLength(150)
                .HasColumnName("faculty_department");
            entity.Property(e => e.OfficerCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("officer_code");
            entity.Property(e => e.PositionTitle)
                .HasMaxLength(100)
                .HasColumnName("position_title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.UniversityOfficer)
                .HasForeignKey<UniversityOfficer>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_uo_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", "auth");

            entity.HasIndex(e => e.Email, "UQ_users_email").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_users_username").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.ToTable("user_roles", "auth");

            entity.HasIndex(e => e.UserId, "IX_user_roles_user_id");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("assigned_at");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_ur_role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ur_user");
        });

        modelBuilder.Entity<WeeklyReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__weekly_r__3213E83FD0A01444");

            entity.ToTable("weekly_reports", "internship");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InternshipRecordId).HasColumnName("internship_record_id");
            entity.Property(e => e.Status)
                .HasDefaultValue((byte)1)
                .HasColumnName("status");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("submitted_at");
            entity.Property(e => e.SupervisorFeedback).HasColumnName("supervisor_feedback");
            entity.Property(e => e.WeekNumber).HasColumnName("week_number");
            entity.Property(e => e.WorkSummary).HasColumnName("work_summary");

            entity.HasOne(d => d.InternshipRecord).WithMany(p => p.WeeklyReports)
                .HasForeignKey(d => d.InternshipRecordId)
                .HasConstraintName("FK__weekly_re__inter__40F9A68C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
