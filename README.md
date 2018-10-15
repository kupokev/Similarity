### Similarity
I had a need for a project to use the similarity functionality from Master Data Services from SQL Server on a SQL Server install running on a Windows Server Core install. 
This version of the install does not support Master Data Services so I set out to roll my own version.
This version is not a 100% match nor contains all of the functionality of the Similarity functionality in Master Data Services. 
It does contain a subset of functionality that is in the neighborhood of 90% similar results based on the testing I have done so far.

Please use this at your own risk. I am not responsible for any damages caused to any systems by running this code.


#### Microsoft SQL Server CLR Integration
In order to make this work you will need to use SQL Server CLR integration. If you are not familiar with it you can read more on it at the link below.

https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/introduction-to-sql-server-clr-integration

The common language runtime (CLR) integration feature is off by default in Microsoft SQL Server, and must be enabled in order to use objects that are implemented using CLR integration.

To enable the CLR Integration run the following script in SQL Server

> -- Enable Clr
> sp_configure 'clr enabled', 1;
> GO
> RECONFIGURE;
> GO


#### Install Instructions

1) Compile the Script

Open the Developer Command Prompt for Visual Studio. In Windows 10 hit the Windows Key and type "Dev".

2) Enter the command:

> csc /target:library C:\Clr\SimilarityProc.cs

3) Install assembly

In SQL Sever run the following script:

> ALTER DATABASE BCC_DWH SET TRUSTWORTHY ON;
> GO
>
> ALTER AUTHORIZATION ON DATABASE::BCC_DWH TO sa;
> 
> CREATE ASSEMBLY Similarity from 'C:\Clr\SimilarityProc.dll' WITH PERMISSION_SET = SAFE
> 
> CREATE FUNCTION Similarity(@source nvarchar(500), @target nvarchar(500), @method int, @weight float, @minScore float) RETURNS float
> EXTERNAL NAME Similarity.SimilarityProc.Similarity;

You can run the following to test and make sure it works.

> -- Test it
> SELECT dbo.Similarity('Wahsington', 'Wqshington', 0, 0, 0);



