using Microsoft.EntityFrameworkCore.Migrations;

namespace ItemTTT.Models.Migrations
{
    public partial class TreeNode01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TreeNode",
                columns: table => new
                {
                    TreeNodeID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(maxLength: 4000, nullable: false),
                    ParentPath = table.Column<string>(maxLength: 4000, nullable: true),
                    Meta = table.Column<string>(maxLength: 4000, nullable: false),
                    Data = table.Column<string>(type: "varchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreeNode", x => x.TreeNodeID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TreeNode_Path",
                table: "TreeNode",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreeNode_ParentPath_Meta",
                table: "TreeNode",
                columns: new[] { "ParentPath", "Meta" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TreeNode");
        }
    }
}
