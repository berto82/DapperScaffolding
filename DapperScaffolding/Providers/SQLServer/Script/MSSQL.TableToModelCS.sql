declare @TableName sysname  = '%TABLENAME%'
-----------------SQL Script-------------------------------
declare @NewLine varchar(10)=char(13)
declare @Tab varchar(10) = char(9)
declare @Imports varchar(max) = 'using System;' + @newline + 'using Dapper.Contrib.Extensions;'
declare @NameSpace varchar(max) = 'namespace %NAMESPACE%' + @newline + '{' + @newline
declare @TableAttribute varchar(max) ='    [Table("'+@tablename+'")]' + @newline
declare @Result varchar(max) = @imports + @newline + @newline + @namespace + @tableattribute + '    public class ' + @TableName + @newline + @Tab + '{' + @newline

select @Result = @Result + case when keyfield <> '' then  KeyField + @newline else '' end + '        public ' +  ColumnType + NullableSign + ' ' + ColumnName + ' { get; set; }' + @newline
from
(
    select 
        replace(col.name, ' ', '_') ColumnName, column_id ColumnId,
        case typ.name 
            when 'bigint' then 'long'
            when 'binary' then 'byte[]'	
            when 'bit' then 'bool'
            when 'char' then 'string'
            when 'date' then 'date'
            when 'datetime' then 'date'
            when 'datetime2' then 'date'
            when 'datetimeoffset' then 'DateTimeOffset'
            when 'decimal' then 'decimal'
            when 'float' then 'double'
            when 'image' then 'byte[]'
            when 'int' then 'int'
            when 'money' then 'decimal'
            when 'nchar' then 'string'
            when 'ntext' then 'string'
            when 'numeric' then 'decimal'
            when 'nvarchar' then 'string'
            when 'real' then 'double'
            when 'smalldatetime' then 'date'
            when 'smallint' then 'short'
            when 'smallmoney' then 'decimal'
            when 'text' then 'string'
            when 'time' then 'timespan'
            when 'timestamp' then 'long'
            when 'tinyint' then 'byte'
            when 'uniqueidentifier' then 'guid'
            when 'varbinary' then 'byte()'
            when 'varchar' then 'string'
            else 'UNKNOWN_' + typ.name
        end ColumnType,
        case 
            when col.is_nullable = 1 and typ.name in ('bigint', 'bit', 'date', 'datetime', 'datetime2', 'datetimeoffset', 'decimal', 'float', 'int', 'money', 'numeric', 'real', 'smalldatetime', 'smallint', 'smallmoney', 'time', 'tinyint', 'uniqueidentifier') 
                then '?' 
            else '' 
        end NullableSign,
		case
		    when col.is_identity = 1
				then '        [Key]'
			else ''
         end KeyField
    from sys.columns col
        join sys.types typ on
            col.system_type_id = typ.system_type_id AND col.user_type_id = typ.user_type_id
    where object_id = object_id(@TableName)
) t
order by ColumnId

set @Result = @Result  + @NewLine+ '    }' + @NewLine + '}'

select @Result