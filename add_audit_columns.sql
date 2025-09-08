-- Add missing audit columns to all tables that inherit from AuditableEntity

-- Customers table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Customers') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE dbo.Customers ADD CreatedBy NVARCHAR(256) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Customers') AND name = 'LastModifiedBy')
BEGIN
    ALTER TABLE dbo.Customers ADD LastModifiedBy NVARCHAR(256) NULL;
END

-- Products table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Products') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE dbo.Products ADD CreatedBy NVARCHAR(256) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Products') AND name = 'LastModifiedBy')
BEGIN
    ALTER TABLE dbo.Products ADD LastModifiedBy NVARCHAR(256) NULL;
END

-- PurchaseOrders table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PurchaseOrders') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE dbo.PurchaseOrders ADD CreatedBy NVARCHAR(256) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PurchaseOrders') AND name = 'LastModifiedBy')
BEGIN
    ALTER TABLE dbo.PurchaseOrders ADD LastModifiedBy NVARCHAR(256) NULL;
END

-- PurchaseOrderLines table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PurchaseOrderLines') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE dbo.PurchaseOrderLines ADD CreatedBy NVARCHAR(256) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PurchaseOrderLines') AND name = 'LastModifiedBy')
BEGIN
    ALTER TABLE dbo.PurchaseOrderLines ADD LastModifiedBy NVARCHAR(256) NULL;
END

-- SalesOrders table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SalesOrders') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE dbo.SalesOrders ADD CreatedBy NVARCHAR(256) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SalesOrders') AND name = 'LastModifiedBy')
BEGIN
    ALTER TABLE dbo.SalesOrders ADD LastModifiedBy NVARCHAR(256) NULL;
END

-- SalesOrderLines table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SalesOrderLines') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE dbo.SalesOrderLines ADD CreatedBy NVARCHAR(256) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SalesOrderLines') AND name = 'LastModifiedBy')
BEGIN
    ALTER TABLE dbo.SalesOrderLines ADD LastModifiedBy NVARCHAR(256) NULL;
END

-- FileProcessingJobs table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.FileProcessingJobs') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE dbo.FileProcessingJobs ADD CreatedBy NVARCHAR(256) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.FileProcessingJobs') AND name = 'LastModifiedBy')
BEGIN
    ALTER TABLE dbo.FileProcessingJobs ADD LastModifiedBy NVARCHAR(256) NULL;
END

-- IntegrationLogs table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IntegrationLogs') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE dbo.IntegrationLogs ADD CreatedBy NVARCHAR(256) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IntegrationLogs') AND name = 'LastModifiedBy')
BEGIN
    ALTER TABLE dbo.IntegrationLogs ADD LastModifiedBy NVARCHAR(256) NULL;
END

-- ErrorLogs table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ErrorLogs') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE dbo.ErrorLogs ADD CreatedBy NVARCHAR(256) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ErrorLogs') AND name = 'LastModifiedBy')
BEGIN
    ALTER TABLE dbo.ErrorLogs ADD LastModifiedBy NVARCHAR(256) NULL;
END

-- IntegrationMessages table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IntegrationMessages') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE dbo.IntegrationMessages ADD CreatedBy NVARCHAR(256) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.IntegrationMessages') AND name = 'LastModifiedBy')
BEGIN
    ALTER TABLE dbo.IntegrationMessages ADD LastModifiedBy NVARCHAR(256) NULL;
END

PRINT 'Audit columns added successfully to all tables';
