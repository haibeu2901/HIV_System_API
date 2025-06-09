using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_BOs;

public partial class HivSystemContext : DbContext
{
    public HivSystemContext()
    {
    }

    public HivSystemContext(DbContextOptions<HivSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<ArvMedicationDetail> ArvMedicationDetails { get; set; }

    public virtual DbSet<ComponentTestResult> ComponentTestResults { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<DoctorPatientMr> DoctorPatientMrs { get; set; }

    public virtual DbSet<MedicalService> MedicalServices { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationAccount> NotificationAccounts { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientArvMedication> PatientArvMedications { get; set; }

    public virtual DbSet<PatientArvRegiman> PatientArvRegimen { get; set; }

    public virtual DbSet<PatientMedicalRecord> PatientMedicalRecords { get; set; }

    public virtual DbSet<SocialBlog> SocialBlogs { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=HIV_SYSTEM;Uid=sa;Pwd=12345;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccId).HasName("PK__Account__91CBC3780DDD1AB4");

            entity.ToTable("Account");

            entity.HasIndex(e => e.AccUsername, "UQ__Account__2F0F28DDF9287497").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Account__A9D105345B4B8372").IsUnique();

            entity.Property(e => e.AccPassword).HasMaxLength(50);
            entity.Property(e => e.AccUsername).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(30);
            entity.Property(e => e.Fullname).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.ApmId).HasName("PK__Appointm__8368D65429C93A96");

            entity.ToTable("Appointment");

            entity.Property(e => e.Notes).HasMaxLength(200);

            entity.HasOne(d => d.Dct).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DctId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_Doctor");

            entity.HasOne(d => d.Dpm).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DpmId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_DoctorPatientMR");
        });

        modelBuilder.Entity<ArvMedicationDetail>(entity =>
        {
            entity.HasKey(e => e.AmdId).HasName("PK__ARV_Medi__FD94DA10411DB165");

            entity.ToTable("ARV_Medication_Detail");

            entity.HasIndex(e => e.MedName, "UQ__ARV_Medi__C9435A9B5D66646A").IsUnique();

            entity.Property(e => e.Dosage).HasMaxLength(20);
            entity.Property(e => e.Manufactorer).HasMaxLength(50);
            entity.Property(e => e.MedDescription).HasMaxLength(200);
            entity.Property(e => e.MedName).HasMaxLength(50);
        });

        modelBuilder.Entity<ComponentTestResult>(entity =>
        {
            entity.HasKey(e => e.CtrId).HasName("PK__Componen__23E51DA115188797");

            entity.ToTable("Component_Test_Result");

            entity.Property(e => e.CtrDescription).HasMaxLength(200);
            entity.Property(e => e.CtrName).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(200);
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
            entity.HasKey(e => e.DctId).HasName("PK__Doctor__EAC98772E3B0E9BC");

            entity.ToTable("Doctor");

            entity.HasIndex(e => e.AccId, "UQ__Doctor__91CBC37976F2B40B").IsUnique();

            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Degree).HasMaxLength(100);

            entity.HasOne(d => d.Acc).WithOne(p => p.Doctor)
                .HasForeignKey<Doctor>(d => d.AccId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Doctor_Account");
        });

        modelBuilder.Entity<DoctorPatientMr>(entity =>
        {
            entity.HasKey(e => e.DpmId).HasName("PK__Doctor_P__45AA979AD8B23D09");

            entity.ToTable("Doctor_PatientMR");

            entity.HasOne(d => d.Dct).WithMany(p => p.DoctorPatientMrs)
                .HasForeignKey(d => d.DctId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DoctorPatientMR_Doctor");

            entity.HasOne(d => d.Pmr).WithMany(p => p.DoctorPatientMrs)
                .HasForeignKey(d => d.PmrId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DoctorPatientMR_PatientMedicalRecord");
        });

        modelBuilder.Entity<MedicalService>(entity =>
        {
            entity.HasKey(e => e.SrvId).HasName("PK__Medical___0367D1B14F0D5C95");

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
            entity.HasKey(e => e.NtfId).HasName("PK__Notifica__E1A7DA53E09E86B8");

            entity.Property(e => e.NotiMessage).HasMaxLength(300);
            entity.Property(e => e.NotiType).HasMaxLength(20);
            entity.Property(e => e.SendAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<NotificationAccount>(entity =>
        {
            entity.HasKey(e => e.NtaId).HasName("PK__Notifica__E23E8D06729D32B4");

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
            entity.HasKey(e => e.PtnId).HasName("PK__Patient__72DC78D5DF797206");

            entity.ToTable("Patient");

            entity.HasIndex(e => e.AccId, "UQ__Patient__91CBC379A37E1AEF").IsUnique();

            entity.Property(e => e.PtnId).ValueGeneratedNever();

            entity.HasOne(d => d.Acc).WithOne(p => p.Patient)
                .HasForeignKey<Patient>(d => d.AccId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Patient_Account");
        });

        modelBuilder.Entity<PatientArvMedication>(entity =>
        {
            entity.HasKey(e => e.PamId).HasName("PK__Patient___F2159F18F6F8A52A");

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

        modelBuilder.Entity<PatientArvRegiman>(entity =>
        {
            entity.HasKey(e => e.ParId).HasName("PK__Patient___F35E4C97A7C40061");

            entity.ToTable("Patient_ARV_Regimen");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(200);

            entity.HasOne(d => d.Pmr).WithMany(p => p.PatientArvRegimen)
                .HasForeignKey(d => d.PmrId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientARVRegimen_PatientMedicalRecord");
        });

        modelBuilder.Entity<PatientMedicalRecord>(entity =>
        {
            entity.HasKey(e => e.PmrId).HasName("PK__Patient___169E747CBC882240");

            entity.ToTable("Patient_Medical_Record");

            entity.HasIndex(e => e.PtnId, "UQ__Patient___72DC78D4504EAC4D").IsUnique();

            entity.HasOne(d => d.Ptn).WithOne(p => p.PatientMedicalRecord)
                .HasForeignKey<PatientMedicalRecord>(d => d.PtnId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientMedicalRecord_Patient");
        });

        modelBuilder.Entity<SocialBlog>(entity =>
        {
            entity.HasKey(e => e.SblId).HasName("PK__Social_B__93CA5D2E3D657D28");

            entity.ToTable("Social_Blog");

            entity.Property(e => e.IsAnonymous).HasDefaultValue(false);
            entity.Property(e => e.Notes).HasMaxLength(200);
            entity.Property(e => e.PublishedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(50);

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
            entity.HasKey(e => e.StfId).HasName("PK__Staff__5B506DEE935BA6A8");

            entity.HasIndex(e => e.AccId, "UQ__Staff__91CBC37988E4FDBA").IsUnique();

            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Degree).HasMaxLength(100);

            entity.HasOne(d => d.Acc).WithOne(p => p.Staff)
                .HasForeignKey<Staff>(d => d.AccId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Staff_Account");
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.TrsId).HasName("PK__Test_Res__BE3DBA3A38C2A3A9");

            entity.ToTable("Test_Result");

            entity.Property(e => e.Notes).HasMaxLength(200);

            entity.HasOne(d => d.Pmr).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.PmrId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestResult_PatientMedicalRecord");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
