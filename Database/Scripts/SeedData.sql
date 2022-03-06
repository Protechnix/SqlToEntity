delete from PersonHobby
delete from HobbyEquipment
delete from Phone
delete from Person
delete from Hobby
delete from Equipment

DBCC CHECKIDENT('Person', RESEED, 0)
DBCC CHECKIDENT('Hobby', RESEED, 0)
DBCC CHECKIDENT('Equipment', RESEED, 0)

insert into Person ([FirstName], [LastName], [EmailAddress], [City]) values ('Joe', 'Shmoe', 'jshmoe@notreal.com', 'Los Angeles')
insert into Person ([FirstName], [LastName], [EmailAddress], [City]) values ('Steve', 'Green', 'sgreen@notreal.com', 'Dublin')
insert into Person ([FirstName], [LastName], [EmailAddress], [City]) values ('Michelle', null, 'mjones@notreal.com', 'Los Angeles')
insert into Person ([FirstName], [LastName], [EmailAddress], [City]) values ('Joan', 'Branch', 'jbranch@notreal.com', 'New York')

insert into Phone ([Number], [Type], [PersonId]) values ('555-555-5550', 'Home', 1)
insert into Phone ([Number], [Type], [PersonId]) values ('555-555-5551', 'Cell', 1)
insert into Phone ([Number], [Type], [PersonId]) values ('555-555-5552', 'Cell', 2)
insert into Phone ([Number], [Type], [PersonId]) values ('555-555-5553', 'Home', 3)
insert into Phone ([Number], [Type], [PersonId]) values ('555-555-5554', 'Cell', 3)
insert into Phone ([Number], [Type], [PersonId]) values ('555-555-5555', 'Cell', 4)

insert into Hobby ([Name]) values ('Skiing')
insert into Hobby ([Name]) values ('Surfing')
insert into Hobby ([Name]) values ('Running')
insert into Hobby ([Name]) values ('Swimming')

insert into Equipment ([Name], [Cost]) values ('Skiis', 723)
insert into Equipment ([Name], [Cost]) values ('Ski Poles', 119)
insert into Equipment ([Name], [Cost]) values ('Ski Goggles', 75)
insert into Equipment ([Name], [Cost]) values ('Surf Board', 400)
insert into Equipment ([Name], [Cost]) values ('Running Shoes', null)
insert into Equipment ([Name], [Cost]) values ('Swimming Goggles', 80)
insert into Equipment ([Name], [Cost]) values ('Bathing Suit',37)

insert into HobbyEquipment (HobbyId, EquipmentId) values (1, 1)
insert into HobbyEquipment (HobbyId, EquipmentId) values (1, 2)
insert into HobbyEquipment (HobbyId, EquipmentId) values (1, 3)
insert into HobbyEquipment (HobbyId, EquipmentId) values (1, 7)
insert into HobbyEquipment (HobbyId, EquipmentId) values (2, 4)
insert into HobbyEquipment (HobbyId, EquipmentId) values (2, 7)
insert into HobbyEquipment (HobbyId, EquipmentId) values (3, 5)
insert into HobbyEquipment (HobbyId, EquipmentId) values (4, 6)
insert into HobbyEquipment (HobbyId, EquipmentId) values (4, 7)

insert into PersonHobby (PersonId, HobbyId, StartDate) values (1, 3, '2019-03-01')
insert into PersonHobby (PersonId, HobbyId, StartDate) values (1, 4, '2021-10-15')
insert into PersonHobby (PersonId, HobbyId, StartDate) values (2, 1, '2007-09-22')
insert into PersonHobby (PersonId, HobbyId, StartDate) values (3, 2, '1998-01-04')
insert into PersonHobby (PersonId, HobbyId, StartDate) values (3, 3, null)
insert into PersonHobby (PersonId, HobbyId, StartDate) values (3, 4, '2005-08-11')
insert into PersonHobby (PersonId, HobbyId, StartDate) values (4, 1, '2001-11-30')
insert into PersonHobby (PersonId, HobbyId, StartDate) values (4, 2, '2003-02-17')
insert into PersonHobby (PersonId, HobbyId, StartDate) values (4, 3, null)
