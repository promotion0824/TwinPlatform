DROP PROCEDURE IF EXISTS [dbo].GetTicketSequenceNumber
GO

CREATE PROCEDURE [dbo].GetTicketSequenceNumber
   @sitePrefix varchar(32),
   @siteSuffix varchar(32)

AS

DECLARE @SequenceSufix varchar(64)

SET @SequenceSufix = REPLACE(REPLACE(CONCAT(@sitePrefix, @siteSuffix), ' ', ''), '-', '_')

WHILE 1=1
  BEGIN 

    BEGIN TRY

	  /* First see if we can the next sequence number */
	  DECLARE @select varchar(256) = 'SELECT ''' + @sitePrefix + ''' as Prefix, NEXT VALUE FOR dbo.TicketSequenceNumber_' + @SequenceSufix + ' as NextNumber';
	  
	  EXEC (@select)

	END TRY
	BEGIN CATCH

		/* If not a invalid object error then we're done */
		IF CHARINDEX('Invalid object name', ERROR_MESSAGE()) = 0
		  BEGIN
		  
			SELECT ERROR_NUMBER() AS ErrorNumber,
		  	  	   ERROR_SEVERITY() AS ErrorSeverity, 
		  		   ERROR_STATE() AS ErrorState,
		  		   ERROR_PROCEDURE() AS ErrorProcedure,
		  		   ERROR_LINE() AS ErrorLine,
		  		   ERROR_MESSAGE() AS ErrorMessage; 			  
		  
		    BREAK
		  
		  END

		BEGIN TRY

			DECLARE @startNumber bigint

			SELECT @startNumber = NextNumber FROM dbo.WF_TicketNextNumber WHERE Prefix = @sitePrefix

			IF @startNumber is null
				SET @startNumber = 1
			ELSE
				SET @startNumber = @startNumber + 100

			/* Create a sequence object for this site */
			DECLARE @select2 varchar(512) = 'CREATE SEQUENCE dbo.TicketSequenceNumber_' + @SequenceSufix + ' AS bigint START WITH ' + CAST(@startNumber as varchar(16)) + ' INCREMENT BY 1';

			EXEC (@select2)

		END TRY
		BEGIN CATCH

		   /* If this object already exists then try to get a sequence number again */
		   IF CHARINDEX('There is already an object named', ERROR_MESSAGE()) > 0
		   	 CONTINUE
		   
		   /* Otherwise return error */
		   SELECT ERROR_NUMBER() AS ErrorNumber,
		   		ERROR_SEVERITY() AS ErrorSeverity, 
		   		ERROR_STATE() AS ErrorState,
		   		ERROR_PROCEDURE() AS ErrorProcedure,
		   		ERROR_LINE() AS ErrorLine,
		   		ERROR_MESSAGE() AS ErrorMessage; 			  
		   
		   BREAK

		END CATCH

 	    CONTINUE

    END CATCH

	BREAK

  END

GO
