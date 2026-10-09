using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace TestDrive_API.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiChatHistory> AiChatHistories { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingStatus> BookingStatuses { get; set; }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ChatSender> ChatSenders { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<FuelsType> FuelsTypes { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Transmission> Transmissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=kidrive;user=root;password=1234", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.44-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AiChatHistory>(entity =>
        {
            entity.HasKey(e => e.IdMessage).HasName("PRIMARY");

            entity.ToTable("ai_chat_history");

            entity.HasIndex(e => e.IdSender, "fk_chat_sender");

            entity.HasIndex(e => e.IdUser, "fk_chat_user_idx");

            entity.Property(e => e.IdMessage).HasColumnName("id_message");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IdSender).HasColumnName("id_sender");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.MessageText)
                .HasColumnType("text")
                .HasColumnName("message_text");

            entity.HasOne(d => d.IdSenderNavigation).WithMany(p => p.AiChatHistories)
                .HasForeignKey(d => d.IdSender)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_chat_sender");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.AiChatHistories)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("fk_chat_user");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.IdBooking).HasName("PRIMARY");

            entity.ToTable("bookings");

            entity.HasIndex(e => e.IdCar, "car_idx");

            entity.HasIndex(e => e.IdPayment, "payment_idx");

            entity.HasIndex(e => e.IdStatus, "status_idx");

            entity.HasIndex(e => e.IdUser, "user_idx");

            entity.Property(e => e.IdBooking).HasColumnName("id_booking");
            entity.Property(e => e.BookingDate).HasColumnName("booking_date");
            entity.Property(e => e.BookingTime)
                .HasColumnType("time")
                .HasColumnName("booking_time");
            entity.Property(e => e.IdCar).HasColumnName("id_car");
            entity.Property(e => e.IdPayment).HasColumnName("id_payment");
            entity.Property(e => e.IdStatus).HasColumnName("id_status");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdCarNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.IdCar)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("car");

            entity.HasOne(d => d.IdPaymentNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.IdPayment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pay");

            entity.HasOne(d => d.IdStatusNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.IdStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("status");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user");
        });

        modelBuilder.Entity<BookingStatus>(entity =>
        {
            entity.HasKey(e => e.IdStatus).HasName("PRIMARY");

            entity.ToTable("booking_statuses");

            entity.Property(e => e.IdStatus).HasColumnName("id_status");
            entity.Property(e => e.StatusName)
                .HasMaxLength(45)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.IdCar).HasName("PRIMARY");

            entity.ToTable("cars");

            entity.HasIndex(e => e.IdCategory, "cat_idx");

            entity.HasIndex(e => e.IdFuelType, "fuels_types_idx");

            entity.HasIndex(e => e.IdTreansmission, "transmissions_idx");

            entity.Property(e => e.IdCar).HasColumnName("id_car");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IdCategory).HasColumnName("id_category");
            entity.Property(e => e.IdFuelType).HasColumnName("id_fuel_type");
            entity.Property(e => e.IdTreansmission).HasColumnName("id_treansmission");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Model)
                .HasMaxLength(45)
                .HasColumnName("model");
            entity.Property(e => e.PowerHp)
                .HasComment("Мощность в л.с.")
                .HasColumnName("power_hp");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.YearCar)
                .HasColumnType("year")
                .HasColumnName("year_car");

            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.Cars)
                .HasForeignKey(d => d.IdCategory)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cat");

            entity.HasOne(d => d.IdFuelTypeNavigation).WithMany(p => p.Cars)
                .HasForeignKey(d => d.IdFuelType)
                .HasConstraintName("fuels_types");

            entity.HasOne(d => d.IdTreansmissionNavigation).WithMany(p => p.Cars)
                .HasForeignKey(d => d.IdTreansmission)
                .HasConstraintName("transmissions");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.IdCategory).HasName("PRIMARY");

            entity.ToTable("categories");

            entity.Property(e => e.IdCategory).HasColumnName("id_category");
            entity.Property(e => e.NameCategory)
                .HasMaxLength(45)
                .HasColumnName("name_category");
        });

        modelBuilder.Entity<ChatSender>(entity =>
        {
            entity.HasKey(e => e.IdSender).HasName("PRIMARY");

            entity.ToTable("chat_senders");

            entity.Property(e => e.IdSender).HasColumnName("id_sender");
            entity.Property(e => e.SenderName)
                .HasMaxLength(45)
                .HasColumnName("sender_name");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.IdFavorite).HasName("PRIMARY");

            entity.ToTable("favorites");

            entity.HasIndex(e => e.IdCar, "fk_fav_car_idx");

            entity.HasIndex(e => e.IdUser, "fk_fav_user_idx");

            entity.HasIndex(e => new { e.IdUser, e.IdCar }, "user_car_unique").IsUnique();

            entity.Property(e => e.IdFavorite).HasColumnName("id_favorite");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IdCar).HasColumnName("id_car");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdCarNavigation).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.IdCar)
                .HasConstraintName("fk_fav_car");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("fk_fav_user");
        });

        modelBuilder.Entity<FuelsType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("fuels_types");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(105)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.IdPaymentMethod).HasName("PRIMARY");

            entity.ToTable("payment_methods");

            entity.Property(e => e.IdPaymentMethod)
                .ValueGeneratedNever()
                .HasColumnName("id_payment_method");
            entity.Property(e => e.MethodName)
                .HasMaxLength(45)
                .HasColumnName("method_name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.RoleName)
                .HasMaxLength(45)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Transmission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transmissions");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(145)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.IdRole, "role_idx");

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.Account).HasColumnName("account");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(45)
                .HasColumnName("first_name");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.IsDelete).HasColumnName("is_delete");
            entity.Property(e => e.LastName)
                .HasMaxLength(45)
                .HasColumnName("last_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(45)
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
