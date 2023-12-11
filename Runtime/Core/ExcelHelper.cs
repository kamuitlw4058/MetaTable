using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.IO;
using System.Linq;


#if UNITY_EDITOR
using Excel;
using OfficeOpenXml;



namespace MetaTable
{
    public class ExcelHelper
    {

        public static List<MetaTableColumn> ParserEPPlus(string excelFile, int headCount = 3)
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
    }
}
#endif