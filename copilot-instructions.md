# **Requirement: Hangfire Job for 3rd Party Data Synchronization**

## **1. Overview**

The objective is to update the existing TAMHR.hangfire project for a new recurring Hangfire background job within the existing .NET solution. This job will be responsible for fetching data from multiple internal database sources, processing it in batches, and synchronizing it with a third-party API. The process must be reliable, performant, and include mechanisms for logging and preventing duplicate data transmission.

remove the hangfireSchedulerApp Reference from the TAMHR.hangfire project. and use the hangfireSchedulerApp just for knowledge reference.

## **2. Functional Requirements**

### **2.1. Hangfire Job Creation**

* A new recurring background job shall be created and registered with Hangfire.  
* The job's recurrence schedule should be configurable (e.g., via appsettings.json), with a suggested default of running once every 24 hours.

### **2.2. Data Fetching Logic**

* The job will execute a sequence of data synchronization tasks in the following order:  
  1. Users  
  2. ActualOrg  
  3. ActualEntity  
  4. OrgObject  
  5. EventsCalendar  
* For each data type, the job must fetch all records that have not yet been successfully synchronized.  
* **Data Sources:**  
  * Users, ActualOrg, ActualEntity, OrgObject will be fetched using the DefaultConnection.  
  * EventsCalendar will be fetched using the EssConnection.

### **2.3. Data Batching and API Submission**

* For each data type, the collected records must be sent to the corresponding third-party API endpoint in batches.  
* The batch size shall be configurable, with a default of **3000 records** per API call.  
* **Example Flow:** If there are 10,000 User records to send, the system will make four separate API calls: three with 3000 records each, and one with the remaining 1000 records.  
* This batching process will be repeated for each subsequent data type (ActualOrg, etc.).

### **2.4. Duplicate Prevention (Tracking)**

* A new database table, TB_R_SYNC_TRACKING, will be created in the database specified by LogConnection.  
* Before fetching data for a given type (e.g., Users), the job must first query the TB_R_SYNC_TRACKING table to identify the IDs of all users that have already been sent. These users should be excluded from the current run.  
* After a batch is **successfully** sent to the third-party API, the unique identifiers of the records in that batch must be inserted into the TB_R_SYNC_TRACKING table. This marks them as "sent" and prevents them from being included in future job runs.

### **2.5. Logging**

* The existing SchedulerLog table (in the database specified by LogConnection) shall be used for all logging.  
* Every individual batch API call (both successful and failed attempts) must be logged as a new entry in the SchedulerLog table.  
* The SchedulerLog fields should be populated as follows:  
  * **Activity**: A descriptive string, e.g., "Sync Batch: Users".  
  * **Status**: "Success" or "Failure".  
  * **AdditionalInformation**: A JSON string containing contextual details of the API call, e.g., { "EndpointUrl": "...", "BatchSize": 3000, "HttpResponseCode": 200, "DurationMs": 1500 }.  
  * **ExceptionMessage**: The exception message text if the API call failed.

### **2.6. Error Handling**

* If an API call for a specific batch fails, the system should:  
  1. Log the failure in the SchedulerLog table as described above.  
  2. **Not** mark the records in the failed batch as "sent" in the TB_R_SYNC_TRACKING table. This ensures they will be picked up in the next run.  
  3. The job should continue processing the remaining batches and data types. A failure in one batch should not halt the entire job.

## **3. Data Models**

This section defines the C\# classes that represent the data to be synchronized and logged.

### **3.1. User (Users)**

public class User  
{  
    public Guid Id { get; set; }  
    public string NoReg { get; set; }  
    public string Username { get; set; }  
    public string Name { get; set; }  
    public string Gender { get; set; }  
    public string? Email { get; set; }  
    public int IsActive { get; set; }  
}

### **3.2. ActualOrganizationStructure (ActualOrg)**

public class ActualOrganizationStructure  
{  
    public Guid Id { get; set; }  
    public string OrgCode { get; set; }  
    public string? ParentOrgCode { get; set; }  
    public string? OrgName { get; set; }  
    public string? Service { get; set; }  
    public string? NoReg { get; set; }  
    public string? Name { get; set; }  
    public string? PostCode { get; set; }  
    public string? PostName { get; set; }  
    public string? JobCode { get; set; }  
    public string? JobName { get; set; }  
    public string? EmployeeGroup { get; set; }  
    public string? EmployeeGroupText { get; set; }  
    public string? EmployeeSubgroup { get; set; }  
    public string? EmployeeSubgroupText { get; set; }  
    public string? WorkContract { get; set; }  
    public string? WorkContractText { get; set; }  
    public string? PersonalArea { get; set; }  
    public string? PersonalSubarea { get; set; }  
    public int? DepthLevel { get; set; }  
    public decimal? Staffing { get; set; }  
    public int? Chief { get; set; }  
    public int? OrgLevel { get; set; }  
    public int? Vacant { get; set; }  
    public string? Structure { get; set; }  
    public string? ObjectDescription { get; set; }  
}

### **3.3. ActualEntityStructure (ActualEntity)**

public class ActualEntityStructure  
{  
    public Guid Id { get; set; }  
    public string OrgCode { get; set; }  
    public string ObjectCode { get; set; }  
    public string ObjectText { get; set; }  
    public string ObjectDescription { get; set; }  
}

