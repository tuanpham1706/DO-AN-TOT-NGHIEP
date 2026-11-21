using System.Data;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace VT3CyberZone.Server.Data;

public static class CafeShopSchemaInitializer
{
    private const string LegacyShopEmail = "legacy-shop@local.invalid";

    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS cafe_shops (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                name VARCHAR(120) NOT NULL,
                owner_name VARCHAR(100) NOT NULL,
                email VARCHAR(255) NOT NULL,
                phone VARCHAR(30) NULL,
                status VARCHAR(20) NOT NULL,
                license_key VARCHAR(64) NULL,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                approved_at DATETIME NULL,
                activated_at DATETIME NULL,
                UNIQUE KEY ix_cafe_shops_email (email),
                UNIQUE KEY ix_cafe_shops_license_key (license_key)
            )
            """);
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS platform_admins (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                username VARCHAR(50) NOT NULL,
                password_hash VARCHAR(255) NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT TRUE,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UNIQUE KEY ix_platform_admins_username (username)
            )
            """);
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS subscription_payments (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                cafe_shop_id INT NOT NULL,
                amount DECIMAL(12,2) NOT NULL,
                period_start DATE NOT NULL,
                period_end DATE NOT NULL,
                paid_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                note VARCHAR(255) NULL,
                recorded_by_admin_id INT NOT NULL,
                INDEX ix_subscription_payments_cafe_shop_id (cafe_shop_id),
                CONSTRAINT fk_subscription_payments_cafe_shops
                    FOREIGN KEY (cafe_shop_id) REFERENCES cafe_shops(id)
                    ON DELETE RESTRICT
            )
            """);
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS shop_events (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                cafe_shop_id INT NOT NULL,
                type VARCHAR(20) NOT NULL,
                app_version VARCHAR(50) NULL,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                INDEX ix_shop_events_cafe_shop_id_created_at (cafe_shop_id, created_at),
                CONSTRAINT fk_shop_events_cafe_shops
                    FOREIGN KEY (cafe_shop_id) REFERENCES cafe_shops(id)
                    ON DELETE CASCADE
            )
            """);

        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();
        await EnsureColumnAsync(connection, "cafe_shops", "activated_at", "DATETIME NULL");

        await using var findLegacyShop = connection.CreateCommand();
        findLegacyShop.CommandText =
            "SELECT id FROM cafe_shops WHERE email = @email LIMIT 1";
        var emailParameter = findLegacyShop.CreateParameter();
        emailParameter.ParameterName = "@email";
        emailParameter.Value = LegacyShopEmail;
        findLegacyShop.Parameters.Add(emailParameter);
        var legacyShopId = await findLegacyShop.ExecuteScalarAsync();
        if (legacyShopId is null or DBNull)
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO cafe_shops (name, owner_name, email, status)
                VALUES ('Quán hiện tại', 'Quản lý', {LegacyShopEmail}, 'ACTIVE')
                """);
            legacyShopId = await findLegacyShop.ExecuteScalarAsync();
        }

        var shopId = Convert.ToInt32(legacyShopId);
        await using (var license = connection.CreateCommand())
        {
            license.CommandText =
                "UPDATE cafe_shops SET license_key = @license WHERE id = @shopId AND license_key IS NULL";
            var licenseParameter = license.CreateParameter();
            licenseParameter.ParameterName = "@license";
            licenseParameter.Value = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
            license.Parameters.Add(licenseParameter);
            var shopIdParameter = license.CreateParameter();
            shopIdParameter.ParameterName = "@shopId";
            shopIdParameter.Value = shopId;
            license.Parameters.Add(shopIdParameter);
            await license.ExecuteNonQueryAsync();
        }

        foreach (var table in new[] { "users", "machine_groups", "transactions", "categories", "ingredients" })
            await EnsureShopIdColumnAsync(connection, table, shopId);

        foreach (var table in new[] { "users", "machine_groups", "transactions", "categories", "ingredients" })
        {
            await EnsureForeignKeyAsync(connection, table, shopId);
        }
    }

    private static async Task EnsureShopIdColumnAsync(
        System.Data.Common.DbConnection connection,
        string table,
        int defaultShopId)
    {
        await using var check = connection.CreateCommand();
        check.CommandText =
            """
            SELECT COUNT(*)
            FROM information_schema.columns
            WHERE table_schema = DATABASE()
              AND table_name = @table
              AND column_name = 'cafe_shop_id'
            """;
        var tableParameter = check.CreateParameter();
        tableParameter.ParameterName = "@table";
        tableParameter.Value = table;
        check.Parameters.Add(tableParameter);
        var exists = Convert.ToInt32(await check.ExecuteScalarAsync()) > 0;
        if (!exists)
        {
            await using var add = connection.CreateCommand();
            add.CommandText = $"ALTER TABLE `{table}` ADD COLUMN cafe_shop_id INT NULL";
            await add.ExecuteNonQueryAsync();
        }

        await using var backfill = connection.CreateCommand();
        backfill.CommandText = $"UPDATE `{table}` SET cafe_shop_id = @shopId WHERE cafe_shop_id IS NULL OR cafe_shop_id = 0";
        var shopIdParameter = backfill.CreateParameter();
        shopIdParameter.ParameterName = "@shopId";
        shopIdParameter.Value = defaultShopId;
        backfill.Parameters.Add(shopIdParameter);
        await backfill.ExecuteNonQueryAsync();

        await EnsureNotNullAsync(connection, table, "cafe_shop_id");
    }

    private static async Task EnsureColumnAsync(
        System.Data.Common.DbConnection connection,
        string table,
        string column,
        string definition)
    {
        await using var check = connection.CreateCommand();
        check.CommandText =
            """
            SELECT COUNT(*)
            FROM information_schema.columns
            WHERE table_schema = DATABASE()
              AND table_name = @table
              AND column_name = @column
            """;
        var tableParameter = check.CreateParameter();
        tableParameter.ParameterName = "@table";
        tableParameter.Value = table;
        check.Parameters.Add(tableParameter);
        var columnParameter = check.CreateParameter();
        columnParameter.ParameterName = "@column";
        columnParameter.Value = column;
        check.Parameters.Add(columnParameter);
        if (Convert.ToInt32(await check.ExecuteScalarAsync()) > 0)
            return;

        await using var add = connection.CreateCommand();
        add.CommandText = $"ALTER TABLE `{table}` ADD COLUMN `{column}` {definition}";
        await add.ExecuteNonQueryAsync();
    }

    private static async Task EnsureForeignKeyAsync(
        System.Data.Common.DbConnection connection,
        string table,
        int shopId)
    {
        await using (var backfill = connection.CreateCommand())
        {
            backfill.CommandText = $"UPDATE `{table}` SET cafe_shop_id = @shopId WHERE cafe_shop_id IS NULL OR cafe_shop_id = 0";
            var shopIdParameter = backfill.CreateParameter();
            shopIdParameter.ParameterName = "@shopId";
            shopIdParameter.Value = shopId;
            backfill.Parameters.Add(shopIdParameter);
            await backfill.ExecuteNonQueryAsync();
        }

        await EnsureNotNullAsync(connection, table, "cafe_shop_id");

        var constraint = $"fk_{table}_cafe_shops";
        await using var check = connection.CreateCommand();
        check.CommandText =
            """
            SELECT COUNT(*)
            FROM information_schema.table_constraints
            WHERE constraint_schema = DATABASE()
              AND table_name = @table
              AND constraint_name = @constraint
              AND constraint_type = 'FOREIGN KEY'
            """;
        var tableParameter = check.CreateParameter();
        tableParameter.ParameterName = "@table";
        tableParameter.Value = table;
        check.Parameters.Add(tableParameter);
        var constraintParameter = check.CreateParameter();
        constraintParameter.ParameterName = "@constraint";
        constraintParameter.Value = constraint;
        check.Parameters.Add(constraintParameter);
        if (Convert.ToInt32(await check.ExecuteScalarAsync()) == 0)
        {
            await using var addForeignKey = connection.CreateCommand();
            addForeignKey.CommandText =
                $"ALTER TABLE `{table}` ADD CONSTRAINT `{constraint}` FOREIGN KEY (cafe_shop_id) REFERENCES cafe_shops(id) ON DELETE RESTRICT";
            await addForeignKey.ExecuteNonQueryAsync();
        }
    }

    private static async Task EnsureNotNullAsync(
        System.Data.Common.DbConnection connection,
        string table,
        string column)
    {
        await using var check = connection.CreateCommand();
        check.CommandText =
            """
            SELECT is_nullable
            FROM information_schema.columns
            WHERE table_schema = DATABASE()
              AND table_name = @table
              AND column_name = @column
            """;
        var tableParameter = check.CreateParameter();
        tableParameter.ParameterName = "@table";
        tableParameter.Value = table;
        check.Parameters.Add(tableParameter);
        var columnParameter = check.CreateParameter();
        columnParameter.ParameterName = "@column";
        columnParameter.Value = column;
        check.Parameters.Add(columnParameter);
        var nullable = Convert.ToString(await check.ExecuteScalarAsync());
        if (nullable == "YES")
        {
            await using var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE `{table}` MODIFY `{column}` INT NOT NULL";
            await alter.ExecuteNonQueryAsync();
        }
    }
}
