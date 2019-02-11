
IF OBJECT_ID('BufferRegisterSave', 'P') IS NOT NULL
DROP PROC BufferRegisterSave
GO

CREATE PROC [dbo].[BufferRegisterSave]  (
	@Email nvarchar(256),
    @FirstName nvarchar(256),   
    @LastName nvarchar(256),
	@City varchar(250),
	@MembershipId numeric(18,0)
	)
AS   

BEGIN
	declare @TokenID nvarchar(256)
	set @TokenID = null
	Select @TokenID = TokenId from BufferRegister WHERE Email=@Email

	if @TokenID is null 
		Begin
		/* insert */
		insert into BufferRegister(Email, FirstName, LastName, City, MembershipId) 
		VALUES(@email,@firstname,@lastname,@city, @MembershipId)
		END
	else
		begin
		update BufferRegister 
		set Email = @Email, 
			FirstName=@FirstName, 
			LastName=@LastName, 
			City=@City,
			MembershipId = @MembershipId
		WHERE Email=@Email
		end

	Select b.*, m.Title MembershipTitle, m.[Description] MembershipDescription, m.RoleName, m.Price from BufferRegister b
		left join Membership m on m.MembershipID = b.MembershipId
	WHERE Email=@Email

END

