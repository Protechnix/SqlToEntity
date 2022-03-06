CREATE TABLE [dbo].[Phone]
(
    [PersonId] INT NOT NULL,
    [Number] VARCHAR(30) NOT NULL,
    [Type] VARCHAR(10) NOT NULL,    
    CONSTRAINT [PK_PhoneNumber] PRIMARY KEY (PersonId, Number),
    CONSTRAINT [FK_PhoneNumber_Person] FOREIGN KEY ([PersonId]) REFERENCES [Person]([Id])
)
