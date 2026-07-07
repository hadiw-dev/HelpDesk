IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Description] nvarchar(500) NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Department] nvarchar(100) NULL,
        [JobTitle] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [Priorities] (
        [Id] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Priorities] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [Statuses] (
        [Id] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Statuses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [ActivityLogs] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [Action] nvarchar(100) NOT NULL,
        [EntityName] nvarchar(100) NOT NULL,
        [EntityId] uniqueidentifier NULL,
        [Details] nvarchar(2000) NULL,
        [IpAddress] nvarchar(45) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ActivityLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [Tickets] (
        [Id] uniqueidentifier NOT NULL,
        [TicketNumber] nvarchar(20) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(4000) NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [PriorityId] uniqueidentifier NOT NULL,
        [StatusId] uniqueidentifier NOT NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [AssignedToUserId] uniqueidentifier NULL,
        [DueDate] datetime2 NULL,
        [ResolvedAt] datetime2 NULL,
        [ClosedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_Tickets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tickets_AspNetUsers_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tickets_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tickets_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tickets_Priorities_PriorityId] FOREIGN KEY ([PriorityId]) REFERENCES [Priorities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tickets_Statuses_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [Statuses] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [RelatedTicketId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Notifications_Tickets_RelatedTicketId] FOREIGN KEY ([RelatedTicketId]) REFERENCES [Tickets] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [TicketAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [TicketId] uniqueidentifier NOT NULL,
        [UploadedByUserId] uniqueidentifier NOT NULL,
        [FileName] nvarchar(260) NOT NULL,
        [StoredFileName] nvarchar(260) NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [FileSizeBytes] bigint NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_TicketAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TicketAttachments_AspNetUsers_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TicketAttachments_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [TicketComments] (
        [Id] uniqueidentifier NOT NULL,
        [TicketId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Content] nvarchar(4000) NOT NULL,
        [IsInternal] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_TicketComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TicketComments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TicketComments_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE TABLE [TicketHistories] (
        [Id] uniqueidentifier NOT NULL,
        [TicketId] uniqueidentifier NOT NULL,
        [ChangedByUserId] uniqueidentifier NULL,
        [FieldName] nvarchar(100) NOT NULL,
        [OldValue] nvarchar(1000) NULL,
        [NewValue] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_TicketHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TicketHistories_AspNetUsers_ChangedByUserId] FOREIGN KEY ([ChangedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TicketHistories_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Description', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] ON;
    EXEC(N'INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Description], [Name], [NormalizedName])
    VALUES (''a0000000-0000-0000-0000-000000000001'', N''a0000000-0000-0000-0000-000000000001'', N''Full system access, including user and configuration management.'', N''Admin'', N''ADMIN''),
    (''a0000000-0000-0000-0000-000000000002'', N''a0000000-0000-0000-0000-000000000002'', N''Handles, triages and resolves support tickets.'', N''IT Support Agent'', N''IT SUPPORT AGENT''),
    (''a0000000-0000-0000-0000-000000000003'', N''a0000000-0000-0000-0000-000000000003'', N''Submits and tracks their own support tickets.'', N''Employee'', N''EMPLOYEE''),
    (''a0000000-0000-0000-0000-000000000004'', N''a0000000-0000-0000-0000-000000000004'', N''Views team tickets and reporting dashboards.'', N''Manager'', N''MANAGER'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Description', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Categories]'))
        SET IDENTITY_INSERT [Categories] ON;
    EXEC(N'INSERT INTO [Categories] ([Id], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [DisplayOrder], [IsActive], [IsDeleted], [Name], [UpdatedAt], [UpdatedBy])
    VALUES (''c0000000-0000-0000-0000-000000000001'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Physical devices such as desktops, laptops, printers and peripherals.'', 1, CAST(1 AS bit), CAST(0 AS bit), N''Hardware'', NULL, NULL),
    (''c0000000-0000-0000-0000-000000000002'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Application installation, licensing and configuration issues.'', 2, CAST(1 AS bit), CAST(0 AS bit), N''Software'', NULL, NULL),
    (''c0000000-0000-0000-0000-000000000003'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Connectivity, VPN and network infrastructure issues.'', 3, CAST(1 AS bit), CAST(0 AS bit), N''Network'', NULL, NULL),
    (''c0000000-0000-0000-0000-000000000004'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Mailbox, delivery and email client issues.'', 4, CAST(1 AS bit), CAST(0 AS bit), N''Email'', NULL, NULL),
    (''c0000000-0000-0000-0000-000000000005'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Requests for new or changed system/application access.'', 5, CAST(1 AS bit), CAST(0 AS bit), N''Access Request'', NULL, NULL),
    (''c0000000-0000-0000-0000-000000000006'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Requests that do not fit another category.'', 6, CAST(1 AS bit), CAST(0 AS bit), N''Other'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Categories]'))
        SET IDENTITY_INSERT [Categories] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Priorities]'))
        SET IDENTITY_INSERT [Priorities] ON;
    EXEC(N'INSERT INTO [Priorities] ([Id], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [DisplayOrder], [IsActive], [IsDeleted], [Name], [UpdatedAt], [UpdatedBy])
    VALUES (''d0000000-0000-0000-0000-000000000001'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Minor issue with no significant impact on work.'', 1, CAST(1 AS bit), CAST(0 AS bit), N''Low'', NULL, NULL),
    (''d0000000-0000-0000-0000-000000000002'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Moderate impact; should be addressed in normal course of business.'', 2, CAST(1 AS bit), CAST(0 AS bit), N''Medium'', NULL, NULL),
    (''d0000000-0000-0000-0000-000000000003'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Significant impact on productivity; needs prompt attention.'', 3, CAST(1 AS bit), CAST(0 AS bit), N''High'', NULL, NULL),
    (''d0000000-0000-0000-0000-000000000004'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Severe impact; business operations are blocked.'', 4, CAST(1 AS bit), CAST(0 AS bit), N''Critical'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Priorities]'))
        SET IDENTITY_INSERT [Priorities] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Statuses]'))
        SET IDENTITY_INSERT [Statuses] ON;
    EXEC(N'INSERT INTO [Statuses] ([Id], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [DisplayOrder], [IsActive], [IsDeleted], [Name], [UpdatedAt], [UpdatedBy])
    VALUES (''e0000000-0000-0000-0000-000000000001'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Ticket has been submitted and is awaiting triage.'', 1, CAST(1 AS bit), CAST(0 AS bit), N''Open'', NULL, NULL),
    (''e0000000-0000-0000-0000-000000000002'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''An agent is actively working on the ticket.'', 2, CAST(1 AS bit), CAST(0 AS bit), N''In Progress'', NULL, NULL),
    (''e0000000-0000-0000-0000-000000000003'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Waiting on additional information, typically from the requester.'', 3, CAST(1 AS bit), CAST(0 AS bit), N''Pending'', NULL, NULL),
    (''e0000000-0000-0000-0000-000000000004'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''A fix has been applied and is awaiting confirmation.'', 4, CAST(1 AS bit), CAST(0 AS bit), N''Resolved'', NULL, NULL),
    (''e0000000-0000-0000-0000-000000000005'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, N''Ticket is fully closed and archived.'', 5, CAST(1 AS bit), CAST(0 AS bit), N''Closed'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Statuses]'))
        SET IDENTITY_INSERT [Statuses] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ActivityLogs_CreatedAt] ON [ActivityLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ActivityLogs_EntityName] ON [ActivityLogs] ([EntityName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ActivityLogs_IsDeleted] ON [ActivityLogs] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ActivityLogs_UserId] ON [ActivityLogs] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_IsActive] ON [AspNetUsers] ([IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_IsDeleted] ON [AspNetUsers] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Categories_IsDeleted] ON [Categories] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_IsDeleted] ON [Notifications] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_IsRead] ON [Notifications] ([IsRead]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_RelatedTicketId] ON [Notifications] ([RelatedTicketId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Priorities_IsDeleted] ON [Priorities] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Priorities_Name] ON [Priorities] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Statuses_IsDeleted] ON [Statuses] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Statuses_Name] ON [Statuses] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketAttachments_IsDeleted] ON [TicketAttachments] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketAttachments_TicketId] ON [TicketAttachments] ([TicketId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketAttachments_UploadedByUserId] ON [TicketAttachments] ([UploadedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketComments_IsDeleted] ON [TicketComments] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketComments_TicketId] ON [TicketComments] ([TicketId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketComments_UserId] ON [TicketComments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketHistories_ChangedByUserId] ON [TicketHistories] ([ChangedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketHistories_IsDeleted] ON [TicketHistories] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketHistories_TicketId] ON [TicketHistories] ([TicketId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_AssignedToUserId] ON [Tickets] ([AssignedToUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_CategoryId] ON [Tickets] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_CreatedAt] ON [Tickets] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_CreatedByUserId] ON [Tickets] ([CreatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_IsDeleted] ON [Tickets] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_PriorityId] ON [Tickets] ([PriorityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_StatusId] ON [Tickets] ([StatusId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tickets_TicketNumber] ON [Tickets] ([TicketNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706172924_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260706172924_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706182946_AddRefreshTokens'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Token] nvarchar(256) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [RevokedAt] datetime2 NULL,
        [ReplacedByToken] nvarchar(256) NULL,
        [CreatedByIp] nvarchar(45) NULL,
        [RevokedByIp] nvarchar(45) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706182946_AddRefreshTokens'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_IsDeleted] ON [RefreshTokens] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706182946_AddRefreshTokens'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706182946_AddRefreshTokens'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706182946_AddRefreshTokens'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260706182946_AddRefreshTokens', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707071853_AddTicketAssignments'
)
BEGIN
    CREATE TABLE [TicketAssignments] (
        [Id] uniqueidentifier NOT NULL,
        [TicketId] uniqueidentifier NOT NULL,
        [PreviousAssignedToUserId] uniqueidentifier NULL,
        [AssignedToUserId] uniqueidentifier NULL,
        [AssignedByUserId] uniqueidentifier NULL,
        [AssignmentType] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_TicketAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TicketAssignments_AspNetUsers_AssignedByUserId] FOREIGN KEY ([AssignedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TicketAssignments_AspNetUsers_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TicketAssignments_AspNetUsers_PreviousAssignedToUserId] FOREIGN KEY ([PreviousAssignedToUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TicketAssignments_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707071853_AddTicketAssignments'
)
BEGIN
    CREATE INDEX [IX_TicketAssignments_AssignedByUserId] ON [TicketAssignments] ([AssignedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707071853_AddTicketAssignments'
)
BEGIN
    CREATE INDEX [IX_TicketAssignments_AssignedToUserId] ON [TicketAssignments] ([AssignedToUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707071853_AddTicketAssignments'
)
BEGIN
    CREATE INDEX [IX_TicketAssignments_IsDeleted] ON [TicketAssignments] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707071853_AddTicketAssignments'
)
BEGIN
    CREATE INDEX [IX_TicketAssignments_PreviousAssignedToUserId] ON [TicketAssignments] ([PreviousAssignedToUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707071853_AddTicketAssignments'
)
BEGIN
    CREATE INDEX [IX_TicketAssignments_TicketId] ON [TicketAssignments] ([TicketId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707071853_AddTicketAssignments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707071853_AddTicketAssignments', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707133401_AddSystemSettings'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] uniqueidentifier NOT NULL,
        [SiteName] nvarchar(200) NOT NULL,
        [MaxFileUploadSizeMb] int NOT NULL,
        [AllowedFileExtensions] nvarchar(500) NOT NULL,
        [DefaultPageSize] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707133401_AddSystemSettings'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AllowedFileExtensions', N'CreatedAt', N'CreatedBy', N'DefaultPageSize', N'DeletedAt', N'DeletedBy', N'IsDeleted', N'MaxFileUploadSizeMb', N'SiteName', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[SystemSettings]'))
        SET IDENTITY_INSERT [SystemSettings] ON;
    EXEC(N'INSERT INTO [SystemSettings] ([Id], [AllowedFileExtensions], [CreatedAt], [CreatedBy], [DefaultPageSize], [DeletedAt], [DeletedBy], [IsDeleted], [MaxFileUploadSizeMb], [SiteName], [UpdatedAt], [UpdatedBy])
    VALUES (''f0000000-0000-0000-0000-000000000001'', N''.pdf,.png,.jpg,.jpeg,.docx,.xlsx,.txt,.zip'', ''2026-01-01T00:00:00.0000000Z'', NULL, 20, NULL, NULL, CAST(0 AS bit), 10, N''HelpDesk System'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AllowedFileExtensions', N'CreatedAt', N'CreatedBy', N'DefaultPageSize', N'DeletedAt', N'DeletedBy', N'IsDeleted', N'MaxFileUploadSizeMb', N'SiteName', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[SystemSettings]'))
        SET IDENTITY_INSERT [SystemSettings] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707133401_AddSystemSettings'
)
BEGIN
    CREATE INDEX [IX_SystemSettings_IsDeleted] ON [SystemSettings] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707133401_AddSystemSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707133401_AddSystemSettings', N'8.0.11');
END;
GO

COMMIT;
GO

