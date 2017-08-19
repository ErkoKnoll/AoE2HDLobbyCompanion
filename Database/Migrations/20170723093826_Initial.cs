using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeyValuePairs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyValuePairs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lobbies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Age = table.Column<int>(nullable: false),
                    CheatsEnabled = table.Column<int>(nullable: false),
                    DataSet = table.Column<int>(nullable: false),
                    EndAge = table.Column<int>(nullable: false),
                    GameType = table.Column<int>(nullable: false),
                    GameVersion = table.Column<int>(nullable: false),
                    Joined = table.Column<DateTime>(nullable: false),
                    LatencyRegion = table.Column<int>(nullable: false),
                    LobbyElo = table.Column<int>(nullable: false),
                    LobbyId = table.Column<ulong>(nullable: false),
                    MapSize = table.Column<int>(nullable: false),
                    MapStyleType = table.Column<int>(nullable: false),
                    MapType = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    NegativeReputations = table.Column<int>(nullable: false),
                    Pop = table.Column<int>(nullable: false),
                    PositiveReputations = table.Column<int>(nullable: false),
                    Ranked = table.Column<int>(nullable: false),
                    Resource = table.Column<int>(nullable: false),
                    Sealed = table.Column<bool>(nullable: false),
                    SlotsFilled = table.Column<int>(nullable: false),
                    Speed = table.Column<int>(nullable: false),
                    Started = table.Column<DateTime>(nullable: true),
                    Victory = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lobbies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reputations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CommentRequired = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    OrderSequence = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reputations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Games = table.Column<int>(nullable: false),
                    GamesEndedDM = table.Column<int>(nullable: false),
                    GamesEndedRM = table.Column<int>(nullable: false),
                    GamesStartedDM = table.Column<int>(nullable: false),
                    GamesStartedRM = table.Column<int>(nullable: false),
                    GamesWonDM = table.Column<int>(nullable: false),
                    GamesWonRM = table.Column<int>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    NegativeReputation = table.Column<int>(nullable: false),
                    PositiveReputation = table.Column<int>(nullable: false),
                    ProfileDataFetched = table.Column<DateTime>(nullable: true),
                    ProfilePrivate = table.Column<bool>(nullable: false),
                    RankDM = table.Column<int>(nullable: false),
                    RankRM = table.Column<int>(nullable: false),
                    SteamId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LobbySlots",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GamesEndedDM = table.Column<int>(nullable: false),
                    GamesEndedRM = table.Column<int>(nullable: false),
                    GamesStartedDM = table.Column<int>(nullable: false),
                    GamesStartedRM = table.Column<int>(nullable: false),
                    GamesWonDM = table.Column<int>(nullable: false),
                    GamesWonRM = table.Column<int>(nullable: false),
                    LobbyId = table.Column<int>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Position = table.Column<int>(nullable: false),
                    RankDM = table.Column<int>(nullable: false),
                    RankRM = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LobbySlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LobbySlots_Lobbies_LobbyId",
                        column: x => x.LobbyId,
                        principalTable: "Lobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LobbySlots_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserReputations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Added = table.Column<DateTime>(nullable: false),
                    Comment = table.Column<string>(nullable: true),
                    LobbyId = table.Column<int>(nullable: true),
                    LobbySlotId = table.Column<int>(nullable: true),
                    ReputationId = table.Column<int>(nullable: true),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReputations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReputations_Lobbies_LobbyId",
                        column: x => x.LobbyId,
                        principalTable: "Lobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserReputations_LobbySlots_LobbySlotId",
                        column: x => x.LobbySlotId,
                        principalTable: "LobbySlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserReputations_Reputations_ReputationId",
                        column: x => x.ReputationId,
                        principalTable: "Reputations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserReputations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LobbySlots_LobbyId",
                table: "LobbySlots",
                column: "LobbyId");

            migrationBuilder.CreateIndex(
                name: "IX_LobbySlots_UserId",
                table: "LobbySlots",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReputations_LobbyId",
                table: "UserReputations",
                column: "LobbyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReputations_LobbySlotId",
                table: "UserReputations",
                column: "LobbySlotId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReputations_ReputationId",
                table: "UserReputations",
                column: "ReputationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReputations_UserId",
                table: "UserReputations",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyValuePairs");

            migrationBuilder.DropTable(
                name: "UserReputations");

            migrationBuilder.DropTable(
                name: "LobbySlots");

            migrationBuilder.DropTable(
                name: "Reputations");

            migrationBuilder.DropTable(
                name: "Lobbies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
