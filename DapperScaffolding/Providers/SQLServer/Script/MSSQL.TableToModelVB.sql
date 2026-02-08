declare @TableName sysname  = '%TABLENAME%'
-----------------SQL Script-------------------------------
DECLARE @NewLine VARCHAR(10)=char(13)
declare @Imports varchar(max) = 'Imports Dapper.Contrib.Extensions'
declare @NameSpace varchar(max) = 'Namespace %NAMESPACE%' + @newline
declare @TableAttribute varchar(max) ='    <Table("'+@tablename+'")>' + @newline
declare @Result varchar(max) = @imports + @newline + @newline + @namespace + @tableattribute + '    Public Class ' + @TableName

select @Result = @Result +@newline + case when keyfield <> '' then  KeyField + @newline else '' end + '        Public Property ' + ColumnName + ' AS ' + ColumnType + NullableSign 
from
(
    select 
        replace(col.name, ' ', '_') ColumnName, column_id ColumnId,
        case typ.name 
            when 'bigint' then 'Long'
            when 'binary' then 'Byte()'	
            when 'bit' then 'Boolean'
            when 'char' then 'string'
            when 'date' then 'Date'
            when 'datetime' then 'Date'
            when 'datetime2' then 'Date'
            when 'datetimeoffset' then 'DateTimeOffset'
            when 'decimal' then 'Decimal'
            when 'float' then 'Double'
            when 'image' then 'Byte()'
            when 'int' then 'Integer'
            when 'money' then 'Decimal'
            when 'nchar' then 'String'
            when 'ntext' then 'String'
            when 'numeric' then 'Decimal'
            when 'nvarchar' then 'String'
            when 'real' then 'Double'
            when 'smalldatetime' then 'Date'
            when 'smallint' then 'Short'
            when 'smallmoney' then 'Decimal'
            when 'text' then 'String'
            when 'time' then 'TimeSpan'
            when 'timestamp' then 'Long'
            when 'tinyint' then 'Byte'
            when 'uniqueidentifier' then 'Guid'
            when 'varbinary' then 'Byte()'
            when 'varchar' then 'String'
            else 'UNKNOWN_' + typ.name
        end ColumnType,
        case 
            when col.is_nullable = 1 and typ.name in ('bigint', 'bit', 'date', 'datetime', 'datetime2', 'datetimeoffset', 'decimal', 'float', 'int', 'money', 'numeric', 'real', 'smalldatetime', 'smallint', 'smallmoney', 'time', 'tinyint', 'uniqueidentifier') 
                then '?' 
            else '' 
        end NullableSign,
		case
		    when col.is_identity = 1
				then '        <Key>'
			else ''
         end KeyField
    from sys.columns col
        join sys.types typ on
            col.system_type_id = typ.system_type_id AND col.user_type_id = typ.user_type_id
    where object_id = object_id(@TableName)
) t
order by ColumnId

set @Result = @Result  + @NewLine+ '    End Class' + @NewLine + 'End Namespace'

select @Result