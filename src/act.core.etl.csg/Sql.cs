namespace act.core.etl.csg
{
    public static class Sql
    {
	    public const string NodeDump = @"SELECT 
    n.InventoryItemId GamInventoryItemId,
    app.Id AppSpecId,
    app.Name AppSpecName,
    appO.SamAccountName AppSpecOwnerLanId,
    COALESCE(appO.PreferredName,
            appO.FirstName + ' ' + appO.LastName) AppSpecOwnerName,
    os.Id OsSpecId,
    os.Name OsSpecName,
    os.OperatingSystemName Os,
    os.OperatingSystemVersion OsVersion,
    osO.SamAccountName OsSpecOwnerLanId,
    COALESCE(osO.PreferredName,
            osO.FirstName + ' ' + osO.LastName) OsSpecOwnerName,
    e.Name NodeEnvironmentName,
    n.ChefNodeId,
    CASE
        WHEN n.ComplianceStatus = 1 THEN 'Failed'
        WHEN n.ComplianceStatus = 2 THEN 'Succeeded'
        ELSE NULL
    END ComplianceStatus,
    n.LastComplianceResultDate,
    n.Fqdn,
    CASE
        WHEN n.Platform = 5 THEN 1
        ELSE 0
    END IsAppliance,
    CASE
        WHEN n.Platform = 3 THEN 1
        ELSE 0
    END IsUnixPhysical,
    CASE
        WHEN n.PciScope = 1 THEN 'A'
        WHEN n.PciScope = 2 THEN 'B'
        ELSE 'C'
    END PciScope,
    o.SamAccountName GamTechContactLanId,
    COALESCE(o.PreferredName,
            o.FirstName + ' ' + o.LastName) GamTechContactName
FROM
    Node n
        LEFT JOIN
    BuildSpecification app ON n.BuildSpecificationId = app.Id
        LEFT JOIN
    BuildSpecification os ON app.ParentId = os.Id
        JOIN
    Environment e ON n.EnvironmentId = e.id
        JOIN
    Employee o ON n.OwnerEmployeeId = o.Id
        LEFT JOIN
    Employee appO ON app.OwnerEmployeeId = appO.Id
        LEFT JOIN
    Employee osO ON os.OwnerEmployeeId = osO.Id
WHERE
    n.IsActive = 1";

// Used [ACCCSGISQLD0065] as prefix to access [Global_Employee] database as it resides in a different database server 
// and is linked to the GAM server through link servers. 
        public const string NodeCount = @"SELECT 
count(1)
FROM [dbo].[AllDataWithActivationStatus] ad
JOIN [ACCCSGISQLD0065].[Global_Employee].[dbo].[EMPLOYEES] emp
on ad.[InventoryItem_SupportGroup_PrimaryContactEmployeeId] = emp.[EMPLOYEE_ID]
JOIN [ACCCSGISQLD0065].[Global_Employee].[dbo].[EMAIL] eml
on eml.[EMPLOYEE_ID] = emp.[EMPLOYEE_ID]
WHERE 
NOT emp.[LAN_LOGIN] is null AND
emp.IsActive = 1 AND
eml.Email_type_cd = 'M' AND
eml.Active_Cd = 'A' AND
NOT [Hosts_FullyQualifiedDomainName] is null AND
(NOT [Hosts_FullyQualifiedDomainName] = 'NONE' OR ([Hosts_FullyQualifiedDomainName] = 'NONE' AND ([Hosts_Domain] not in('none','workgroup') or [Hosts_HostName] like '%.com') AND Hosts_HostType in('Windows Server', 'UNIX Server', 'Midrange'))) AND
NOT [InventoryItem_SupportGroup_PrimaryContactEmployeeId] is null AND
NOT [InventoryItem_ProductCodeId] is null AND
NOT [InventoryItem_FunctionId] is null AND
NOT [InventoryItem_FunctionName] is null AND
DateDeactivated is null";

        public const string EmployeeCount = @"SELECT
COUNT(1)  
FROM [dbo].[EMPLOYEES] emp	
JOIN [dbo].[Email] eml
	on emp.EMPLOYEE_ID = eml.EMPLOYEE_ID	
WHERE not [LAN_LOGIN] is null and IsActive = 1 and Email_type_cd = 'M' and Active_Cd = 'A'";

        public const string EmployeeQuery = @"SELECT
	Id
	,FirstName
	,LastName
	,PreferredName
	,SamAccountName
	,SupervisorId
	,IsActive
	,Email
	,ReportingDirectorId
	,PciDirectorId
FROM (
	SELECT
	ROW_NUMBER() OVER( ORDER BY emp.[EMPLOYEE_ID] ASC) [RowNumber]
	,emp.[EMPLOYEE_ID][Id]
	,[EMP_FIRST_NAME][FirstName]
	,[EMP_LAST_NAME][LastName]
	,[PREFERRED_NAME][PreferredName]
	,[LAN_LOGIN][SamAccountName]
	,cast([SUPERVISOR_EMP_NUM] as bigint) [SupervisorId]
	,[IsActive]
	,[EMAIL_ADDRESS] [Email]      
	,[ReportingDirectorId]
	,[PciDirectorId]
	FROM [dbo].[EMPLOYEES] emp
	JOIN [dbo].[Email] eml
	on emp.EMPLOYEE_ID = eml.EMPLOYEE_ID
	LEFT JOIN (
		SELECT 
			[Employee_EMPLOYEE_ID][EmployeeId],
			case 
				when [Level_12_JOB_TITLE] like '%Dir%' or  [Level_12_JOB_TITLE] like 'Head%' or  [Level_12_JOB_TITLE] like '%VP%'
					then [Level_12_EMPLOYEE_ID] 
				when [Level_11_JOB_TITLE] like '%Dir%' or  [Level_11_JOB_TITLE] like 'Head%' or  [Level_11_JOB_TITLE] like '%VP%'
					then [Level_11_EMPLOYEE_ID] 
				when [Level_10_JOB_TITLE] like '%Dir%' or  [Level_10_JOB_TITLE] like 'Head%' or  [Level_10_JOB_TITLE] like '%VP%'
					then [Level_10_EMPLOYEE_ID] 
				when [Level_09_JOB_TITLE] like '%Dir%' or  [Level_09_JOB_TITLE] like 'Head%' or  [Level_09_JOB_TITLE] like '%VP%'
					then [Level_09_EMPLOYEE_ID] 
				when [Level_08_JOB_TITLE] like '%Dir%' or  [Level_08_JOB_TITLE] like 'Head%' or  [Level_08_JOB_TITLE] like '%VP%'
					then [Level_08_EMPLOYEE_ID] 
				when [Level_07_JOB_TITLE] like '%Dir%' or  [Level_07_JOB_TITLE] like 'Head%' or  [Level_07_JOB_TITLE] like '%VP%'
					then [Level_07_EMPLOYEE_ID] 
				when [Level_06_JOB_TITLE] like '%Dir%' or  [Level_06_JOB_TITLE] like 'Head%' or  [Level_06_JOB_TITLE] like '%VP%'
					then [Level_06_EMPLOYEE_ID] 
				when [Level_05_JOB_TITLE] like '%Dir%' or  [Level_05_JOB_TITLE] like 'Head%' or  [Level_05_JOB_TITLE] like '%VP%'
					then [Level_05_EMPLOYEE_ID] 
				when [Level_04_JOB_TITLE] like '%Dir%' or  [Level_04_JOB_TITLE] like 'Head%' or  [Level_04_JOB_TITLE] like '%VP%'
					then [Level_04_EMPLOYEE_ID] 
				when [Level_03_JOB_TITLE] like '%Dir%' or  [Level_03_JOB_TITLE] like 'Head%' or  [Level_03_JOB_TITLE] like '%VP%'
					then [Level_03_EMPLOYEE_ID] 
				when [Level_02_JOB_TITLE] like '%Dir%' or  [Level_02_JOB_TITLE] like 'Head%' or  [Level_02_JOB_TITLE] like '%VP%'
					then [Level_02_EMPLOYEE_ID] 
				when [Level_01_JOB_TITLE] like '%Dir%' or  [Level_01_JOB_TITLE] like 'Head%' or  [Level_01_JOB_TITLE] like '%VP%'
					then [Level_01_EMPLOYEE_ID] 
				else null end [ReportingDirectorId],
			case 
				when [Level_12_JOB_TITLE] like '%Dir%' or  [Level_12_JOB_TITLE] like 'Head%'
					then [Level_12_EMPLOYEE_ID] 
				when [Level_11_JOB_TITLE] like '%Dir%' or  [Level_11_JOB_TITLE] like 'Head%'
					then [Level_11_EMPLOYEE_ID] 
				when [Level_10_JOB_TITLE] like '%Dir%' or  [Level_10_JOB_TITLE] like 'Head%'
					then [Level_10_EMPLOYEE_ID] 
				when [Level_09_JOB_TITLE] like '%Dir%' or  [Level_09_JOB_TITLE] like 'Head%'
					then [Level_09_EMPLOYEE_ID] 
				when [Level_08_JOB_TITLE] like '%Dir%' or  [Level_08_JOB_TITLE] like 'Head%'
					then [Level_08_EMPLOYEE_ID] 
				when [Level_07_JOB_TITLE] like '%Dir%' or  [Level_07_JOB_TITLE] like 'Head%'
					then [Level_07_EMPLOYEE_ID] 
				when [Level_06_JOB_TITLE] like '%Dir%' or  [Level_06_JOB_TITLE] like 'Head%'
					then [Level_06_EMPLOYEE_ID] 
				when [Level_05_JOB_TITLE] like '%Dir%' or  [Level_05_JOB_TITLE] like 'Head%'
					then [Level_05_EMPLOYEE_ID] 
				when [Level_04_JOB_TITLE] like '%Dir%' or  [Level_04_JOB_TITLE] like 'Head%'
					then [Level_04_EMPLOYEE_ID] 
				when [Level_03_JOB_TITLE] like '%Dir%' or  [Level_03_JOB_TITLE] like 'Head%'
					then [Level_03_EMPLOYEE_ID] 
				when [Level_02_JOB_TITLE] like '%Dir%' or  [Level_02_JOB_TITLE] like 'Head%'
					then [Level_02_EMPLOYEE_ID] 
				when [Level_01_JOB_TITLE] like '%Dir%' or  [Level_01_JOB_TITLE] like 'Head%'
					then [Level_01_EMPLOYEE_ID] 
				else null end [PciDirectorId]
		FROM 
			[dbo].[vw_EmplevelsExpanded]
		) dir
	on emp.EMPLOYEE_ID = dir.EmployeeId
	WHERE not [LAN_LOGIN] is null and IsActive = 1 and Email_type_cd = 'M' and Active_Cd = 'A'
) t
WHERE t.RowNumber between @from and @to";
        public const string ProductQuery = @"SELECT DISTINCT
      [InventoryItem_ProductCodeId] [Code]
      ,[InventoryItem_ProductCodeDescription] [Name]      
  FROM [dbo].[AllData]
  WHERE 
	not [InventoryItem_SupportGroup_PrimaryContactEmployeeId] is null AND
	not [InventoryItem_ProductCodeId] is null AND
	not [InventoryItem_FunctionId] is null AND
	not [InventoryItem_FunctionName] is null";

        public const string FunctionQuery = @"SELECT DISTINCT
      [InventoryItem_FunctionId] [Id]
	  ,[InventoryItem_FunctionName] [Name]
  FROM [dbo].[AllData]
  WHERE 
	not [InventoryItem_SupportGroup_PrimaryContactEmployeeId] is null AND
	not [InventoryItem_ProductCodeId] is null AND
	not [InventoryItem_FunctionId] is null AND
	not [InventoryItem_FunctionName] is null";

// Used [ACCCSGISQLD0065] as prefix to access [Global_Employee] database as it resides in a different database server 
// and is linked to the GAM server through link servers. 
        public const string NodeQuery = @"SELECT 
	[InventoryItemId]
	,[Fqdn]
	,[OwnerEmployeeId]
	,[ProductCode]
	,[FunctionId]
	,[Platform]
	,[PciScope]
	,[EnvironmentId]
	,[IsActive]
	,[DeactivatedDate]
	,[RemedyGroupName]
	,[RemedyGroupEmailList]
FROM (
		SELECT
		ROW_NUMBER() OVER( ORDER BY [InventoryItem_Id] ASC) [RowNumber]
		,[InventoryItem_Id] [InventoryItemId]
		,case when [Hosts_FullyQualifiedDomainName] = 'None' then [Hosts_HostName] + (case when [Hosts_HostName] like '%.com' then '' else '.' + [Hosts_Domain] + (CASE WHEN [Hosts_Domain] like '%.com' then '' else '.com' end) end) else [Hosts_FullyQualifiedDomainName] end [Fqdn]
		,[InventoryItem_SupportGroup_PrimaryContactEmployeeId] [OwnerEmployeeId]
		,[InventoryItem_ProductCodeId] [ProductCode]
		,[InventoryItem_FunctionId] [FunctionId]
		,[InventoryItem_SupportGroupName] [RemedyGroupName]
		,[InventoryItem_SupportGroup_EmailList] [RemedyGroupEmailList]
	,case 
		when [IsAppliance] = 1 then 5
		when [Hosts_HostType] = 'UNIX Server' and [Hosts_OsVendor] in ('Sun','IBM','HP') then 3 
		when [Hosts_HostType] = 'UNIX Server 'and not [Hosts_OsVendor] in ('Sun','IBM','HP') then 0
		when  [Hosts_HostType] = 'Windows Client' or [Hosts_HostType] = 'Windows Desktop' then 4
		when  [Hosts_HostType] = 'Windows Server' then 1
	else 2
end [Platform]
,CASE WHEN [SecurityClass] = 'A' THEN 1 WHEN [SecurityClass] = 'B' THEN 2 ELSE 4 END [PciScope]
,cast(case when InventoryItem_ServiceLevelId = 12 or InventoryItem_ServiceLevelId = 19 then InventoryItem_ServiceLevelId  else 1 end as int) [EnvironmentId]
,cast(case when [Hosts_ActiveCD] = 'Y' then 1 else 0 end as bit) [IsActive]
,(case when [Hosts_ActiveCD] = 'Y' then NULL else [DateDeactivated] end)[DeactivatedDate]
FROM [dbo].[AllDataWithActivationStatus] ad
JOIN [ACCCSGISQLD0065].[Global_Employee].[dbo].[EMPLOYEES] emp
on ad.[InventoryItem_SupportGroup_PrimaryContactEmployeeId] = emp.[EMPLOYEE_ID]
JOIN [ACCCSGISQLD0065].[Global_Employee].[dbo].[EMAIL] eml
on eml.[EMPLOYEE_ID] = emp.[EMPLOYEE_ID]
WHERE 
NOT emp.[LAN_LOGIN] is null AND
emp.IsActive = 1 AND
eml.Email_type_cd = 'M' AND
eml.Active_Cd = 'A' AND
NOT [Hosts_FullyQualifiedDomainName] is null AND
(NOT [Hosts_FullyQualifiedDomainName] = 'NONE' OR ([Hosts_FullyQualifiedDomainName] = 'NONE' AND ([Hosts_Domain] not in('none','workgroup') or [Hosts_HostName] like '%.com') AND Hosts_HostType in('Windows Server', 'UNIX Server', 'Midrange'))) AND
not [InventoryItem_SupportGroup_PrimaryContactEmployeeId] is null AND
not [InventoryItem_ProductCodeId] is null AND
not [InventoryItem_FunctionId] is null AND
not [InventoryItem_FunctionName] is null and 
DateDeactivated is null
) t
WHERE t.RowNumber between @from and @to";
    }
}