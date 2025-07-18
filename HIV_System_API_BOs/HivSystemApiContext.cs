using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class HivSystemApiContext : DbContext
{
    public HivSystemApiContext()
    {
    }

    public HivSystemApiContext(DbContextOptions<HivSystemApiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<ArvMedicationDetail> ArvMedicationDetails { get; set; }

    public virtual DbSet<ArvMedicationTemplate> ArvMedicationTemplates { get; set; }

    public virtual DbSet<ArvRegimenTemplate> ArvRegimenTemplates { get; set; }

    public virtual DbSet<BlogImage> BlogImages { get; set; }

    public virtual DbSet<BlogReaction> BlogReactions { get; set; }

    public virtual DbSet<ComponentTestResult> ComponentTestResults { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<DoctorWorkSchedule> DoctorWorkSchedules { get; set; }

    public virtual DbSet<MedicalService> MedicalServices { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationAccount> NotificationAccounts { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientArvMedication> PatientArvMedications { get; set; }

    public virtual DbSet<PatientArvRegimen> PatientArvRegimen { get; set; }

    public virtual DbSet<PatientMedicalRecord> PatientMedicalRecords { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<SocialBlog> SocialBlogs { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer(GetConnectionString());

    private static string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config.GetConnectionString("HIVSystemDatabase");
        return strConn;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccId).HasName("PK__Account__91CBC3787EFE5B9B");

            entity.ToTable("Account");

            entity.HasIndex(e => e.AccUsername, "UQ__Account__2F0F28DDDE438899").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Account__A9D10534782DBC79").IsUnique();

            entity.Property(e => e.AccPassword).HasMaxLength(50);
            entity.Property(e => e.AccUsername).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Fullname).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.ApmId).HasName("PK__Appointm__8368D654E5AA9721");

            entity.ToTable("Appointment");

            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(d => d.Dct).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DctId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_Doctor");

            entity.HasOne(d => d.Ptn).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PtnId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_Patient");
        });

        modelBuilder.Entity<ArvMedicationDetail>(entity =>
        {
            entity.HasKey(e => e.AmdId).HasName("PK__ARV_Medi__FD94DA10F9732FF9");

            entity.ToTable("ARV_Medication_Detail");

            entity.HasIndex(e => e.MedName, "UQ__ARV_Medi__C9435A9B4BBDDE60").IsUnique();

            entity.Property(e => e.Dosage).HasMaxLength(20);
            entity.Property(e => e.Manufactorer).HasMaxLength(50);
            entity.Property(e => e.MedDescription).HasMaxLength(200);
            entity.Property(e => e.MedName).HasMaxLength(50);
        });

        modelBuilder.Entity<ArvMedicationTemplate>(entity =>
        {
            entity.HasKey(e => e.AmtId).HasName("PK__ARV_Medi__6A58991F30090AC9");

            entity.ToTable("ARV_Medication_Template");

            entity.HasOne(d => d.Amd).WithMany(p => p.ArvMedicationTemplates)
                .HasForeignKey(d => d.AmdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ARVMedTemplate_MedDetail");

            entity.HasOne(d => d.Art).WithMany(p => p.ArvMedicationTemplates)
                .HasForeignKey(d => d.ArtId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ARVMedTemplate_RegimenTemplate");
        });

        modelBuilder.Entity<ArvRegimenTemplate>(entity =>
        {
            entity.HasKey(e => e.ArtId).HasName("PK__ARV_Regi__3FB70AE668147B2C");

            entity.ToTable("ARV_Regimen_Template");

            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<BlogImage>(entity =>
        {
            entity.HasKey(e => e.ImgId).HasName("PK__Blog_Ima__352F54F358BF0066");

            entity.ToTable("Blog_Images");

            entity.HasIndex(e => e.SblId, "IX_BlogImages_SblId");

            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Sbl).WithMany(p => p.BlogImages)
                .HasForeignKey(d => d.SblId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BlogImages_SocialBlog");
        });

        modelBuilder.Entity<BlogReaction>(entity =>
        {
            entity.HasKey(e => e.BrtId).HasName("PK__Blog_Rea__AFCE435CC70FDE80");

            entity.ToTable("Blog_Reactions");

            entity.HasIndex(e => new { e.SblId, e.AccId }, "IX_BlogReactions_SblId_AccId");

            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.ReactedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Acc).WithMany(p => p.BlogReactions)
                .HasForeignKey(d => d.AccId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BlogReactions_Account");

            entity.HasOne(d => d.Sbl).WithMany(p => p.BlogReactions)
                .HasForeignKey(d => d.SblId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BlogReactions_SocialBlog");
        });

        modelBuilder.Entity<ComponentTestResult>(entity =>
        {
            entity.HasKey(e => e.CtrId).HasName("PK__Componen__23E51DA1EB60FA1E");

            entity.ToTable("Component_Test_Result");

            entity.Property(e => e.CtrDescription).HasMaxLength(200);
            entity.Property(e => e.CtrName).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ResultValue).HasMaxLength(20);

            entity.HasOne(d => d.Stf).WithMany(p => p.ComponentTestResults)
                .HasForeignKey(d => d.StfId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ComponentTestResult_Staff");

            entity.HasOne(d => d.Trs).WithMany(p => p.ComponentTestResults)
                .HasForeignKey(d => d.TrsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ComponentTestResult_TestResult");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DctId).HasName("PK__Doctor__EAC987724EA77AC7");

            entity.ToTable("Doctor");

            entity.HasIndex(e => e.AccId, "UQ__Doctor__91CBC37946032906").IsUnique();

            entity.Property(e => e.DctId).ValueGeneratedNever();
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Degree).HasMaxLength(100);

            entity.HasOne(d => d.Acc).WithOne(p => p.Doctor)
                .HasForeignKey<Doctor>(d => d.AccId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Doctor_Account");
        });

        modelBuilder.Entity<DoctorWorkSchedule>(entity =>
        {
            entity.HasKey(e => e.DwsId).HasName("PK__Doctor_W__9CB300CFBF5B7260");

            entity.ToTable("Doctor_Work_Schedule");

            entity.HasIndex(e => new { e.DoctorId, e.WorkDate, e.IsAvailable }, "IX_DoctorWorkSchedule_Doctor_Date_Available");

            entity.HasIndex(e => new { e.DoctorId, e.WorkDate, e.StartTime, e.EndTime }, "UQ_Doctor_Date_Time").IsUnique();

            entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(true)
                .HasColumnName("isAvailable");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.WorkDate)
                .HasDefaultValue(new DateOnly(2025, 1, 1))
                .HasColumnName("work_date");

            entity.HasOne(d => d.Doctor).WithMany(p => p.DoctorWorkSchedules)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Doctor_Work_Schedule");
        });

        modelBuilder.Entity<MedicalService>(entity =>
        {
            entity.HasKey(e => e.SrvId).HasName("PK__Medical___0367D1B1DD5DACBE");

            entity.ToTable("Medical_Service");

            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.ServiceDescription).HasMaxLength(200);
            entity.Property(e => e.ServiceName).HasMaxLength(50);

            entity.HasOne(d => d.Acc).WithMany(p => p.MedicalServices)
                .HasForeignKey(d => d.AccId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Service_Account");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NtfId).HasName("PK__Notifica__E23E8D0644D5F2C5");

            entity.Property(e => e.NotiMessage).HasMaxLength(300);
            entity.Property(e => e.NotiType).HasMaxLength(20);
            entity.Property(e => e.SendAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<NotificationAccount>(entity =>
        {
            entity.HasKey(e => e.NtaId).HasName("PK__Notifica__E1A7DA5324520B35");

            entity.ToTable("Notification_Account");

            entity.HasOne(d => d.Acc).WithMany(p => p.NotificationAccounts)
                .HasForeignKey(d => d.AccId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NotificationAccount_Account");

            entity.HasOne(d => d.Ntf).WithMany(p => p.NotificationAccounts)
                .HasForeignKey(d => d.NtfId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NotificationAccount_Notification");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PtnId).HasName("PK__Patient__72DC78D500687517");

            entity.ToTable("Patient");

            entity.HasIndex(e => e.AccId, "UQ__Patient__91CBC379B5D7299F").IsUnique();

            entity.Property(e => e.PtnId).ValueGeneratedNever();

            entity.HasOne(d => d.Acc).WithOne(p => p.Patient)
                .HasForeignKey<Patient>(d => d.AccId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Patient_Account");
        });

        modelBuilder.Entity<PatientArvMedication>(entity =>
        {
            entity.HasKey(e => e.PamId).HasName("PK__Patient___F2159F185082BC2E");

            entity.ToTable("Patient_ARV_Medication");

            entity.HasOne(d => d.Amd).WithMany(p => p.PatientArvMedications)
                .HasForeignKey(d => d.AmdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientARVMedication_ARVMedicationDetail");

            entity.HasOne(d => d.Par).WithMany(p => p.PatientArvMedications)
                .HasForeignKey(d => d.ParId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientARVMedication_PatientARVRegimen");
        });

        modelBuilder.Entity<PatientArvRegimen>(entity =>
        {
            entity.HasKey(e => e.ParId).HasName("PK__Patient___F35E4C9778DC8BF2");

            entity.ToTable("Patient_ARV_Regimen");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(d => d.Pmr).WithMany(p => p.PatientArvRegimen)
                .HasForeignKey(d => d.PmrId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientARVRegimen_PatientMedicalrecord");
        });

        modelBuilder.Entity<PatientMedicalRecord>(entity =>
        {
            entity.HasKey(e => e.PmrId).HasName("PK__Patient___169E747C27F4874B");

            entity.ToTable("Patient_Medical_Record");

            entity.HasIndex(e => e.PtnId, "UQ__Patient___72DC78D422F2C45C").IsUnique();

            entity.HasOne(d => d.Ptn).WithOne(p => p.PatientMedicalRecord)
                .HasForeignKey<PatientMedicalRecord>(d => d.PtnId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientMedicalRecord_Patient");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PayId).HasName("PK__Payment__EE8FCECF84F02A32");

            entity.ToTable("Payment", tb => tb.HasTrigger("TRG_Payment_Update"));

            entity.HasIndex(e => new { e.PmrId, e.PaymentDate }, "IX_Payment_Patient_Date");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentIntentId).HasMaxLength(30);
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Pmr).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PmrId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_PatientMedicalRecord");

            entity.HasOne(d => d.Srv).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SrvId)
                .HasConstraintName("FK_Payment_MedicalService");
        });

        modelBuilder.Entity<SocialBlog>(entity =>
        {
            entity.HasKey(e => e.SblId).HasName("PK__Social_B__93CA5D2E46E22F13");

            entity.ToTable("Social_Blog");

            entity.Property(e => e.IsAnonymous).HasDefaultValue(false);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PublishedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Acc).WithMany(p => p.SocialBlogs)
                .HasForeignKey(d => d.AccId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SocialBlog_Account");

            entity.HasOne(d => d.Stf).WithMany(p => p.SocialBlogs)
                .HasForeignKey(d => d.StfId)
                .HasConstraintName("FK_SocialBlog_Staff");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StfId).HasName("PK__Staff__5B506DEE6A5B8ED1");

            entity.HasIndex(e => e.AccId, "UQ__Staff__91CBC379988F6226").IsUnique();

            entity.Property(e => e.StfId).ValueGeneratedNever();
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Degree).HasMaxLength(100);

            entity.HasOne(d => d.Acc).WithOne(p => p.Staff)
                .HasForeignKey<Staff>(d => d.AccId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Staff_Account");
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.TrsId).HasName("PK__Test_Res__BE3DBA3A3CD09F38");

            entity.ToTable("Test_Result");

            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(d => d.Pmr).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.PmrId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestResult_PatientMedicalrecord");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