### **3.4. OrganizationObject (OrgObject)**

public class OrganizationObject  
{  
    public Guid Id { get; set; }  
    public string ObjectID { get; set; }  
    public string ObjectType { get; set; }  
    public string Abbreviation { get; set; }  
    public string? ObjectText { get; set; }  
    public DateTime StartDate { get; set; }  
    public DateTime EndDate { get; set; }  
    public string? ObjectDescription { get; set; }  
}

### **3.5. EventsCalendar (EventsCalendar)**

public class EventsCalendar  
{  
    public Guid Id { get; set; }  
    public string Title { get; set; }  
    public string EventTypeCode { get; set; }  
    public DateTime StartDate { get; set; }  
    public DateTime EndDate { get; set; }  
    public string? Description { get; set; }  
    public string CreatedBy { get; set; }  
    public DateTime CreatedOn { get; set; }  
    public string? ModifiedBy { get; set; }  
    public DateTime? ModifiedOn { get; set; }  
    public bool RowStatus { get; set; }  
}

### **3.6. SchedulerLog (Log) \- Existing**

public class SchedulerLog  
{  
    public Guid ID { get; set; }  
    public string ApplicationName { get; set; }  
    public string LogID { get; set; }  
    public string LogCategory { get; set; }  
    public string Activity { get; set; }  
    public string ApplicationModule { get; set; }  
    public string IPHostName { get; set; }  
    public string Status { get; set; }  
    public string? AdditionalInformation { get; set; }  
    public string CreatedBy { get; set; }  
    public DateTime CreatedOn { get; set; }  
    public string? ModifiedBy { get; set; }  
    public DateTime? ModifiedOn { get; set; }  
    public bool RowStatus { get; set; }  
    public string? ExceptionMessage { get; set; }  
}

## **4. Non-Functional Requirements**

### **4.1. Performance**

* Data fetching from the source databases must be efficient. It should not load all records into memory at once. Use techniques like pagination (.Skip(), .Take()) in conjunction with IQueryable to process data in manageable chunks that align with the batch size.  
* Database connections should be managed efficiently and closed/disposed of correctly.

### **4.2. Reliability & Idempotency**

* The job must be idempotent. Running the job multiple times should not result in duplicate data being sent to the third-party API, thanks to the tracking mechanism.  
* The process must be resilient to transient API failures.

### **4.3. Configuration**

* All four database connection strings must be configurable within appsettings.json.  
* Key operational values like API endpoints, batch size, and the Hangfire CRON schedule must also be configurable in appsettings.json.

### **4.4. Code & Project Structure**

* The new logic must be implemented following the existing architectural patterns and coding standards of the project.  
* No new third-party libraries should be introduced unless absolutely necessary for unit testing.

## **5. Database Requirements**

### **5.1. Connection Strings**

The following connection strings must be added to the ConnectionStrings section of the appsettings.json file.

**Example appsettings.json configuration:**

{  
  "ConnectionStrings": {  
    "HangfireConnection": "...",  
    "DefaultConnection": "data source=172.188.58.125;initial catalog=TAMHR\_MDM;persist security info=True;user id=user\_kms;password=@git123456789;TrustServerCertificate=True;MultipleActiveResultSets=True;App=EntityFramework;",  
    "EssConnection": "data source=172.188.58.125;initial catalog=TAMHR\_ESS;persist security info=True;user id=user\_kms;password=@git123456789;TrustServerCertificate=True;MultipleActiveResultSets=True;App=EntityFramework;",  
    "LogConnection": "data source=172.188.58.125;initial catalog=TAMHR\_LOG;persist security info=True;user id=user\_kms;password=@git123456789;TrustServerCertificate=True;MultipleActiveResultSets=True;App=EntityFramework;"  
  }  
}

### **5.2. New Table: TB_R_SYNC_TRACKING**

* **Database**: LogConnection  
* **Purpose**: To track every individual record that has been successfully sent.

CREATE TABLE TB_R_SYNC_TRACKING (  
    Id INT PRIMARY KEY IDENTITY(1,1),  
    EntityType NVARCHAR(100) NOT NULL, \-- e.g., 'User', 'ActualOrg'  
    EntityId NVARCHAR(255) NOT NULL,    \-- The primary key of the source record (string to be flexible)  
    SyncTimestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE()  
);  
\-- Add a unique index to prevent duplicate entries and speed up lookups  
CREATE UNIQUE INDEX UQ\_TB_R_SYNC_TRACKING\_EntityType\_EntityId   
ON TB_R_SYNC_TRACKING(EntityType, EntityId);

## **6. Unit Testing Requirements**

* Unit tests must be created for the new service/logic layer.  
* **Scope of Tests:**  
  * Verify that data is correctly filtered to exclude already synchronized records.  
  * Verify that data is correctly split into batches according to the configured size.  
  * Mock the API client to test both successful and failed API call scenarios.  
  * Verify that TB_R_SYNC_TRACKING and SchedulerLog are correctly updated after successful/failed calls.  
* **Tools:**  
  * Utilize the existing testing framework (e.g., xUnit, NUnit).  
  * If a mocking library (e.g., Moq, NSubstitute) is not already in the project, it should be added to facilitate effective testing of dependencies (database contexts, API clients).