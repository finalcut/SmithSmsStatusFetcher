using Microsoft.EntityFrameworkCore;

namespace SmithSmsStatusFetcher.Models
{
    public partial class SmithDbContext : DbContext
    {
        public SmithDbContext()
        {
        }

        public SmithDbContext(DbContextOptions<SmithDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AssigmentMessagesStatus> AssigmentMessagesStatus { get; set; }
        public virtual DbSet<AssignmentMessages> AssignmentMessages { get; set; }
        public virtual DbSet<DriveContacts> DriveContacts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssigmentMessagesStatus>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("assigment_messages_status");

                entity.Property(e => e.ContactId)
                    .HasColumnName("contact_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.MessageSid)
                    .HasColumnName("message_sid")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("latin1")
                    .HasCollation("latin1_general_ci");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("latin2")
                    .HasCollation("latin2_general_ci");
            });

            modelBuilder.Entity<AssignmentMessages>(entity =>
            {
                entity.ToTable("assignment_messages");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20) unsigned");

                entity.Property(e => e.AssignmentId)
                    .HasColumnName("assignment_id")
                    .HasColumnType("int(10) unsigned");

                entity.Property(e => e.Body)
                    .IsRequired()
                    .HasColumnName("body")
                    .HasColumnType("text")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.ContactId)
                    .HasColumnName("contact_id")
                    .HasColumnType("int(10) unsigned");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.Property(e => e.Incoming).HasColumnName("incoming");

                entity.Property(e => e.MessageSid)
                    .HasColumnName("message_sid")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Microtime)
                    .HasColumnName("microtime")
                    .HasColumnType("timestamp(6)")
                    .HasDefaultValueSql("'2020-04-20 03:20:13.000000'");

                entity.Property(e => e.ParticipantPhone)
                    .IsRequired()
                    .HasColumnName("participant_phone")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.ReplyId)
                    .HasColumnName("reply_id")
                    .HasColumnType("int(10) unsigned");

                entity.Property(e => e.ServicePhone)
                    .IsRequired()
                    .HasColumnName("service_phone")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("timestamp");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(10) unsigned");
            });

            modelBuilder.Entity<DriveContacts>(entity =>
            {
                entity.ToTable("drive_contacts");

                entity.HasIndex(e => e.PhoneNumber)
                    .HasName("drive_contacts_phone_number_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20) unsigned");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.Property(e => e.DriveId)
                    .HasColumnName("drive_id")
                    .HasColumnType("int(10) unsigned");

                entity.Property(e => e.ExternalId)
                    .HasColumnName("external_id")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("first_name")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Friends)
                    .HasColumnName("friends")
                    .HasColumnType("varchar(500)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasColumnName("phone_number")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("timestamp");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
