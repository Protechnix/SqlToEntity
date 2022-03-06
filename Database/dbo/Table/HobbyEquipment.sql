CREATE TABLE [dbo].[HobbyEquipment]
(
    [HobbyId] INT NOT NULL,
    [EquipmentId] INT NOT NULL,
    CONSTRAINT [PK_HobbyEquipment] PRIMARY KEY (HobbyId, EquipmentId),
    CONSTRAINT [FK_HobbyEquipment_Hobby] FOREIGN KEY ([HobbyId]) REFERENCES [Hobby]([Id]),
    CONSTRAINT [FK_HobbyEquipment_Equipment] FOREIGN KEY ([EquipmentId]) REFERENCES [Equipment]([Id])
)
