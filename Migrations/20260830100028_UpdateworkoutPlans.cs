using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateworkoutPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrainerId",
                table: "WorkoutPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrainerId",
                table: "NutritionPlans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_TrainerId",
                table: "WorkoutPlans",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionPlans_TrainerId",
                table: "NutritionPlans",
                column: "TrainerId");

            migrationBuilder.AddForeignKey(
                name: "FK_NutritionPlans_TrainerProfiles_TrainerId",
                table: "NutritionPlans",
                column: "TrainerId",
                principalTable: "TrainerProfiles",
                principalColumn: "TrainerID");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutPlans_TrainerProfiles_TrainerId",
                table: "WorkoutPlans",
                column: "TrainerId",
                principalTable: "TrainerProfiles",
                principalColumn: "TrainerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NutritionPlans_TrainerProfiles_TrainerId",
                table: "NutritionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutPlans_TrainerProfiles_TrainerId",
                table: "WorkoutPlans");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutPlans_TrainerId",
                table: "WorkoutPlans");

            migrationBuilder.DropIndex(
                name: "IX_NutritionPlans_TrainerId",
                table: "NutritionPlans");

            migrationBuilder.DropColumn(
                name: "TrainerId",
                table: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "TrainerId",
                table: "NutritionPlans");
        }
    }
}
