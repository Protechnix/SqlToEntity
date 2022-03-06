CREATE TABLE [dbo].[PersonHobby]
(
    [PersonId] INT NOT NULL, 
    [HobbyId] INT NOT NULL,
    [StartDate] DATE NULL, 
    CONSTRAINT [PK_PersonHobby] PRIMARY KEY (PersonId, HobbyId),
    CONSTRAINT [FK_PersonHobby_Person] FOREIGN KEY ([PersonId]) REFERENCES [Person]([Id]),
    CONSTRAINT [FK_PersonHobby_Hobby] FOREIGN KEY ([PersonId]) REFERENCES [Hobby]([Id])
)
