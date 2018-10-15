https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/introduction-to-sql-server-clr-integration


Compile the Script
Open the Developer Command Prompt for Visual Studio. In Windows 10 hit the Windows Key and type "Dev".

Enter the command:
csc /target:library C:\Clr\SimilarityProc.cs


Enabling CLR Integration
The common language runtime (CLR) integration feature is off by default in Microsoft SQL Server, and must be enabled in order to use objects that are implemented using CLR integration.

In SQL Server:
-- Enable Clr
sp_configure 'clr enabled', 1;
GO
RECONFIGURE;
GO

-- Install assembly
ALTER DATABASE BCC_DWH SET TRUSTWORTHY ON;
GO

ALTER AUTHORIZATION ON DATABASE::BCC_DWH TO sa;

CREATE ASSEMBLY Similarity from 'C:\Clr\SimilarityProc.dll' WITH PERMISSION_SET = SAFE

CREATE FUNCTION Similarity(@source nvarchar(500), @target nvarchar(500), @method int, @weight float, @minScore float) RETURNS float
EXTERNAL NAME Similarity.SimilarityProc.Similarity;

-- Test it
SELECT dbo.Similarity('Wahsington', 'Wqshington', 0, 0, 0);



