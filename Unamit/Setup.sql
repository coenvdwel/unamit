create table [User]
(
    [Id] nvarchar(255) not null,
    [Password] nvarchar(255) not null,
    [Partner] nvarchar(255) null,
    constraint [PK_User] primary key clustered([Id] asc)
)

create table [Name]
(
    [Id] nvarchar(255) not null,
    [Gender] int not null,
    constraint [PK_Name] primary key clustered([Id] asc)
)

create table [Group]
(
    [Id] nvarchar(255) not null,
    constraint [PK_Group] primary key clustered([Id] asc)
)

create table [NameGroups]
(
    [Name] nvarchar(255) not null,
    [Group] nvarchar(255) not null,
    constraint [PK_NameGroups] primary key clustered([Name] asc, [Group] asc)
)

create table [Rating]
(
    [User] nvarchar(255) not null,
    [Name] nvarchar(255) not null,
    [Value] int not null
    constraint [PK_Rating] primary key clustered([User] asc, [Name] asc)
)
