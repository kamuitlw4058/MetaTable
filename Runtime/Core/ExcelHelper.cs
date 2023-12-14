using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.IO;
using System.Linq;
using LitJson;
using System.Reflection;
using Pangoo.Common;

#if UNITY_EDITOR
using Excel;
using OfficeOpenXml;



namespace MetaTable
{
    public class ExcelHelper
    {

        public static List<MetaTableColumn> ParserEPPlusColumns(string excelFile, int headCount = 3)
        {

            FileInfo existingFile = new FileInfo(excelFile);
            ExcelPackage package = new ExcelPackage(existingFile);

            //判断Excel文件中是否存在数据表
            if (package.Workbook.Worksheets.Count < 1)
            {
                return null;
            }

            //默认读取第一个数据表
            DataTable mSheet = EPPlusHelper.WorksheetToTable(excelFile);

            //判断数据表内是否存在数据
            if (mSheet.Rows.Count < 1)
            {
                return null;
            }

            //读取数据表行数和列数
            int rowCount = mSheet.Rows.Count;       //行
            int colCount = mSheet.Columns.Count;    //列


            Dictionary<string, MetaTableColumn> colDict = new Dictionary<string, MetaTableColumn>();


            for (int i = 0; i < colCount; i++)
            {

                var name = mSheet.Columns[i].ToString();
                var type = mSheet.Rows[0][i].ToString();
                var cnName = mSheet.Rows[1][i].ToString();

                // Debug.Log($"Head:{name}<<<Type:{type}<<<CNName:{cnName}");

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type))
                {
                    throw new Exception($"ExcelTableData 文件:{excelFile} ColsIndex:{i} 名字或者类型为空:{name},{type}");
                }


                if (colDict.ContainsKey(name))
                {
                    throw new Exception($"ExcelTableData 文件:{excelFile} Cols:{colCount} 有重名列:{name}");
                }

