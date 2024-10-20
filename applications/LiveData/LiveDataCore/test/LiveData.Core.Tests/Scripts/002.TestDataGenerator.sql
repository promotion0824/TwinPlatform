DO $$ 
DECLARE
  siteId uuid [] := '{76c0cdf8-430d-46ed-9c07-9dd127aa479f, 5f37484b-4c10-40ae-90f3-69b25b3c19b3}';
  equipmentId uuid [] := '{e14d1a69-c326-4478-8345-678522fe9146, f8c54db2-c74a-44eb-848a-0e4638433db4}';
  pointEntityId uuid [] := '{0979c55a-d2fe-482a-9eff-23a96789c5c5, 8e618850-e717-4817-abcb-d0f3b7874878, ebfbe65f-57b4-4ef8-8d32-9d5b8fa819c6, ba00c7b3-5fb8-47d2-bc24-95ff3d2bd94e}';
  timestamp timestamp := '2019-09-19 00:00:00';
  interval interval := interval '5 minutes';
  i INTEGER := 1;
  j INTEGER := 1;
  values double precision [][] := '{{1,0,0,0,0,1,1,1,0,0,1,1,0,0,0}, {2,3,35,23,2,5,6,34,6,3,4,4,3,1.534,53.34}}'; -- Both Time Series Data Array should have same length

BEGIN 
  LOOP
    EXIT WHEN i > array_length(values, 2);
    j := 1;
    LOOP
      EXIT WHEN j > array_length(siteId, 1);
      INSERT INTO public.livedata("Timestamp", "Value", "SiteId", "EquipmentId", "PointEntityId")
        VALUES (timestamp, values[j][i], siteId[j], equipmentId[j], pointEntityId[j]);
	  INSERT INTO public.livedata("Timestamp", "Value", "SiteId", "EquipmentId", "PointEntityId")
        VALUES (timestamp, values[j][i], siteId[j], equipmentId[j], pointEntityId[j+2]);
      j := j + 1;
    END LOOP;
    timestamp := timestamp + interval;
    i := i + 1;
  END LOOP;
END $$;