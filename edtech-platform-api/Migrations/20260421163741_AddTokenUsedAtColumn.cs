using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace edtech_platform_api.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenUsedAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if UsedAt exists, rename it. Otherwise add TokenUsedAt as new column
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'PasswordResetTokens' 
                        AND column_name = 'UsedAt'
                    ) THEN
                        ALTER TABLE ""PasswordResetTokens"" 
                        RENAME COLUMN ""UsedAt"" TO ""TokenUsedAt"";
                    ELSIF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'PasswordResetTokens' 
                        AND column_name = 'TokenUsedAt'
                    ) THEN
                        ALTER TABLE ""PasswordResetTokens"" 
                        ADD COLUMN ""TokenUsedAt"" timestamp with time zone NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenUsedAt",
                table: "PasswordResetTokens",
                newName: "UsedAt");
        }
    }
}
