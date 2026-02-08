SET SESSION group_concat_max_len = 1000000;

SELECT 
    CONCAT(
        'Import Dapper.Contrib.Extensions\n\n',
        'Namespace %NAMESPACE% \n\t\n',
        '    <Table("', TABLE_NAME, '")>\n',
        '    Public Class ', TABLE_NAME, '\n',
        GROUP_CONCAT(
            CONCAT(
                CASE WHEN COLUMN_KEY = 'PRI' THEN '        <Key>\n' ELSE '' END,
                '        Public ',
              
                CONCAT(LEFT(COLUMN_NAME,1), SUBSTRING(COLUMN_NAME,2)),
                  CASE DATA_TYPE
                    WHEN 'int' THEN ' As Integer'
                    WHEN 'bigint' THEN 'As Long'
                    WHEN 'smallint' THEN ' As Short'
                    WHEN 'tinyint' THEN 
                        CASE 
                            WHEN COLUMN_TYPE = 'tinyint(1)' THEN ' As Boolean'
                            ELSE ' As Byte'
                        END
                    WHEN 'varchar' THEN ' As String'
                    WHEN 'text' THEN ' As String'
                    WHEN 'datetime' THEN ' As DateTime'
                    WHEN 'date' THEN ' As DateTime'
                    WHEN 'decimal' THEN ' As Decimal'
                    WHEN 'float' THEN ' As Double'
                    WHEN 'double' THEN ' As Double'
                    WHEN 'time' THEN ' As TimeSpan'
                    WHEN 'bit' THEN ' As Boolean'
                    ELSE ' As String'
                END,
                CASE 
                    WHEN IS_NULLABLE = 'YES' 
                         AND DATA_TYPE NOT IN ('varchar','text') 
                    THEN '?' 
                    ELSE '' 
                END,
                ' '            
            )
            ORDER BY ORDINAL_POSITION
            SEPARATOR '\n'
        ),
        '\n    End Class\n',
        'End Namespace'
    ) AS ModelClass
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = '%DATABASE%'
and TABLE_NAME = '%TABLENAME%'
GROUP BY TABLE_NAME;