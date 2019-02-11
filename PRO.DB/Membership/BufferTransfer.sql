IF OBJECT_ID('BufferTransfer', 'P') IS NOT NULL
DROP PROC BufferTransfer
GO

CREATE PROC [dbo].[BufferTransfer]  (
	@Email nvarchar(256)
	)
AS   

BEGIN

	MERGE CONTACTS C
	USING BufferRegister B
	ON B.Email = C.Email AND B.Email = @Email
	WHEN MATCHED THEN
	  UPDATE
	  SET C.FirstName = B.FirstName,
		C.LastName = B.LastName,
		C.Email = B.Email,
		C.City = B.City
	WHEN NOT MATCHED BY TARGET THEN
	  INSERT (FirstName, LastName, Email, City)
	  VALUES (B.FirstName, B.LastName, B.Email, B.City);

	declare @GatewayID numeric(18,0)
	select top 1 @GatewayID = GatewayID from Gateways where IsDefault=1


	INSERT INTO [TRANSACTIONS] ([TransactionNumber], PayerID, TransactionToken, Amount, GatewayID, ContactID, TransactionDate)
	SELECT B.TransactionNumber, B.PayerID, B.TransactionToken, M.Price, @GatewayID, C.ContactID, getdate()
	FROM BufferRegister	B 
		LEFT JOIN Membership M
			ON M.MembershipId=B.MembershipId
		LEFT JOIN Contacts C ON C.Email = B.Email
	WHERE B.Email = @Email

	DELETE BufferRegister
	WHERE Email = @Email
		 
END

GO
