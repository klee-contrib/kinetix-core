﻿UPDATE WFAD
   SET WFAD.NAME = @NAME
  FROM WF_ACTIVITY_DEFINITION WFAD
  WHERE WFAD.WFAD_ID = @WFAD_ID;