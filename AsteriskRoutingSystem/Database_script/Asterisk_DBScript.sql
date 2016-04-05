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
name_user nvarchar(9) primary key,
original_context nvarchar(20),
original_asterisk nvarchar(20),
current_asterisk nvarchar(20)
)

create proc insertAsterisk
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

create proc updateAsterisk
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

delete from Asterisks