                colDict.Add(name, new MetaTableColumn()
                {
                    Name = name,
                    Type = type,
                    CnName = cnName,
                });
            }
            return colDict.Values.ToList();
        }

        public static Dictionary<string, PropertyMetadata> GetPropertyDict(ObjectMetadata metadata)
        {
            Dictionary<string, PropertyMetadata> dic = new Dictionary<string, PropertyMetadata>();
            foreach (var propPair in metadata.Properties)
            {
                var prop_data = propPair.Value;
                var excelTableRowAttribute = prop_data.Info.GetCustomAttribute<MetaTableRowColumnAttribute>();
                if (excelTableRowAttribute != null)
                {
                    dic.Add(excelTableRowAttribute.Name, prop_data);
                }
            }

            return dic;
        }

        public static List<MetaTableRow> LoadFromExcelFile(string excelFilePath, Type rowType)
        {
            List<MetaTableRow> ret = new List<MetaTableRow>();
            var fileInfo = new FileInfo(excelFilePath);
            // Debug.Log($"excelFilePath:{excelFilePath}");
            ExcelPackage excelPackage = new ExcelPackage(fileInfo);
            if (excelPackage.Workbook.Worksheets.Count == 0)
            {
                return ret;
            }

            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[1];

            JsonMapper.AddObjectMetadata(rowType);
            ObjectMetadata metadata = JsonMapper.GetObjectMetadata(rowType);
            var propertyDict = GetPropertyDict(metadata);

            Dictionary<int, string> headDict = new Dictionary<int, string>();
            bool hasUuid = false;
            for (int j = 0; j < worksheet.Dimension.Columns; j++)
            {
                var cellValue = worksheet.Cells[0 + 1, j + 1].Value;
                headDict.Add(j, cellValue.ToString());
                if (cellValue.ToString().Equals("uuid"))
                {
                    hasUuid = true;
                }
            }


            for (int row = 3; row < worksheet.Dimension.Rows; row++)
            {
                MetaTableRow tableRow = ClassUtility.CreateInstance(rowType) as MetaTableRow;
                if (tableRow == null)
                {
                    return ret;
                }

                for (int col = 0; col < worksheet.Dimension.Columns; col++)
                {
                    string head;
                    PropertyMetadata propertyMetadata;
                    if (headDict.TryGetValue(col, out head) && propertyDict.TryGetValue(head, out propertyMetadata))
                    {
                        var excelTableRowAttribute = propertyMetadata.Info.GetCustomAttribute<MetaTableRowColumnAttribute>();
                        if (excelTableRowAttribute != null && excelTableRowAttribute.Name == head)
                        {
                            var cellValue = worksheet.Cells[row + 1, col + 1].Value;

                            if (propertyMetadata.IsField)
                            {
                                var fieldInfo = propertyMetadata.Info as FieldInfo;
                                var value = cellValue.ToString().ToValue(fieldInfo.FieldType);
                                ((FieldInfo)propertyMetadata.Info).SetValue(
                                    tableRow, value);
                            }
                            // 属性的设置暂时不考虑
                            // else
                            // {
                            //     PropertyInfo p_info = (PropertyInfo)propertyMetadata.Info;

                            //     if (p_info.CanWrite)
                            //         p_info.SetValue(eventsRow, cellValue, null);
                            // }
                            //    Debug.Log($"Set  cellValue:{cellValue} propertyMetadata.Info:{propertyMetadata.Info.GetType()} excelTableRowAttribute:{excelTableRowAttribute.Name}");
                        }
                    }

                }

                if (!hasUuid)
                {
                    tableRow.Uuid = UuidUtility.GetNewUuid();
                }

                ret.Add(tableRow);
            }


            return ret;
        }

        public static List<T> LoadFromExcelFile<T>(string excelFilePath) where T : MetaTableRow, new()
        {
            List<T> ret = new List<T>();
            var fileInfo = new FileInfo(excelFilePath);
            // Debug.Log($"excelFilePath:{excelFilePath}");
            ExcelPackage excelPackage = new ExcelPackage(fileInfo);
            if (excelPackage.Workbook.Worksheets.Count == 0)
            {
                return ret;
            }

            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[1];

            Type rowType = typeof(T);
            JsonMapper.AddObjectMetadata(rowType);
            ObjectMetadata metadata = JsonMapper.GetObjectMetadata(rowType);
            var propertyDict = GetPropertyDict(metadata);

            Dictionary<int, string> headDict = new Dictionary<int, string>();
            bool hasUuid = false;
            for (int j = 0; j < worksheet.Dimension.Columns; j++)
            {
                var cellValue = worksheet.Cells[0 + 1, j + 1].Value;
                headDict.Add(j, cellValue.ToString());
                if (cellValue.ToString().Equals("uuid"))
                {
                    hasUuid = true;
                }
            }


            for (int row = 3; row < worksheet.Dimension.Rows; row++)
            {
                var tableRow = new T();
                for (int col = 0; col < worksheet.Dimension.Columns; col++)
                {
                    string head;
                    PropertyMetadata propertyMetadata;
                    if (headDict.TryGetValue(col, out head) && propertyDict.TryGetValue(head, out propertyMetadata))
                    {
                        var excelTableRowAttribute = propertyMetadata.Info.GetCustomAttribute<MetaTableRowColumnAttribute>();
                        if (excelTableRowAttribute != null && excelTableRowAttribute.Name == head)
                        {
                            var cellValue = worksheet.Cells[row + 1, col + 1].Value;

                            if (propertyMetadata.IsField)
                            {
                                var fieldInfo = propertyMetadata.Info as FieldInfo;
                                var value = cellValue.ToString().ToValue(fieldInfo.FieldType);
                                ((FieldInfo)propertyMetadata.Info).SetValue(
                                    tableRow, value);
                            }
                            // 属性的设置暂时不考虑
                            // else
                            // {
                            //     PropertyInfo p_info = (PropertyInfo)propertyMetadata.Info;

                            //     if (p_info.CanWrite)
                            //         p_info.SetValue(eventsRow, cellValue, null);
                            // }
                            //    Debug.Log($"Set  cellValue:{cellValue} propertyMetadata.Info:{propertyMetadata.Info.GetType()} excelTableRowAttribute:{excelTableRowAttribute.Name}");
                        }
                    }

                }

                if (!hasUuid)
                {
                    tableRow.Uuid = UuidUtility.GetNewUuid();
                }

                ret.Add(tableRow);
            }

            return ret;

        }
    }


}
#endif