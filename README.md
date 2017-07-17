# ACRMIf.FireWebHandler
WF 4 Activities for Aurea CRM Interface in order to raise event on EventHub (workaround)

## Developer Notes
At this moment this is a premature (working) release, in which the most important <import> and <update> commands already trigger the Eventhandler developed for web. Building this project and customizing it for further needs requires a deep understanding in Aurea CRM (formerly update.CRM) development. On-demand contact me at miklos.moricz@strictline.biz

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

### Dependencies for VS Project - StrictLine.ACRMIf.SampleEventHandler
Location (web Assemblies): \<webInstallationFolder\>\bin\
- **update.Crm.Base**.dll
- **update.Crm.Base.Contracts**.dll
- **update.Lib.Contracts**.dll
- **update.Lib**.dll
- **update.Lib.Services**.dll

Settings.xml Customization:
_________________________________
	<update.crm></update.crm>
	<update.crm.base>
		<DefaultLanguage>eng</DefaultLanguage>
		<Vertical>FS</Vertical>
		<MaxCommunicationProcesses>10</MaxCommunicationProcesses>
		<DataBase>
			<LockFieldAttributes>Hidden</LockFieldAttributes>
			<Schema>
				<CustomFields>customfields.xml</CustomFields>
			</Schema>
		</DataBase>		
	</update.crm.base>
	<update.crm.core type="update.Crm.Core.Fs.Application,update.Crm.Core.Fs">
	</update.crm.core>
	
	<update.server>
		<PlugIns>
			<PlugIn type="update.Crm.Base.PlugIn,update.Crm.Base"/>
			<PlugIn type="update.Configuration.PlugIn,update.Configuration"/>
			<PlugIn type="update.Crm.PlugIn,update.Crm"/>			
			
			<PlugIn type="update.Lib.Services.PlugIn,update.Lib.Services" />
			<PlugIn type="update.Interface.PlugIn,update.Interface" />
			<PlugIn type="update.Interface.Web.PlugIn,update.Interface.Web" />
			
			<PlugIn type="StrictLine.ACRMIf.FireWebHandler.PlugIn,StrictLine.ACRMIf.FireWebHandler" required="true"/>
			<PlugIn type="StrictLine.ACRMIf.SampleEventHandler.PlugIn,StrictLine.ACRMIf.SampleEventHandler" required="true"/>
		</PlugIns>
	</update.server>
	
	<update.configuration>
		<OleDbDsn>MyOLEDBConnectionStringComesHere</OleDbDsn>
		<Vertical>FS</Vertical>
	</update.configuration>	
