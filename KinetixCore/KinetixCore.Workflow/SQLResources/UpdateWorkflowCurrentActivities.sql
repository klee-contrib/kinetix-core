﻿UPDATE WFW
   SET WFW.WFA_ID_2 = tab.WFA_ID_2
  FROM WF_WORKFLOW WFW
  JOIN @table tab ON (WFW.WFW_ID = tab.WFW_ID);