using Microsoft.EntityFrameworkCore.Migrations;

public partial class CreateChuyenXeImagesTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ChuyenXeImage",
            columns: table => new
            {
                ImageId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ChuyenId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChuyenXeImage", x => x.ImageId);
                table.ForeignKey(
                    name: "FK_ChuyenXeImage_ChuyenXe_ChuyenId",
                    column: x => x.ChuyenId,
                    principalTable: "ChuyenXe",
                    principalColumn: "ChuyenId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChuyenXeImage_ChuyenId",
            table: "ChuyenXeImage",
            column: "ChuyenId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ChuyenXeImage");
    }
}