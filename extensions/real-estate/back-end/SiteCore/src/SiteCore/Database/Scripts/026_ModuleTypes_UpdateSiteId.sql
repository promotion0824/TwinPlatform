DECLARE @MyCursor CURSOR;
DECLARE @ModuleId uniqueidentifier;
DECLARE @SiteId uniqueidentifier;
DECLARE @Is3D bit;
DECLARE @Prefix nvarchar(100);
DECLARE @ExistingModuleTypeId uniqueidentifier;
DECLARE @NewModuleTypeId uniqueidentifier;

BEGIN
    SET @MyCursor = CURSOR FOR
    select Id from dbo.Modules     

    OPEN @MyCursor 
    FETCH NEXT FROM @MyCursor 
    INTO @ModuleId

    WHILE @@FETCH_STATUS = 0
    BEGIN
    
      PRINT 'Retrieving data for ' + convert(nvarchar(50), @ModuleId)
    
      select @SiteId = f.SiteId, @Is3D = mt.Is3D, @Prefix = mt.Prefix from Modules m
      inner join Floors f on f.Id = m.FloorId
      left join ModuleTypes mt on mt.Id = m.ModuleTypeId
      where m.Id = @ModuleId
      
      PRINT 'Retrieving existing module for ' + convert(nvarchar(50), @ModuleId) + ' ' + convert(nvarchar(50), @SiteId) + ' ' + @Prefix + ' ' + convert(nvarchar(1), @Is3D)  
          
      SELECT @ExistingModuleTypeId = Id FROM ModuleTypes 
      where Prefix = @Prefix and Is3D = @Is3D 
      and SiteId = @SiteId
            
	  IF @ExistingModuleTypeId is null		
	  BEGIN
	    SET @NewModuleTypeId = NEWID()
	    
		INSERT into ModuleTypes
		select @NewModuleTypeId, mt.Name, mt.Prefix, mt.ModuleGroup, mt.SortOrder, mt.Is3D, mt.CanBeDeleted, f.SiteId
		from Modules m
        inner join Floors f on f.Id = m.FloorId
        left join ModuleTypes mt on mt.Id = m.ModuleTypeId
        where m.Id = @ModuleId    
	  	
	  	UPDATE Modules SET ModuleTypeId = @NewModuleTypeId WHERE Id = @ModuleId
	  	
		PRINT 'Created module type ' + convert(nvarchar(50), @NewModuleTypeId) 
	  END     
	  ELSE
	  BEGIN
		PRINT 'Should update with ' + convert(nvarchar(50), @ExistingModuleTypeId)
		
	  	UPDATE Modules SET ModuleTypeId = @ExistingModuleTypeId WHERE Id = @ModuleId
	  END   
            
      SET @ExistingModuleTypeId = null
      
      FETCH NEXT FROM @MyCursor 
      INTO @ModuleId 
    END; 
        
    DELETE FROM ModuleTypes where SiteId is null

    CLOSE @MyCursor ;
    DEALLOCATE @MyCursor;
END


