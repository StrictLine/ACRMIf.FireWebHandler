# ACRMIf.FireWebHandler
WF 4 Activities for Aurea CRM Interface in order to raise event on EventHub (workaround)

## First steps

### Dependencies for VS Project - StrictLine.ARCMIf.FireWebHandler
Location (web Assemblies): \<webInstallationFolder\>\bin\
- **update.Crm.Base**.dll
- **update.Crm.Base.Contracts**.dll
- **update.Lib.Contracts**.dll
- **update.Lib**.dll
- **update.Lib.Services**.dll

### Dependencies for VS Project - StrictLine.ACRMIf.SampleWorkflows
Location (interface Assemblies): \<interfaceInstallationFolder\>\bin\
- **update.Interface**.dll (in VS > Toolbox > Choose Items > Browse << than choose ProcessXml)
- **update.Crm.Base**.dll
- **update.Crm.Base.Contracts**.dll
- **update.Lib.Contracts**.dll
- **update.Lib**.dll
- **update.Lib.Services**.dll
