create database Asterisk_DB;

use Asterisk_DB;

create table Asterisks(
id_Asterisk int primary key identity,
name_Asterisk nvarchar(20) not null unique,
prefix_Asterisk nvarchar(10) not null unique,
ip_address nvarchar(50) not null unique,
login_AMI nvarchar(20) not null,
password_AMI nvarchar(256) not null,
asterisk_owner uniqueidentifier foreign key references dbo.aspnet_Users(UserId),
tls_enabled int,
tls_certDestination nvarchar(50)
)

create table transferedUser(
transferedUser nvarchar(9) primary key,
originalContext nvarchar(20),
originalAsterisk nvarchar(20),
currentAsterisk nvarchar(20)
)

alter proc insertUniqueAsterisk
@name_asterisk nvarchar(20),
@prefix_Asterisk nvarchar(10),
@ip_address nvarchar(20),
@login_AMI nvarchar(20),
@password_AMI nvarchar(256),
@asterisk_owner nvarchar(256),
@tls_enabled int,
@tls_certDestination nvarchar(50)
as
Begin
	declare @countName int
	declare @countIP int
	declare @countPrefix int

	Select @countName = COUNT(name_Asterisk) from Asterisks
	where [name_Asterisk] = @name_asterisk 

	Select @countIP = COUNT(ip_address) from Asterisks
	where [ip_address] = @ip_address

	Select @countPrefix = COUNT(prefix_Asterisk)from Asterisks
	where [prefix_Asterisk] = @prefix_asterisk

	if(@countName = 1)
		begin
			select 1 as ReturnCode
		end
	else if(@countIP = 1)
		begin
			select 2 as ReturnCode
		end
	else if(@countPrefix = 1)
		begin
			select 3 as ReturnCode
		end
	else
	begin	
		INSERT INTO Asterisks(name_Asterisk, prefix_Asterisk, ip_address, login_AMI, password_AMI, asterisk_owner, tls_enabled, tls_certDestination)
                    values(@name_Asterisk, @prefix_Asterisk, @ip_address, @login_AMI, @password_AMI, (SELECT UserId FROM dbo.aspnet_Users WHERE UserName = @asterisk_owner), @tls_enabled, @tls_certDestination)
		select -1 as ReturnCode
	end
End

alter proc updateAsterisk
@id_Asterisk int,
@name_asterisk nvarchar(20),
@prefix_Asterisk nvarchar(10),
@ip_address nvarchar(20),
@login_AMI nvarchar(20),
@password_AMI nvarchar(256),
@tls_enabled int,
@tls_certDestination nvarchar(50)
as
Begin	
	begin try
		UPDATE Asterisks 
		SET name_Asterisk = @name_asterisk, prefix_Asterisk = @prefix_Asterisk, ip_address = @ip_address, login_AMI = @login_AMI, password_AMI = @password_AMI,
		tls_enabled = @tls_enabled, tls_certDestination = @tls_certDestination
		WHERE id_Asterisk = @id_Asterisk 
		select 'OK' as ReturnCode
	end try
	begin catch
		select ERROR_MESSAGE() as ReturnCode
	end catch
End

select * from Asterisks 
delete from Asterisks where id_Asterisk = 126 
delete from Trunks
drop table Asterisks
delete from dbo.aspnet_Users
delete from transferedUser
select * from transferedUser

update transferedUser set currentAsterisk = 'asterisk225' where transferedUser = '222221111'

update Asterisks
set prefix_Asterisk = '1'
where id_Asterisk = 4

select COUNT(name_Asterisk), COUNT(ip_address), COUNT(prefix_Asterisk) from Asterisks where name_Asterisk = 'asterisk214' or ip_address = '158.196.244.214' or prefix_Asterisk = '1'

INSERT INTO Trunks(trunk_name, host_ip, context_name, id_Asterisk)
values('trunk1', '1.1.1.1','context','1')

select * from Asterisks where asterisk_owner = (select UserId from dbo.aspnet_Users where UserName = 'bud0019')

INSERT INTO Asterisks(name_Asterisk, prefix_Asterisk, ip_address, login_AMI, password_AMI, asterisk_owner)
                    values('asterisk223', '2', '158.196.244.223', 'asterisk214', 'asterisk214', (SELECT UserId FROM dbo.aspnet_Users WHERE UserName = 'bud0019'))
SELECT * FROM sys.messages
WHERE text like '%duplicate%' and text like '%key%' and language_id = 1033