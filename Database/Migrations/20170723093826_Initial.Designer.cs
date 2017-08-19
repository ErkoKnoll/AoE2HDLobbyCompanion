using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Database;
using Database.Domain;

namespace Database.Migrations
{
    [DbContext(typeof(Repository))]
    [Migration("20170723093826_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("Database.Domain.KeyValuePair", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("KeyValuePairs");
                });

            modelBuilder.Entity("Database.Domain.Lobby", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Age");

                    b.Property<int>("CheatsEnabled");

                    b.Property<int>("DataSet");

                    b.Property<int>("EndAge");

                    b.Property<int>("GameType");

                    b.Property<int>("GameVersion");

                    b.Property<DateTime>("Joined");

                    b.Property<int>("LatencyRegion");

                    b.Property<int>("LobbyElo");

                    b.Property<ulong>("LobbyId");

                    b.Property<int>("MapSize");

                    b.Property<int>("MapStyleType");

                    b.Property<int>("MapType");

                    b.Property<string>("Name");

                    b.Property<int>("NegativeReputations");

                    b.Property<int>("Pop");

                    b.Property<int>("PositiveReputations");

                    b.Property<int>("Ranked");

                    b.Property<int>("Resource");

                    b.Property<bool>("Sealed");

                    b.Property<int>("SlotsFilled");

                    b.Property<int>("Speed");

                    b.Property<DateTime?>("Started");

                    b.Property<int>("Victory");

                    b.HasKey("Id");

                    b.ToTable("Lobbies");
                });

            modelBuilder.Entity("Database.Domain.LobbySlot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("GamesEndedDM");

                    b.Property<int>("GamesEndedRM");

                    b.Property<int>("GamesStartedDM");

                    b.Property<int>("GamesStartedRM");

                    b.Property<int>("GamesWonDM");

                    b.Property<int>("GamesWonRM");

                    b.Property<int?>("LobbyId");

                    b.Property<string>("Location");

                    b.Property<string>("Name");

                    b.Property<int>("Position");

                    b.Property<int>("RankDM");

                    b.Property<int>("RankRM");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("LobbyId");

                    b.HasIndex("UserId");

                    b.ToTable("LobbySlots");
                });

            modelBuilder.Entity("Database.Domain.Reputation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("CommentRequired");

                    b.Property<string>("Name");

                    b.Property<int>("OrderSequence");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.ToTable("Reputations");
                });

            modelBuilder.Entity("Database.Domain.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Games");

                    b.Property<int>("GamesEndedDM");

                    b.Property<int>("GamesEndedRM");

                    b.Property<int>("GamesStartedDM");

                    b.Property<int>("GamesStartedRM");

                    b.Property<int>("GamesWonDM");

                    b.Property<int>("GamesWonRM");

                    b.Property<string>("Location");

                    b.Property<string>("Name");

                    b.Property<int>("NegativeReputation");

                    b.Property<int>("PositiveReputation");

                    b.Property<DateTime?>("ProfileDataFetched");

                    b.Property<bool>("ProfilePrivate");

                    b.Property<int>("RankDM");

                    b.Property<int>("RankRM");

                    b.Property<ulong>("SteamId");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Database.Domain.UserReputation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Added");

                    b.Property<string>("Comment");

                    b.Property<int?>("LobbyId");

                    b.Property<int?>("LobbySlotId");

                    b.Property<int?>("ReputationId");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("LobbyId");

                    b.HasIndex("LobbySlotId");

                    b.HasIndex("ReputationId");

                    b.HasIndex("UserId");

                    b.ToTable("UserReputations");
                });

            modelBuilder.Entity("Database.Domain.LobbySlot", b =>
                {
                    b.HasOne("Database.Domain.Lobby", "Lobby")
                        .WithMany("Players")
                        .HasForeignKey("LobbyId");

                    b.HasOne("Database.Domain.User", "User")
                        .WithMany("LobbySlots")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Database.Domain.UserReputation", b =>
                {
                    b.HasOne("Database.Domain.Lobby", "Lobby")
                        .WithMany("Reputations")
                        .HasForeignKey("LobbyId");

                    b.HasOne("Database.Domain.LobbySlot", "LobbySlot")
                        .WithMany("Reputations")
                        .HasForeignKey("LobbySlotId");

                    b.HasOne("Database.Domain.Reputation", "Reputation")
                        .WithMany("UserReputations")
                        .HasForeignKey("ReputationId");

                    b.HasOne("Database.Domain.User", "User")
                        .WithMany("Reputations")
                        .HasForeignKey("UserId");
                });
        }
    }
}
