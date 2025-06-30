using EduSyncApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EduSyncApi.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Assessment> Assessments { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<ResultModel> ResultModel { get; set; }

    public virtual DbSet<UserModel> UserModel { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assessment>(entity =>
        {
            entity.Property(e => e.AssessmentId)
                .ValueGeneratedNever()
                .HasColumnName("AssessmentID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.Question)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.Course).WithMany(p => p.Assessment)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Assessments_Courses");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(e => e.CourseId)
                .ValueGeneratedNever()
                .HasColumnName("CourseID");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.InstructorId).HasColumnName("InstructorID");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("MediaURL");
            entity.Property(e => e.Title).HasMaxLength(500);

            entity.HasOne(d => d.Instructor).WithMany(p => p.Courses)
                .HasForeignKey(d => d.InstructorId)
                .HasConstraintName("FK_Courses_UserModel");
        });

        modelBuilder.Entity<ResultModel>(entity =>
        {
            entity.HasKey(e => e.ResultId);

            entity.ToTable("ResultModel");

            entity.Property(e => e.ResultId)
                .ValueGeneratedNever()
                .HasColumnName("ResultID");
            entity.Property(e => e.AssessmentId).HasColumnName("AssessmentID");
            entity.Property(e => e.AttemptDate).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Assessment).WithMany(p => p.ResultModel)
                .HasForeignKey(d => d.AssessmentId)
                .HasConstraintName("FK_ResultModel_Assessments");

            entity.HasOne(d => d.User).WithMany(p => p.ResultModel)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ResultModel_UserModel");
        });

        modelBuilder.Entity<UserModel>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("UserModel");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.Email)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Role)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
