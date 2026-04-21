using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace edtech_platform_api.Migrations
{
    /// <inheritdoc />
    public partial class SyncPasswordResetTokenModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "PasswordResetTokens" ALTER COLUMN "IsUsed" SET DEFAULT FALSE;""");
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_PasswordResetTokens_Users_UserId'
                    ) THEN
                        ALTER TABLE "PasswordResetTokens"
                        ADD CONSTRAINT "FK_PasswordResetTokens_Users_UserId"
                        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
                    END IF;
                END
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_PasswordResetTokens_Users_UserId'
                    ) THEN
                        ALTER TABLE "PasswordResetTokens"
                        DROP CONSTRAINT "FK_PasswordResetTokens_Users_UserId";
                    END IF;
                END
                $$;
                """);
            migrationBuilder.Sql("""ALTER TABLE "PasswordResetTokens" ALTER COLUMN "IsUsed" DROP DEFAULT;""");
        }
    }
}
