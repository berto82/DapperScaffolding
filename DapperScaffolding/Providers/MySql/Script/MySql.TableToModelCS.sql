SET SESSION group_concat_max_len = 1000000;

SELECT 
    CONCAT(
        'using Dapper.Contrib.Extensions;\n\n',
        'namespace %NAMESPACE% \n\t{\n',
        '    [Table("', TABLE_NAME, '")]\n',
        '    public class ', TABLE_NAME, ' {\n',
        GROUP_CONCAT(
            CONCAT(
                CASE WHEN COLUMN_KEY = 'PRI' THEN '        [Key]\n' ELSE '' END,
                '        public ',
                CASE DATA_TYPE
                    WHEN 'int' THEN 'int'
                    WHEN 'bigint' THEN 'long'
                    WHEN 'smallint' THEN 'short'
                    WHEN 'tinyint' THEN 
                        CASE 
                            WHEN COLUMN_TYPE = 'tinyint(1)' THEN 'bool'
                            ELSE 'byte'
                        END
                    WHEN 'varchar' THEN 'string'
                    WHEN 'text' THEN 'string'
                    WHEN 'datetime' THEN 'DateTime'
                    WHEN 'date' THEN 'DateTime'
                    WHEN 'decimal' THEN 'decimal'
                    WHEN 'float' THEN 'float'
                    WHEN 'double' THEN 'double'
                    WHEN 'time' THEN 'TimeSpan'
                    WHEN 'bit' THEN 'bool'
                    ELSE 'string'
                END,
                CASE 
                    WHEN IS_NULLABLE = 'YES' 
                         AND DATA_TYPE NOT IN ('varchar','text') 
                    THEN '?' 
                    ELSE '' 
                END,
                ' ',
                CONCAT(UCASE(LEFT(COLUMN_NAME,1)), SUBSTRING(COLUMN_NAME,2)),
                ' { get; set; }'
            )
            ORDER BY ORDINAL_POSITION
            SEPARATOR '\n'
        ),
        '\n    }\n',
        '}'
    ) AS ModelClass
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = '%DATABASE%'
and TABLE_NAME = '%TABLENAME%'
GROUP BY TABLE_NAME;