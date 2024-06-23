using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Lớp đọc file CSV.
/// </summary>
public class CSVReader
{
    // Biểu thức chính quy để phân chia các trường CSV
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";

    // Biểu thức chính quy để phân chia dòng
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";

    // Mảng kí tự cần xóa khỏi giá trị
    static char[] TRIM_CHARS = { '\"' };

    /// <summary>
    /// Đây là một phương thức tĩnh đọc tệp CSV và trả về dưới dạng danh sách Từ điển.
    /// </summary>
    /// <param name="file">Tệp CSV để đọc</param>
    /// <returns>Danh sách từ điển chứa dữ liệu CSV</returns>
    public static List<Dictionary<string, object>> Read(string file)
    {
        var list = new List<Dictionary<string, object>>();

        // Tải tệp CSV từ thư mục Tài nguyên
        TextAsset data = Resources.Load(file) as TextAsset;

        // Chia nội dung file theo dòng
        var lines = Regex.Split(data.text, LINE_SPLIT_RE);

        // Nếu nội dung có ít hơn một dòng, một danh sách trống sẽ được trả về.
        if (lines.Length <= 1) return list;

        // Trích xuất tiêu đề.
        var header = Regex.Split(lines[0], SPLIT_RE);

        // Đặt nội dung của mỗi dòng vào một danh sách.
        for (var i = 1; i < lines.Length; i++)
        {
            // Chia giá trị bằng biểu thức chính quy.
            // Bỏ qua nếu không có giá trị hoặc giá trị đầu tiên trống.
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                // Xóa dấu ngoặc kép khỏi giá trị và thay thế các ký tự đặc biệt.
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                value = value.Replace("<br>", "\n");
                value = value.Replace("<c>", ",");

                // chuyển đổi giá trị
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                else if (value.ToLower() == "true")
                {
                    finalvalue = true;
                }
                else if (value.ToLower() == "false")
                {
                    finalvalue = false;
                }

                // Thêm giá trị
                entry[header[j]] = finalvalue;
            }

            // Thêm từ vào danh sách
            list.Add(entry);
        }
        return list;
    }
}