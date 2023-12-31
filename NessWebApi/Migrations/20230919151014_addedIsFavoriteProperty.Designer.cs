﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NessWebApi.Data;

#nullable disable

namespace NessWebApi.Migrations
{
    [DbContext(typeof(DbContextNessApp))]
    [Migration("20230919151014_addedIsFavoriteProperty")]
    partial class addedIsFavoriteProperty
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("NessWebApi.Models.Event", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Author")
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("DurationHours")
                        .HasColumnType("real");

                    b.Property<DateTime>("EndDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Location")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("createdBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("eventLink")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("isDraft")
                        .HasColumnType("bit");

                    b.Property<bool>("isFavorite")
                        .HasColumnType("bit");

                    b.Property<bool>("isFree")
                        .HasColumnType("bit");

                    b.Property<bool>("isKidFriendly")
                        .HasColumnType("bit");

                    b.Property<bool>("isPetFriendly")
                        .HasColumnType("bit");

                    b.Property<string>("ticketLink")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("withTicket")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("NessWebApi.Models.UploadedFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UploadDateTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("UploadedFiles");
                });

            modelBuilder.Entity("NessWebApi.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ResetPasswordExpiry")
                        .HasColumnType("datetime2");

                    b.Property<string>("ResetPasswordToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Token")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("NessWebApi.Models.UserFavoriteEvents", b =>
                {
                    b.Property<int>("UserFavoriteEventsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserFavoriteEventsId"));

                    b.Property<int>("EventId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("UserFavoriteEventsId");

                    b.HasIndex("EventId");

                    b.HasIndex("UserId");

                    b.ToTable("UserFavoriteEvents");
                });

            modelBuilder.Entity("NessWebApi.Models.Event", b =>
                {
                    b.HasOne("NessWebApi.Models.User", null)
                        .WithMany("FavoriteEvents")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("NessWebApi.Models.UserFavoriteEvents", b =>
                {
                    b.HasOne("NessWebApi.Models.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NessWebApi.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("User");
                });

            modelBuilder.Entity("NessWebApi.Models.User", b =>
                {
                    b.Navigation("FavoriteEvents");
                });
#pragma warning restore 612, 618
        }
    }
}
