DECLARE @MyCursor CURSOR;
DECLARE @ModuleTypeId uniqueidentifier;
DECLARE @SiteId uniqueidentifier;
DECLARE @ModuleGroup nvarchar(100);
DECLARE @ExistingModuleGroupId uniqueidentifier;
DECLARE @NewModuleGroupId uniqueidentifier;

BEGIN
    SET @MyCursor = CURSOR FOR
    select Id from dbo.ModuleTypes     

    OPEN @MyCursor 
    FETCH NEXT FROM @MyCursor 
    INTO @ModuleTypeId

    WHILE @@FETCH_STATUS = 0
    BEGIN
    
      PRINT 'Retrieving data for ' + convert(nvarchar(50), @ModuleTypeId)
    
      select @SiteId = SiteId, @ModuleGroup = mt.ModuleGroup
      from ModuleTypes mt
      where mt.Id = @ModuleTypeId
      
      PRINT 'Retrieving existing module type for ' + convert(nvarchar(50), @ModuleTypeId) + ' - Module Group:' + @ModuleGroup 
          
      SELECT @ExistingModuleGroupId = Id 
      FROM ModuleGroups 
      where UPPER(Name) = UPPER(@ModuleGroup)
      and SiteId = @SiteId
        
      IF LEN(ISNULL(@ModuleGroup,'')) > 0
      BEGIN
	      IF @ExistingModuleGroupId is null		
	      BEGIN
	        SET @NewModuleGroupId = NEWID()
	    
		    INSERT into ModuleGroups
		    (Id, Name, SiteId, SortOrder)
		    Values
		    (@NewModuleGroupId, @ModuleGroup, @SiteId, 0)
	  	
	  	    UPDATE ModuleTypes SET ModuleGroupId = @NewModuleGroupId WHERE Id = @ModuleTypeId
	  	
		    PRINT 'Created module type ' + convert(nvarchar(50), @NewModuleGroupId) 
	      END     
	      ELSE
	      BEGIN
		    PRINT 'Should update with ' + convert(nvarchar(50), @ExistingModuleGroupId)
		
	  	    UPDATE ModuleTypes SET ModuleGroupId = @ExistingModuleGroupId WHERE Id = @ModuleTypeId	  	
	      END
      END
      ELSE
	  BEGIN
	    PRINT 'Not updating due to empty module group'
      END
            
      SET @ExistingModuleGroupId = null
      
      FETCH NEXT FROM @MyCursor 
      INTO @ModuleTypeId 
    END;

    CLOSE @MyCursor ;
    DEALLOCATE @MyCursor;
END