CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'automation') THEN
            CREATE SCHEMA automation;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'core') THEN
            CREATE SCHEMA core;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'identity') THEN
            CREATE SCHEMA identity;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE automation.audit_logs (
        "Id" uuid NOT NULL,
        "Actor" character varying(180) NOT NULL,
        "Action" character varying(180) NOT NULL,
        "EntityName" character varying(180) NOT NULL,
        "EntityId" character varying(80),
        "DetailsJson" jsonb,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_audit_logs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE core.categories (
        "Id" uuid NOT NULL,
        "Name" character varying(140) NOT NULL,
        "Slug" character varying(180) NOT NULL,
        "Description" character varying(1000),
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_categories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE core.configurations (
        "Id" uuid NOT NULL,
        "Key" character varying(180) NOT NULL,
        "Value" character varying(4000) NOT NULL,
        "Description" character varying(1000),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_configurations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE core.customers (
        "Id" uuid NOT NULL,
        "FullName" character varying(180) NOT NULL,
        "Email" character varying(180),
        "Phone" character varying(80),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_customers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE automation.notifications (
        "Id" uuid NOT NULL,
        "Channel" character varying(80) NOT NULL,
        "Recipient" character varying(180) NOT NULL,
        "Message" character varying(2000) NOT NULL,
        "SentAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_notifications" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE identity.permissions (
        "Id" uuid NOT NULL,
        "Code" character varying(160) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_permissions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE identity.refresh_tokens (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TokenHash" character varying(500) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_refresh_tokens" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE identity.roles (
        "Id" uuid NOT NULL,
        "Name" character varying(120) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_roles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE identity.users (
        "Id" uuid NOT NULL,
        "Email" character varying(180) NOT NULL,
        "PasswordHash" character varying(500) NOT NULL,
        "FullName" character varying(180) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE automation.workflow_events (
        "Id" uuid NOT NULL,
        "Name" character varying(180) NOT NULL,
        "PayloadJson" jsonb NOT NULL,
        "OccurredAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_workflow_events" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE core.products (
        "Id" uuid NOT NULL,
        "Name" character varying(180) NOT NULL,
        "Slug" character varying(220) NOT NULL,
        "Description" character varying(2000),
        "Sku" character varying(80),
        "Price" numeric(18,2) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CategoryId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_products" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_products_categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES core.categories ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE core.orders (
        "Id" uuid NOT NULL,
        "CustomerId" uuid,
        "Status" character varying(80) NOT NULL,
        "Total" numeric(18,2) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_orders" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_orders_customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES core.customers ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE core.quotes (
        "Id" uuid NOT NULL,
        "CustomerId" uuid,
        "Status" character varying(80) NOT NULL,
        "Total" numeric(18,2) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_quotes" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_quotes_customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES core.customers ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE TABLE core.product_images (
        "Id" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "Url" character varying(1000) NOT NULL,
        "AltText" character varying(250),
        "SortOrder" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_product_images" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_product_images_products_ProductId" FOREIGN KEY ("ProductId") REFERENCES core.products ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_categories_Slug" ON core.categories ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_configurations_Key" ON core.configurations ("Key");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE INDEX "IX_orders_CustomerId" ON core.orders ("CustomerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_permissions_Code" ON identity.permissions ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE INDEX "IX_product_images_ProductId" ON core.product_images ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE INDEX "IX_products_CategoryId" ON core.products ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_products_Sku" ON core.products ("Sku");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_products_Slug" ON core.products ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE INDEX "IX_quotes_CustomerId" ON core.quotes ("CustomerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_refresh_tokens_TokenHash" ON identity.refresh_tokens ("TokenHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_roles_Name" ON identity.roles ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_users_Email" ON identity.users ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260603033801_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260603033801_InitialCreate', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604000000_AddCategoryImages') THEN
    CREATE TABLE core.category_images (
        "Id" uuid NOT NULL,
        "CategoryId" uuid NOT NULL,
        "Url" character varying(1000) NOT NULL,
        "StoragePath" character varying(1000) NOT NULL,
        "StorageProvider" character varying(80) NOT NULL,
        "FileName" character varying(260) NOT NULL,
        "ContentType" character varying(120) NOT NULL,
        "SizeBytes" bigint NOT NULL,
        "AltText" character varying(250),
        "SortOrder" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_category_images" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_category_images_categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES core.categories ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604000000_AddCategoryImages') THEN
    CREATE INDEX "IX_category_images_CategoryId_SortOrder" ON core.category_images ("CategoryId", "SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604000000_AddCategoryImages') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260604000000_AddCategoryImages', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    CREATE TABLE identity.role_permissions (
        "RoleId" uuid NOT NULL,
        "PermissionId" uuid NOT NULL,
        CONSTRAINT "PK_role_permissions" PRIMARY KEY ("RoleId", "PermissionId"),
        CONSTRAINT "FK_role_permissions_permissions_PermissionId" FOREIGN KEY ("PermissionId") REFERENCES identity.permissions ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_role_permissions_roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES identity.roles ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    CREATE TABLE identity.user_roles (
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        CONSTRAINT "PK_user_roles" PRIMARY KEY ("UserId", "RoleId"),
        CONSTRAINT "FK_user_roles_roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES identity.roles ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_user_roles_users_UserId" FOREIGN KEY ("UserId") REFERENCES identity.users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    CREATE INDEX "IX_role_permissions_PermissionId" ON identity.role_permissions ("PermissionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    CREATE INDEX "IX_user_roles_RoleId" ON identity.user_roles ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.roles ("Id", "Name", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('11111111-1111-1111-1111-111111111111', 'Admin', 'Administracion completa de AMAS.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222201', 'admin.full_access', 'Acceso completo al administrador.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222201')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222202', 'products.read', 'Ver inventario.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222202')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222203', 'products.create', 'Crear productos.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222203')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222204', 'products.update', 'Actualizar productos.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222204')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222205', 'products.delete', 'Eliminar productos.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222205')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222206', 'categories.read', 'Ver categorias.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222206')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222207', 'categories.create', 'Crear categorias.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222207')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222208', 'categories.update', 'Actualizar categorias.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222208')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222209', 'categories.delete', 'Eliminar categorias.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222209')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222210', 'content.read', 'Ver contenido de landing.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222210')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222211', 'content.update', 'Actualizar contenido de landing.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222211')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222212', 'images.read', 'Ver banco de imagenes.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222212')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222213', 'images.create', 'Cargar imagenes.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222213')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222214', 'users.read', 'Ver usuarios y roles.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222214')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222215', 'users.create', 'Crear usuarios.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222215')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222216', 'users.update', 'Actualizar usuarios.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222216')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222217', 'roles.create', 'Crear roles.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222217')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222218', 'roles.update', 'Actualizar roles.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222218')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260605000000_AddIdentityRolesPermissions') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260605000000_AddIdentityRolesPermissions', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610171313_AddInventory') THEN
    CREATE TABLE core.inventory_items (
        "Id" uuid NOT NULL,
        "ProductId" uuid,
        "Name" character varying(180) NOT NULL,
        "Sku" character varying(80) NOT NULL,
        "Type" character varying(40) NOT NULL,
        "Unit" character varying(40) NOT NULL,
        "CurrentStock" numeric(18,3) NOT NULL,
        "MinimumStock" numeric(18,3) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_inventory_items" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_inventory_items_products_ProductId" FOREIGN KEY ("ProductId") REFERENCES core.products ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610171313_AddInventory') THEN
    CREATE TABLE core.inventory_movements (
        "Id" uuid NOT NULL,
        "InventoryItemId" uuid NOT NULL,
        "MovementType" character varying(40) NOT NULL,
        "Quantity" numeric(18,3) NOT NULL,
        "StockAfter" numeric(18,3) NOT NULL,
        "UnitCost" numeric(18,2),
        "Reason" character varying(500),
        "Reference" character varying(160),
        "OccurredAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_inventory_movements" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_inventory_movements_inventory_items_InventoryItemId" FOREIGN KEY ("InventoryItemId") REFERENCES core.inventory_items ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610171313_AddInventory') THEN
    CREATE UNIQUE INDEX "IX_inventory_items_ProductId" ON core.inventory_items ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610171313_AddInventory') THEN
    CREATE UNIQUE INDEX "IX_inventory_items_Sku" ON core.inventory_items ("Sku");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610171313_AddInventory') THEN
    CREATE INDEX "IX_inventory_movements_InventoryItemId_OccurredAt" ON core.inventory_movements ("InventoryItemId", "OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260610171313_AddInventory') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260610171313_AddInventory', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611170105_AddInvoiceImports') THEN
    CREATE TABLE core.inventory_invoice_imports (
        "Id" uuid NOT NULL,
        "Status" character varying(40) NOT NULL,
        "OriginalFileName" character varying(260) NOT NULL,
        "StoredFileName" character varying(260) NOT NULL,
        "ContentType" character varying(120) NOT NULL,
        "SizeBytes" bigint NOT NULL,
        "StorageProvider" character varying(80) NOT NULL,
        "StoragePath" character varying(1000) NOT NULL,
        "Url" character varying(1000) NOT NULL,
        "SupplierName" character varying(180),
        "InvoiceNumber" character varying(120),
        "InvoiceDate" timestamp with time zone,
        "Subtotal" numeric(18,2),
        "TaxTotal" numeric(18,2),
        "Total" numeric(18,2),
        "Notes" character varying(1000),
        "CreatedBy" character varying(180) NOT NULL,
        "ConfirmedAt" timestamp with time zone,
        "ConfirmedBy" character varying(180),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_inventory_invoice_imports" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611170105_AddInvoiceImports') THEN
    CREATE TABLE core.inventory_invoice_import_lines (
        "Id" uuid NOT NULL,
        "InventoryInvoiceImportId" uuid NOT NULL,
        "InventoryItemId" uuid,
        "LineNumber" integer NOT NULL,
        "Status" character varying(40) NOT NULL,
        "MatchStatus" character varying(40) NOT NULL,
        "MatchConfidence" integer NOT NULL,
        "RawText" character varying(1000),
        "ExtractedSku" character varying(120),
        "ExtractedName" character varying(260) NOT NULL,
        "Quantity" numeric(18,3) NOT NULL,
        "UnitCost" numeric(18,2),
        "LineTotal" numeric(18,2),
        "Notes" character varying(1000),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_inventory_invoice_import_lines" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_inventory_invoice_import_lines_inventory_invoice_imports_In~" FOREIGN KEY ("InventoryInvoiceImportId") REFERENCES core.inventory_invoice_imports ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_inventory_invoice_import_lines_inventory_items_InventoryIte~" FOREIGN KEY ("InventoryItemId") REFERENCES core.inventory_items ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611170105_AddInvoiceImports') THEN
    CREATE UNIQUE INDEX "IX_inventory_invoice_import_lines_InventoryInvoiceImportId_Lin~" ON core.inventory_invoice_import_lines ("InventoryInvoiceImportId", "LineNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611170105_AddInvoiceImports') THEN
    CREATE INDEX "IX_inventory_invoice_import_lines_InventoryItemId" ON core.inventory_invoice_import_lines ("InventoryItemId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611170105_AddInvoiceImports') THEN
    CREATE INDEX "IX_inventory_invoice_imports_InvoiceNumber" ON core.inventory_invoice_imports ("InvoiceNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611170105_AddInvoiceImports') THEN
    CREATE INDEX "IX_inventory_invoice_imports_Status" ON core.inventory_invoice_imports ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611170105_AddInvoiceImports') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611170105_AddInvoiceImports', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611191221_AddInvoiceExtractionJson') THEN
    ALTER TABLE core.inventory_invoice_imports ADD "ExtractedJson" jsonb;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611191221_AddInvoiceExtractionJson') THEN
    ALTER TABLE core.inventory_invoice_imports ADD "ExtractionProvider" character varying(80);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611191221_AddInvoiceExtractionJson') THEN
    ALTER TABLE core.inventory_invoice_import_lines ADD "TaxAmount" numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611191221_AddInvoiceExtractionJson') THEN
    ALTER TABLE core.inventory_invoice_import_lines ADD "TaxPercent" numeric(7,3);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611191221_AddInvoiceExtractionJson') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611191221_AddInvoiceExtractionJson', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611203000_SeedInventoryPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222219', 'inventory.read', 'Ver Kardex de inventario y movimientos.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222219')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611203000_SeedInventoryPermissions') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222220', 'inventory.invoices.read', 'Ver y gestionar facturas de entrada.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222220')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611203000_SeedInventoryPermissions') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611203000_SeedInventoryPermissions', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    ALTER TABLE core.inventory_invoice_imports ADD "SupplierId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    ALTER TABLE core.inventory_invoice_imports ADD "SupplierTaxId" character varying(80);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    CREATE TABLE core.suppliers (
        "Id" uuid NOT NULL,
        "Name" character varying(180) NOT NULL,
        "TaxId" character varying(80),
        "ContactName" character varying(180),
        "Email" character varying(180),
        "Phone" character varying(80),
        "Address" character varying(260),
        "City" character varying(120),
        "Country" character varying(120),
        "CategoryId" uuid,
        "Status" character varying(40) NOT NULL,
        "Notes" character varying(1000),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_suppliers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_suppliers_categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES core.categories ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    CREATE INDEX "IX_inventory_invoice_imports_SupplierId" ON core.inventory_invoice_imports ("SupplierId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    CREATE INDEX "IX_suppliers_CategoryId" ON core.suppliers ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    CREATE INDEX "IX_suppliers_Name" ON core.suppliers ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    CREATE UNIQUE INDEX "IX_suppliers_TaxId" ON core.suppliers ("TaxId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    ALTER TABLE core.inventory_invoice_imports ADD CONSTRAINT "FK_inventory_invoice_imports_suppliers_SupplierId" FOREIGN KEY ("SupplierId") REFERENCES core.suppliers ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222221', 'suppliers.read', 'Ver proveedores.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222221')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222222', 'suppliers.create', 'Crear proveedores.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222223', 'suppliers.update', 'Actualizar proveedores.', NOW(), NULL)
    ON CONFLICT ("Id") DO NOTHING;

    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
    VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222223')
    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612194106_AddSuppliers') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260612194106_AddSuppliers', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    ALTER TABLE "core"."suppliers" ADD COLUMN "SupplierNumber" bigint;
    CREATE SEQUENCE "core"."suppliers_suppliernumber_seq" START WITH 1001 INCREMENT BY 1;

    WITH numbered AS (
        SELECT "Id", nextval('"core"."suppliers_suppliernumber_seq"') AS value
        FROM "core"."suppliers"
        ORDER BY "CreatedAt", "Id"
    )
    UPDATE "core"."suppliers" target
    SET "SupplierNumber" = numbered.value
    FROM numbered
    WHERE target."Id" = numbered."Id";

    ALTER TABLE "core"."suppliers" ALTER COLUMN "SupplierNumber" SET NOT NULL;
    ALTER TABLE "core"."suppliers" ALTER COLUMN "SupplierNumber" SET DEFAULT nextval('"core"."suppliers_suppliernumber_seq"');
    ALTER SEQUENCE "core"."suppliers_suppliernumber_seq" OWNED BY "core"."suppliers"."SupplierNumber";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    ALTER TABLE "core"."products" ADD COLUMN "ProductNumber" bigint;
    CREATE SEQUENCE "core"."products_productnumber_seq" START WITH 1001 INCREMENT BY 1;

    WITH numbered AS (
        SELECT "Id", nextval('"core"."products_productnumber_seq"') AS value
        FROM "core"."products"
        ORDER BY "CreatedAt", "Id"
    )
    UPDATE "core"."products" target
    SET "ProductNumber" = numbered.value
    FROM numbered
    WHERE target."Id" = numbered."Id";

    ALTER TABLE "core"."products" ALTER COLUMN "ProductNumber" SET NOT NULL;
    ALTER TABLE "core"."products" ALTER COLUMN "ProductNumber" SET DEFAULT nextval('"core"."products_productnumber_seq"');
    ALTER SEQUENCE "core"."products_productnumber_seq" OWNED BY "core"."products"."ProductNumber";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    ALTER TABLE "core"."inventory_movements" ADD COLUMN "InventoryMovementNumber" bigint;
    CREATE SEQUENCE "core"."inventory_movements_inventorymovementnumber_seq" START WITH 1001 INCREMENT BY 1;

    WITH numbered AS (
        SELECT "Id", nextval('"core"."inventory_movements_inventorymovementnumber_seq"') AS value
        FROM "core"."inventory_movements"
        ORDER BY "CreatedAt", "Id"
    )
    UPDATE "core"."inventory_movements" target
    SET "InventoryMovementNumber" = numbered.value
    FROM numbered
    WHERE target."Id" = numbered."Id";

    ALTER TABLE "core"."inventory_movements" ALTER COLUMN "InventoryMovementNumber" SET NOT NULL;
    ALTER TABLE "core"."inventory_movements" ALTER COLUMN "InventoryMovementNumber" SET DEFAULT nextval('"core"."inventory_movements_inventorymovementnumber_seq"');
    ALTER SEQUENCE "core"."inventory_movements_inventorymovementnumber_seq" OWNED BY "core"."inventory_movements"."InventoryMovementNumber";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    ALTER TABLE "core"."inventory_items" ADD COLUMN "InventoryItemNumber" bigint;
    CREATE SEQUENCE "core"."inventory_items_inventoryitemnumber_seq" START WITH 1001 INCREMENT BY 1;

    WITH numbered AS (
        SELECT "Id", nextval('"core"."inventory_items_inventoryitemnumber_seq"') AS value
        FROM "core"."inventory_items"
        ORDER BY "CreatedAt", "Id"
    )
    UPDATE "core"."inventory_items" target
    SET "InventoryItemNumber" = numbered.value
    FROM numbered
    WHERE target."Id" = numbered."Id";

    ALTER TABLE "core"."inventory_items" ALTER COLUMN "InventoryItemNumber" SET NOT NULL;
    ALTER TABLE "core"."inventory_items" ALTER COLUMN "InventoryItemNumber" SET DEFAULT nextval('"core"."inventory_items_inventoryitemnumber_seq"');
    ALTER SEQUENCE "core"."inventory_items_inventoryitemnumber_seq" OWNED BY "core"."inventory_items"."InventoryItemNumber";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    ALTER TABLE "core"."inventory_invoice_imports" ADD COLUMN "InvoiceImportNumber" bigint;
    CREATE SEQUENCE "core"."inventory_invoice_imports_invoiceimportnumber_seq" START WITH 1001 INCREMENT BY 1;

    WITH numbered AS (
        SELECT "Id", nextval('"core"."inventory_invoice_imports_invoiceimportnumber_seq"') AS value
        FROM "core"."inventory_invoice_imports"
        ORDER BY "CreatedAt", "Id"
    )
    UPDATE "core"."inventory_invoice_imports" target
    SET "InvoiceImportNumber" = numbered.value
    FROM numbered
    WHERE target."Id" = numbered."Id";

    ALTER TABLE "core"."inventory_invoice_imports" ALTER COLUMN "InvoiceImportNumber" SET NOT NULL;
    ALTER TABLE "core"."inventory_invoice_imports" ALTER COLUMN "InvoiceImportNumber" SET DEFAULT nextval('"core"."inventory_invoice_imports_invoiceimportnumber_seq"');
    ALTER SEQUENCE "core"."inventory_invoice_imports_invoiceimportnumber_seq" OWNED BY "core"."inventory_invoice_imports"."InvoiceImportNumber";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    ALTER TABLE "core"."categories" ADD COLUMN "CategoryNumber" bigint;
    CREATE SEQUENCE "core"."categories_categorynumber_seq" START WITH 1001 INCREMENT BY 1;

    WITH numbered AS (
        SELECT "Id", nextval('"core"."categories_categorynumber_seq"') AS value
        FROM "core"."categories"
        ORDER BY "CreatedAt", "Id"
    )
    UPDATE "core"."categories" target
    SET "CategoryNumber" = numbered.value
    FROM numbered
    WHERE target."Id" = numbered."Id";

    ALTER TABLE "core"."categories" ALTER COLUMN "CategoryNumber" SET NOT NULL;
    ALTER TABLE "core"."categories" ALTER COLUMN "CategoryNumber" SET DEFAULT nextval('"core"."categories_categorynumber_seq"');
    ALTER SEQUENCE "core"."categories_categorynumber_seq" OWNED BY "core"."categories"."CategoryNumber";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    CREATE UNIQUE INDEX "IX_suppliers_SupplierNumber" ON core.suppliers ("SupplierNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    CREATE UNIQUE INDEX "IX_products_ProductNumber" ON core.products ("ProductNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    CREATE UNIQUE INDEX "IX_inventory_movements_InventoryMovementNumber" ON core.inventory_movements ("InventoryMovementNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    CREATE UNIQUE INDEX "IX_inventory_items_InventoryItemNumber" ON core.inventory_items ("InventoryItemNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    CREATE UNIQUE INDEX "IX_inventory_invoice_imports_InvoiceImportNumber" ON core.inventory_invoice_imports ("InvoiceImportNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    CREATE UNIQUE INDEX "IX_categories_CategoryNumber" ON core.categories ("CategoryNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260616201951_AddVisibleNumbers') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260616201951_AddVisibleNumbers', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    DROP INDEX core."IX_product_images_ProductId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.product_images ADD "ContentType" character varying(120) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.product_images ADD "FileName" character varying(260) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.product_images ADD "IsPrimary" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.product_images ADD "SizeBytes" bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.product_images ADD "StoragePath" character varying(1000) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.product_images ADD "StorageProvider" character varying(80) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.inventory_items ADD "ImageContentType" character varying(120);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.inventory_items ADD "ImageFileName" character varying(260);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.inventory_items ADD "ImageSizeBytes" bigint;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.inventory_items ADD "ImageStoragePath" character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.inventory_items ADD "ImageStorageProvider" character varying(80);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    ALTER TABLE core.inventory_items ADD "ImageUrl" character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    CREATE TABLE core.product_categories (
        "Id" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "CategoryId" uuid NOT NULL,
        "SortOrder" integer NOT NULL,
        "IsFeatured" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_product_categories" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_product_categories_categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES core.categories ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_product_categories_products_ProductId" FOREIGN KEY ("ProductId") REFERENCES core.products ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    CREATE INDEX "IX_product_images_ProductId_IsPrimary" ON core.product_images ("ProductId", "IsPrimary");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    CREATE INDEX "IX_product_images_ProductId_SortOrder" ON core.product_images ("ProductId", "SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    CREATE UNIQUE INDEX "IX_product_categories_CategoryId_ProductId" ON core.product_categories ("CategoryId", "ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    CREATE INDEX "IX_product_categories_CategoryId_SortOrder" ON core.product_categories ("CategoryId", "SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    CREATE INDEX "IX_product_categories_ProductId" ON core.product_categories ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    INSERT INTO core.product_categories ("Id", "ProductId", "CategoryId", "SortOrder", "IsFeatured", "CreatedAt", "UpdatedAt")
    SELECT gen_random_uuid(), product_data."Id", product_data."CategoryId", product_data."SortOrder", FALSE, NOW(), NULL
    FROM (
        SELECT
            p."Id",
            p."CategoryId",
            ROW_NUMBER() OVER (PARTITION BY p."CategoryId" ORDER BY p."Name", p."CreatedAt")::integer AS "SortOrder"
        FROM core.products p
        WHERE p."CategoryId" IS NOT NULL
    ) product_data
    WHERE NOT EXISTS (
        SELECT 1
        FROM core.product_categories pc
        WHERE pc."ProductId" = product_data."Id"
          AND pc."CategoryId" = product_data."CategoryId"
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617201332_AddProductGalleryAndInventoryImage') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260617201332_AddProductGalleryAndInventoryImage', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223140_AddContactRequests') THEN
    CREATE TABLE core.contact_requests (
        "Id" uuid NOT NULL,
        "ContactRequestNumber" bigint GENERATED BY DEFAULT AS IDENTITY,
        "FullName" character varying(120) NOT NULL,
        "Email" character varying(180) NOT NULL,
        "Phone" character varying(80),
        "RequestType" character varying(80) NOT NULL,
        "Message" character varying(1200) NOT NULL,
        "SourcePage" character varying(260) NOT NULL,
        "Status" character varying(40) NOT NULL,
        "IpAddressHash" character varying(128),
        "UserAgent" character varying(500),
        "CaptchaProvider" character varying(80),
        "CaptchaTokenProvided" boolean NOT NULL,
        "WebhookDelivered" boolean NOT NULL,
        "WebhookDeliveredAt" timestamp with time zone,
        "WebhookError" character varying(1000),
        "ReviewedAt" timestamp with time zone,
        "ReviewedBy" character varying(180),
        "Notes" character varying(1000),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_contact_requests" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223140_AddContactRequests') THEN
    ALTER TABLE "core"."contact_requests" ALTER COLUMN "ContactRequestNumber" RESTART WITH 1001;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223140_AddContactRequests') THEN
    CREATE UNIQUE INDEX "IX_contact_requests_ContactRequestNumber" ON core.contact_requests ("ContactRequestNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223140_AddContactRequests') THEN
    CREATE INDEX "IX_contact_requests_CreatedAt" ON core.contact_requests ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223140_AddContactRequests') THEN
    CREATE INDEX "IX_contact_requests_Email" ON core.contact_requests ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223140_AddContactRequests') THEN
    CREATE INDEX "IX_contact_requests_Status" ON core.contact_requests ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223140_AddContactRequests') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260618223140_AddContactRequests', '10.0.4');
    END IF;
END $EF$;
COMMIT;

