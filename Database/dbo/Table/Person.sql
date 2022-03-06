CREATE TABLE [dbo].[Person]
(
    [Id] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY,
    [FirstName] VARCHAR(30) NOT NULL,
    [LastName] VARCHAR(30) NULL,
    [EmailAddress] VARCHAR(30) NOT NULL,
    [City] VARCHAR(30) NOT NULL
)
